using CommandLine;

namespace tdotnetbridge.Generator;

public class Options
{
    [Value(0, MetaName = "project", HelpText = "Project to generate code for", Required = true)]
    public string InputProject { get; set; } = string.Empty;
    
    [Value(1, MetaName = "output", HelpText = "Directory to place generated include files", Default = "-")]
    public string Output { get; set; } = string.Empty;

    [Option('f', "framework", Default = "net8.0", HelpText = "Target Framework")]
    public string Framework { get; set; } = string.Empty;

    [Option('n', "dry-run")]
    public bool DryRun { get; set; } = false;
}
