using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mppt.LiedPresentator
{
    public class LiedFormatResult
    {
        public string Naam { get; set; }
        public string SubNaam { get; set; }
        public string Verzen { get; set; }
        public string Display { get
            {
                var returnValue = new StringBuilder(Naam);
                if (!string.IsNullOrWhiteSpace(SubNaam))
                    returnValue.Append($" {SubNaam}");
                if (!string.IsNullOrWhiteSpace(Verzen))
                    returnValue.Append($": {Verzen}");
                return returnValue.ToString();
            }
        }
        public override string ToString()
        {
            return Display;
        }
    }
}
