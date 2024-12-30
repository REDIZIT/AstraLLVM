public static class AbstractSyntaxTreeBuilder
{
    private static List<Token> tokens;
    private static int current;

    public static List<Statement> Parse(List<Token> tokens)
    {
        AbstractSyntaxTreeBuilder.tokens = tokens;
        current = 0;

        List<Statement> statements = new();

        while (IsAtEnd() == false)
        {
            Statement statement = Statement();
            statements.Add(statement);
        }
        

        return statements;
    }

    private static Statement Statement()
    {
        if (Match(typeof(Token_Print))) return PrintStatement();

        return Expression();
    }
    private static Expr Expression()
    {
        return Equality();
    }
    private static Expr Equality()
    {
        Expr expr = Comprassion();

        while (Match(typeof(Token_Equality)))
        {
            Token @operator = Previous();
            Expr right = Comprassion();
            expr = new Expr_Binary()
            {
                left = expr,
                @operator = @operator,
                right = right
            };
        }

        return expr;
    }
    private static Expr Comprassion()
    {
        Expr expr = Term();

        while (Match(typeof(Token_Comprassion)))
        {
            Token @operator = Previous();
            Expr right = Term();
            expr = new Expr_Binary()
            {
                left = expr,
                @operator = @operator,
                right = right
            };
        }

        return expr;
    }
    private static Expr Term()
    {
        Expr expr = Factor();

        while (Match(typeof(Token_Term)))
        {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new Expr_Binary()
            {
                left = expr,
                @operator = @operator,
                right = right
            };
        }

        return expr;
    }
    private static Expr Factor()
    {
        Expr expr = Unary();

        while (Match(typeof(Token_Factor)))
        {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new Expr_Binary()
            {
                left = expr,
                @operator = @operator,
                right = right
            };
        }

        return expr;
    }
    private static Expr Unary()
    {
        while (Match(typeof(Token_Factor)))
        {
            Token @operator = Previous();
            Expr right = Unary();
            return new Expr_Unray()
            {
                @operator = @operator,
                right = right
            };
        }

        return Primary();
    }
    private static Expr Primary()
    {
        if (Match(typeof(Token_Constant)))
        {
            return new Expr_Literal()
            {
                value = Previous()
            };
        }


        if (Match(typeof(Token_BracketOpen)))
        {
            Expr expr = Expression();
            Consume(typeof(Token_BracketClose), "Expect ')' after expression.");
            return new Expr_Grouping()
            {
                expression = expr
            };
        }

        throw new Exception($"Unknown token '{Peek()}'");
    }
    private static PrintStmt PrintStatement()
    {
        Expr value = Expression();
        return new PrintStmt()
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