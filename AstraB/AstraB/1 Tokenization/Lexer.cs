using System.Globalization;

public class Lexer
{
    public List<char> chars = new();

    public int currentPos;
    public int startRead;
    public int endRead;
    public int markedPos;

    public int beginLine;
    public int currentLine;
    public int linedCurrentPos;

    public bool splitBlockCommentsIntoSeveralTokens = true;

    private bool isCollectingMultilineComment = false;
    private int multilineCommentOpenningLength = 0;

    private string CurrentWord => string.Concat(chars[startRead..currentPos]);


    private static Dictionary<string, Type> tokenTypeBySingleWord = new Dictionary<string, Type>()
    {
        { "if", typeof(Token_If) },
        { "else", typeof(Token_Else) },
        { "while", typeof(Token_While) },
        { "for", typeof(Token_For) },
        { "fn", typeof(Token_Fn) },
        { "return", typeof(Token_Return) },
        { "class", typeof(Token_Class) },
        { "new", typeof(Token_New) },
        // { "static", typeof(Token_Static) },
        // { "abstract", typeof(Token_Abstract) },
        // { "try", typeof(Token_Try) },
        // { "catch", typeof(Token_Catch) },
        // { "throw", typeof(Token_Throw) },
        { "as", typeof(Token_As) },
        { "to", typeof(Token_To) },
    };
    private static Dictionary<string, Type> tokenTypeBySingleChar = new Dictionary<string, Type>()
    {
        { "(", typeof(Token_BracketOpen) },
        { ")", typeof(Token_BracketClose) },
        { "{", typeof(Token_BlockOpen) },
        { "}", typeof(Token_BlockClose) },
        { ";", typeof(Token_Terminator) },
        { ":", typeof(Token_Colon) },
        { ",", typeof(Token_Comma) },
        { ".", typeof(Token_Dot) },
        { "[", typeof(Token_SquareBracketOpen) },
        { "]", typeof(Token_SquareBracketClose) },
    };

    private static Dictionary<Type, List<string>> operatorTokens = new()
    {
        { typeof(Token_Plus), new() { "+" } },
        { typeof(Token_Minus), new() { "-" } },
        // { typeof(Token_IncDec), new() { "++", "--" } },
        { typeof(Token_Star), new() { "*" } },
        { typeof(Token_Slash), new() { "/" } },
        // { typeof(Token_Slash), new() { "*", "/", "%" } },
        // { typeof(Token_BitOperator), new() { "<<", ">>", "&", "|" } },
        { typeof(Token_LogicalNot), new() { "not" } },
        { typeof(Token_Less), new() { "<" } },
        { typeof(Token_LessOrEqual), new() { "<=" } },
        { typeof(Token_Greater), new() { ">" } },
        { typeof(Token_GreaterOrEqual), new() { ">=" } },
        { typeof(Token_Equality), new() { "==", "!=" } },
        { typeof(Token_Assign), new() { "=" } },
        { typeof(Token_AssignByPointer), new() { "~=" } },
    };

    public List<Token> Tokenize(List<char> chars, bool includeSpacesAndEOF)
    {
        Reset(chars, 0, chars.Count, 0);
        return Tokenize(includeSpacesAndEOF);
    }

    public List<Token> Tokenize(string astraCode, bool includeSpacesAndEOF)
    {
        Reset(astraCode.ToList(), 0, astraCode.Length, 0);
        return Tokenize(includeSpacesAndEOF);
    }
    private List<Token> Tokenize(bool includeSpacesAndEOF)
    {
        List<Token> tokens = new();

        while (true)
        {
            Token token = Advance();

            if (includeSpacesAndEOF == false && (token is Token_Space || token is Token_EOF))
            {
                // Don't add to tokens
            }
            else
            {
                tokens.Add(token);
            }

            if (token is Token_EOF) break;
        }

        return tokens;
    }


