using System.Diagnostics;
using System.Net.NetworkInformation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Generator;

[Generator]
public class ValueObjectSourceGenerator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor ValueObjectNotPartial = new(
        "SEED0001",
        "ValueObject is not partial",
        "The record '{0}' derives from ValueObject, but is not partial. Value objects must be marked as partial.",
        "SeedWork Domain",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ValueObjectSyntaxReceiver());

#if DEBUG
        if (!Debugger.IsAttached)
        {
            // uncomment to debug
            ////Debugger.Launch();
        }
        else
        {
            ////Debugger.Break();
        }
#endif
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (ValueObjectSyntaxReceiver)context.SyntaxReceiver!;
        var valueObjectRecord = syntaxReceiver.ValueObjectRecord;

        if (valueObjectRecord is null)
        {
            return;
        }

        if (!valueObjectRecord.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            var diagnostic = Diagnostic.Create(ValueObjectNotPartial,
                                               Location.Create(valueObjectRecord.SyntaxTree,
                                                               valueObjectRecord.Modifiers.Span),
                                               valueObjectRecord.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);

            context.;
        }
    }
}

public sealed class ValueObjectSyntaxReceiver : ISyntaxReceiver
{
    public RecordDeclarationSyntax? ValueObjectRecord { get; private set; }

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is RecordDeclarationSyntax rds)
        {
            if (rds.BaseList?.Types.Any(t => t.Type is GenericNameSyntax { Identifier.ValueText: "ValueObject" }) ?? false)
            {
                ValueObjectRecord = rds;
            }
        }
    }
}
