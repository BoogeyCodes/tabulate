namespace Tabulate.Tests;

public sealed class CliTests
{
    private static (int Code, string Out, string Err) Run(string stdin, params string[] args)
    {
        var input = new StringReader(stdin);
        var outW = new StringWriter();
        var errW = new StringWriter();
        var code = Cli.Run(args, input, outW, errW);
        return (code, outW.ToString(), errW.ToString());
    }

    [Fact]
    public void Renders_stdin_as_an_ascii_table()
    {
        var (code, output, _) = Run("a,b\n1,2");
        Assert.Equal(0, code);
        Assert.Contains("| a | b |", output);
        Assert.Contains("+---+---+", output);
    }

    [Fact]
    public void Markdown_flag_produces_a_markdown_table()
    {
        var (code, output, _) = Run("a,b\n1,2", "--markdown");
        Assert.Equal(0, code);
        Assert.Contains("| a | b |", output);
        Assert.Contains("| --- | --- |", output);
    }

    [Fact]
    public void Tab_delimiter_reads_tsv()
    {
        var (code, output, _) = Run("a\tb\n1\t2", "-d", "tab");
        Assert.Equal(0, code);
        Assert.Contains("| a | b |", output);
    }

    [Fact]
    public void Alignment_spec_is_applied()
    {
        var (code, output, _) = Run("name,n\nabc,1", "-a", "l,r");
        Assert.Equal(0, code);
        Assert.Contains("| abc  | 1 |", output); // 'n' column right-aligned within width 1 header
    }

    [Fact]
    public void Help_exits_zero()
    {
        var (code, output, _) = Run("", "--help");
        Assert.Equal(0, code);
        Assert.Contains("Usage:", output);
    }

    [Fact]
    public void Unknown_option_is_a_usage_error()
    {
        var (code, _, err) = Run("a,b", "--bogus");
        Assert.Equal(2, code);
        Assert.Contains("unknown option", err);
    }

    [Fact]
    public void Missing_file_is_an_error()
    {
        var (code, _, err) = Run("", Path.Combine(Path.GetTempPath(), "no-such-file.csv"));
        Assert.Equal(2, code);
        Assert.Contains("tabulate:", err);
    }

    [Fact]
    public void Empty_input_is_an_error()
    {
        var (code, _, err) = Run("");
        Assert.Equal(2, code);
        Assert.Contains("no data", err);
    }
}
