namespace EntityToModel
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityToModelCodeFixProvider))]
    [Shared]
    public class EntityToModelCodeFixProvider : CodeFixProvider
    {
        #region methods

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var classDeclaration = root.FindToken(diagnosticSpan.Start)
                .Parent.AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            if (classDeclaration == null)
            {
                return;
            }
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Generate model class",
                    c => GenerateModelClassAsync(context.Document, classDeclaration, c),
                    "GenerateModel"),
                diagnostic);
        }

        private static async Task<Solution> GenerateModelClassAsync(
            Document document,
            ClassDeclarationSyntax classDecl,
            CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var targetProject = solution.Projects.FirstOrDefault(p => p.Name == "Logic.Models");
            if (targetProject == null)
            {
                return solution;
            }

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, cancellationToken);
            if (classSymbol == null)
            {
                return solution;
            }
            var modelClassName = $"{classSymbol.Name}Model";
            var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .Select(RemoveAttributes);
            var modelClass = SyntaxFactory.ClassDeclaration(modelClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties.ToArray());
            var namespaceDecl = SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.ParseName("Logic.Models"))
                .AddMembers(modelClass);
            var newFileContent = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(namespaceDecl)
                .NormalizeWhitespace();

            // Create a new document in the Logic.Models project
            var newDocument = targetProject.AddDocument($"{modelClassName}.cs", newFileContent.ToFullString());

            return newDocument.Project.Solution;
        }

        private static PropertyDeclarationSyntax RemoveAttributes(PropertyDeclarationSyntax property)
        {
            return property.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
        }

        #endregion

        #region properties

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(EntityToModelAnalyzer.DiagnosticId);

        #endregion
    }
}