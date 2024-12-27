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
            string[] words = line.Split(' ');

            for (int wi = 0; wi < words.Length; wi++)
            {
                string word = words[wi];

                Token token = null;

                if (word.Contains('(') && word.Contains(')'))
                {
                    token = new Token_FunctionDefine()
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


                if (token == null) throw new Exception($"Failed to tokenize word '{word}'");

                tokens.Add(token);
            }
        }

        return tokens;
    }
}
