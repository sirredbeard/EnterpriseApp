using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class IntegrationRunnerTests
{
    [Fact]
    public async Task Runner_ExitsWithZero()
    {
        // Find repository root by walking parents from the test assembly base directory.
        string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "LICENSE")) || Directory.Exists(Path.Combine(dir.FullName, "src")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            throw new Exception("Repository root not found");
        }

        var repoRoot = FindRepoRoot();
        var runnerPath = Path.Combine(repoRoot, "tests", "integration-runner");
        if (!Directory.Exists(runnerPath)) throw new DirectoryNotFoundException(runnerPath);

        var psi = new ProcessStartInfo("dotnet", "run --project \"tests/integration-runner\"")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var proc = Process.Start(psi) ?? throw new Exception("failed to start dotnet run");
        // Read output asynchronously and wait for the process to finish (give it up to 60s)
        var stdOutTask = proc.StandardOutput.ReadToEndAsync();
        var stdErrTask = proc.StandardError.ReadToEndAsync();

        var finished = proc.WaitForExit(60000);
        if (!finished)
        {
            try { proc.Kill(true); } catch { }
            throw new Exception("Integration runner did not exit within the timeout");
        }

        var stdout = await stdOutTask;
        var stderr = await stdErrTask;
        var exit = proc.ExitCode;

        // Include logs in the assertion message for diagnostics
        Assert.True(exit == 0, $"Integration runner failed with exit {exit}\n--- STDOUT ---\n{stdout}\n--- STDERR ---\n{stderr}");
    }
}
