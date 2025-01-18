public static class Program
{
    public static void Main()
    {
        bool doRecompile = true;

        if (doRecompile)
        {
            string fileContent = File.ReadAllText("../../../source.ac");
            string llvmCode = Compiler.Compile_Astra_to_LLVM(fileContent);
            File.WriteAllText("../../../output.ll", llvmCode);
            //File.Copy("../../../output.ll", "\\\\wsl.localhost\\Ubuntu\\home\\redizit\\LLVM\\program.ll", true);
        }

        CmdExecutor.CompileRunAndCheck("../../..", "output.ll", 123);
    }
}