    public void Reset(List<char> chars, int start, int end, int initialState)
    {
        this.chars = chars;
        currentPos = markedPos = startRead = start;
        endRead = end;

        currentLine = linedCurrentPos = 0;
        
        isCollectingMultilineComment = false;
        multilineCommentOpenningLength = 0;

        //Console.WriteLine($"Lexer got {chars.Count} chars, {start}:{end} with state {initialState}");
    }

    public Token Advance()
    {
        if (currentPos >= endRead)
        {
            return new Token_EOF();
        }

        // Save word start pos
        startRead = currentPos;

        beginLine = currentLine;


        Token token = AdvanceInternal();
        markedPos = currentPos; // Save word end pos

        if (token == null)
        {
            return new Token_Bad();
        }
        else
        {
            if (token is Token_Terminator)
            {
                linedCurrentPos = 0;
            }

            FillToken(token);

            //currentLine = token.endLine;

            if (token is Token_Terminator == false)
            {
                linedCurrentPos += currentPos - startRead;
            }

            return token;
        }
    }

    private void FillToken(Token token)
    {
        token.begin = startRead;
        token.end = currentPos;
        token.line = beginLine;
        token.endLine = currentLine;
        token.linedBegin = linedCurrentPos;

        token.chars = chars[token.begin..token.end].ToArray();
    }

    private Token AdvanceInternal()
    {
        char startChar = chars[currentPos];
        currentPos++;

        if (startChar == ' ' || startChar == '\t')
        {
            return new Token_Space();
        }


        if (startChar == '\r' || startChar == '\n' || startChar == ';')
        {
            while (currentPos < endRead && (chars[currentPos] == '\r' || chars[currentPos] == '\n' || chars[currentPos] == ';'))
            {
                if (chars[currentPos] == '\n')
                {
                    currentLine++;
                }
                currentPos++;
            }
            return new Token_Terminator();
        }


        if (tokenTypeBySingleChar.TryGetValue(startChar.ToString(), out Type singleCharTokenType))
        {
            return (Token)Activator.CreateInstance(singleCharTokenType);
        }



        if (isCollectingMultilineComment)
        {
            return ContinueParseComment();
        }
        if (startChar == '-' && currentPos < endRead && chars[currentPos] == '-')
        {
            return BeginParseComment();
        }
        




        if (char.IsDigit(startChar))
        {
            //
            // Iterate digits for numbers
            //
            return ParseNumber();
        }
        else if (startChar == '\'' || startChar == '"')
        {
            return ParseString();
        }
        else
        {
            //
            // Iterate chars for tokens
            //
            string word = "";
            
            if (TryParseOperator(out Token op))
            {
                return op;
            }
            
            if (TryParseWord(out Token token))
            {
                return token;
            }
            else
            {
                throw new Exception($"Failed to parse '{CurrentWord}'");
            }
        }
    }

    private Token ParseString()
    {
        bool isParsingString = chars[currentPos - 1] == '"';

        List<char> stringChars = new();
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            if (currentChar == '\'')
            {
                currentPos++;
                if (stringChars.Count > 1)
                {
                    return new Token_Bad();
                }
                else
                {
                    return new Token_Char(stringChars[0]);
                }
            }
            else if (currentChar == '\"')
            {
                currentPos++;
                return new Token_String(string.Concat(stringChars));
            }
            else if (currentChar == '\n' || currentChar == '\r')
            {
                return new Token_Bad();
            }
            else
            {
                stringChars.Add(currentChar);
            }

            currentPos++;
        }

