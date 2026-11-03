using System.Text;

namespace Tabulate.Core;

public enum Alignment
{
    Left,
    Right,
    Center,
}

public enum TableStyle
{
    /// <summary>ASCII box: +---+, | a | b |.</summary>
    Ascii,

    /// <summary>GitHub-flavoured Markdown table.</summary>
    Markdown,
}

public sealed class TableOptions
{
    /// <summary>Treat the first row as a header. Default: true.</summary>
    public bool HasHeader { get; init; } = true;

    public TableStyle Style { get; init; } = TableStyle.Ascii;

    /// <summary>Per-column alignment. A shorter list is padded with Left; null means Left everywhere.</summary>
    public IReadOnlyList<Alignment>? Alignments { get; init; }

    /// <summary>Maximum width of any cell; longer cells are truncated with an ellipsis. Null means no limit.</summary>
    public int? MaxColumnWidth { get; init; }
}

/// <summary>Renders rows of cells into an aligned ASCII or Markdown table. Uses '\n' line endings.</summary>
public static class TableRenderer
{
    private const char Nl = '\n';

    public static string Render(IReadOnlyList<IReadOnlyList<string>> rows, TableOptions? options = null)
    {
        options ??= new TableOptions();
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        if (options.MaxColumnWidth is int max && max > 0)
        {
            rows = rows
                .Select(r => (IReadOnlyList<string>)r.Select(cell => Truncate(cell, max)).ToList())
                .ToList();
        }

        var columnCount = rows.Max(r => r.Count);
        var widths = new int[columnCount];
        for (var c = 0; c < columnCount; c++)
        {
            widths[c] = Math.Max(1, rows.Max(r => Cell(r, c).Length));
        }

        var align = new Alignment[columnCount];
        for (var c = 0; c < columnCount; c++)
        {
            align[c] = options.Alignments is not null && c < options.Alignments.Count ? options.Alignments[c] : Alignment.Left;
        }

        return options.Style == TableStyle.Markdown
            ? RenderMarkdown(rows, widths, align, options)
            : RenderAscii(rows, widths, align, options);
    }

    private static string RenderAscii(
        IReadOnlyList<IReadOnlyList<string>> rows, int[] widths, Alignment[] align, TableOptions options)
    {
        var sb = new StringBuilder();
        string Border() => "+" + string.Join("+", widths.Select(w => new string('-', w + 2))) + "+";

        sb.Append(Border()).Append(Nl);
        for (var r = 0; r < rows.Count; r++)
        {
            sb.Append("| ");
            for (var c = 0; c < widths.Length; c++)
            {
                sb.Append(Pad(Cell(rows[r], c), widths[c], align[c]));
                sb.Append(c == widths.Length - 1 ? " |" : " | ");
            }
            sb.Append(Nl);
            if (r == 0 && options.HasHeader)
            {
                sb.Append(Border()).Append(Nl);
            }
        }
        sb.Append(Border());
        return sb.ToString();
    }

    private static string RenderMarkdown(
        IReadOnlyList<IReadOnlyList<string>> rows, int[] widths, Alignment[] align, TableOptions options)
    {
        var sb = new StringBuilder();

        string Row(IReadOnlyList<string> cells) =>
            "| " + string.Join(" | ", widths.Select((w, c) => Pad(Cell(cells, c), w, align[c]))) + " |";

        var bodyStart = 0;
        if (options.HasHeader)
        {
            sb.Append(Row(rows[0])).Append(Nl);
            bodyStart = 1;
        }
        else
        {
            sb.Append("| " + string.Join(" | ", widths.Select(w => new string(' ', w))) + " |").Append(Nl);
        }

        sb.Append("| " + string.Join(" | ", widths.Select((w, c) => Separator(w, align[c]))) + " |").Append(Nl);

        for (var r = bodyStart; r < rows.Count; r++)
        {
            sb.Append(Row(rows[r])).Append(Nl);
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }

    private static string Separator(int width, Alignment align) => align switch
    {
        Alignment.Right => new string('-', Math.Max(1, width - 1)) + ":",
        Alignment.Center => ":" + new string('-', Math.Max(1, width - 2)) + ":",
        _ => new string('-', Math.Max(3, width)),
    };

    /// <summary>Shorten text to <paramref name="max"/> characters, using a trailing ellipsis.</summary>
    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : max <= 1 ? "…" : text[..(max - 1)] + "…";

    private static string Pad(string text, int width, Alignment align)
    {
        var slack = width - text.Length;
        if (slack <= 0)
        {
            return text;
        }

        return align switch
        {
            Alignment.Right => new string(' ', slack) + text,
            Alignment.Center => new string(' ', slack / 2) + text + new string(' ', slack - slack / 2),
            _ => text + new string(' ', slack),
        };
    }

    private static string Cell(IReadOnlyList<string> row, int column) => column < row.Count ? row[column] : string.Empty;
}
