public static class AbstractSyntaxTreeBuilder
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

    private static Node Declaration()
    {
        if (Match(typeof(Token_Class))) return ClassDeclaration();

        return FunctionsAndFieldsDeclaration();
    }
    public static Node FunctionsAndFieldsDeclaration()
    {
        if (Match(typeof(Token_Type))) return VariableDeclaration();
        if (Match(typeof(Token_Visibility))) return FunctionDeclaration();

        return Statement();
    }
    private static Node ClassDeclaration()
    {
        Token_Identifier ident = Consume<Token_Identifier>();
        Consume<Token_BlockOpen>("Expected '{' after class declaration", skipTerminators: true);

        var body = FunctionsAndFieldsDeclaration();

        Consume<Token_BlockClose>("Expected '}' after class body", skipTerminators: true);

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

        List<Token> parameters = new();
        if (Check(typeof(Token_BracketClose)) == false)
        {
            do
            {
                parameters.Add(Consume<Token_Identifier>("Expect parameter name."));
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
            returnValues = returnValues
        };

    }
    private static VariableRawData ReturnValueDeclaration()
    {
        if (Match(typeof(Token_Type)))
        {
            Token_Type type = (Token_Type)Previous();

            VariableRawData data = new()
            {
                type = type.type
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
    private static Node VariableDeclaration()
    {
        Token_Type type = (Token_Type)Previous();
        Token_Identifier varNameToken = (Token_Identifier)Consume(typeof(Token_Identifier), "Expect variable name.");

        Node initValue = null;
        if (Match(typeof(Token_Assign)))
        {
            initValue = Expression();
        }

        return new Node_VariableDeclaration()
        {
            variable = new VariableRawData()
            {
                type = type.type,
                name = varNameToken.name
            },
            initValue = initValue
        };
    }
    private static Node Statement()
    {
        if (Match(typeof(Token_BlockOpen))) return Block();
        if (Match(typeof(Token_If))) return If();
        if (Match(typeof(Token_While))) return While();
        if (Match(typeof(Token_For))) return For();
        if (Match(typeof(Token_Return))) return Return();
        if (Match(typeof(Token_New))) return New();

        return Expression();
    }
    private static Node New()
    {
        Token_Type ident = Consume<Token_Type>();

        Consume<Token_BlockOpen>();
        Consume<Token_BlockClose>();

        return new Node_New()
        {
            className = ident.type,
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
    private static Node Block()
    {
        List<Node> nodes = new();

        while (Check(typeof(Token_BlockClose)) == false && IsAtEnd() == false)
        {
            nodes.Add(Declaration());
        }

        Consume(typeof(Token_BlockClose), "Expect '}' after block.", skipTerminators: true);
        return new Node_Block()
        {
            children = nodes
        };
    }
    private static Node Expression()
    {
        return Assignment();
    }
    private static Node Assignment()
    {
        Node expr = Equality();

        if (Match(typeof(Token_Assign)))
        {
            Token_Assign token = (Token_Assign)Previous();
            Node value = Assignment();

            if (expr is Node_VariableUse variableUse)
            {
                return new Node_VariableAssign()
                {
                    variableName = variableUse.variableName,
                    value = value
                };
            }
        }

        return expr;
    }
    private static Node Equality()
    {
        Node expr = Comprassion();

        while (Match(typeof(Token_Equality)))
        {
            Token @operator = Previous();
            Node right = Comprassion();
            expr = new Node_Binary()
            {
                left = expr,
                @operator = (Token_Operator)@operator,
                right = right
            };
        }

        return expr;
    }
    private static Node Comprassion()
    {
        Node expr = Term();

        while (Match(typeof(Token_Comprassion)))
        {
            Token @operator = Previous();
            Node right = Term();
            expr = new Node_Binary()
            {
                left = expr,
                @operator = (Token_Operator)@operator,
                right = right
            };
        }

        return expr;
    }
    private static Node Term()
    {
        Node expr = Factor();

        while (Match(typeof(Token_Term)))
        {
            Token @operator = Previous();
            Node right = Factor();
            expr = new Node_Binary()
            {
                left = expr,
                @operator = (Token_Operator)@operator,
                right = right
            };
        }

        return expr;
    }
    private static Node Factor()
    {
        Node expr = Unary();

        while (Match(typeof(Token_Factor)))
        {
            Token @operator = Previous();
            Node right = Unary();
            expr = new Node_Binary()
            {
                left = expr,
                @operator = (Token_Operator)@operator,
                right = right
            };
        }

        return expr;
    }
    private static Node Unary()
    {
        while (Match(typeof(Token_Unary)))
        {
            Token @operator = Previous();
            Node right = Unary();
            return new Node_Unary()
            {
                @operator = (Token_Operator)@operator,
                right = right
            };
        }

        return Call();
    }
    private static Node Call()
    {
        Node expr = Primary();

        while (true)
        {
            Token_Identifier token = Previous() as Token_Identifier;
            if (Match(typeof(Token_BracketOpen)))
            {
                expr = FinishCall(expr, token);
            }
            else
            {
                break;
            }
        }

        return expr;
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
            return new Expr_Grouping()
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

        throw new Exception($"Totally unexpected token '{Peek()}'");
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
    private static Token Previous()
    {
        return tokens[current - 1];
    }
    private static T Consume<T>(string errorMessage = "Not mentioned error", bool skipTerminators = false) where T : Token
    {
        return (T)Consume(typeof(T), errorMessage, skipTerminators);
    }
    private static Token Consume(Type awaitingTokenType, string errorMessage, bool skipTerminators = false)
    {
        if (Check(awaitingTokenType, skipTerminators)) return Advance();
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