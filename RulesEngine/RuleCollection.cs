using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace RulesEngine
{
    public interface IRuleCollection
    {
        /// <summary>
        /// Queries the rules looking for a match based on name or alias
        /// The first result will always be an exact match if one is found.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<SearchResult> Lookup(string query);
    }

    public class RuleCollection : IRuleCollection
    {
        private Entry[] _entries;
        public void LoadEntries(string path)
        {
            Entry[] results = JsonConvert.DeserializeObject<Entry[]>(File.ReadAllText(path));
            _entries = results;
        }

        public IEnumerable<SearchResult> Lookup(string query)
        {
            query = query.ToLower();
            Entry exactMatch = _entries.FirstOrDefault(e => e.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase));

            if (exactMatch != null)
            {
                yield return new SearchResult
                {
                    Entry = exactMatch,
                    ExactMatch = true
                };
            }

            //First pass, look for entries by name
            foreach (Entry entry in _entries.Where(e => e.Name.ToLower().Contains(query)))
            {
                if (entry == exactMatch)
                {
                    continue;
                }

                yield return new SearchResult
                {
                    Entry = entry,
                    ExactMatch = entry.Name.Equals(query,StringComparison.InvariantCultureIgnoreCase)
                };
            }


            foreach (Entry entry in _entries.Where(e=>e.Aliases.Any(a=>a.ToLower().Contains(query))))
            {
                yield return new SearchResult
                {
                    Entry = entry,
                    ExactMatch = false
                };
            }
        }
    }

    public class SearchResult
    {
        public bool ExactMatch { get; set; }
        public Entry Entry { get; set; }
    }
}
