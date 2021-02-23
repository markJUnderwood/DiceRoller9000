using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceRoller9000Bot.Utilities
{
    public static class StringExtensions
    {
        public static string TruncateForDisplay(this string value, int length, string replacement = " ...")
        {
            length = length - replacement.Length;
            if (string.IsNullOrEmpty(value)) return string.Empty;
            string returnValue = value;
            if (value.Length > length)
            {
                string tmp = value.Substring(0, length) ;
                if (tmp.LastIndexOf(' ') > 0)
                {
                    returnValue = $"{tmp.Substring(0, tmp.LastIndexOf(' '))} {replacement}";
                }
            }                
            return returnValue;
        }
    }
}
