using Tabulate.Core;

namespace Tabulate.Tests;

public sealed class CsvTests
{
    [Fact]
    public void Parses_simple_rows()
    {
        var rows = Csv.Parse("a,b,c\n1,2,3");
        Assert.Equal(2, rows.Count);
        Assert.Equal(["a", "b", "c"], rows[0]);
        Assert.Equal(["1", "2", "3"], rows[1]);
    }

    [Fact]
    public void Handles_quotes_delimiters_and_newlines()
    {
        var rows = Csv.Parse("name,note\n\"Smith, John\",\"a\nb\"");
        Assert.Equal(["Smith, John", "a\nb"], rows[1]);
    }

    [Fact]
    public void Unescapes_doubled_quotes()
    {
        var rows = Csv.Parse("\"she said \"\"hi\"\"\"");
        Assert.Equal(["she said \"hi\""], rows[0]);
    }

    [Fact]
    public void Supports_a_tab_delimiter_and_crlf()
    {
        var rows = Csv.Parse("a\tb\r\n1\t2\r\n", '\t');
        Assert.Equal(["a", "b"], rows[0]);
        Assert.Equal(["1", "2"], rows[1]);
    }

    [Fact]
    public void Preserves_empty_fields_and_skips_blank_lines()
    {
        var rows = Csv.Parse("a,,c\n\nd,e,f");
        Assert.Equal(["a", "", "c"], rows[0]);
        Assert.Equal(2, rows.Count);
    }
}
