using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tdotnetbridge.Generator.Tests;

public class TestTypeContext
{
    [Theory]
    [InlineData("", "string", "QString")]
    [InlineData("", "bool", "bool")]
    [InlineData("", "byte", "quint8")]
    [InlineData("", "short", "qint16")]
    [InlineData("", "ushort", "quint16")]
    [InlineData("", "int", "qint32")]
    [InlineData("", "uint", "quint32")]
    [InlineData("", "long", "qint64")]
    [InlineData("", "ulong", "quint64")]
    [InlineData("", "float", "float")]
    [InlineData("", "double", "double")]
    [InlineData("", "byte[]", "QDotNetArray<quint8>")]
    [InlineData("System.Threading.Tasks", "Task", "QDotNetTask<void>")]
    [InlineData("System.Threading.Tasks", "Task<string>", "QDotNetTask<QString>")]
    public async Task ToCppType_TypesShouldReturnCorrectTypes(string usingStatements, string typeName, string expectedType)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText($$"""
            {{string.Join('\n', usingStatements.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => $"using {x};"))}}
            
            public class TestType {
                public {{typeName}} _testType;
            }
            """);
        
        var compilation = CSharpCompilation.Create("TestCompilation", new[] { syntaxTree },
        new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var root = await syntaxTree.GetRootAsync();
        var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

        var typeContext = new TypeContext(new SemanticPackage()
        {
            PreExportedClasses = new(),
            SemanticModel = compilation.GetSemanticModel(syntaxTree)
        }, new());
        var type = typeContext.ToCppType(field.Declaration.Type);
        Assert.Equal(expectedType, type);
    }
}