using CommandLine;
using CommandLine.Text;

namespace tdotnetbridge.Generator;

public class Options
{
    [Value(0, MetaName = "project", HelpText = "Project to generate code for", Required = true)]
    public string InputProject { get; set; } = string.Empty;

    [Option('f', "framework", Default = "net7.0", HelpText = "Target Framework")]
    public string Framework { get; set; } = string.Empty;
}
