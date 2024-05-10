using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikipedia_Seçim_Sonuçları.Classes
{
	public class Candidate
	{
		
			public object adaylik_TURU { get; set; }
			public object aday_SIRA_NO { get; set; }
			public object il_KODU { get; set; }
			public object il_ADI { get; set; }
			public object ilce_KODU { get; set; }	
			public string ilce_ADI { get; set; }
			public object belde_ADI { get; set; }
			public string adi_SOYADI { get; set; }
			public string secim_CEVRESI_ADI { get; set; }
			public object secilme_TURU { get; set; }
			public object secilme_SIRASI { get; set; }
			public object parti_RENK { get; set; }
			public string parti_KISA_ADI { get; set; }
			public string parti_ADI { get; set; }
		

	}
}
