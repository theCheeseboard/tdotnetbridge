using Microsoft.CodeAnalysis;

namespace tdotnetbridge.Generator;

public class SemanticPackage
{
    public required List<PreExportedClass> PreExportedClasses { get; init; }
    public required SemanticModel SemanticModel { get; init; }
}