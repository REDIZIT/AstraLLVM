public class VM
{
    private CompiledModule module;

    private int current;
    
    public void Run(CompiledModule module)
    {
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
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }
}