using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tdotnetbridge.Generator;

public class TypeContext
{
    private readonly SemanticModel _semanticModel;

    public TypeContext(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public string ToCppType(TypeSyntax type)
    {
        return ToCppType((ITypeSymbol)_semanticModel.GetSymbolInfo(type).Symbol!);
    }

    private static string ToCppType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol { TypeArguments.Length: > 0 } namedTypeSymbol)
        {
            var containerType = $"{namedTypeSymbol.ContainingNamespace}.{namedTypeSymbol.Name}" switch
            {
                "System.Threading.Tasks.Task" => "QDotNetTask",
                _ => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            };
            return $"{containerType}<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(ToCppType))}>";
        }

        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Void => "void",
            SpecialType.System_String => "QString",
            _ => (typeSymbol.ToString() switch
            {
                "String" => "QString",
                "System.Threading.Tasks.Task" => "QDotNetTask<void>",
                _ => typeSymbol.ToString()
            })!
        };
    }
}