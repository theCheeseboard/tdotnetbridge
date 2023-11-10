namespace tdotnetbridge.Generator;

public class PreExportedClass
{
    public required string Namespace { get; set; }
    public required string Name { get; set; }
    public string HeaderName => $"{Name.ToLower()}.h";
}