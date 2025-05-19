public static class Parser
{
    private static int current;
    private static List<Token> tokens;
    private static Node_Root root;
    
    
    public static Node_Root Parse(List<Token> tokens)
    {
        current = 0;
        root = new Node_Root();

        tokens.RemoveAll(t => t is Token_Comment);
        Parser.tokens = tokens;

        SkipTerminators();

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
            if (Check<Token_BlockOpen>(offset: 1) || Check<Token_Less>(offset: 1))
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

        List<RawFieldInfo> parameters = new();
        while (Check<Token_BracketClose>() == false)
        {
            Token_Identifier typeIdent = Consume<Token_Identifier>();
            Token_Identifier nameIdent = Consume<Token_Identifier>();
            
            parameters.Add(new(typeIdent.name, nameIdent.name));

            if (Check<Token_BracketClose>())
            {
                break;
            }
            else
            {
                Consume<Token_Comma>();
            }
        } 
        Consume<Token_BracketClose>();

        SkipTerminators();

        List<string> returns = new();
        if (Check<Token_Minus>() && Check<Token_Greater>(offset: 1))
        {
            Consume<Token_Minus>();
            Consume<Token_Greater>();

            SkipTerminators();

            Token_Identifier typeIdent = Consume<Token_Identifier>();
            returns.Add(typeIdent.name);
        }
        
        return new Node_FunctionDeclaration()
        {
            name = nameToken.name,
            returns = returns,
            parameters = parameters,
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
        if ((Check<Token_Identifier>() && Check<Token_Identifier>(offset: 1)) || (Check<Token_Identifier>() && Check<Token_Less>(offset: 1)))
        {
            return FieldDeclaration();
        }
        
        throw new Exception($"Failed to parse {nameof(InTypeDeclaration)} due to unexpected tokens sequence '{Peek()}' at {current}");
    }

    private static Node InFunctionDeclaration()
    {
        return If();
    }

    private static Node If()
    {
        if (Check<Token_If>())
        {
            Consume<Token_If>();

            Node condition = Cast();

            Node trueBranch;
            if (Check<Token_BlockOpen>()) trueBranch = Block(InFunctionDeclaration);
            else trueBranch = Cast();
            
            Node elseBranch = null!;

            if (Check<Token_Else>())
            {
                if (Check<Token_BlockOpen>(offset: 1)) elseBranch = Block(InFunctionDeclaration);
                else elseBranch = Cast();
            }

            return new Node_If()
            {
                condition = condition,
                trueBranch = trueBranch,
                elseBranch = elseBranch
            };
        }
        
        return VariableAssignment();
    }
    
    private static Node VariableAssignment()
    {
        Node left = VariableDeclaration();
        
        if (Check<Token_Assign>())
        {
            Consume<Token_Assign>();
            Node right = VariableDeclaration();

            return new Node_VariableAssign()
            {
                left = left,
                value = right
            };
        }
        else if (Check<Token_AssignByPointer>())
        {
            Consume<Token_AssignByPointer>();
            Node right = VariableDeclaration();
            
            return new Node_VariableAssign()
            {
                left = left,
                value = right,
                isByPointer = true
            };
        }

        return left;
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

        return Return();
    }

    private static Node Return()
    {
        if (Check<Token_Return>())
        {
            Consume<Token_Return>();

            if (Check<Token_Terminator>())
            {
                return new Node_Return();
            }
            else
            {
                return new Node_Return()
                {
                    value = Cast()
                };
            }
        }

        return Cast();
    }

    private static Node Cast()
    {
        Node left = Math();
        
        if (Check<Token_To>() || Check<Token_As>())
        {
            while (Check<Token_To>() || Check<Token_As>())
            {
                bool isToCast = Check<Token_To>();
                if (isToCast) Consume<Token_To>();
                else Consume<Token_As>();

                if (TryGenericTypeName(out string genericTypeName))
                {
                    left = new Node_Cast()
                    {
                        valueToCast = left,
                        typeName = genericTypeName,
                        isToCast = isToCast
                    };
                }
                else
                {
                    Token_Identifier typeIdent = Consume<Token_Identifier>();
                    left = new Node_Cast()
                    {
                        valueToCast = left,
                        typeName = typeIdent.name,
                        isToCast = isToCast
                    };
                }
            }

            return left;
        }

        return left;
    }

    private static bool TryGenericTypeName(out string name)
    {
        if (Check<Token_Identifier>() && IsGenericBand(offset: 1))
        {
            Token_Identifier baseTypeName = Consume<Token_Identifier>();
            List<Token_Identifier> concreteGenericTypeNames = TryParseGenericBand();

            name = baseTypeName.name + "<" + string.Join(", ", concreteGenericTypeNames.Select(t => t.name)) + ">";
            return true;
        }

        name = null;
        return false;
    }

    private static Node Math()
    {
        return Equality();
    }

    private static Node BinaryOperator(Func<Node> leftLayer, Type[] tokenTypes)
    {
        Node left = leftLayer();

        while (Match(tokenTypes, out Token t))
        {
            left = new Node_Binary()
            {
                left = left,
                op = t,
                right = leftLayer()
            };
        }

        return left;
    }

    private static Node Equality()
    {
        return BinaryOperator(Comprassion, new [] { typeof(Token_Equality) });
    }
    private static Node Comprassion()
    {
        return BinaryOperator(AddSub, new[] { typeof(Token_Less), typeof(Token_LessOrEqual), typeof(Token_Greater), typeof(Token_GreaterOrEqual) });
    }
    private static Node AddSub()
    {
        return BinaryOperator(MulDiv, new[] { typeof(Token_Plus), typeof(Token_Minus) });
    }
    private static Node MulDiv()
    {
        return BinaryOperator(FunctionCall, new[] { typeof(Token_Star), typeof(Token_Slash) });
    }
    
    private static Node FunctionCall()
    {
        Node left = Access();

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
    
    private static Node Access()
    {
        Node left = ConstantNumber();
        
        if (Check<Token_Dot>())
        {
            Consume<Token_Dot>();
            Token_Identifier nameIdent = Consume<Token_Identifier>();
            
            return new Node_Access()
            {
                target = left,
                name = nameIdent.name
            };
        }
    
        return left;
    }

    private static Node ConstantNumber()
    {
        if (Check<Token_Constant>())
        {
            string value = Consume<Token_Constant>().word;
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

        return Grouping();
    }

    private static Node Grouping()
    {
        if (Match<Token_BracketOpen>())
        {
            Node expr = Math();
            Consume(typeof(Token_BracketClose), "Expect ')' after expression.");
            return new Node_Grouping()
            {
                body = expr
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
        return Check<Token_Less>(offset: offset);
    }
    private static List<Token_Identifier> TryParseGenericBand()
    {
        List<Token_Identifier> idents = new();
        
        if (IsGenericBand())
        {
            Consume<Token_Less>();
            
            Token_Identifier ident = Consume<Token_Identifier>();
            idents.Add(ident);
            
            Consume<Token_Greater>();
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

    
    
    
    
    private static bool Match<T>() where T : Token
    {
        return Match<T>(out _);
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
    private static bool Match(Type tokenType, out Token token)
    {
        if (Check(tokenType, true))
        {
            token = Advance();
            return true;
        }

        token = null;
        return false;
    }
    private static bool Match(Type[] tokenTypes, out Token token)
    {
        foreach (Type tokenType in tokenTypes)
        {
            if (Match(tokenType, out token))
            {
                return true;
            }
        }

        token = null;
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
        throw new Exception($"Awaited {awaitingTokenType}, but got {gotToken}: errorMessage");
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