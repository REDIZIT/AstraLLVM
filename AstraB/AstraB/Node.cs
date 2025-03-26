public abstract class Node
{
    
}

public class Node_Root : Node_Block
{
    
}

public class Node_TypeDeclaration : Node
{
    public string name;
    public Node_Block block;
}

public class Node_FunctionDeclaration : Node
{
    public string name;
    public Node_Block block;
}

public class Node_Block : Node
{
    public List<Node> children = new();
}

public class Node_FieldDeclaration : Node
{
    public string typeName, fieldName;
}

public class Node_VariableDeclaration : Node
{
    
}

public class Node_Print : Node
{
    
}