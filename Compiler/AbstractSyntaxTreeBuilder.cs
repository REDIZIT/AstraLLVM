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
        }
        

        return statements;
    }

    private static Node Declaration()
    {
        if (Match(typeof(Token_Type))) return VariableDeclaration();

        return Statement();
    }
    private static Node VariableDeclaration()
    {
        Token_Identifier varNameToken = (Token_Identifier)Consume(typeof(Token_Identifier), "Expect variable name.");

        Node initValue = null;
        if (Match(typeof(Token_Assign)))
        {
            initValue = Expression();
        }

        return new Node_VariableDeclaration()
        {
            variableName = varNameToken.name,
            initValue = initValue
        };
    }
    private static Node Statement()
    {
        if (Match(typeof(Token_Print))) return PrintStatement();
        if (Match(typeof(Token_BlockOpen))) return Block();
        if (Match(typeof(Token_If))) return If();
        if (Match(typeof(Token_While))) return While();
        if (Match(typeof(Token_For))) return For();

        return Expression();
    }
    private static Node For()
    {
        Consume(typeof(Token_BracketOpen), "Expected '(' after 'for'");
        Node declaration = Declaration();
        Consume(typeof(Token_Semicolon), "Expected ';' after declaration");
        Node condition = Expression();
        Consume(typeof(Token_Semicolon), "Expected ';' after condition");
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

        Consume(typeof(Token_BlockClose), "Expect '}' after block.");
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

        return Primary();
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

        throw new Exception($"Failed to build Node for token '{Peek()}'");
    }
    private static Node_PrintStatement PrintStatement()
    {
        Node value = Expression();
        return new Node_PrintStatement()
        {
            expression = value
        };
    }



    private static bool Match(params Type[] tokenTypes)
    {
        foreach (Type type in tokenTypes)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }
    private static bool Check(Type tokenType)
    {
        if (IsAtEnd()) return false;
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
    private static Token Consume(Type awaitingTokenType, string errorMessage)
    {
        if (Check(awaitingTokenType)) return Advance();
        throw new Exception(errorMessage);
    }
}