public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;
    private static List<Instruction> instructions;
    
    private static Scope_GenerationPhase currentScope;
    private static int tempNameIndex;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();
        instructions = new();
        Generator.module = module;
        currentScope = new();
        
        //
        // Generate entry point
        //
        Add(new FunctionCall_Instruction(0, false));
        Add(new Quit_Instruction());

        //
        // Generate instructions
        //
        foreach (FunctionInfo function in module.functions)
        {
            int pointer = -1;
            int id = compiled.functionPointerByID.Count;
            compiled.functionPointerByID.Add(id, pointer);
            
            Add(new FunctionDeclaration_Instruction(function));
            
            Generate(function.node);
        }
        
        //
        // Encode instruction into byte-code
        //
        InstructionEncoder encoder = new();
        foreach (Instruction instruction in instructions)
        {
            if (instruction is FunctionDeclaration_Instruction decl)
            {
                compiled.functionPointerByID[decl.functionInfo.inModuleIndex] = encoder.code.Count;
            }

            int a = encoder.code.Count;
            instruction.Encode(encoder);
            int b = encoder.code.Count;

            instruction.bytecodeRange = new()
            {
                begin = a,
                end = b,
                includeBegin = true,
                includeEnd = false,
            };
        }
        
        //
        // Resolve byte-code indexes
        //
        foreach (Instruction instruction in instructions)
        {
            if (instruction is Jump_Instruction jump)
            {
                Instruction targetInstruction = instructions[jump.index];
                
                AbsByteCodeIndex jumpPlaceholderIndex = new(jump.bytecodeRange.begin);
                AbsByteCodeIndex jumpTargetIndex = new(targetInstruction.bytecodeRange.begin);
                
                jump.Recode(encoder, jumpPlaceholderIndex, jumpTargetIndex);
            }
            else if (instruction is If_Instruction ifInstruction)
            {
                Instruction elseBranchInstruction = instructions[ifInstruction.elseJump];

                AbsByteCodeIndex instructionBeginIndex = new(ifInstruction.bytecodeRange.begin);
                AbsByteCodeIndex elseBranchBeginIndex = new(elseBranchInstruction.bytecodeRange.begin);

                ifInstruction.Recode(encoder, instructionBeginIndex, elseBranchBeginIndex);
            }
        }

        compiled.code = encoder.code;

        return compiled;
    }

    private static void Generate(Node node)
    {
        switch (node)
        {
            case Node_FunctionDeclaration functionDeclaration: FunctionDeclaration(functionDeclaration); break;
            case Node_FunctionCall call: FunctionCall(call); break;
            case Node_VariableDeclaration variableDeclaration: VariableDeclaration(variableDeclaration); break;
            case Node_VariableAssign variableAssignment: VariableAssignment(variableAssignment); break;
            case Node_Binary binary: Binary(binary); break;
            case Node_ConstantNumber constantNumber: Constant(constantNumber); break;
            case Node_Identifier ident: LoadVariable(ident); break;
            case Node_CastTo cast: Cast(cast); break;
            case Node_Grouping grouping: Grouping(grouping); break;
            case Node_Return ret: Return(ret); break;
            case Node_If nodeIf: If(nodeIf); break;
            case Node_Block block: Block(block); break;
            case Node_Access access: Access(access); break;
            default: throw new Exception($"Failed to generate due to unexpected node '{node}'");
        }
    }

    private static void Access(Node_Access access)
    {
        Generate(access.target);

        if (access.target is Node_Identifier ident)
        {
            if (module.TryGetFunction(ident.name, out FunctionInfo info))
            {
                access.functionInfo = info;
            }
            else if (currentScope.TryGetVariable(ident.name, out StaticVariable variable))
            {
                access.variable = variable;
            }
            else
            {
                throw new Exception($"Failed to generate access for '{ident.name}' due not any match found");
            }
        }
        else
        {
            throw new Exception($"Failed to generate access due to invalid target '{access.target}'");
        }
    }

    private static void If(Node_If node)
    {
        // Condition
        Generate(node.condition);

        ScopeRelativeRbpOffset condition = currentScope.GetRelativeRBP(node.condition.result);
        If_Instruction ifInstruction = new If_Instruction(condition, AbsInstructionIndex.Invalid);
        Add(ifInstruction);

        
        // True branch
        BeginSubScope();
        Generate(node.trueBranch);
        DropSubScope();
        
        Jump_Instruction trueBranchEndJump = new(new(AbsInstructionIndex.Invalid));
        Add(trueBranchEndJump);
        
        

        
        // Else branch
        if (node.elseBranch != null)
        {
            ifInstruction.elseJump = GetCurrentInstructionIndex();
            Generate(node.elseBranch);
        }
        else
        {
            AbsInstructionIndex trueBranchEnd = GetCurrentInstructionIndex();
            ifInstruction.elseJump = trueBranchEnd;
        }
        
        // End
        AbsInstructionIndex end = GetCurrentInstructionIndex();
        trueBranchEndJump.index = end;
    }

    private static void Return(Node_Return ret)
    {
        FunctionInfo function = currentScope.functionNode.functionInfo;

        if (function.returns.Count > 0 && ret.value == null)
            throw new BadAstraCode($"Function '{function.name}' returns {function.returns.Count} values, but there is a return keyword, that returns nothing");
        
        if (function.returns.Count == 0 && ret.value != null)
            throw new BadAstraCode($"Function '{function.name}' returns nothing, but there is a return keyword, that returns a value");
        
        if (function.returns.Count > 1)
            throw new NotSupportedAstraFeature($"Function '{function.name}' returns more than 1 value, but this feature is not supported yet");

        if (function.returns.Count == 0)
        {
            // Return nothing
            Add(new Return_Instruction());
        }
        else
        {
            // Return 1 value
            
            Generate(ret.value);
            StaticVariable outVariable = currentScope.functionNode.result;
            SetValue_Var_Var(outVariable, ret.value.result);
            
            Add(new Return_Instruction());
        }
    }

    private static void LoadVariable(Node_Identifier ident)
    {
        ident.result = currentScope.GetVariable(ident.name);
    }

    private static void Grouping(Node_Grouping grouping)
    {
        Generate(grouping.body);
        grouping.result = grouping.body.result;
    }
    
    private static void Constant(Node_ConstantNumber constant)
    {
        constant.result = AllocateVariable(constant.typeName, NextTempName());
        SetValue_Var_Const(constant.result, constant.value);
    }

    private static void Binary(Node_Binary node)
    {
        Generate(node.left);
        Generate(node.right);
        
        TypeInfo resultType = module.GetType("int");
        node.result = AllocateVariable(resultType.name, NextTempName());

        MathOperator op = GetMathOperator(node.op);

        ScopeRelativeRbpOffset leftOffset = currentScope.GetRelativeRBP(node.left.result);
        ScopeRelativeRbpOffset rightOffset = currentScope.GetRelativeRBP(node.right.result);
        ScopeRelativeRbpOffset resultOffset = currentScope.GetRelativeRBP(node.result);
        
        Add(new Math_Instruction(leftOffset, rightOffset, resultOffset, op));
    }

    private static MathOperator GetMathOperator(Token token)
    {
        switch (token)
        {
            case Token_Plus: return MathOperator.Add;
            case Token_Minus: return MathOperator.Sub;
            case Token_Star: return MathOperator.Mul;
            case Token_Slash: return MathOperator.Div;
            case Token_Less: return MathOperator.Less;
            case Token_LessOrEqual: return MathOperator.LessOrEqual;
            case Token_Greater: return MathOperator.Greater;
            case Token_GreaterOrEqual: return MathOperator.GreaterOrEqual;
            default: throw new Exception($"Token '{token}' is not a math operator");
        }
    }

    private static void Cast(Node_CastTo node)
    {
        Generate(node.valueToCast);

        ITypeInfo sourceType = node.valueToCast.result.type;
        ITypeInfo targetType = module.GetBaseOrGenericType(node.typeName);

        if (targetType.Name == "ptr")
        {
            node.result = AllocateVariable(targetType, NextTempName());
            GetPointer_To_Variable(node.result, node.valueToCast.result);
        }
        else if (sourceType.Name == "ptr")
        {
            node.result = AllocateVariable(targetType, NextTempName());
            GetValue_ByPointer(node.result, node.valueToCast.result);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void BeginSubScope()
    {
        // Prologue before sub scope creation
        Add(new Scope_Instruction(true));
        currentScope = currentScope.CreateSubScope();
    }

    private static void DropSubScope()
    {
        // Epilogue after sub scope drop
        Add(new Scope_Instruction(false));
        currentScope = currentScope.parent;
    }

    private static void Block(Node_Block node)
    {
        foreach (Node children in node.children)
        {
            Generate(children);
        }
    }

    private static void VariableDeclaration(Node_VariableDeclaration node)
    {
        node.result = AllocateVariable(node.typeName, node.concreteGenericTypes, node.variableName);
    }

    private static StaticVariable AllocateVariable(string typeName, string variableName)
    {
        return AllocateVariable(typeName, null, variableName);
    }
    private static StaticVariable AllocateVariable(string typeName, List<Token_Identifier> concreteGenericTypes, string variableName)
    {
        TypeInfo type = module.GetType(typeName);

        if (type.IsGeneric)
        {
            GenericImplementationInfo genericType = module.GetGeneric(type, concreteGenericTypes.Select(t => module.GetType(t.name)));
            return AllocateVariable(genericType, variableName);
        }
        else
        {
            return AllocateVariable(type, variableName);
        }
    }
    private static StaticVariable AllocateVariable(ITypeInfo type, string variableName)
    {
        StaticVariable variable = currentScope.RegisterLocalVariable(type, variableName);
        Add(new AllocateVariable_Instruction(type.RefSizeInBytes).WithDebug(variable));

        return variable;
    }

    private static void VariableAssignment(Node_VariableAssign node)
    {
        Generate(node.left);
        StaticVariable variable = node.left.result;
        
        Generate(node.value);

        if (node.isByPointer)
        {
            SetValue_Var_To_Ptr(variable, node.value.result);
        }
        else
        {
            SetValue_Var_Var(variable, node.value.result);   
        }
    }

    private static void SetValue_Var_Var(StaticVariable dest, StaticVariable value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        ScopeRelativeRbpOffset valueOffset = currentScope.GetRelativeRBP(value);
        Add(SetValue_Instruction.Variable_to_Variable(destOffset, valueOffset, dest.sizeInBytes));
    }

    private static void SetValue_Var_Const(StaticVariable dest, byte[] value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        Add(SetValue_Instruction.Const_to_Variable(destOffset, value));
    }
    
    private static void GetPointer_To_Variable(StaticVariable dest, StaticVariable value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        ScopeRelativeRbpOffset valueOffset = currentScope.GetRelativeRBP(value);
        Add(SetValue_Instruction.GetPointer_To_Variable(destOffset, valueOffset, dest.sizeInBytes));
    }
    
    private static void SetValue_Var_To_Ptr(StaticVariable dest, StaticVariable value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        ScopeRelativeRbpOffset valueOffset = currentScope.GetRelativeRBP(value);
        Add(SetValue_Instruction.SetValue_ByPointer(destOffset, valueOffset, dest.sizeInBytes));
    }
    
    private static void GetValue_ByPointer(StaticVariable result, StaticVariable pointer)
    {
        ScopeRelativeRbpOffset resultOffset = currentScope.GetRelativeRBP(result);
        ScopeRelativeRbpOffset pointerOffset = currentScope.GetRelativeRBP(pointer);
        Add(SetValue_Instruction.GetValue_ByPointer(resultOffset, pointerOffset, result.sizeInBytes));
    }
    
    private static void FunctionDeclaration(Node_FunctionDeclaration node)
    {
        // Register arguments and returns before creating function body subscope
        // Registered arguments will have negative RBP offset due to body subscope and current scope are different

        // We promise/provide named arguments inside function body

        Stack<StaticVariable> pushedVariables = new();
        
        // Promise returns
        foreach (FieldInfo ret in node.functionInfo.returns)
        {
            StaticVariable pushedVariable = currentScope.RegisterLocalVariable(ret.type, ret.name);
            pushedVariables.Push(pushedVariable);
            node.result = pushedVariable;
        }

        
        // if (node.functionInfo.isStatic == false)
        // {
        //     StaticVariable pushedVariable = ctx.gen.currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "self");
        //     pushedVariables.Push(pushedVariable);
        // }
        
        // Promise arguments
        foreach (FieldInfo arg in node.functionInfo.parameters)
        {
            StaticVariable pushedVariable = currentScope.RegisterLocalVariable(arg.type, arg.name);
            pushedVariables.Push(pushedVariable);
        }


        StaticVariable callPushed = currentScope.RegisterLocalVariable(module.GetType("ptr"), "call_pushed_instruction");
        
        // // Bind function before BeginSubScope due to: BeginSubScope will push rbp, so, we want to take it for function call
        // ctx.gen.BindFunction(functionInfo);
        
        // Creating function body subscope (all arguments, returns genereted not in sub scope, but in current scope)
        BeginSubScope();
        currentScope.functionNode = node;
        
        Block(node.block);
        
        DropSubScope();
        
        
        // Delete our promises/provided named arguments, because they will not be accessable outside the function body
        currentScope.UnregisterLocalVariable(callPushed);
        foreach (StaticVariable var in pushedVariables)
        {
            currentScope.UnregisterLocalVariable(var);
        }
    }

    private static void FunctionCall(Node_FunctionCall node)
    {
        //
        // Define FunctionInfo and target
        //
        StaticVariable target = null;
        string functionName;
        if (node.functionNode is Node_Identifier ident)
        {
            functionName = ident.name;
        }
        else if (node.functionNode is Node_Access access)
        {
            Generate(access);
            functionName = access.name;
            target = access.variable;
        }
        else
        {
            throw new Exception($"Failed to generate {nameof(Node_FunctionCall)} due to unknown functionNode ({node.functionNode})");
        }
        
        
        FunctionInfo info = module.GetFunction(functionName);

        
        //
        // Generate argument Nodes
        //
        List<StaticVariable> passedArguments = new();
        
        // Pass 'self' argument 
        if (info.owner != null)
        {
            if (target != null) passedArguments.Add(target);
        }
        
        foreach (Node argNode in node.passedArguments)
        {
            Generate(argNode);
            passedArguments.Add(argNode.result);
        }

        if (info.parameters.Count != passedArguments.Count)
        {
            throw new Exception($"Failed to generate function '{info.name}' due to different count of passed ({passedArguments.Count}) and required ({info.parameters.Count}) arguments");
        }

        
        //
        // Arguments and parameters type-check
        //
        for (int i = 0; i < info.parameters.Count; i++)
        {
            FieldInfo paramInfo = info.parameters[i];
            StaticVariable argumentResult = passedArguments[i];

            ITypeInfo argType = argumentResult.type;
            TypeInfo paramType = paramInfo.type;

            if (paramType.IsGeneric)
            {
                if (argType is GenericImplementationInfo argGenericType)
                {
                    if (argGenericType.baseType != paramType)
                    {
                        throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected generic '{paramType.name}' at argument with index {i}, but got generic '{argType.Name}' with another base type '{argGenericType.baseType.Name}'");
                    }
                }
                else
                {
                    throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected generic '{paramType.name}' at argument with index {i}, but got non-generic '{argType.Name}'");
                }
            }
            else if (argType != paramType)
            {
                throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected '{paramType.name}' at argument with index {i}, but got '{argType.Name}'");
            }
        }
        
        
        //
        // Generate instructions for placeholders of returns
        //
        if (info.returns != null)
        {
            if (info.returns.Count > 1)
            {
                throw new NotSupportedException("Only 1 function return variable supported yet.");
            }
            else if (info.returns.Count == 1)
            {
                FieldInfo retInfo = info.returns[0];
                node.result = AllocateVariable(retInfo.type, NextTempName());
            }
        }

        
        // Collect info about pushed arguments and write byte-code for pushing
        // This info is required to write byte-code for deallocation
    
        // Here, we DO NOT PROMISE/PROVIDE any arguments
        // Here, we only write byte-code for pushing these arguments
        
        int pushedArgumentsSizeInBytes = 0;
        List<StaticVariable> argumentVariables = new();
        
        for (int i = 0; i < info.parameters.Count; i++)
        {
            StaticVariable argumentResult = passedArguments[i];

            StaticVariable argumentVariable = AllocateVariable(argumentResult.type, NextTempName());
            argumentVariables.Add(argumentVariable);
            pushedArgumentsSizeInBytes += argumentVariable.sizeInBytes;
            
            SetValue_Var_Var(argumentVariable, argumentResult);
        }
        
        
        Add(new FunctionCall_Instruction(info.inModuleIndex, info.module != module));
        
        
        // Write byte-code for deallocation of pushed arguments
        // Here, we DO NOT DEALLOCATE variables, but bytes because
        // we DID NOT give any PROMISES/PROVIDED variables, but only write byte-code
        Add(new DeallocateStackBytes_Instruction(pushedArgumentsSizeInBytes));

        for (int i = argumentVariables.Count - 1; i >= 0; i--)
        {
            StaticVariable argument = argumentVariables[i];
            currentScope.UnregisterLocalVariable(argument);
        }
    }

    private static string NextTempName()
    {
        tempNameIndex++;
        return "temp_" + tempNameIndex;
    }

    private static AbsInstructionIndex GetCurrentInstructionIndex()
    {
        return new AbsInstructionIndex(instructions.Count);
    }

    private static void Add(Instruction instruction)
    {
        instructions.Add(instruction);
    }
}

public enum OpCode : byte
{
    Invalid = 0,
    Nop,
    InternalCall,
    ExternalCall,
    Allocate_Variable,
    Variable_SetValue,
    Math,
    BeginScope,
    DropScope,
    Return,
    DeallocateStackBytes,
    Quit,
    If,
    Jump,
}

public class StaticVariable
{
    public string name;
    public int rbpOffset, sizeInBytes;
    public ITypeInfo type;
    public Scope_GenerationPhase scope;
}