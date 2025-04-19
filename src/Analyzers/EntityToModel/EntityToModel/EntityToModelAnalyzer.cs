namespace EntityToModel
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Analyzer that detects Entity Framework entity classes (marked with [Table])
    /// and suggests generating a corresponding model class without EF-specific attributes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityToModelAnalyzer : DiagnosticAnalyzer
    {
        #region properties

        #region Properties

        /// <summary>
        /// Gets the set of supported diagnostics for this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        #endregion

        #endregion

        #region Constants

        /// <summary>
        /// The diagnostic ID used to identify this analyzer's diagnostics.
        /// </summary>
        public const string DiagnosticId = "EntityToModel";

        /// <summary>
        /// The category used to classify the diagnostic.
        /// </summary>
        private const string Category = "CodeGeneration";

        /// <summary>
        /// A description of the diagnostic shown in tools and documentation.
        /// </summary>
        private static readonly LocalizableString Description =
            "This class appears to be an EF entity. Consider generating a model class without EF attributes.";

        /// <summary>
        /// The message format used when reporting a diagnostic instance.
        /// </summary>
        private static readonly LocalizableString MessageFormat =
            "Entity class '{0}' should have a corresponding model class";

        /// <summary>
        /// The title displayed in the IDE for this diagnostic.
        /// </summary>
        private static readonly LocalizableString Title = "Entity class detected";

        /// <summary>
        /// The diagnostic descriptor used to configure and describe this analyzer's rule.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            // Hidden: no squiggle, but code fix still shows
            DiagnosticSeverity.Hidden,
            true,
            Description);

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the analyzer and registers actions to be performed during analysis.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClassAttributes, SyntaxKind.ClassDeclaration);
        }

        /// <summary>
        /// Analyzes class declarations to determine if they are EF entity classes and
        /// reports a diagnostic if a corresponding model class may be needed.
        /// </summary>
        /// <param name="context">The syntax node analysis context.</param>
        private static void AnalyzeClassAttributes(SyntaxNodeAnalysisContext context)
        {
            // Check if the class has a [Table] attribute (indicating it's an EF entity)
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
    }
}