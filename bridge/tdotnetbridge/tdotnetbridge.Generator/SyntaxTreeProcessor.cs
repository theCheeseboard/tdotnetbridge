using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tdotnetbridge.Generator;

public class SyntaxTreeProcessor
{
    private const string TargetAttribute = "tdotnetbridge.ClientLibrary.QObjectAttribute.QObjectAttribute()";
    private const string TargetAttributeConstructor = "tdotnetbridge.ClientLibrary.ExportToQtAttribute.ExportToQtAttribute()";
    private readonly SyntaxTree _tree;
    public SemanticModel SemanticModel { get; }

    private async Task<IEnumerable<ClassDeclarationSyntax>> GetClasses() {
        var root = await _tree.GetRootAsync();
        return root.DescendantNodes().OfType<ClassDeclarationSyntax>();
    }

    public SyntaxTreeProcessor(SyntaxTree tree, SemanticModel semanticModel)
    {
        _tree = tree;
        SemanticModel = semanticModel;
    }

    public async Task<IEnumerable<PreExportedClass>> PreProcess()
    {
        var classes = await GetClasses();
        return classes.Select(PreProcessClass).Where(x => x is not null)!;
    }

    public async Task<IEnumerable<ExportedClass>> Process()
    {
        var classes = await GetClasses();
        return classes.Select(ProcessClass).Where(x => x is not null)!;
    }

    private PreExportedClass? PreProcessClass(MemberDeclarationSyntax @class)
    {
        if (!HasQObjectAttribute(@class))
        {
            return null;
        }
        
        var classSymbol = SemanticModel.GetDeclaredSymbol(@class)!;
        return new()
        {
            Namespace = classSymbol.ContainingNamespace.ToString()!,
            Name = classSymbol.Name,
        };
    }

    private ExportedClass? ProcessClass(MemberDeclarationSyntax @class)
    {
        if (!HasQObjectAttribute(@class))
        {
            return null;
        }

        // Find all constructor and method declarations
        var constructors = new List<ConstructorDeclarationSyntax>();
        var methods = new List<MethodDeclarationSyntax>();
        var methodAndConstructorDeclarations = @class.DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
        foreach (var methodOrConstructor in methodAndConstructorDeclarations)
        {
            var attributesConstructorOrMethod = methodOrConstructor.AttributeLists.SelectMany(a => a.Attributes).ToList();
            if (!attributesConstructorOrMethod.Select(attrC => SemanticModel.GetSymbolInfo(attrC)).Any(
                    attrCSymbolInfo => attrCSymbolInfo.Symbol != null &&
                                       attrCSymbolInfo.Symbol.ToString() == TargetAttributeConstructor))
            {
                continue;
            }

            switch (methodOrConstructor)
            {
                case ConstructorDeclarationSyntax constructor:
                    constructors.Add(constructor);
                    break;
                case MethodDeclarationSyntax method:
                    methods.Add(method);
                    break;
            }
        }

        var properties = new List<PropertyDeclarationSyntax>();
        var propertyDeclarations = @class.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        foreach (var property in propertyDeclarations)
        {
            var attributesProperty = property.AttributeLists.SelectMany(a => a.Attributes).ToList();
            if (!attributesProperty.Select(attrP => SemanticModel.GetSymbolInfo(attrP)).Any(
                    attrPSymbolInfo => attrPSymbolInfo.Symbol is not null &&
                                       attrPSymbolInfo.Symbol.ToString() == TargetAttributeConstructor))
            {
                continue;
            }

            properties.Add(property);
        }

        var classSymbol = SemanticModel.GetDeclaredSymbol(@class)!;

        var exported = new ExportedClass
        {
            Namespace = classSymbol.ContainingNamespace.ToString()!,
            Name = classSymbol.Name,
            Constructors = constructors,
            Methods = methods,
            Properties = properties
        };

        return exported;
    }

    private bool HasQObjectAttribute(MemberDeclarationSyntax @class)
    {
        var attributes = @class.AttributeLists.SelectMany(a => a.Attributes).ToList();
        return attributes.Select(attr => SemanticModel.GetSymbolInfo(attr))
            .Any(attrSymbolInfo =>
                attrSymbolInfo.Symbol != null && attrSymbolInfo.Symbol.ToString() == TargetAttribute);
    }
}