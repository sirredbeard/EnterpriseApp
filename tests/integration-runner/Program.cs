using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

int GetFreePort()
{
    var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

int port = GetFreePort();
// Find repository root (look for LICENSE or src folder) so this works when run from tests or CLI
string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "LICENSE")) || Directory.Exists(Path.Combine(dir.FullName, "src")))
            return dir.FullName;
        dir = dir.Parent;
    }
    dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "LICENSE")) || Directory.Exists(Path.Combine(dir.FullName, "src")))
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new Exception("Repository root not found");
}

var repoRoot = FindRepoRoot();
var workingDir = Path.Combine(repoRoot, "src", "EnterpriseApp");

var startInfo = new ProcessStartInfo("dotnet", $"run --urls \"http://localhost:{port}\"")
{
    WorkingDirectory = workingDir,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};
// Ensure the app runs in Development so DeveloperExceptionPage prints stack traces to stdout
startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
// Pass the Authority down so the JwtBearer middleware can find the IdentityServer discovery document
startInfo.EnvironmentVariables["Authority"] = $"http://localhost:{port}";

using var proc = Process.Start(startInfo) ?? throw new Exception("Failed to start dotnet");

// Stream the child process stdout/stderr so we can see server logs and exceptions in the runner output
proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine("APP: " + e.Data); };
proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine("APPERR: " + e.Data); };
proc.BeginOutputReadLine();
proc.BeginErrorReadLine();
try
{
    var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };
    var ready = false;
    for (int i = 0; i < 60; i++)
    {
        try
        {
            var r = await client.GetAsync("/swagger/v1/swagger.json");
            if (r.IsSuccessStatusCode) { ready = true; break; }
        }
        catch { }
        await Task.Delay(500);
    }
    if (!ready) { Console.Error.WriteLine("Server did not become ready"); return 2; }

    var form = new Dictionary<string, string>
    {
        ["grant_type"] = "client_credentials",
        ["client_id"] = "client",
        ["scope"] = "api"
    };

    var tokenResp = await client.PostAsync("/connect/token", new FormUrlEncodedContent(form));
    if (!tokenResp.IsSuccessStatusCode)
    {
        Console.Error.WriteLine("Token request failed: " + tokenResp.StatusCode);
        return 3;
    }
    var tokenJson = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync());
    if (!tokenJson.RootElement.TryGetProperty("access_token", out var accessTokenEl))
    {
        Console.Error.WriteLine("Token response missing access_token");
        return 4;
    }
    var accessToken = accessTokenEl.GetString();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var apiResp = await client.GetAsync("/protected/secret");
    if (!apiResp.IsSuccessStatusCode)
    {
        Console.Error.WriteLine("Protected endpoint returned: " + apiResp.StatusCode);
        return 5;
    }

    Console.WriteLine("Integration check succeeded");
    return 0;
}
finally
{
    if (!proc.HasExited)
    {
        proc.Kill(true);
        proc.WaitForExit(5000);
    }
}