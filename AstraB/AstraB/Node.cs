public abstract class Node
{
    public StaticVariable result;

    public abstract IEnumerable<Node> EnumerateChildren();

    public IEnumerable<Node> EnumerateEachChild()
    {
        foreach (Node child in EnumerateChildren())
        {
            yield return child;

            foreach (Node subchild in child.EnumerateEachChild())
            {
                yield return subchild;
            }
        }
    }
}

public class Node_Root : Node_Block
{
}

public class Node_TypeDeclaration : Node
{
    public string name;
    public List<Token_Identifier> genericTypeAliases;
    public Node_Block block;

    public override IEnumerable<Node> EnumerateChildren()
    {
        return block.EnumerateChildren();
    }
}

public class Node_FunctionDeclaration : Node
{
    public string name;
    public Node_Block block;

    public override IEnumerable<Node> EnumerateChildren()
    {
        return block.EnumerateChildren();
    }
}

public class Node_Block : Node
{
    public List<Node> children = new();

    public override IEnumerable<Node> EnumerateChildren()
    {
        return children;
    }
}

public class Node_FieldDeclaration : Node
{
    public string typeName, fieldName;
    public List<Token_Identifier> concreteGenericTypes;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }
}

public class Node_VariableDeclaration : Node
{
    public string typeName;
    public string variableName;
    public List<Token_Identifier> concreteGenericTypes;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }
}

public class Node_Print : Node
{
    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }
}

public class Node_FunctionCall : Node
{
    public Node functionNode;
    public List<Node> passedArguments;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return functionNode;
        foreach (Node argument in passedArguments)
        {
            yield return argument;
        }
    }
}

public class Node_Identifier : Node
{
    public string name;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }
}

public class Node_ConstantNumber : Node
{
    public string typeName;
    public byte[] value;

    public Node_ConstantNumber()
    {
    }

    public Node_ConstantNumber(string str)
    {
        long number = long.Parse(str);

        if (number <= byte.MaxValue)
        {
            typeName = "byte";
            value = [(byte)number];
        }
        else if (number <= short.MaxValue)
        {
            typeName = "short";
            value = BitConverter.GetBytes((short)number);
        }
        else if (number <= int.MaxValue)
        {
            typeName = "int";
            value = BitConverter.GetBytes((int)number);
        }
        else
        {
            typeName = "long";
            value = BitConverter.GetBytes(number);
        }
    }

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }
}

public class Node_VariableAssign : Node
{
    public Node left, value;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return left;
        yield return value;
    }
}

public class Node_Binary : Node
{
    public Node left, right;
    public Token_Operator tokenOperator;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return left;
        yield return right;
    }
}