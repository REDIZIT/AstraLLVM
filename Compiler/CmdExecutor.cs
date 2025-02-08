using System.Diagnostics;

public static class CmdExecutor
{
    public static void CompileRunAndCheck(string folder, string llvmFileName, int expectedExitCode = int.MinValue)
    {
        folder = Path.GetFullPath(folder);

        string exeName = Path.GetFileNameWithoutExtension(llvmFileName) + ".exe";
        string exePath = folder + "/" + exeName;

        if (File.Exists(exePath)) File.Delete(exePath);


        try
        {
            Process compileProcess = Compile_LLVM_to_Exe(folder, llvmFileName, exeName);
            compileProcess.WaitForExit();
            if (compileProcess.ExitCode != 0) throw new Exception($"Compilation to .exe failed with exit code: {compileProcess.ExitCode}");

            Process process = Run_Exe(folder, exeName);
            process.WaitForExit();

            int exitCode = process.ExitCode;

            if (expectedExitCode != int.MinValue)
            {
                if (exitCode != expectedExitCode)
                {
                    if (exitCode == -1073741819)
                    {
                        throw new Exception($"Unexpected .exe exit code. Expected {expectedExitCode}, got Segmentation Fault");
                    }
                    throw new Exception($"Unexpected .exe exit code. Expected {expectedExitCode}, got {exitCode}");
                }
                else
                {
                    Debug.WriteLine($"Successful run test with exit code {exitCode}");
                }
            }
            else
            {
                if (exitCode != 0)
                {
                    Debug.WriteLine($"Exe file has been compiled, but unknown run-time error threw. Exit code: {exitCode}");
                }
                else
                {
                    Debug.WriteLine($"Successful run test with exit code {exitCode}");
                }
            }
        }
        finally
        {
            File.Delete(exePath);
        }       
    }
    public static Process Run_Exe(string folder, string filepath)
    {
        string cmd_runExe = filepath;
        return ExecuteCommand(folder, cmd_runExe);
    }
    public static Process Compile_LLVM_to_Exe(string folder, string sourceFile, string destFile)
    {
        string cmd_compileToExe = $"clang {sourceFile} -o {destFile}";
        return ExecuteCommand(folder, cmd_compileToExe);
    }
    public static Process ExecuteCommand(string folder, string cmd)
    {
        string strCmdText;
        strCmdText = $"/C {cmd.Trim().Replace("\n", "&")}";

        ProcessStartInfo info = new()
        {
            FileName = "cmd.exe",
            Arguments = strCmdText,
            RedirectStandardOutput = true,
            WorkingDirectory = folder,
        };

        Process process = Process.Start(info);

        //string strOutput = process.StandardOutput.ReadToEnd();

        return process;
    }
}
