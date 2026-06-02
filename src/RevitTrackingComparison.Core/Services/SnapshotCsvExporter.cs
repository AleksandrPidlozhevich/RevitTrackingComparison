using System.Globalization;
using System.Text;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Services;

/// <summary>
/// Writes a <see cref="DocumentSnapshot"/> to CSV (UTF-8 with BOM for Excel). One row per element;
/// parameter names become additional columns (union of all captured parameters).
/// </summary>
public static class SnapshotCsvExporter
{
    private static readonly string[] FixedColumns = ["Category", "ElementId", "UniqueId", "Name"];

    public static void ExportToFile(DocumentSnapshot snapshot, string filePath)
    {
        using var writer = new StreamWriter(filePath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        Export(snapshot, writer);
    }

    public static void Export(DocumentSnapshot snapshot, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(writer);

        var parameterColumns = snapshot.Elements
            .SelectMany(e => e.Parameters.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        WriteRow(writer, FixedColumns.Concat(parameterColumns));

        foreach (var element in snapshot.Elements.OrderBy(e => e.Category, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(e => e.ElementId))
        {
            var values = new List<string>(FixedColumns.Length + parameterColumns.Count)
            {
                element.Category,
                element.ElementId.ToString(CultureInfo.InvariantCulture),
                element.UniqueId,
                element.Name
            };

            foreach (var column in parameterColumns)
                values.Add(element.Parameters.TryGetValue(column, out var value) ? value : string.Empty);

            WriteRow(writer, values);
        }
    }

    private static void WriteRow(TextWriter writer, IEnumerable<string> fields)
    {
        writer.WriteLine(string.Join(",", fields.Select(Escape)));
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n');
        if (!needsQuotes)
            return value;

        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
