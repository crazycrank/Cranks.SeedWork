using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Analyzers.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SmartEnumAnalyzerCodeFixProvider))]
[Shared]
public class SmartEnumAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(Rules.SmartEnum_MustBePartial.Id,
                                 Rules.SmartEnum_MustBeRecord.Id,
                                 Rules.SmartEnum_MustBeSealed.Id);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
#if LAUNCH_DEBUGGER
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id == Rules.SmartEnum_MustBeRecord.Id)
            {
                var title = Rules.SmartEnum_MustBeRecord.Title.ToString();

                var action = CodeAction.Create(title,
                                               token => MakeRecordAsync(context, diagnostic, token),
                                               title);

                context.RegisterCodeFix(action, diagnostic);
            }

            if (diagnostic.Id == Rules.SmartEnum_MustBePartial.Id)
            {
                var title = Rules.SmartEnum_MustBePartial.Title.ToString();

                var action = CodeAction.Create(title,
                                               token => MakePartialAsync(context, diagnostic, token),
                                               title);

                context.RegisterCodeFix(action, diagnostic);
            }

            if (diagnostic.Id == Rules.SmartEnum_MustBeSealed.Id)
            {
                var title = Rules.SmartEnum_MustBeSealed.Title.ToString();

                var action = CodeAction.Create(title,
                                               token => MakeSealedAsync(context, diagnostic, token),
                                               title);

                context.RegisterCodeFix(action, diagnostic);
            }
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> MakeRecordAsync(CodeFixContext context,
                                                        Diagnostic diagnostic,
                                                        CancellationToken cancellationToken)
    {
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return context.Document;
        }

        var cds = FindTypeDeclaration(diagnostic, root);

        var newRds = SyntaxFactory.RecordDeclaration(cds.AttributeLists,
                                                     cds.Modifiers,
                                                     SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                                                     cds.Identifier,
                                                     cds.TypeParameterList,
                                                     null,
                                                     cds.BaseList,
                                                     cds.ConstraintClauses,
                                                     cds.OpenBraceToken,
                                                     cds.Members,
                                                     cds.CloseBraceToken,
                                                     cds.SemicolonToken);

        var newRoot = root.ReplaceNode(cds, newRds);
        var newDocument = context.Document.WithSyntaxRoot(newRoot);

        return newDocument;
    }

    private static async Task<Document> MakePartialAsync(CodeFixContext context,
                                                         Diagnostic diagnostic,
                                                         CancellationToken cancellationToken)
    {
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return context.Document;
        }

        var rds = FindRecordDeclaration(diagnostic, root);
        var newRds = rds.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        var newRoot = root.ReplaceNode(rds, newRds);
        var newDocument = context.Document.WithSyntaxRoot(newRoot);

        return newDocument;
    }

    private static async Task<Document> MakeSealedAsync(CodeFixContext context,
                                                        Diagnostic diagnostic,
                                                        CancellationToken cancellationToken)
    {
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return context.Document;
        }

        var rds = FindRecordDeclaration(diagnostic, root);

        var indexOfPartial = rds.Modifiers.IndexOf(SyntaxKind.PartialKeyword);
        var newRds = rds.WithModifiers(rds.Modifiers.Insert(indexOfPartial, SyntaxFactory.Token(SyntaxKind.SealedKeyword)));
        var newRoot = root.ReplaceNode(rds, newRds);
        var newDocument = context.Document.WithSyntaxRoot(newRoot);

        return newDocument;
    }

    private static TypeDeclarationSyntax FindTypeDeclaration(Diagnostic diagnostic,
                                                             SyntaxNode root)
    {
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        return root.FindToken(diagnosticSpan.Start)
                   .Parent?.AncestorsAndSelf()
                   .OfType<TypeDeclarationSyntax>()
                   .First()!;
    }

    private static TypeDeclarationSyntax FindRecordDeclaration(Diagnostic diagnostic,
                                                               SyntaxNode root)
    {
        return (FindTypeDeclaration(diagnostic, root) as RecordDeclarationSyntax)!;
    }
}
