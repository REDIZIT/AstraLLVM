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
            if (Check<Token_BlockOpen>(offset: 1) || Check<Token_Comprassion>(offset: 1))
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
        Token_Identifier ident = Consume<Token_Identifier>();

        List<Token_Identifier> genericTypeAliases = TryParseGenericBand();
        
        Node_Block typeBody = Block(InTypeDeclaration);
        
        return new Node_TypeDeclaration()
        {
            name = ident.name,
            genericTypeAliases = genericTypeAliases,
            block = typeBody
        };
    }

    private static Node InTypeDeclaration()
    {
        if ((Check<Token_Identifier>() && Check<Token_Identifier>(offset: 1)) || (Check<Token_Identifier>() && Check<Token_Comprassion>(offset: 1)))
        {
            return FieldDeclaration();
        }
        
        throw new Exception($"Failed to parse {nameof(InTypeDeclaration)} due to unexpected tokens sequence '{Peek()}' at {current}");
    }

    private static Node InFunctionDeclaration()
    {
        return VariableDeclaration();
    }
    
    private static Node VariableDeclaration()
    {
        if ((Check<Token_Identifier>() && Check<Token_Identifier>(offset: 1)) || (Check<Token_Identifier>() && IsGenericBand(offset: 1)))
        {
            Token_Identifier typeName = Consume<Token_Identifier>();
            List<Token_Identifier> concreteGenericTypes = TryParseGenericBand();
            SkipTerminators();
            Token_Identifier variableName = Consume<Token_Identifier>();
            
            return new Node_VariableDeclaration()
            {
                typeName = typeName.name,
                concreteGenericTypes = concreteGenericTypes,
                variableName = variableName.name
            };
        }

        return VariableAssignment();
    }

    private static Node VariableAssignment()
    {
        Node left = FunctionCall();
        
        if (Check<Token_Assign>())
        {
            Consume<Token_Assign>();
            Node right = FunctionCall();

            return new Node_VariableAssign()
            {
                left = left,
                value = right
            };
        }

        return left;
    }

    private static Node FunctionCall()
    {
        Node left = Cast();

        if (Check<Token_BracketOpen>())
        {
            Consume<Token_BracketOpen>();

            List<Node> passedArguments = new();
            while (Check<Token_Identifier>() || Check<Token_Constant>())
            {
                Node passedArgument = Cast();
                passedArguments.Add(passedArgument);

                if (Check<Token_Comma>()) Consume<Token_Comma>();

                SkipTerminators();
            }
            
            
            Consume<Token_BracketClose>();

            return new Node_FunctionCall()
            {
                functionNode = left,
                passedArguments = passedArguments
            };
        }

        return left;
    }

    private static Node Cast()
    {
        Node left = Math();

        if (Check<Token_CastTo>())
        {
            Consume<Token_CastTo>();

            return new Node_CastTo()
            {
                valueToCast = left,
                typeName = Consume<Token_Identifier>().name
            };
        }

        return left;
    }

    private static Node Math()
    {
        Node left = ConstantNumber();

        if (Check<Token_AddSub>())
        {
            Token_Operator tokenOperator = Consume<Token_AddSub>();

            return new Node_Binary()
            {
                left = left,
                right = ConstantNumber(),
                tokenOperator = tokenOperator
            };
        }

        return left;
    }

    private static Node ConstantNumber()
    {
        if (Check<Token_Constant>())
        {
            string value = Consume<Token_Constant>().value;
            return new Node_ConstantNumber(value);
        }
        
        return Identifier();
    }

    private static Node Identifier()
    {
        if (Check<Token_Identifier>())
        {
            return new Node_Identifier()
            {
                name = Consume<Token_Identifier>().name
            };
        }

        return Unexpected();
    }

    private static Node Unexpected()
    {
        throw new Exception($"Failed to parse tokens due to unexpected token '{Peek()}' at {current}");
    }

    private static Node_FieldDeclaration FieldDeclaration()
    {
        Token_Identifier typeName = Consume<Token_Identifier>();

        List<Token_Identifier> concreteGenericTypes = TryParseGenericBand();
        
        Token_Identifier fieldName = Consume<Token_Identifier>();
        
        
        return new Node_FieldDeclaration()
        {
            typeName = typeName.name,
            concreteGenericTypes = concreteGenericTypes,
            fieldName = fieldName.name
        };
    }

    private static bool IsGenericBand(int offset = 0)
    {
        return Check<Token_Comprassion>(offset: offset) && Peek<Token_Comprassion>(offset: offset).asmOperatorName == "<";
    }
    private static List<Token_Identifier> TryParseGenericBand()
    {
        List<Token_Identifier> idents = new();
        
        if (IsGenericBand())
        {
            Consume<Token_Comprassion>();
            
            Token_Identifier ident = Consume<Token_Identifier>();
            idents.Add(ident);
            
            Consume<Token_Comprassion>();
        }

        return idents;
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
    private static Token Peek(int offset = 0)
    {
        return tokens[current + offset];
    }
    private static T Peek<T>(int offset = 0) where T : Token
    {
        return (T)Peek(offset);
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