using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngine
{
    public class TableRenderer
    {
        private int _width;
        private List<string> _header;
        private readonly List<List<string>> _table = new List<List<string>>();

        private char _empty = ' ';

        public TableRenderer SetHeader(IEnumerable<string> header)
        {
            _header = header.ToList();
            if (_header.Count > _width)
            {
                _width = _header.Count;
            }

            return this;
        }

        public TableRenderer AddRow(IEnumerable<string> row)
        {
            List<string> rowList = row.ToList();
            _table.Add(rowList);
            if (rowList.Count > _width)
            {
                _width = rowList.Count;
            }

            return this;
        }

        public TableRenderer SetEmptyString(char character)
        {
            _empty = character;
            return this;
        }

        private string[][] NormalizeTable()
        {
            int height = _header == null ? _table.Count : _table.Count + 1;
            var normalized = new string[height][];

            for (var i = 0; i < height; i++)
            {
                normalized[i] = new string[_width];
            }

            var vIndex = 0;

            if (_header != null)
            {
                for (var hIndex = 0; hIndex < _width; hIndex++)
                {
                    if (_header.Count > hIndex)
                    {
                        normalized[vIndex][hIndex] = _header[hIndex];
                    }
                    else
                    {
                        normalized[vIndex][hIndex] = _empty.ToString();
                    }
                }

                vIndex++;
            }

            foreach (List<string> list in _table)
            {
                for (var hIndex = 0; hIndex < _width; hIndex++)
                {
                    if (list.Count > hIndex)
                    {
                        normalized[vIndex][hIndex] = list[hIndex];
                    }
                    else
                    {
                        normalized[vIndex][hIndex] = $"{_empty}s";
                    }
                }

                vIndex++;
            }

            return normalized;
        }

        private int[] GetColumnWidths(IEnumerable<string[]> table, int padding)
        {
            var columns = new int[_width];
            foreach (string[] row in table)
            {
                for (var hIndex = 0; hIndex < _width; hIndex++)
                {
                    if (row[hIndex].Length + padding > columns[hIndex])
                    {
                        columns[hIndex] = row[hIndex].Length + padding;
                    }
                }
            }

            columns[^1] -= padding;
            return columns;
        }

        private static string BuildElement(string element, int width, char emptyCharacter)
        {
            string result = element;

            if (result.Length < width)
            {
                result = result.PadRight(width, emptyCharacter);
            }

            return result;
        }

        private string BuildLine(IReadOnlyList<string> strings, IReadOnlyList<int> widths, bool header)
        {
            string line = string.Join('\uFE31', Enumerable.Range(0, strings.Count).Select(i => BuildElement(strings[i], widths[i], _empty)));

            if (header)
            {
                string separator = string.Join('\u2015', Enumerable.Range(0, strings.Count).Select(i => BuildElement("", widths[i], '\u2015')));
                line += '\n' + separator;
            }

            return line;
        }

        public string Build()
        {
            string[][] table = NormalizeTable();
            int[] widths = GetColumnWidths(table, 1);
            return string.Join('\n', Enumerable.Range(0, table.Length).Select(i => BuildLine(table[i], widths, _header != null && i == 0)));
        }
    }
}