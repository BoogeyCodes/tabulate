using Tabulate.Core;

return Cli.Run(args, Console.In, Console.Out, Console.Error);

public static class Cli
{
    private const string Usage = """
        tabulate - render CSV or TSV as an aligned ASCII or Markdown table.

        Usage:
          tabulate [file] [options]        reads stdin when no file is given

        Options:
          -d, --delimiter <char>   field delimiter (default ','; use 'tab' for TSV)
          -m, --markdown           output a Markdown table instead of ASCII
          --no-header              do not treat the first row as a header
          -a, --align <spec>       per-column alignment, e.g. l,r,c
          -h, --help               show this help

        Exit codes: 0 success, 2 usage or input error.
        """;

    public static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        if (args.Contains("-h") || args.Contains("--help"))
        {
            output.WriteLine(Usage);
            return 0;
        }

        var delimiter = ',';
        var style = TableStyle.Ascii;
        var hasHeader = true;
        IReadOnlyList<Alignment>? alignments = null;
        string? file = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-m" or "--markdown":
                    style = TableStyle.Markdown;
                    break;
                case "--no-header":
                    hasHeader = false;
                    break;
                case "-d" or "--delimiter":
                    if (++i >= args.Length)
                    {
                        return Fail(error, "missing value for --delimiter");
                    }
                    delimiter = args[i] is "tab" or "\\t" ? '\t' : args[i][0];
                    break;
                case "-a" or "--align":
                    if (++i >= args.Length || !TryParseAlignments(args[i], out alignments))
                    {
                        return Fail(error, "invalid --align spec (use letters l, r, c)");
                    }
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        return Fail(error, $"unknown option '{arg}'");
                    }
                    file = arg;
                    break;
            }
        }

        string text;
        try
        {
            text = file is null ? input.ReadToEnd() : File.ReadAllText(file);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Fail(error, ex.Message);
        }

        var rows = Csv.Parse(text, delimiter);
        if (rows.Count == 0)
        {
            return Fail(error, "no data to render");
        }

        var table = TableRenderer.Render(
            rows,
            new TableOptions { HasHeader = hasHeader, Style = style, Alignments = alignments });
        output.WriteLine(table);
        return 0;
    }

    private static bool TryParseAlignments(string spec, out IReadOnlyList<Alignment>? alignments)
    {
        var result = new List<Alignment>();
        foreach (var token in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (token.ToLowerInvariant())
            {
                case "l" or "left":
                    result.Add(Alignment.Left);
                    break;
                case "r" or "right":
                    result.Add(Alignment.Right);
                    break;
                case "c" or "center" or "centre":
                    result.Add(Alignment.Center);
                    break;
                default:
                    alignments = null;
                    return false;
            }
        }

        alignments = result;
        return result.Count > 0;
    }

    private static int Fail(TextWriter error, string message)
    {
        error.WriteLine($"tabulate: {message}");
        return 2;
    }
}
