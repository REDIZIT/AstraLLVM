public static partial class AbstractSyntaxTreeBuilder
{
    private static List<Token> tokens;
    private static int current;
    private static List<Node> statements;

    public static List<Node> Parse(List<Token> tokens)
    {
        AbstractSyntaxTreeBuilder.tokens = tokens;
        current = 0;

        statements = new();

        while (IsAtEnd() == false)
        {
            statements.Add(Declaration());
            SkipTerminators();
        }
        

        return statements;
    }



    #region Layers

    private static Node Declaration()
    {
        if (Match(typeof(Token_Class))) return ClassDeclaration();

        return FunctionsAndFieldsDeclaration();
    }
    public static Node FunctionsAndFieldsDeclaration()
    {
        if (Check<Token_Identifier>()) return Variable();
        if (Match(typeof(Token_Visibility))) return FunctionDeclaration();

        return Statement();
    }
    private static Node Statement()
    {
        if (Match(typeof(Token_BlockOpen))) return Block();
        if (Match(typeof(Token_If))) return If();
        if (Match(typeof(Token_While))) return While();
        if (Match(typeof(Token_For))) return For();
        if (Match(typeof(Token_Return))) return Return();

        return Expression();
    }
    private static Node Expression()
    {
        return Assignment();
    }
    private static Node Assignment()
    {
        // Target
        Node left = Equality();

        // '='
        if (Match(typeof(Token_Assign)))
        {
            // Value
            Node right = Assignment();

            if (left is Node_VariableUse || left is Node_FieldAccess)
            {
                return new Node_VariableAssign()
                {
                    target = left,
                    value = right
                };
            }
            else
            {
                throw new Exception("Expected variable name or field access to assign, but no such token found after '='");
            }
        }

        return left;
    }
    private static Node Equality()
    {
        Node left = Comprassion();

        while (Match(out Token_Equality operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = Comprassion()
            };
        }

        return left;
    }
    private static Node Comprassion()
    {
        Node left = AddSub();

        while (Match(out Token_Comprassion operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = AddSub()
            };
        }

        return left;
    }
    private static Node AddSub()
    {
        Node left = MulDiv();

        while (Match(out Token_AddSub operatorToken))
        {
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = MulDiv()
            };
        }

        return left;
    }
    private static Node MulDiv()
    {
        Node left = NotNeg();

        while (Match(out Token_Factor operatorToken))
        {
            Node right = NotNeg();
            left = new Node_Binary()
            {
                left = left,
                @operator = operatorToken,
                right = right
            };
        }

        return left;
    }
    private static Node NotNeg()
    {
        if (Match(out Token_Unary operatorToken))
        {
            return new Node_Unary()
            {
                @operator = operatorToken,
                right = NotNeg()
            };
        }
        else if (Peek() is Token_AddSub tokenAddSub && tokenAddSub.asmOperatorName == "sub")
        {
            // Handle unary '-' token
            // @code_example - 3_ret_-1.ac
            try
            {
                return Call();
            }
            catch (UnexpectedTokenException err)
            {
                if (err.unexpectedToken == tokenAddSub)
                {
                    Consume<Token_AddSub>();

                    return new Node_Unary()
                    {
                        @operator = tokenAddSub,
                        right = NotNeg()
                    };
                }
                else
                {
                    throw;
                }
            }
        }
        
        return Call();
    }
    private static Node Call()
    {
        if (Match(typeof(Token_New))) return New();

        Node expr = Primary();

        while (true)
        {
            Token_Identifier token = Previous() as Token_Identifier;
            if (Match(typeof(Token_BracketOpen)))
            {
                expr = FinishCall(expr, token);
            }
            else if (Match(typeof(Token_Dot)))
            {
                expr = Property(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    #endregion


    private static Node ClassDeclaration()
    {
        Token_Identifier ident = Consume<Token_Identifier>();
        Consume<Token_BlockOpen>("Expected '{' after class declaration", skipTerminators: true);

        var body = (Node_Block)Block();

        return new Node_Class()
        {
            name = ident.name,
            body = body
        };
    }
    private static Node FunctionDeclaration()
    {
        //Consume(typeof(Token_Fn), "Expected 'fn' before function declaration.");
        return Function();
    }
    private static Node Function()
    {
        Token_Identifier functionName = Consume<Token_Identifier>("Expected function name");
        Consume<Token_BracketOpen>("Expected '(' after function name");



        List<VariableRawData> parameters = new();
        if (Check(typeof(Token_BracketClose)) == false)
        {
            do
            {
                var paramType = Consume<Token_Identifier>("Expected parameter type");
                var paramName = Consume<Token_Identifier>("Expected parameter name");
                parameters.Add(new VariableRawData()
                {
                    name = paramName.name,
                    rawType = paramType.name,
                });

            } while (Match(typeof(Token_Comma)));
        }

        Consume<Token_BracketClose>("Expected ')' after parameters");



        // Has return type
        List<VariableRawData> returnValues = new();
        if (Match(typeof(Token_Colon)))
        {
            do
            {
                VariableRawData variable = ReturnValueDeclaration();
                returnValues.Add(variable);
            } while (Match(typeof(Token_Comma)));
        }

        Consume<Token_BlockOpen>("Expected '{' before function body", skipTerminators: true);
        Node body = Block();
        return new Node_Function()
        {
            name = functionName.name,
            body = body,
            parameters = parameters,
            returnValues = returnValues,
        };

    }
    private static VariableRawData ReturnValueDeclaration()
    {
        if (Match(typeof(Token_Identifier)))
        {
            var type = Previous<Token_Identifier>();

            VariableRawData data = new()
            {
                rawType = type.name
            };

            if (Check(typeof(Token_Identifier)))
            {
                Token_Identifier name = Consume<Token_Identifier>("Expected argument name");
                data.name = name.name;
            }

            return data;
        }
        else
        {
            throw new Exception("Expected type inside argument declaration");
        }
    }
    private static Node Variable()
    {
        //var firstName = Consume<Token_Identifier>();

        // SomeType myVar ...
        //if (Check<Token_Identifier>() && ) return VariableDeclaration(firstName);
        if (Next() is Token_Identifier || Next() is Token_SquareBracketOpen) return VariableDeclaration();

        // myVar ...
        return Expression();
    }
    private static Node VariableDeclaration()
    {
        var type = Consume<Token_Identifier>();

        bool isArray = false;
        if (Check(typeof(Token_SquareBracketOpen)))
        {
            isArray = true;
            Consume<Token_SquareBracketOpen>();
            Consume<Token_SquareBracketClose>();
        }

        var varNameToken = Consume<Token_Identifier>("Expect variable name.");

        Node initValue = null;
        if (Match(typeof(Token_Assign)))
        {
            initValue = Expression();
        }

        return new Node_VariableDeclaration()
        {
            variable = new VariableRawData()
            {
                rawType = isArray ? "array" : type.name,
                name = varNameToken.name
            },
            initValue = initValue
        };
    }
    
    private static Node New()
    {
        Token_Identifier ident = Consume<Token_Identifier>();

        Consume<Token_BracketOpen>();
        Consume<Token_BracketClose>();

        return new Node_New()
        {
            className = ident.name,
        };
    }
    private static Node Return()
    {
        if (Match(typeof(Token_Terminator)))
        {
            return new Node_Return();
        }
        else
        {
            Node expr = Expression();
            //Consume<Token_Terminator>("Expected terminator after return", skipTerminators: false);
            return new Node_Return()
            {
                expr = expr
            };
        }
    }
    private static Node For()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' after 'for'");
        Node declaration = Declaration();
        Consume(typeof(Token_Terminator), "Expected ';' after declaration");
        Node condition = Expression();
        Consume(typeof(Token_Terminator), "Expected ';' after condition");
        Node action = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after action");

        Node body = Statement();

        return new Node_Block()
        {
            children = new List<Node>()
            {
                declaration,
                new Node_While()
                {
                    condition = condition,
                    body = new Node_Block()
                    {
                        children = new List<Node>()
                        {
                            body,
                            action
                        }
                    }
                }
            }
        };
    }
    private static Node While()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' before condition.");
        Node condition = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after condition.");

        Node body = Statement();

        return new Node_While()
        {
            condition = condition,
            body = body
        };
    }
    private static Node If()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' before condition.");
        Node condition = Expression();
        Consume(typeof(Token_BracketClose), "Expected ')' after condition.");

        Node thenBranch = Statement();
        Node elseBranch = null;

        if (Match(typeof(Token_Else)))
        {
            elseBranch = Statement();
        }

        return new Node_If()
        {
            condition = condition,
            thenBranch = thenBranch,
            elseBranch = elseBranch
        };
    }

    /// <summary>
    /// Before entering Block method you make sure that <see cref="Token_BlockOpen"></see> is already consumed.<br/>
    /// After exiting Block method will be guaranteed that <see cref="Token_BlockClose"></see> is already consumed. (You should not consume it manually)
    /// </summary>
    private static Node Block()
    {
        List<Node> nodes = new();

        while (Check(typeof(Token_BlockClose), skipTerminators: true) == false && IsAtEnd() == false)
        {
            nodes.Add(Declaration());
        }

        Consume(typeof(Token_BlockClose), "Expect '}' after block.", skipTerminators: true);
        return new Node_Block()
        {
            children = nodes
        };
    }


    private static Node Property(Node target)
    {
        Token_Identifier ident = Consume<Token_Identifier>();

        return new Node_FieldAccess()
        {
            target = target,
            targetFieldName = ident.name,
        };
    }
    private static Node FinishCall(Node caller, Token_Identifier ident)
    {
        List<Node> arguments = new();

        if (Check(typeof(Token_BracketClose)) == false)
        {
            do
            {
                arguments.Add(Expression());
            }
            while (Match(typeof(Token_Comma)));
        }

        Consume(typeof(Token_BracketClose), "Expected ')' after arguments for function call");
        return new Node_FunctionCall()
        {
            functionName = ident == null ? "<anon>" : ident.name,
            caller = caller,
            arguments = arguments,
        };
    }
    private static Node Primary()
    {
        if (Match(typeof(Token_Constant)))
        {
            return new Node_Literal()
            {
                constant = (Token_Constant)Previous()
            };
        }

        if (Match(typeof(Token_Identifier)))
        {
            return new Node_VariableUse()
            {
                variableName = ((Token_Identifier)Previous()).name
            };
        }


        if (Match(typeof(Token_BracketOpen)))
        {
            Node expr = Expression();
            Consume(typeof(Token_BracketClose), "Expect ')' after expression.");
            return new Node_Grouping()
            {
                expression = expr
            };
        }


        if (Check(typeof(Token_Terminator)))
        {
            bool anyTerminatorSkipped = SkipTerminators();

            if (IsAtEnd()) return null;
            else if (anyTerminatorSkipped) return Declaration();
        }

        //throw new Exception($"Totally unexpected token '{Peek()}'");
        throw new UnexpectedTokenException(Peek());
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

    private static bool Check<T>(bool skipTerminators = false) where T : Token
    {
        return Check(typeof(T), skipTerminators);
    }
    private static bool Check(Type tokenType, bool skipTerminators = false)
    {
        if (IsAtEnd()) return false;

        if (tokenType != typeof(Token_Terminator))
        {
            if (skipTerminators)
            {
                SkipTerminators();
            }
        }
        
        return Peek().GetType() == tokenType;
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