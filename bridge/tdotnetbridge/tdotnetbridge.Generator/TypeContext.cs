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
        if (type is PredefinedTypeSyntax predefinedType)
        {
            return predefinedType.Keyword.Text switch
            {
                "void" => "void",
                "string" => "QString",
                _ => predefinedType.Keyword.Text
            };
        }
        
        var typeSymbol = _semanticModel.GetSymbolInfo(type).Symbol!;
        
        return (typeSymbol.ToString() switch
        {
            "String" => "QString",
            _ => typeSymbol.ToString()
        })!;
    }
}