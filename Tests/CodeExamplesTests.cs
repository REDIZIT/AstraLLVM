using System.Diagnostics;

namespace Astra.Tests;

public class CodeExamplesTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void Test()
    {
        string folder = "../../../CodeExamples";
        string[] files = Directory.GetFiles(folder, "*.ac");

        string cmd_compileToExe = "clang temp.ll -o temp.exe";
        string cmd_runExe = "temp.exe";

        try
        {
            foreach (string filepath in files)
            {
                string testContent = File.ReadAllText(filepath);

                string[] split = testContent.Split("---");

                string code = split[0].Trim();
                string returnResult = split[1].Trim();

                string llvm;

                try
                {
                    llvm = Compiler.Compile_Astra_to_LLVM(code);
                }
                catch
                {
                    string message = $"Compilation failed: '{Path.GetFileName(filepath)}'";

                    Console.WriteLine(message);
                    throw;
                }

                File.WriteAllText(folder + "/temp.ll", llvm);


                Process compileProcess = ExecuteCommand(folder, cmd_compileToExe);
                compileProcess.WaitForExit();

                Process runProcess = ExecuteCommand(folder, cmd_runExe);
                runProcess.WaitForExit();

                if (runProcess.ExitCode != int.Parse(returnResult))
                {
                    Assert.Fail($"Run failed: '{Path.GetFileName(filepath)}'\nExpected: {returnResult}\nGot: {runProcess.ExitCode}");
                }
            }
        }
        finally
        {
            File.Delete(folder + "/temp.ll");
            File.Delete(folder + "/temp.exe");
        }

        Assert.Pass($"{files.Length} code examples checked");
    }

    private Process ExecuteCommand(string folder, string cmd)
    {
        string strCmdText;
        strCmdText = $"/C {cmd.Trim().Replace("\n", "&")}";

        ProcessStartInfo info = new()
        {
            FileName = "CMD.exe",
            Arguments = strCmdText,
            RedirectStandardOutput = true,
            WorkingDirectory = folder,
        };

        Process process = Process.Start(info);

        //string strOutput = process.StandardOutput.ReadToEnd();
        //process.WaitForExit();

        return process;
    }
}