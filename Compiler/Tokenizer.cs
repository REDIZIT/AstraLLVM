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

                Token token = null;

                if (word == "def")
                {
                    token = new Token_FunctionDefine()
                    {
                        name = words[wi+1].Split('(')[0]
                    };

                    wi = words.Length;
                }
                if (word.Contains('(') && word.Contains(')'))
                {
                    token = new Token_FunctionCall()
                    {
                        name = word.Split('(')[0]
                    };
                }
                if (word == "{" || word == "}")
                {
                    token = new Token_Block()
                    {
                        isClosing = word == "}"
                    };
                }
                if (word == "return")
                {
                    token = new Token_Return();
                }
                if (int.TryParse(word, out _))
                {
                    token = new Token_Constant()
                    {
                        value = word
                    };
                }

                if (int.TryParse(word, out int _))
                {
                    token = new Token_Constant()
                    {
                        value = word
                    };
                }

                if (Token_Equality.TryMatch(word, out var eq)) token = eq;
                if (Token_Comprassion.TryMatch(word, out var cmp)) token = cmp;
                if (Token_Term.TryMatch(word, out var term)) token = term;
                if (Token_Factor.TryMatch(word, out var fact)) token = fact;

                if (Token_BracketOpen.IsMatch(word)) token = new Token_BracketOpen();
                if (Token_BracketClose.IsMatch(word)) token = new Token_BracketClose();
                if (Token_Print.IsMatch(word)) token = new Token_Print();

                //if (Token_Operator.IsOperator(word))
                //{
                //    token = new Token_Operator()
                //    {
                //        @operator = word
                //    };
                //}


                if (token == null) throw new Exception($"Failed to tokenize word '{word}'");

                tokens.Add(token);
            }
        }

        return tokens;
    }
}
