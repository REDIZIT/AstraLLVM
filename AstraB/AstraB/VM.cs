public class VM
{
    private CompiledModule module;

    private int current;
    
    public void Run(CompiledModule module)
    {
        this.module = module;
        current = module.functionPointerByID[0];

        int opsDone = 0;
        int opsLimit = 1000;

        while (current < module.code.Count)
        {
            if (opsDone >= opsLimit)
            {
                throw new Exception("Operations limit exceeded");
            }

            OpCode opCode = (OpCode)module.code[current];
            current++;
            
            Execute(opCode);
            
            opsDone++;
        }
    }

    private void Execute(OpCode opCode)
    {
        switch (opCode)
        {
            case OpCode.Nop: return;
            case OpCode.Print: Console.WriteLine("Printed!"); break;
            case OpCode.InternalCall: InternalCall(); break;
            case OpCode.ExternalCall: ExternalCall(); break;
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }

    private void InternalCall()
    {
        int inModuleIndex = NextInt();

        int pointer = module.functionPointerByID[inModuleIndex];
        current = pointer;
    }

    private void ExternalCall()
    {
        int inModuleIndex = NextInt();

        if (inModuleIndex == 0)
        {
            Console.WriteLine("Printed as external call!");
        }
        else
        {
            throw new Exception($"Unknown external function with inModuleIndex = {inModuleIndex}");
        }
    }

    private int NextInt()
    {
        return BitConverter.ToInt32(NextRange(4));
    }

    private byte[] NextRange(int count)
    {
        byte[] bytes = module.code.Slice(current, count).ToArray();
        current += count;
        return bytes;
    }
}