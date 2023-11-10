namespace tdotnetbridge.Generator;

public record RequiredHeader(bool IsGlobalHeader, string HeaderName)
{
    public string OutputCode()
    {
        var start = IsGlobalHeader ? '<' : '"';
        var end = IsGlobalHeader ? '>' : '"';
        return $"#include {start}{HeaderName}{end}";
    }
};