public static class Parser
{
    private static int current;
    private static List<Token> tokens;
    private static Node_Root root;
    
    
    public static Node_Root Parse(List<Token> tokens)
    {
        current = 0;
        root = new Node_Root();
        Parser.tokens = tokens;

        while (IsAtEnd() == false)
        {
            root.children.Add(Declaration());
            SkipTerminators();
        }


        return root;
    }

    private static Node Declaration()
    {
        if (Check<Token_Identifier>())
        {
            if (Check<Token_BlockOpen>(offset: 1))
            {
                return TypeDeclaration();
            }

            if (Check<Token_BracketOpen>(offset: 1))
            {
                return FunctionDeclaration();
            }
        }

        throw new Exception($"Failed to parse tokens due to unexpected token '{Peek()}' at {current}");
    }

    private static Node_FunctionDeclaration FunctionDeclaration()
    {
        Token_Identifier nameToken = Consume<Token_Identifier>();

        Consume<Token_BracketOpen>();
        Consume<Token_BracketClose>();
        
        return new Node_FunctionDeclaration()
        {
            name = nameToken.name,
            block = Block(InFunctionDeclaration)
        };
    }

    private static Node_TypeDeclaration TypeDeclaration()
    {
        return new Node_TypeDeclaration()
        {
            name = Consume<Token_Identifier>().name,
            block = Block(InTypeDeclaration)
        };
    }

    private static Node InTypeDeclaration()
    {
        if (Check<Token_Identifier>() && Check<Token_Identifier>(offset: 1))
        {
            return FieldDeclaration();
        }
        
        throw new Exception($"Failed to parse {nameof(InTypeDeclaration)} due to unexpected tokens sequence '{Peek()}' at {current}");
    }

    private static Node InFunctionDeclaration()
    {
        if (Check<Token_Identifier>() && (Peek() as Token_Identifier).name == "print")
        {
            Consume<Token_Identifier>();
            return new Node_Print();
        }
        
        throw new Exception($"Failed to parse {nameof(InFunctionDeclaration)} due to unexpected tokens sequence '{Peek()}' at {current}");
    }

    private static Node_FieldDeclaration FieldDeclaration()
    {
        return new Node_FieldDeclaration()
        {
            typeName = Consume<Token_Identifier>().name,
            fieldName = Consume<Token_Identifier>().name
        };
    }

    private static Node_Block Block(Func<Node> layer)
    {
        SkipTerminators();
        Consume<Token_BlockOpen>();
        
        Node_Block block = new();
        
        SkipTerminators();
        while (IsAtEnd() == false && Check<Token_BlockClose>() == false)
        {
            block.children.Add(layer());
            SkipTerminators();
        }

        Consume<Token_BlockClose>();
        return block;
    }

    
    
    
    
    private static bool Match<T>(out T token) where T : Token
    {
        if (Check(typeof(T), true))
        {
            token = (T)Advance();
            return true;
        }
        token = null;
        return false;
    }
    private static bool Match(Type tokenType)
    {
        if (Check(tokenType, true))
        {
            Advance();
            return true;
        }
        return false;
    }

    private static bool Check<T>(bool skipTerminators = false, int offset = 0) where T : Token
    {
        return Check(typeof(T), skipTerminators, offset);
    }
    private static bool Check(Type tokenType, bool skipTerminators = false, int offset = 0)
    {
        int index = current + offset;
        if (index >= tokens.Count) return false;

        if (tokenType != typeof(Token_Terminator))
        {
            if (skipTerminators)
            {
                SkipTerminators();
            }
        }
        
        return tokens[index].GetType() == tokenType;
    }
    private static Token Advance()
    {
        if (IsAtEnd() == false) current++;
        return Previous();
    }
    private static bool IsAtEnd()
    {
        return current >= tokens.Count;
    }
    private static Token Peek()
    {
        return tokens[current];
    }
    private static T Previous<T>() where T : Token
    {
        return (T)Previous();
    }
    private static Token Previous()
    {
        return tokens[current - 1];
    }
    private static Token Next()
    {
        return tokens[current + 1];
    }
    private static T Consume<T>(string errorMessage = "Not mentioned error", bool skipTerminators = false) where T : Token
    {
        return (T)Consume(typeof(T), errorMessage, skipTerminators);
    }
    private static Token Consume(Type awaitingTokenType, string errorMessage, bool skipTerminators = false)
    {
        if (Check(awaitingTokenType, skipTerminators)) return Advance();

        Token gotToken = Peek();
        throw new Exception(errorMessage);
    }

    private static bool SkipTerminators()
    {
        if (IsAtEnd()) return false;

        while (Peek().GetType() == typeof(Token_Terminator))
        {
            Advance();
            if (IsAtEnd()) return true;
        }

        return false;
    }
}