        return new Token_Bad();
    }


    private Token ParseNumber()
    {
        NumberStyles numberStyle = NumberStyles.Integer;

        List<char> valueChars = new();

        if (currentPos < endRead)
        {
            char secondChar = chars[currentPos];

            if (secondChar == 'x')
            {
                numberStyle = NumberStyles.HexNumber;
                currentPos++;

                valueChars.Add('0');
                valueChars.Add('x');
            }
            else if (secondChar == 'b')
            {
                numberStyle = NumberStyles.BinaryNumber;
                currentPos++;

                valueChars.Add('0');
                valueChars.Add('b');
            }
            else if (char.IsDigit(secondChar) == false && currentPos + 1 < endRead && char.IsDigit(chars[currentPos + 1]))
            {
                return new Token_Bad();
                //throw new Exception($"Unknown number format '{secondChar}'");
            }
            else
            {
                valueChars.Add(chars[currentPos - 1]);
            }
        }

        Token parsedNumberToken = null;
        bool isCollectingBadTrail = false;

        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            if (currentChar != '_')
            {
                bool isFormatDigit;

                if (numberStyle == NumberStyles.HexNumber) isFormatDigit = char.IsAsciiHexDigit(currentChar);
                else if (numberStyle == NumberStyles.BinaryNumber) isFormatDigit = currentChar == '0' || currentChar == '1';
                else isFormatDigit = char.IsDigit(currentChar);

                bool isAnyLetter = char.IsLetterOrDigit(currentChar);

                if (isCollectingBadTrail == false)
                {
                    //if (isFormatDigit == false)
                    if (isAnyLetter == false)
                    {
                        // Reached end of formatted number -> parse it
                        parsedNumberToken = ParseNumber(string.Concat(valueChars), numberStyle);

                        if (parsedNumberToken is Token_Bad)
                        {
                            isCollectingBadTrail = true;
                        }
                        else
                        {
                            return parsedNumberToken;
                        }
                    }
                }
                else
                {
                    // Reached end of any text (like dot, bracket or new line)
                    if (char.IsLetterOrDigit(currentChar) == false)
                    {
                        break;
                    }
                }

                valueChars.Add(currentChar);
            }

            currentPos++;
        }

        if (parsedNumberToken == null) throw new Exception($"Failed to parse number '{string.Concat(valueChars)}'");
        return parsedNumberToken;
    }

    private Token ParseNumber(string word, NumberStyles numberStyle)
    {
        if (numberStyle == NumberStyles.Integer)
        {
            if (long.TryParse(word, out _))
            {
                return new Token_Constant(word);
            }
            else
            {
                return new Token_Bad();
                //throw new Exception($"Failed to parse Integer number '{word}'");
            }
        }
        else if (numberStyle == NumberStyles.HexNumber)
        {
            string valueWord = word.Substring(2, word.Length - 2);

            if (long.TryParse(valueWord, numberStyle, null, out _))
            {
                return new Token_Constant(word);
            }
            else
            {
                return new Token_Bad();
                //throw new Exception($"Failed to parse Hex number '{word}'");
            }
        }
        else if (numberStyle == NumberStyles.BinaryNumber)
        {
            string valueWord = word.Substring(2, word.Length - 2);

            if (long.TryParse(valueWord, numberStyle, null, out _))
            {
                return new Token_Constant(word);
            }
            else
            {
                return new Token_Bad();
                //throw new Exception($"Failed to parse Binary number '{word}'");
            }
        }
        else
        {
            throw new Exception($"Failed to parse number due to unknown format '{numberStyle}'");
        }
    }

    private Token BeginParseComment()
    {
        // Read comment openning
        multilineCommentOpenningLength = 1;
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            if (currentChar == '-')
            {
                multilineCommentOpenningLength++;
            }
            else
            {
                break;
            }

            currentPos++;
        }

        bool isBlock = multilineCommentOpenningLength > 2;
        isCollectingMultilineComment = isBlock;

        // Skip text section
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            if (currentChar == '\r' || currentChar == '\n')
            {
                break;
            }

            if (currentChar == '-')
            {
                // Inline block
                int endingLength = 0;
                while (currentPos < endRead)
                {
                    currentChar = chars[currentPos];

                    if (currentChar == '-')
                    {
                        endingLength++;
                    }
                    else
                    {
                        break;
                    }

                    currentPos++;
                }

                bool isEnding = endingLength == multilineCommentOpenningLength;
                if (isEnding)
                {
                    isCollectingMultilineComment = false;
                    break;
                }
            }

            currentPos++;
        }

        return new Token_Comment();
    }
    private Token ContinueParseComment()
    {
        // Skip text section
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            // For single-line comment new line is end
            if (currentChar == '\r' || currentChar == '\n')
            {
                break;
            }

            // For block comment wait for same length ending
            if (currentChar == '-')
            {
                // Read comment ending
                int endingLength = 1;
                while (currentPos < endRead)
                {
                    char commentChar = chars[currentPos];

                    if (commentChar == '-')
                    {
                        endingLength++;
                    }
                    else
                    {
                        break;
                    }

                    currentPos++;
                }

                if (endingLength == multilineCommentOpenningLength)
                {
                    isCollectingMultilineComment = false;
                    break;
                }
            }

            currentPos++;
        }

        return new Token_Comment();
    }

    private Token ParseComment()
    {
        // Read comment openning
        int openningLength = 1;
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            if (currentChar == '-')
            {
                openningLength++;
            }
            else
            {
                break;
            }

            currentPos++;
        }

        bool isBlock = openningLength > 2;

        // Skip text section
        while (currentPos < endRead)
        {
            char currentChar = chars[currentPos];

            // For single-line comment new line is end
            if (currentChar == '\r' || currentChar == '\n')
            {
                if (isBlock == false)
                {
                    break;
                }
            }

            // For block comment wait for same length ending
            if (currentChar == '-')
            {
                // Read comment ending
                int endingLength = 0;
                while (currentPos < endRead)
                {
                    char commentChar = chars[currentPos];

                    if (commentChar == '-')
                    {
                        endingLength++;
                    }
                    else
                    {
                        break;
                    }

                    currentPos++;
                }

                if (endingLength == openningLength)
                {
                    break;
                }
            }

            currentPos++;
        }

        return new Token_Comment();
    }

    private bool TryParseOperator(out Token operatorToken)
    {
        string word;
        operatorToken = null;
        
        bool? startsWithOperatorChar = null;
        Type fullMatchTokenType = null;

        while (currentPos < endRead)
        {
            word = string.Concat(chars[startRead..currentPos]);

            int matchingCount = 0;


            foreach ((Type tokenType, List<string> ls) in operatorTokens)
            {
                foreach (string op in ls)
                {
                    if (op.StartsWith(word))
                    {
                        matchingCount++;

                        if (op == word)
                        {
                            fullMatchTokenType = tokenType;
                        }
                    }
                }
            }

            if (startsWithOperatorChar == null)
            {
                startsWithOperatorChar = matchingCount > 0;
            }

            if (startsWithOperatorChar.Value)
            {
                if (matchingCount <= 1)
                {
                    if (fullMatchTokenType != null)
                    {
                        operatorToken = (Token)Activator.CreateInstance(fullMatchTokenType);

                        if (matchingCount == 0)
                        {
                            currentPos--;
                            string prevWord = string.Concat(chars[startRead..(currentPos)]);
                            // operatorToken.asmOperatorName = ParseMathOperator(prevWord);
                            // throw new NotImplementedException();
                        }
                        else
                        {
                            // throw new NotImplementedException();
                            // operatorToken.asmOperatorName = word;
                        }
                        
                        return true;
                    }
                    else if (matchingCount == 0)
                    {
                        return false;
                    }
                    else
                    {
                        currentPos++;
                    }
                }
                else
                {
                    currentPos++;
                }
            }
            else
            {
                break;
            }
        }

        return false;
    }

    private bool TryParseWord(out Token token)
    {
        while (currentPos < endRead)
        {
            if (tokenTypeBySingleWord.TryGetValue(CurrentWord, out Type tokenType))
            {
                token = (Token)Activator.CreateInstance(tokenType);
                return true;
            }
            
            if (currentPos < endRead && (char.IsLetterOrDigit(chars[currentPos]) == false && chars[currentPos] != '_'))
            {
                break;
            }

            currentPos++;
        }

        token = new Token_Identifier()
        {
            name = CurrentWord
        };
        return true;
    }
}