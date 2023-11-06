// See https://aka.ms/new-console-template for more information

using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using tdotnetbridge.Generator;

static async Task OptionsParsedSuccessfully(Options options)
{
    MSBuildLocator.RegisterDefaults();
    
    var workspace = MSBuildWorkspace.Create();
    var project = await workspace.OpenProjectAsync(options.InputProject);
    
    var syntaxTrees = new List<SyntaxTree>();
    foreach (var document in project.Documents)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var syntaxTreeProcessor = new SyntaxTreeProcessor(syntaxTree, semanticModel);
        await syntaxTreeProcessor.Process();
    }
}

var result = Parser.Default.ParseArguments<Options>(args);
result = await result.WithParsedAsync(OptionsParsedSuccessfully);
await result.WithNotParsedAsync(e =>
{
    foreach (var error in e)
    {
        Console.WriteLine(error.ToString());
    }

    return Task.CompletedTask;
});

