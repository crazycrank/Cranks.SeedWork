using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueObjectAnalyzerCodeFixProvider))]
[Shared]
public class ValueObjectAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ValueObjectAnalyzer.MustBePartialId,
                                 ValueObjectAnalyzer.MustBeRecordId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id == ValueObjectAnalyzer.MustBePartialId)
            {
                var title = ValueObjectAnalyzer.MustBePartial.Title.ToString();

                var action = CodeAction.Create(title,
                                               token => MakePartialAsync(context, diagnostic, token),
                                               title);

                context.RegisterCodeFix(action, diagnostic);
            }

            if (diagnostic.Id == ValueObjectAnalyzer.MustBeRecordId)
            {
                var title = ValueObjectAnalyzer.MustBeRecord.Title.ToString();

                var action = CodeAction.Create(title,
                                               token => MakeRecordAsync(context, diagnostic, token),
                                               title);

                context.RegisterCodeFix(action, diagnostic);
            }
        }

        return Task.CompletedTask;
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

        var rds = FindTypeDeclaration(diagnostic, root);
        var newRds = rds.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        var newRoot = root.ReplaceNode(rds, newRds);
        var newDocument = context.Document.WithSyntaxRoot(newRoot);

        return newDocument;
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

    private static async Task<Document> DeriveFromValueObjectAsync(CodeFixContext context,
                                                                   Diagnostic diagnostic,
                                                                   CancellationToken cancellationToken)
    {
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return context.Document;
        }

        var rds = FindTypeDeclaration(diagnostic, root);

        // TODO replace nodes
        var newRds = rds.WithBaseList(rds.BaseList);

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
}
