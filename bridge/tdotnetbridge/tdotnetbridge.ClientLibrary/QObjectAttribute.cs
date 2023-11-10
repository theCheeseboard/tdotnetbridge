using JetBrains.Annotations;

namespace tdotnetbridge.ClientLibrary;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
[UsedImplicitly]
public class QObjectAttribute : Attribute
{
}
