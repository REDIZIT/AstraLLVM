using Astra.Compilation;

public class Scope_GenerationPhase
{
    // public Scope_StaticAnalysis staticScope;
    public Node_FunctionDeclaration functionNode;
    
    public Dictionary<string, StaticVariable> variableByName = new();
    public Stack<StaticVariable> variableStack = new();

    public Scope_GenerationPhase parent;

    public UniqueGenerator uniqueGenerator;

    public int CurrentRbpOffset
    {
        get
        {
            if (variableStack.Count == 0) return 0;
            
            StaticVariable lastLocalVariable = variableStack.Peek();
            return lastLocalVariable.rbpOffset + (lastLocalVariable.type.RefSizeInBytes);
        }
    }

    public Scope_GenerationPhase CreateSubScope()
    {
        Scope_GenerationPhase child = new();
        child.parent = this;
        child.uniqueGenerator = uniqueGenerator;
        child.functionNode = functionNode;

        return child;
    }

    public StaticVariable RegisterLocalVariable(ITypeInfo type, string name)
    {
        StaticVariable variable = new StaticVariable()
        {
            name = name,
            rbpOffset = CurrentRbpOffset,
            sizeInBytes = type.RefSizeInBytes,
            type = type,
            scope = this
        };

        variableByName.Add(variable.name, variable);
        variableStack.Push(variable);
        
        return variable;
    }

    public void UnregisterLocalVariable(StaticVariable variable)
    {
        if (variable == null)
            throw new Exception($"Failed to deallocate null variable.");
        
        if (variableByName.ContainsKey(variable.name) == false)
            throw new Exception($"Failed to deallocate '{variable.name}' because it is not even allocated (or already deallocated) on stack.");
        
        if (variableStack.Peek() != variable)
            throw new Exception($"Failed to deallocate variable '{variable.name}' because it is not the last variable on stack, last is '{variableStack.Peek().name}'. Only last variable can be deallocated on stack.'");

        variableStack.Pop();
        variableByName.Remove(variable.name);
    }

    public void UnregisterLocalVariable(string name)
    {
        UnregisterLocalVariable(variableByName[name]);
    }

    public StaticVariable GetVariable(string name)
    {
        if (TryGetVariable(name, out StaticVariable variable)) return variable;
        throw new Exception($"Variable '{name}' not found in current or parents scope");
    }

    public bool TryGetVariable(string name, out StaticVariable variable)
    {
        if (variableByName.TryGetValue(name, out variable)) return true;

        if (parent != null) return parent.TryGetVariable(name, out variable);

        variable = null;
        return false;
    }

    public ScopeRelativeRbpOffset GetRelativeRBP(StaticVariable askedVariable)
    {
        if (askedVariable == null) throw new Exception("Failed to get relative rbp due to null variable");

        if (askedVariable.IsField) askedVariable = askedVariable.sliceParent;
        
        if (variableByName.ContainsValue(askedVariable))
        {
            // Asked variable is local variable of current scope
            // Return positive rbp offset
            return new ScopeRelativeRbpOffset(askedVariable.rbpOffset);
        }
        
        int relativeRBP = 0;
        Scope_GenerationPhase scope = parent;

        // if (askedVariable.IsSlice) relativeRBP += askedVariable.rbpOffset;

        while (scope != null)
        {
            relativeRBP -= Constants.RBP_REG_SIZE;
            
            for (int i = 0; i < scope.variableStack.Count; i++)
            {
                StaticVariable variable = scope.variableStack.ElementAt(i); // where Stack.ElementAt(0) is Stack.Peek()
                relativeRBP -= variable.type.RefSizeInBytes;

                if (variable == askedVariable)
                {
                    return new ScopeRelativeRbpOffset(relativeRBP);
                }
            }
            
            scope = scope.parent;
        }
        
        throw new Exception($"Variable '{askedVariable.name}' not found in current or parents scope");
    }
}