using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;


internal class Program
{
    private static void Main(string[] args)
    {

        string userName = GetGitConfig("user.name");
        string userEmail = GetGitConfig("user.email");

        string appdata = @"%LOCALAPPDATA%";
        appdata = Environment.ExpandEnvironmentVariables(appdata);

        string path = $"{appdata}/.gitprofile";


        string profileConfig = (@$"{path}/gitprofile.json");

        List<string> profileIndex = new List<string>();

        var jsonDoc = JsonDocument.Parse(File.ReadAllText(profileConfig));
        foreach (var profile in jsonDoc.RootElement.GetProperty("profiles").EnumerateArray())
        {
            profileIndex.Add(profile.GetProperty("profileName").GetString());
        }


            if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            System.IO.File.Create($"{path}/gitprofile.json");
        }

        if (!File.Exists($"{path}/gitprofile.json"))
        {
            System.IO.File.Create($"{path}/gitprofile.json");
        }

        if (args.Length == 0)
        {
            Console.WriteLine(" GitProfile v1");
            Console.WriteLine(" Use 'git-profile help' for usage information.");
        }
        else
        {
            switch (args[0].ToLower())
            {
                case "create":
                    break;
                case "delete":

                    break;
                case "list":
                    list(profileConfig);
                    break;
                case "help":
                    Console.WriteLine("         who                                    - Show current git config");
                    Console.WriteLine("         create <name> <email> <profile name>   - Create a new profile");
                    Console.WriteLine("         delete <profile name>                  - delete profile");
                    Console.WriteLine("         list                                   - List all profiles");
                    Console.WriteLine("         help                                   - help");
                    break;
                case "who":
                    Console.WriteLine($" user.name: {userName}");
                    Console.WriteLine($" user.email: {userEmail}");
                    break;
                default:
                    Console.WriteLine("Unknown command. Use 'git-profile help' for usage information.");
                    break;
            }
        }
    }

    static void list(string profileConfig)
    {
        var jsonDoc = JsonDocument.Parse(File.ReadAllText(profileConfig));
        foreach (var profile in jsonDoc.RootElement.GetProperty("profiles").EnumerateArray())
        {
            Console.WriteLine($"[{profile.GetProperty("profileName").GetString()}]");
            Console.WriteLine($" {profile.GetProperty("userName").GetString()}");
            Console.WriteLine($" {profile.GetProperty("email").GetString()}"); ;
            Console.WriteLine();
        }
    }

    static string GetGitConfig(string key)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"config --global {key}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        return output;
    }
}