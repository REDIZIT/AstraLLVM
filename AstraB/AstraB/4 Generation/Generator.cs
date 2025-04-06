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

        compiled.code = encoder.code;

        return compiled;
    }

    private static void Generate(Node node)
    {
        switch (node)
        {
            case Node_FunctionDeclaration functionDeclaration:
                FunctionDeclaration(functionDeclaration);
                break;
            case Node_FunctionCall call:
                FunctionCall(call);
                break;
            case Node_VariableDeclaration variableDeclaration:
                VariableDeclaration(variableDeclaration);
                break;
            case Node_VariableAssign variableAssignment:
                VariableAssignment(variableAssignment);
                break;
            case Node_Binary binary:
                Binary(binary);
                break;
            case Node_ConstantNumber constantNumber:
                Constant(constantNumber);
                break;
            case Node_Identifier ident:
                LoadVariable(ident);
                break;
            case Node_CastTo cast:
                Cast(cast);
                break;
            case Node_Grouping grouping:
                Grouping(grouping);
                break;
            case Node_Return ret:
                Return(ret);
                break;
            default:
                throw new Exception($"Failed to generate due to unexpected node '{node}'");
        }
    }

    private static void Return(Node_Return ret)
    {
        if (ret.value != null)
        {
            Generate(ret.value);
            ret.result = ret.value.result;
        }
        
        Add(new Return_Instruction());
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
        
        Add(new Math_Instruction(node.left.result.rbpOffset, node.right.result.rbpOffset, node.result.rbpOffset, op));
    }

    private static MathOperator GetMathOperator(Token token)
    {
        switch (token)
        {
            case Token_Plus: return MathOperator.Add;
            case Token_Minus: return MathOperator.Sub;
            case Token_Star: return MathOperator.Mul;
            case Token_Slash: return MathOperator.Div;
            default: throw new Exception($"Token '{token}' is not a math operator");
        }
    }

    private static void Cast(Node_CastTo node)
    {
        Generate(node.valueToCast);

        ITypeInfo sourceType = node.valueToCast.result.type;
        TypeInfo targetType = module.GetType(node.typeName);

        if (targetType.name == "ptr")
        {
            node.result = AllocateVariable(targetType, NextTempName());
            SetValue_Var_Ptr(node.result, node.valueToCast.result);
        }
        else if (targetType.name == "int" && sourceType.Name == "ptr")
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
        currentScope = currentScope.parent;
        
        // Epilogue after sub scope drop
        Add(new Scope_Instruction(false));
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
        Add(new AllocateVariable_Instruction(type.SizeInBytes).WithDebug(variable));

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
    
    private static void SetValue_Var_Ptr(StaticVariable dest, StaticVariable value)
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
        Stack<StaticVariable> pushedVariables = new();

        // if (node.functionInfo.isStatic == false)
        // {
        //     StaticVariable pushedVariable = ctx.gen.currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "self");
        //     pushedVariables.Push(pushedVariable);
        // }
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
        if (node.functionNode is Node_Identifier ident)
        {
            FunctionInfo info = module.GetFunction(ident.name);

            
            if (info.parameters.Count != node.passedArguments.Count)
            {
                throw new Exception($"Failed to generate function '{info.name}' due to different count of passed ({node.passedArguments.Count}) and required ({info.parameters.Count}) arguments");
            }

            // Generate placeholders for returns
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
            
            
            // Generate arguments nodes
            for (int i = 0; i < info.parameters.Count; i++)
            {
                FieldInfo paramInfo = info.parameters[i];
                Node argumentNode = node.passedArguments[i];

                Generate(argumentNode);
                if (argumentNode.result.type != paramInfo.type)
                {
                    throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected '{paramInfo.type.name}' at argument with index {i}, but got '{argumentNode.result.type.Name}'");
                }
            }

            int pushedArgumentsSizeInBytes = 0;
            
            // Allocate (duplicate) arguments
            for (int i = 0; i < info.parameters.Count; i++)
            {
                Node argumentNode = node.passedArguments[i];

                StaticVariable argumentVariable = AllocateVariable(argumentNode.result.type, NextTempName());
                pushedArgumentsSizeInBytes += argumentVariable.sizeInBytes;
                
                SetValue_Var_Var(argumentVariable, argumentNode.result);
            }
            
            Add(new FunctionCall_Instruction(info.inModuleIndex, info.module != module));
            
            Add(new DeallocateStackBytes_Instruction(pushedArgumentsSizeInBytes));
        }
        else
        {
            throw new Exception($"Failed to generate {nameof(Node_FunctionCall)} due to unknown functionNode ({node.functionNode})");
        }
    }

    private static string NextTempName()
    {
        tempNameIndex++;
        return "temp_" + tempNameIndex;
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
}

public class StaticVariable
{
    public string name;
    public int rbpOffset, sizeInBytes;
    public ITypeInfo type;
    public Scope_GenerationPhase scope;
}