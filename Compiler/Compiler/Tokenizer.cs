public static class Tokenizer
{
    public static Dictionary<string, Type> tokenTypeBySingleWord = new()
    {
        { "(", typeof(Token_BracketOpen) },
        { ")", typeof(Token_BracketClose) },
        { "=", typeof(Token_Assign) },
        { "{", typeof(Token_BlockOpen) },
        { "}", typeof(Token_BlockClose) },
        { "if", typeof(Token_If) },
        { "else", typeof(Token_Else) },
        { "while", typeof(Token_While) },
        { "for", typeof(Token_For) },
        { ";", typeof(Token_Terminator) },
        { ":", typeof(Token_Colon) },
        { ",", typeof(Token_Comma) },
        { "fn", typeof(Token_Fn) },
        { "return", typeof(Token_Return) },
        { "class", typeof(Token_Class) },
        { "new", typeof(Token_New) },
        { ".", typeof(Token_Dot) },
        { "[", typeof(Token_SquareBracketOpen) },
        { "]", typeof(Token_SquareBracketClose) },
    };

    public static List<Token> Tokenize(string rawCode)
    {
        List<Token> tokens = new();

        bool isCollecting_Constant = false;

        int ci = 0;
        string word = "";
        while (ci < rawCode.Length)
        {
            char currentChar = rawCode[ci++];

            // Skip spaces and tabs
            if (currentChar == '\t')
            {
                continue;
            }


            // Start tokenize constant value (int, float, double)
            if (char.IsDigit(currentChar) && word == "")
            {
                isCollecting_Constant = true;
            }
            if (char.IsDigit(currentChar) && isCollecting_Constant)
            {                
                word += currentChar;
                continue;
            }
            else
            {
                if (isCollecting_Constant)
                {
                    if (int.TryParse(word, out int _))
                    {
                        tokens.Add(new Token_Constant()
                        {
                            value = word
                        });
                    }
                    word = "";
                }
                isCollecting_Constant = false;
            }


            // On line end reach
            if (currentChar == '\r' || currentChar == '\n' || currentChar == ' ')
            {
                // If line reached, but word has not be recognized
                if (word != "")
                {
                    if (Token_Visibility.TryMatch(word, out var vis))
                    {
                        tokens.Add(vis);
                    }
                    else if (Token_Identifier.IsMatch(word))
                    {
                        tokens.Add(new Token_Identifier()
                        {
                            name = word
                        });
                    }
                    else
                    {
                        throw new Exception($"Failed to tokenize word '{word}'");
                    }
                }

                if (currentChar != ' ')
                {
                    TerminateLine(tokens);
                }
                
                word = "";
                continue;
            }            


            // Try tokenize single char
            Token token = TryTokenize(currentChar.ToString());
            if (token != null)
            {
                if (Token_Identifier.IsMatch(word))
                {
                    tokens.Add(new Token_Identifier()
                    {
                        name = word
                    });
                    word = "";
                }
            }


            // Try tokenize whole word
            word += currentChar;

            token = TryTokenize(word);
            if (token != null)
            {
                tokens.Add(token);
                word = "";
                continue;
            }
        }

        return tokens;
    }

    private static Token TryTokenize(string word)
    {
        if (Token_Equality.TryMatch(word, out var eq)) return eq;
        if (Token_Comprassion.TryMatch(word, out var cmp)) return cmp;
        if (Token_AddSub.TryMatch(word, out var term)) return term;
        if (Token_Factor.TryMatch(word, out var fact)) return fact;
        if (Token_Unary.TryMatch(word, out var un)) return un;

        if (tokenTypeBySingleWord.TryGetValue(word, out Type tokenType))
        {
            return (Token)Activator.CreateInstance(tokenType);
        }

        return null;
    }

    private static void TerminateLine(List<Token> tokens)
    {
        if (tokens.Last() is Token_Terminator == false)
        {
            tokens.Add(new Token_Terminator());
        }
    }
}
