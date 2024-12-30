public static class Tokenizer
{
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
        

        if (Token_BracketOpen.IsMatch(word)) return new Token_BracketOpen();
        if (Token_BracketClose.IsMatch(word)) return new Token_BracketClose();
        if (Token_Print.IsMatch(word)) return new Token_Print();

        if (word == "var") return new Token_Type();

        if (Token_Assign.IsMatch(word)) return new Token_Assign();

        if (Token_BlockOpen.IsMatch(word)) return new Token_BlockOpen();
        if (Token_BlockClose.IsMatch(word)) return new Token_BlockClose();

        if (Token_If.IsMatch(word)) return new Token_If();
        if (Token_Else.IsMatch(word)) return new Token_Else();

        if (Token_While.IsMatch(word)) return new Token_While();
        if (Token_For.IsMatch(word)) return new Token_For();

        if (Token_Semicolon.IsMatch(word)) return new Token_Semicolon();
        if (Token_Comma.IsMatch(word)) return new Token_Comma();
        if (Token_Fn.IsMatch(word)) return new Token_Fn();
        if (Token_Return.IsMatch(word)) return new Token_Return();


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
