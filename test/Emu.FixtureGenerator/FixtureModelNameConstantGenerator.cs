namespace Emu.FixtureGenerator;

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;



[Generator]
public partial class FixtureModelNameConstant : IIncrementalGenerator
{
    public static readonly string TargetAttributeName = "Emu.FixtureGenerator.GenerateFixtureHelpersAttribute";
    private static readonly DiagnosticDescriptor IsNotPartial = new(
        "EFG001",
        "Class must be partial",
        $"Class '{{0}}' must be partial when using '{TargetAttributeName}'",
        "Emu.FixtureGenerator",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor IsStatic= new(
        "EFG002",
        "Class must not be static",
        $"Class '{{0}}' must not be static when using '{TargetAttributeName}'",
        "Emu.FixtureGenerator",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor FileError = new(
        "EFG003",
        $"Error reading {nameof(GenerateFixtureHelpersAttribute.FixtureFile)}",
        "The file '{0}' could not be found or there was an error reading the file.{1}",
        "Emu.FixtureGenerator",
        DiagnosticSeverity.Error,
        true);



    private readonly record struct Model(ClassDeclarationSyntax Target, string FixtureFile, AdditionalText? Text);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}

        var targets = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                TargetAttributeName,
                IsCandidateForGenerator,
                IsPartialNonStaticClass
            );


        var withFiles = targets.Combine(context.AdditionalTextsProvider.Collect()).Select(ChooseAdditionalText);

        var combined = context.CompilationProvider.Combine(withFiles.Collect());

        context.RegisterSourceOutput(combined, Generate);
    }

    static bool IsCandidateForGenerator(SyntaxNode node, CancellationToken _)
    {
        if (!node.IsKind(SyntaxKind.ClassDeclaration))
        {
            return false;
        }

        return true;
    }

    static Model IsPartialNonStaticClass(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode ?? throw new InvalidOperationException("TargetNode is not a ClassDeclarationSyntax!");
        var attribute = context.Attributes.Single();
        var filename = attribute.ConstructorArguments.First().Value as string ?? throw new InvalidOperationException("Invalid fixture filename");

        return new Model(
            classDeclaration,
            filename,
            null);
    }

    static string GetClassNamespace(ClassDeclarationSyntax classDeclaration)
    {
        string? nameSpace;

        // determine the namespace the class is declared in, if any
        SyntaxNode? potentialNamespaceParent = classDeclaration.Parent;
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
            && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
            while (true)
            {
                if (namespaceParent.Parent is NamespaceDeclarationSyntax parent)
                {
                    namespaceParent = parent;
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                }
                else
                {
                    return nameSpace;
                }
            }
        }

        return string.Empty;
    }

    static Model ChooseAdditionalText((Model Left, ImmutableArray<AdditionalText> Right) parameters, CancellationToken token)
    {
        var filename = parameters.Left.FixtureFile;
        var additionalTexts = parameters.Right;

        var additionalText = additionalTexts.FirstOrDefault((t) => t.Path.EndsWith(filename!));
        return parameters.Left with { Text = additionalText };
    }


    static void Generate(SourceProductionContext context, (Compilation Left, ImmutableArray<Model> Right) source)
    {
        foreach (var model in source.Right)
        {
            var classDeclaration = model.Target;
            var className = classDeclaration.Identifier.ToString();
            var namespaceName = GetClassNamespace(classDeclaration);
            var fullName = $"{namespaceName}.{className}";

            if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(IsStatic, classDeclaration.GetLocation(), fullName));
                continue;
            }

            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(IsNotPartial, classDeclaration.GetLocation(), fullName));
                continue;
            }

            var text = model.Text?.GetText()?.ToString();

            if (text is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    FileError,
                    classDeclaration.GetLocation(),
                    model.FixtureFile,
                    " Could not find any AdditionalTexts with that name. Is your source file added as an additional text?"));
                continue;
            }

            IEnumerable<string> fixtureNames;
            try
            {
                using var reader = new StringReader(text);
                var yaml = new YamlStream();
                yaml.Load(reader);
                fixtureNames = GetNames(yaml);
            }
            catch (YamlException yex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    FileError,
                    classDeclaration.GetLocation(),
                    model.FixtureFile,
                    " " + yex.Message));
                continue;
            }

            var generatedText = GenerateSource(className, namespaceName, fixtureNames);
            context.AddSource(
                $"{className}.g.cs",
                SourceText.From(generatedText, Encoding.UTF8)
                );
        }
    }

    static IEnumerable<string> GetNames(YamlStream yaml)
    {
        foreach (var document in yaml.Documents)
        {
#pragma warning disable IDE0220 // Add explicit cast
            foreach (YamlMappingNode item in (YamlSequenceNode)document.RootNode)
            {
                foreach (var kvp in item.Children)
                {
                    if (((YamlScalarNode)kvp.Key).Value == "Name")
                    {
                        yield return ((YamlScalarNode)kvp.Value).Value ?? throw new InvalidDataException("Names must be non-null");
                    }
                }

            }
#pragma warning restore IDE0220 // Add explicit cast
        }
    }

    internal static Regex SafeCharacters = new("[^a-zA-Z0-9_]+");
    public static char[] SplitChars = new char[] { ' ', '_', '-' };

    static string GenerateSource(string className, string namespaceName, IEnumerable<string> names)
    {
        static string Sanitize(string name)
        {
            var split = name.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            var recased = split.Select(s => char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1).ToLower() : string.Empty));
            var joined = string.Join("", recased);
            return SafeCharacters.Replace(joined, "");
        }

        string declarations = string.Join(
            SyntaxFactory.ElasticCarriageReturnLineFeed.ToFullString(),
            names.Select(static name => $"        public const string {Sanitize(name)} = @\"{name}\";"));

        return @$"// <auto-generated>
namespace {namespaceName}
{{
    public partial class {className}
    {{
{declarations}
    }}
}}
";
    }
}
