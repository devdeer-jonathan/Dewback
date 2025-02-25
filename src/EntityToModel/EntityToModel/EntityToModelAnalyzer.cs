namespace EntityToModel
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityToModelAnalyzer : DiagnosticAnalyzer
    {
        #region constants

        public const string DiagnosticId = "EntityToModel";
        private const string Category = "CodeGeneration";

        private static readonly LocalizableString Description =
            "This class appears to be an EF entity. Consider generating a model class without EF attributes.";

        private static readonly LocalizableString MessageFormat =
            "Entity class '{0}' should have a corresponding model class";

        private static readonly LocalizableString Title = "Entity class detected";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            true,
            Description);

        #endregion

        #region methods

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClassAttributes, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassAttributes(SyntaxNodeAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var location = classDeclaration.GetLocation();
            var isTable = classDeclaration.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString() == "Table");
            if (isTable)
            {
                var diagnostic = Diagnostic.Create(Rule, location, classDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        #endregion

        #region properties

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        #endregion
    }
}