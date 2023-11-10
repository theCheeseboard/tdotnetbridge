using JetBrains.Annotations;

namespace tdotnetbridge.ClientLibrary;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
[MeansImplicitUse]
[UsedImplicitly]
public class ExportToQtAttribute : Attribute
{
    
}
