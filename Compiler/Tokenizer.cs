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

        if (Token_Equality.TryMatch(word, out var eq)) return eq;
        if (Token_Comprassion.TryMatch(word, out var cmp)) return cmp;
        if (Token_Term.TryMatch(word, out var term)) return term;
        if (Token_Factor.TryMatch(word, out var fact)) return fact;

        if (Token_BracketOpen.IsMatch(word)) return new Token_BracketOpen();
        if (Token_BracketClose.IsMatch(word)) return new Token_BracketClose();
        if (Token_Print.IsMatch(word)) return new Token_Print();

        if (word == "var") return new Token_Type();

        if (Token_Identifier.IsMatch(word))
        {
            return new Token_Identifier()
            {
                name = word
            };
        }

        if (Token_Assign.IsMatch(word)) return new Token_Assign();


        return null;
    }
}
