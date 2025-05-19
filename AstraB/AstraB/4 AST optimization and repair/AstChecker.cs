public static class AstChecker
{
    public static void CheckAndModify(Node_Root root)
    {
        foreach (Node node in root.EnumerateEachChild())
        {
            if (node is Node_FunctionDeclaration declaration)
            {
                Fix_FunctionReturn(declaration);
            }
        }
    }

    private static void Fix_FunctionReturn(Node_FunctionDeclaration node)
    {
        if (node.block.children.Last() is Node_Return == false)
        {
            if (node.functionInfo.returns.Count > 0)
            {
                throw new Exception($"Function '{node.functionInfo.name}' does not return any value at the end");
            }
            else
            {
                // Return nothing
                node.block.children.Add(new Node_Return());   
            }
        }
    }
}