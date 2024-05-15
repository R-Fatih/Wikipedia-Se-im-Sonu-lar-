using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia_Seçim_Sonuçları.Classes
{
	public class MergedData
	{
        public string Candidate { get; set; }
        public string Party { get; set; }
        public string Province { get; set; }
        public string PartyFullName { get; set; }
        public int Votes { get; set; }
        public double Percentage { get; set; }
        public double TotalVotesInCity { get; set; }


    }
}
