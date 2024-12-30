public static class Tokenizer
{
    public static Dictionary<string, Type> tokenTypeBySingleWord = new()
    {
        { "(", typeof(Token_BracketOpen) },
        { ")", typeof(Token_BracketClose) },
        { "var", typeof(Token_Type) },
        { "=", typeof(Token_Assign) },
        { "{", typeof(Token_BlockOpen) },
        { "}", typeof(Token_BlockClose) },
        { "if", typeof(Token_If) },
        { "else", typeof(Token_Else) },
        { "while", typeof(Token_While) },
        { "for", typeof(Token_For) },
        { ";", typeof(Token_Semicolon) },
        { ",", typeof(Token_Comma) },
        { "fn", typeof(Token_Fn) },
        { "return", typeof(Token_Return) },
    };

    public static List<Token> Tokenize(string rawCode)
    {
        List<Token> tokens = new();

        rawCode = rawCode.Replace("\r", "").Replace("\t", "");
        string[] lines = rawCode.Split('\n');

        for (int li = 0; li < lines.Length; li++)
        {
            string line = lines[li];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] words = line.Split(' ');

            for (int wi = 0; wi < words.Length; wi++)
            {
                string word = words[wi];
                Token token = Tokenize(ref wi, words);

                if (token == null) throw new Exception($"Failed to tokenize word '{word}'");

                tokens.Add(token);
            }


            if (tokens.Last() is Token_Terminator == false)
            {
                tokens.Add(new Token_Terminator());
            }
        }

        return tokens;
    }

    private static Token Tokenize(ref int wi, string[] words)
    {
        string word = words[wi];
;
        if (int.TryParse(word, out int _))
        {
            return new Token_Constant()
            {
                value = word
            };
        }

        // Unary '-' negate must be higher Binary '-' sub operator
        if (Token_Unary.TryMatch(word, out var un)) return un;

        if (Token_Equality.TryMatch(word, out var eq)) return eq;
        if (Token_Comprassion.TryMatch(word, out var cmp)) return cmp;
        if (Token_Term.TryMatch(word, out var term)) return term;
        if (Token_Factor.TryMatch(word, out var fact)) return fact;


        if (tokenTypeBySingleWord.TryGetValue(word, out Type tokenType))
        {
            return (Token)Activator.CreateInstance(tokenType);
        }

        // Should be the last one
        if (Token_Identifier.IsMatch(word))
        {
            return new Token_Identifier()
            {
                name = word
            };
        }

        return null;
    }
}
