using System.Text;

using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Extensions;

internal class RecordContext : IDisposable
{
    private readonly StringBuilder _code;

    public RecordContext(StringBuilder code, string recordName, params ITypeSymbol[] baseTypes)
    {
        _code = code;
        _code.AppendLine($"partial record {recordName}");

        for (var i = 0; i < baseTypes.Length; i++)
        {
            var type = baseTypes[i];
            _code.Append(i == 0
                             ? $"    : {type.ContainingNamespace}.{type.Name}"
                             : $"      {type.ContainingNamespace}.{type.Name}");

            if (i != baseTypes.Length - 1)
            {
                _code.Append(',');
            }

            _code.AppendLine();
        }

        _code.AppendLine("{");
    }

    public void Dispose()
    {
        _code.AppendLine("}");
    }
}
