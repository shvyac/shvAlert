using System;
using System.Collections.Generic;
using System.Text;

namespace shvAlert
{
    class CallSignRegExp
    {
        public string strRegExp { get; set; }    
        public string strCountry { get; set; } 

        /*
        ^[U][EH-I]?[0-9][A-Z]{3}$ Russia Cat4
        ^[RU].*1A[A-Z]*$ 1A	Saint Petersburg /Northwest Russia
        ^[RU].*1C[A-Z]*$ 1C	Leningrad /Northwest Russia
        ^[RU].*1D[A-Z]*$ 1D	Saint Petersburg /Northwest Russia
        */
    }
}
