using System.Text;

namespace Tabulate.Core;

/// <summary>Minimal RFC 4180 CSV reader: quoted fields may contain the delimiter, newlines and "" escapes.</summary>
public static class Csv
{
    public static List<List<string>> Parse(string input, char delimiter = ',')
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var sawContent = false;

        void EndField()
        {
            row.Add(field.ToString());
            field.Clear();
        }

        void EndRow()
        {
            EndField();
            if (!(row.Count == 1 && row[0].Length == 0))
            {
                rows.Add(row);
            }
            row = new List<string>();
            sawContent = false;
        }

        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < input.Length && input[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(ch);
                }
                continue;
            }

            if (ch == '"' && field.Length == 0)
            {
                inQuotes = true;
                sawContent = true;
            }
            else if (ch == delimiter)
            {
                EndField();
                sawContent = true;
            }
            else if (ch == '\n')
            {
                EndRow();
            }
            else if (ch == '\r')
            {
                if (i + 1 < input.Length && input[i + 1] == '\n')
                {
                    i++;
                }
                EndRow();
            }
            else
            {
                field.Append(ch);
                sawContent = true;
            }
        }

        if (sawContent || field.Length > 0 || row.Count > 0)
        {
            EndRow();
        }

        return rows;
    }
}
