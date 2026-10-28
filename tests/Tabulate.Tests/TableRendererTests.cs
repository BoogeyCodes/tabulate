using Tabulate.Core;

namespace Tabulate.Tests;

public sealed class TableRendererTests
{
    private static readonly List<List<string>> Sample =
    [
        ["Name", "Age", "City"],
        ["Alice", "30", "Lahore"],
        ["Bob", "7", "Karachi"],
    ];

    [Fact]
    public void Ascii_table_is_aligned_and_bordered()
    {
        var table = TableRenderer.Render(Sample);
        var lines = table.Split('\n');

        Assert.Equal("+-------+-----+---------+", lines[0]);
        Assert.Equal("| Name  | Age | City    |", lines[1]);
        Assert.Equal("+-------+-----+---------+", lines[2]); // header separator
        Assert.Equal("| Alice | 30  | Lahore  |", lines[3]);
        Assert.Equal("| Bob   | 7   | Karachi |", lines[4]);
        Assert.Equal("+-------+-----+---------+", lines[5]);
    }

    [Fact]
    public void Right_alignment_pads_on_the_left()
    {
        var options = new TableOptions { Alignments = [Alignment.Left, Alignment.Right, Alignment.Left] };
        var table = TableRenderer.Render(Sample, options);
        Assert.Contains("| Alice |  30 | Lahore  |", table);
        Assert.Contains("| Bob   |   7 | Karachi |", table);
    }

    [Fact]
    public void Markdown_table_has_a_separator_row_with_alignment_colons()
    {
        var options = new TableOptions
        {
            Style = TableStyle.Markdown,
            Alignments = [Alignment.Left, Alignment.Right, Alignment.Center],
        };
        var lines = TableRenderer.Render(Sample, options).Split('\n');

        Assert.Equal("| Name  | Age |  City   |", lines[0]);
        Assert.Equal("| ----- | --: | :-----: |", lines[1]);
        Assert.Equal("| Alice |  30 | Lahore  |", lines[2]);
    }

    [Fact]
    public void Without_a_header_there_is_no_inner_separator()
    {
        var table = TableRenderer.Render(Sample, new TableOptions { HasHeader = false });
        // Only the top and bottom borders, no separator after the first row.
        Assert.Equal(2, table.Split('\n').Count(l => l.StartsWith('+')));
    }

    [Fact]
    public void Ragged_rows_are_padded_to_the_widest()
    {
        var rows = new List<List<string>> { new() { "a", "b", "c" }, new() { "1" } };
        var table = TableRenderer.Render(rows, new TableOptions { HasHeader = false });
        Assert.Contains("| 1 |   |   |", table);
    }

    [Fact]
    public void Empty_input_renders_nothing()
    {
        Assert.Equal(string.Empty, TableRenderer.Render([]));
    }
}
