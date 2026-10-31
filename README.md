# tabulate

[![CI](https://github.com/BoogeyCodes/tabulate/actions/workflows/ci.yml/badge.svg)](https://github.com/BoogeyCodes/tabulate/actions/workflows/ci.yml)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

A .NET command-line tool that turns CSV or TSV into an aligned **ASCII** or **Markdown** table. Pipe
data in, get a readable table out.

```console
$ printf 'Name,Age,City\nAlice,30,Lahore\nBob,7,Karachi\n' | tabulate -a l,r,l
+-------+-----+---------+
| Name  | Age | City    |
+-------+-----+---------+
| Alice |  30 | Lahore  |
| Bob   |   7 | Karachi |
+-------+-----+---------+
```

```console
$ cat data.csv | tabulate --markdown -a l,r,c
| Name  | Age |  City   |
| ----- | --: | :-----: |
| Alice |  30 | Lahore  |
| Bob   |   7 | Karachi |
```

## Why

Raw CSV is unreadable in a terminal, and pasting data into a Markdown document means hand-aligning
pipes. tabulate does both: it parses the CSV properly (quoted fields, embedded commas and newlines),
measures each column, and prints a correctly aligned table — as an ASCII box for the terminal or as a
GitHub-flavoured Markdown table for a document, with per-column alignment.

## Usage

```
tabulate [file] [options]        reads stdin when no file is given

  -d, --delimiter <char>   field delimiter (default ','; use 'tab' for TSV)
  -m, --markdown           output a Markdown table instead of ASCII
  --no-header              do not treat the first row as a header
  -a, --align <spec>       per-column alignment, e.g. l,r,c
  -h, --help               show help
```

- **Alignment** is a comma-separated list of `l` (left), `r` (right) or `c` (center), one per column;
  columns beyond the list default to left. In Markdown mode the alignment is encoded with the
  standard `:---`, `---:`, `:---:` separators.
- **Exit codes**: `0` on success, `2` on a usage or input error.

```bash
tabulate report.csv                  # ASCII table from a file
cat report.tsv | tabulate -d tab     # TSV from stdin
tabulate data.csv -m > table.md      # Markdown to a file
```

## How it works

The project is split into a core library and a thin CLI:

- `Csv.Parse` is a small RFC 4180 reader: quoted fields may contain the delimiter, newlines and `""`
  escapes, and both `\n` and `\r\n` endings are accepted.
- `TableRenderer.Render` measures column widths, pads each cell to the chosen alignment, and emits the
  ASCII or Markdown form. It uses `\n` endings so output is identical on every platform.
- The CLI parses arguments, reads the file or stdin, and prints the table.

## Testing

```bash
dotnet test
```

Nineteen tests: the CSV reader (quotes, delimiters, newlines, escapes, empty fields), the renderer
(alignment, borders, the Markdown separator row, header handling, ragged rows, empty input), and the
CLI end to end (ASCII and Markdown output, TSV, alignment, help, and every error path). CI runs on
Linux and Windows.

## Project layout

```
src/Tabulate.Core
├── Csv.cs             RFC 4180 CSV reader
└── TableRenderer.cs   ASCII and Markdown rendering
src/Tabulate           the CLI (argument parsing, I/O, exit codes)
tests/Tabulate.Tests   xUnit tests for the reader, renderer and CLI
```

## Ideas for later

- Reading column headers from a `--header a,b,c` flag.
- Unicode-width awareness for wide (CJK) characters.
- A `--max-width` that wraps or truncates long cells.

## License

MIT
