using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tdotnetbridge.Generator;

public class TypeContext
{
    private readonly SemanticPackage _semanticPackage;
    private readonly List<RequiredHeader> _headerDatabase;

    public TypeContext(SemanticPackage semanticPackage, List<RequiredHeader> headerDatabase)
    {
        _semanticPackage = semanticPackage;
        _headerDatabase = headerDatabase;
    }

    public string ToCppType(TypeSyntax type)
    {
        return ToCppType((ITypeSymbol)_semanticPackage.SemanticModel.GetSymbolInfo(type).Symbol!);
    }

    private string ToCppType(ITypeSymbol typeSymbol)
    {
        if (_semanticPackage.PreExportedClasses.FirstOrDefault(@class =>
                typeSymbol.ToString() == $"{@class.Namespace}.{@class.Name}") is { } referencedClass)
        {
            var requiredHeader = new RequiredHeader(false, referencedClass.HeaderName);
            if (!_headerDatabase.Contains(requiredHeader))
            {
                _headerDatabase.Add(requiredHeader);
            }
            return referencedClass.Name;
        }
        
        switch (typeSymbol)
        {
            case IArrayTypeSymbol arrayTypeSymbol:
                return $"QDotNetArray<{ToCppType(arrayTypeSymbol.ElementType)}>";
            case INamedTypeSymbol { TypeArguments.Length: > 0 } namedTypeSymbol:
            {
                var containerType = $"{namedTypeSymbol.ContainingNamespace}.{namedTypeSymbol.Name}" switch
                {
                    "System.Threading.Tasks.Task" => "QDotNetTask",
                    _ => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };
                return $"{containerType}<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(ToCppType))}>";
            }
            default:
                return typeSymbol.SpecialType switch
                {
                    SpecialType.System_Void => "void",
                    SpecialType.System_Boolean => "bool",
                    SpecialType.System_String => "QString",
                    SpecialType.System_Byte => "quint8",
                    SpecialType.System_Int16 => "qint16",
                    SpecialType.System_UInt16 => "quint16",
                    SpecialType.System_Int32 => "qint32",
                    SpecialType.System_UInt32 => "quint32",
                    SpecialType.System_Int64 => "qint64",
                    SpecialType.System_UInt64 => "quint64",
                    _ => (typeSymbol.ToString() switch
                    {
                        "String" => "QString",
                        "System.Threading.Tasks.Task" => "QDotNetTask<void>",
                        _ => typeSymbol.ToString()
                    })!
                };
        }
    }
}