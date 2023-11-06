using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tdotnetbridge.Generator;

public class SyntaxTreeProcessor
{
    private const string TargetAttribute = "tdotnetbridge.ClientLibrary.QObjectAttribute.QObjectAttribute()";
    private const string TargetAttributeConstructor = "tdotnetbridge.ClientLibrary.ExportToQtAttribute.ExportToQtAttribute()";
    private readonly SyntaxTree _tree;
    private readonly SemanticModel _semanticModel;

    public SyntaxTreeProcessor(SyntaxTree tree, SemanticModel semanticModel)
    {
        _tree = tree;
        _semanticModel = semanticModel;
    }

    public async Task Process()
    {
        var root = await _tree.GetRootAsync();
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
    
        foreach (var @class in classes)
        {
            ProcessClass(@class);
        }
    }

    private void ProcessClass(MemberDeclarationSyntax @class)
    {
        if (!HasQObjectAttribute(@class))
        {
            return;
        }

        // Find all constructor declarations
        var constructors = new List<ConstructorDeclarationSyntax>();
        var methods = new List<MethodDeclarationSyntax>();
        var methodAndConstructorDeclarations = @class.DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
        foreach (var methodOrConstructor in methodAndConstructorDeclarations)
        {
            var attributesConstructorOrMethod = methodOrConstructor.AttributeLists.SelectMany(a => a.Attributes).ToList();
            foreach (var attrC in attributesConstructorOrMethod)
            {
                var attrCSymbolInfo = _semanticModel.GetSymbolInfo(attrC);
                if (attrCSymbolInfo.Symbol != null && attrCSymbolInfo.Symbol.ToString() == TargetAttributeConstructor)
                {
                    if (methodOrConstructor is ConstructorDeclarationSyntax constructor)
                    {
                        constructors.Add(constructor);
                    }

                    if (methodOrConstructor is MethodDeclarationSyntax method)
                    {
                        methods.Add(method);
                    }
                    break;
                }
            }
        }


        var classSymbol = _semanticModel.GetDeclaredSymbol(@class)!;

        var exported = new ExportedClass
        {
            Namespace = classSymbol.ContainingNamespace.ToString()!,
            Name = classSymbol.Name,
            Constructors = constructors,
            Methods = methods
        };

        Console.WriteLine(exported.OutputCode(_semanticModel));
    }

    private bool HasQObjectAttribute(MemberDeclarationSyntax @class)
    {
        var attributes = @class.AttributeLists.SelectMany(a => a.Attributes).ToList();
        return attributes.Select(attr => _semanticModel.GetSymbolInfo(attr))
            .Any(attrSymbolInfo =>
                attrSymbolInfo.Symbol != null && attrSymbolInfo.Symbol.ToString() == TargetAttribute);
    }
}