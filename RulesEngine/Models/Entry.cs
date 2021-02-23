using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.Models
{
    public class Entry
    {
        public string Name { get; set; }
        public EntryType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public string[] Aliases { get; set; } = new string[0];
        public Table[] Tables { get; set; } = new Table[0];
        public string[] Examples { get; set; } = new string[0];
        public string Note { get; set; } = string.Empty;
        public string Calling { get; set; } = string.Empty;
        public string InnatePower { get; set; } = string.Empty;
    }
}
