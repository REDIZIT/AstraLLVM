namespace Astra.Compilation;

public class Scope_StaticAnalysis
{
    public TypeInfo typeInfo;
    public FunctionInfo functionInfo;
    public List<FieldInfo> namedVariables = new();

    public Scope_StaticAnalysis parent;
    public List<Scope_StaticAnalysis> children = new();

    public Scope_StaticAnalysis CreateSubScope()
    {
        Scope_StaticAnalysis child = new();
        child.parent = this;
        children.Add(child);

        return child;
    }

    public Scope_StaticAnalysis Find(Func<Scope_StaticAnalysis, bool> predicate)
    {
        if (predicate(this)) return this;
        if (parent == null) throw new Exception("Failed to find scope for predicate");
        return parent.Find(predicate);
    }

    
    public bool TryFindVariable(string variableName, out FieldInfo variable)
    {
        variable = TryFindVariable(variableName);
        return variable != null;
    }
    public FieldInfo FindVariable(string variableName)
    {
        FieldInfo variable = TryFindVariable(variableName);
        if (variable != null) return variable;
        else throw new Exception($"Failed to find variable '{variableName}' in scopes");
    }
    public FieldInfo TryFindVariable(string variableName)
    {
        FieldInfo variable = namedVariables.FirstOrDefault(f => f.name == variableName);
        if (variable != null) return variable;
        if (parent == null) return null;
        return parent.TryFindVariable(variableName);
    }
}