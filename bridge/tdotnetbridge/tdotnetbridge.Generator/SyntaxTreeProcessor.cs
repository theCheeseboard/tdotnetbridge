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

    public async Task<IEnumerable<ExportedClass>> Process()
    {
        var root = await _tree.GetRootAsync();
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        return classes.Select(ProcessClass).Where(x => x is not null)!;
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
            if (!attributesConstructorOrMethod.Select(attrC => _semanticModel.GetSymbolInfo(attrC)).Any(
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
            if (!attributesProperty.Select(attrP => _semanticModel.GetSymbolInfo(attrP)).Any(
                    attrPSymbolInfo => attrPSymbolInfo.Symbol is not null &&
                                       attrPSymbolInfo.Symbol.ToString() == TargetAttributeConstructor))
            {
                continue;
            }

            properties.Add(property);
        }

        var classSymbol = _semanticModel.GetDeclaredSymbol(@class)!;

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
        return attributes.Select(attr => _semanticModel.GetSymbolInfo(attr))
            .Any(attrSymbolInfo =>
                attrSymbolInfo.Symbol != null && attrSymbolInfo.Symbol.ToString() == TargetAttribute);
    }
}