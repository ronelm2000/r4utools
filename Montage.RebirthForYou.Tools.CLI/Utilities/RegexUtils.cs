using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public class RegexUtils
    {
        public Regex CreateBalancedRegexQuotedText(string beginningQuotes, string endingQuotes)
        {
            return new Regex(string.Format(@"^
              {0}                       # Match first opeing delimiter
              (?<inner>
                (?>
                    {0} (?<LEVEL>)      # On opening delimiter push level
                  | 
                    {1} (?<-LEVEL>)     # On closing delimiter pop level
                  |
                    (?! {0} | {1} ) .   # Match any char unless the opening   
                )+                      # or closing delimiters are in the lookahead string
                (?(LEVEL)(?!))          # If level exists then fail
              )
              {1}                       # Match last closing delimiter
              $", beginningQuotes, endingQuotes), RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
        }
    }
}
