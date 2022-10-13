using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Analyzers.Extensions;

internal static class SyntaxExtensions
{
    public static string? ExtractName(this SyntaxNode? node)
        => (node as NameSyntax).ExtractName();

    public static string? ExtractName(this NameSyntax? node)
        => node switch
           {
               SimpleNameSyntax sns => sns.Identifier.Text,
               QualifiedNameSyntax qns => qns.Right.Identifier.Text,
               _ => null,
           };

    public static string? ExtractTypeName(this SyntaxNode? node)
    {
        return (node as NameSyntax).ExtractTypeName();
    }

    public static string? ExtractTypeName(this TypeSyntax? node)
    {
        return node switch
               {
                   PredefinedTypeSyntax pds => pds.Keyword.Text,
                   _ => null,
               };
    }

    public static SyntaxNode? GetAttributeNode(this SyntaxNode? node)
        => (node as AttributeSyntax).GetAttributeNode();

    public static SyntaxNode? GetAttributeNode(this AttributeSyntax? node)
        => node?.Parent?.Parent;

    public static bool IsPartial(this SyntaxNode? node)
        => (node as BaseTypeDeclarationSyntax).IsPartial();

    public static bool IsPartial(this BaseTypeDeclarationSyntax? node)
        => node?.Modifiers.Any(SyntaxKind.PartialKeyword) ?? false;

    public static bool IsSubTypeOf(this SyntaxNode? node, string baseTypeName)
        => (node as BaseTypeDeclarationSyntax).IsSubTypeOf(baseTypeName);

    public static bool IsSubTypeOf(this BaseTypeDeclarationSyntax? node, string baseTypeName)
        => node?.BaseList?.Types.Any(t => t.ExtractName()?.Equals(baseTypeName, StringComparison.Ordinal) ?? false) ?? false;

    public static bool IsNestedTypeDeclaration(this SyntaxNode? node)
    {
        if (node is null)
        {
            return false;
        }

        return node.Parent is BaseTypeDeclarationSyntax
               || IsNestedTypeDeclaration(node.Parent);
    }
}
