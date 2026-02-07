using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

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


        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/gitprofile.json", "{\"profiles\": []}");

        }

        if (!File.Exists($"{path}/gitprofile.json"))
        {
            File.WriteAllText($"{path}/gitprofile.json", "{\"profiles\": []}");
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
                case "use":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: git-profile use <profile name>");
                        return;
                    }
                    string selectedProfile = args[1];
                    use(selectedProfile, profileConfig);
                    break;
                case "create":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Usage: git-profile create <name> <email> <profile name>");
                        return;
                    }
                    string name = args[1];
                        string email = args[2];
                        string profileName = args[3];
                        create(name, email, profileName, profileConfig);
                        break;

                case "delete":

                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: git-profile delete <profile name>");
                        return;
                    }
                    string profile = args[1];

                    delete(profile, profileConfig);
                    break;

                case "list":
                    list(profileConfig);
                    break;
                case "help":
                    Console.WriteLine("         who                                    - Show current git config");
                    Console.WriteLine("         use <profile name>                     - Use selected profile");
                    Console.WriteLine("         create <name> <email> <profile name>   - Create a new profile");
                    Console.WriteLine("         delete <profile name>                  - Delete profile");
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

    static void use(string selectedProfile, string profileConfig)
    {
        var jsonContent = File.ReadAllText(profileConfig);
        var node = JsonNode.Parse(jsonContent);
        var foundProfile = node?["profiles"]?.AsArray()
            .FirstOrDefault(p => p?["profileName"]?.GetValue<string>() == selectedProfile);

        if (foundProfile != null)
        {
            string n = foundProfile["userName"]!.GetValue<string>();
            string e = foundProfile["email"]!.GetValue<string>();
            SetGitConfig(n, e);
        }
        else
        {
            return;
        }
    }

    static void create(string userName, string email, string profileName, string profileConfig)
    {
        try
        {


            var jsonNode = JsonNode.Parse(File.ReadAllText(profileConfig))!;
            var profilesArray = jsonNode["profiles"]!.AsArray();
            bool profileExists = profilesArray.Any(p => p!["profileName"]!.GetValue<string>() == profileName);

            profilesArray.Add(
            new JsonObject
            {
                ["profileName"] = profileName,
                ["userName"] = userName,
                ["email"] = email
            }

    );

            if (profileName != null && !profileExists )
            {
            File.WriteAllText(profileConfig, jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Profile '{profileName}' already exists or is empty.");
            }

        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }

    }

    static void delete(string profileName, string profileConfig)
    {
        try
        {
            var json = JsonNode.Parse(File.ReadAllText(profileConfig))!;
            var profiles = json["profiles"]!.AsArray();

            for (int i = profiles.Count - 1; i >= 0; i--)
            {
                string currentProfileName = profiles[i]!["profileName"]!.GetValue<string>();
                if (currentProfileName == profileName)
                {
                    profiles.RemoveAt(i);
                }
            }

            File.WriteAllText(profileConfig, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
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


    static void SetGitConfig(string name, string email)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"config --global user.name \"{name}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"config --global user.email \"{email}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating git config: {ex.Message}");
        }
    }
}
