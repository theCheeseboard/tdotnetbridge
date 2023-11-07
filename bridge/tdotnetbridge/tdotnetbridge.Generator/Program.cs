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

    if (options.Output != "-")
    {
        Directory.CreateDirectory(options.Output);
    }
    
    foreach (var document in project.Documents)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        if (syntaxTree is null || semanticModel is null) continue;
        var syntaxTreeProcessor = new SyntaxTreeProcessor(syntaxTree, semanticModel);
        foreach (var exportedClass in await syntaxTreeProcessor.Process())
        {
            if (options.Output == "-")
            {
                Console.WriteLine(exportedClass.HeaderName);
                Console.WriteLine(exportedClass.OutputCode(semanticModel));
                return;
            }

            var outputFile = Path.Combine(options.Output, exportedClass.HeaderName);
            await File.WriteAllTextAsync(outputFile, exportedClass.OutputCode(semanticModel));
            Console.WriteLine(exportedClass.HeaderName);
        }
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

