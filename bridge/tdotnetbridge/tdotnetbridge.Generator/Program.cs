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

    var trees = await Task.WhenAll(project.Documents.Select(async document => (await document.GetSyntaxTreeAsync(), await document.GetSemanticModelAsync())));
    var syntaxTreeProcessors = trees.Where(x => x.Item1 is not null && x.Item2 is not null)
        .Select(x => new SyntaxTreeProcessor(x.Item1!, x.Item2!)).ToList();

    var preprocessed =
        (await Task.WhenAll(syntaxTreeProcessors.Select(async processor => await processor.PreProcess())))
        .SelectMany(x => x).ToList();

    foreach (var syntaxTreeProcessor in syntaxTreeProcessors)
    {
        var semanticPackage = new SemanticPackage
        {
            SemanticModel = syntaxTreeProcessor.SemanticModel,
            PreExportedClasses = preprocessed
        };
        
        foreach (var exportedClass in await syntaxTreeProcessor.Process())
        {
            if (options.Output == "-")
            {
                Console.WriteLine(exportedClass.HeaderName);
                Console.WriteLine(exportedClass.OutputCode(semanticPackage));
                return;
            }

            var outputFile = Path.Combine(options.Output, exportedClass.HeaderName);
            await File.WriteAllTextAsync(outputFile, exportedClass.OutputCode(semanticPackage));
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

