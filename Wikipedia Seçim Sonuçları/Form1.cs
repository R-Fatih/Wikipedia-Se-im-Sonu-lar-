using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wikipedia_Seçim_Sonuçları.Classes;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace Wikipedia_Seçim_Sonuçları
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string urlTitle, urlCandidate, urlResult, urlProvinces, bottomData;
        List<MergedData> mergedDatas2 = new List<MergedData>();
        List<MergedData> mergedDatas = new List<MergedData>();
        int countIndependent = 1;
        int count = 0;
        private async void button1_Click(object sender, EventArgs e)
        {
            //await MetropolitanData();
            //await Province();
            //await Town();
            await MetropolitanDataByProvinces();
        }

        private async Task MetropolitanData()
        {
            richTextBox1.Clear();
            count = 0;
            countIndependent = 1;
            mergedDatas.Clear();

            urlTitle = $"https://acikveri.ysk.gov.tr/api/getSandikSecimSonucBaslikList?secimId=20260&secimCevresiId=&ilId=&bagimsiz=1&secimTuru=6&yurtIciDisi=1";
            urlCandidate = $"https://acikveri.ysk.gov.tr/api/getBuyuksehirBelediyeBaskanligiAdayListesi?secimId=20260&ilId={textBox1.Text}";
            urlResult = $"https://acikveri.ysk.gov.tr/api/getSecimSandikSonucList?secimId=20260&secimTuru=6&ilId=&ilceId=&beldeId=&birimId=&muhtarlikId=&cezaeviId=&sandikTuru=&sandikNoIlk=&sandikNoSon=&ulkeId=&disTemsilcilikId=&gumrukId=&yurtIciDisi=1&sandikRumuzIlk=&sandikRumuzSon=&secimCevresiId=&sandikId=&sorguTuru=2";
            HttpClient client = new HttpClient();
            var titleData = await client.GetFromJsonAsync<List<TitleOfParty>>(urlTitle);
            var candidateData = await client.GetFromJsonAsync<List<Candidate>>(urlCandidate);
            var resultData = await client.GetFromJsonAsync<List<Result>>(urlResult);
            TextInfo textInfo = new CultureInfo("tr", false).TextInfo;
            // Verileri oy oranına göre sırala


            foreach (var item in candidateData)
            {

                var party = titleData.FirstOrDefault(x => x.ad == item.parti_KISA_ADI);
                var filteredResults = resultData.Where(x => x.il_ADI == item.secim_CEVRESI_ADI).FirstOrDefault();
                if (item.parti_KISA_ADI == "BAĞIMSIZ")
                {
                    party = new TitleOfParty
                    {
                        ad = "BAĞIMSIZ",
                        column_NAME = $"bagimsiz{countIndependent}_ALDIGI_OY"
                    };
                    countIndependent++;
                }
                var vote = Convert.ToInt32(GetPropertyDynamically(filteredResults, party.column_NAME));

                mergedDatas.Add(new MergedData
                {
                    Candidate = item.adi_SOYADI.ToLower(),
                    PartyFullName = textInfo.ToTitleCase(item.parti_ADI.ToLower()).Replace(" Ve ", " ve "),
                    Party = textInfo.ToTitleCase(item.parti_ADI.ToLower())
                    .Replace("İyi Parti", "İYİ Parti")
                    .Replace(" Ve ", " ve ")
                    .Replace("Türkiye Komünist Partisi", "Türkiye Komünist Partisi (2001)")
                    .Replace("Yeni Türkiye Partisi", "Yeni Türkiye Partisi (2023)")
                    .Replace("Vatan Partisi", "Vatan Partisi (2015)")
                    .Replace("Millet Partisi", "Millet Partisi (1992)")
                    .Replace("Adalet Partisi", "Adalet Partisi (2015)")
                                                .Replace("Demokrat Parti", "Demokrat Parti (2007)")
                            .Replace("Türkiye İşçi Partisi", "Türkiye İşçi Partisi (2017)")
                    ,
                    Votes = vote,
                    Percentage = Math.Round(((double)vote / filteredResults.gecerli_OY_TOPLAMI * 100), 2)

                });
                bottomData = $"|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | '''Toplam'''\r\n| {filteredResults.gecerli_OY_TOPLAMI:N0}\r\n| %100\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Geçersiz ya da boş''\r\n|  align=\"center\" colspan=\"5\" | {filteredResults.gecersiz_OY_TOPLAMI.ToString("N0")}\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Katılım oranı''\r\n| align=\"center\" colspan=\"5\" |  %{Math.Round(((double)filteredResults.oy_KULLANAN_SECMEN_SAYISI / filteredResults.secmen_SAYISI) * 100, 2).ToString()}";

            }
            mergedDatas = mergedDatas.OrderByDescending(x => x.Votes).ToList();
            mergedDatas.ForEach(x =>
            {
                richTextBox1.AppendText($"{{{{Siyasi parti haritarenk|{x.Party}|{textInfo.ToTitleCase(x.Candidate)}|{x.Votes.ToString("N0")}|%{x.Percentage.ToString()}{(count == 0 ? "|1" : "|0")}{(x.Party != x.PartyFullName ? "|" + x.PartyFullName + "}}" : "}}")}\n");
                count++;
            });
            richTextBox1.AppendText(bottomData);
            File.WriteAllText(textBox1.Text + ".txt", richTextBox1.Text);


        }
        private async Task<List<MergedData>> MetropolitanDat()
        {
            richTextBox1.Clear();
            count = 0;
            countIndependent = 1;
            mergedDatas2.Clear();

            urlTitle = $"https://acikveri.ysk.gov.tr/api/getSandikSecimSonucBaslikList?secimId=20260&secimCevresiId=&ilId=&bagimsiz=1&secimTuru=6&yurtIciDisi=1";
            urlCandidate = $"https://acikveri.ysk.gov.tr/api/getBuyuksehirBelediyeBaskanligiAdayListesi?secimId=20260&ilId={textBox1.Text}";
            urlResult = $"https://acikveri.ysk.gov.tr/api/getSecimSandikSonucList?secimId=20260&secimTuru=6&ilId=&ilceId=&beldeId=&birimId=&muhtarlikId=&cezaeviId=&sandikTuru=&sandikNoIlk=&sandikNoSon=&ulkeId=&disTemsilcilikId=&gumrukId=&yurtIciDisi=1&sandikRumuzIlk=&sandikRumuzSon=&secimCevresiId=&sandikId=&sorguTuru=2";
            HttpClient client = new HttpClient();
            var titleData = await client.GetFromJsonAsync<List<TitleOfParty>>(urlTitle);
            var candidateData = await client.GetFromJsonAsync<List<Candidate>>(urlCandidate);
            var resultData = await client.GetFromJsonAsync<List<Result>>(urlResult);
            TextInfo textInfo = new CultureInfo("tr", false).TextInfo;
            // Verileri oy oranına göre sırala


            foreach (var item in candidateData)
            {

                var party = titleData.FirstOrDefault(x => x.ad == item.parti_KISA_ADI);
                var filteredResults = resultData.Where(x => x.il_ADI == item.secim_CEVRESI_ADI).FirstOrDefault();
                if (item.parti_KISA_ADI == "BAĞIMSIZ")
                {
                    party = new TitleOfParty
                    {
                        ad = "BAĞIMSIZ",
                        column_NAME = $"bagimsiz{countIndependent}_ALDIGI_OY"
                    };
                    countIndependent++;
                }
                var vote = Convert.ToInt32(GetPropertyDynamically(filteredResults, party.column_NAME));

                mergedDatas2.Add(new MergedData
                {
                    Candidate = item.adi_SOYADI.ToLower(),
                    PartyFullName = textInfo.ToTitleCase(item.parti_ADI.ToLower()).Replace(" Ve ", " ve "),
                    Party = textInfo.ToTitleCase(item.parti_ADI.ToLower())
                    .Replace("İyi Parti", "İYİ Parti")
                    .Replace(" Ve ", " ve ")
                    .Replace("Türkiye Komünist Partisi", "Türkiye Komünist Partisi (2001)")
                    .Replace("Yeni Türkiye Partisi", "Yeni Türkiye Partisi (2023)")
                    .Replace("Vatan Partisi", "Vatan Partisi (2015)")
                    .Replace("Millet Partisi", "Millet Partisi (1992)")
                    .Replace("Adalet Partisi", "Adalet Partisi (2015)")
                                                .Replace("Demokrat Parti", "Demokrat Parti (2007)")
                            .Replace("Türkiye İşçi Partisi", "Türkiye İşçi Partisi (2017)")
                    ,
                    Votes = vote,
                    Percentage = Math.Round(((double)vote / filteredResults.gecerli_OY_TOPLAMI * 100), 2)

                });
                bottomData = $"|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | '''Toplam'''\r\n| {filteredResults.gecerli_OY_TOPLAMI:N0}\r\n| %100\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Geçersiz ya da boş''\r\n|  align=\"center\" colspan=\"5\" | {filteredResults.gecersiz_OY_TOPLAMI.ToString("N0")}\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Katılım oranı''\r\n| align=\"center\" colspan=\"5\" |  %{Math.Round(((double)filteredResults.oy_KULLANAN_SECMEN_SAYISI / filteredResults.secmen_SAYISI) * 100, 2).ToString()}";

            }
            mergedDatas2 = mergedDatas2.OrderByDescending(x => x.Votes).ToList();
            return mergedDatas2;


        }
        private async Task Province()
        {
            for (int i = 60; i < 82; i++)
            {

                urlProvinces = $"https://sonuc.ysk.gov.tr/api/getIlceList?secimId=60882&secimTuru=2&ilId={i}&secimCevresiId=0&sandikTuru=-1&yurtIciDisi=1";
                urlCandidate = $"https://acikveri.ysk.gov.tr/api/getIlceBelediyeBaskanligiAdayListesi?secimId=20260&ilId={i}";
                urlResult = $"https://acikveri.ysk.gov.tr/api/getSecimSandikSonucList?secimId=20260&secimTuru=2&ilId={i}&ilceId=&beldeId=&birimId=&muhtarlikId=&cezaeviId=&sandikTuru=&sandikNoIlk=&sandikNoSon=&ulkeId=&disTemsilcilikId=&gumrukId=&yurtIciDisi=1&sandikRumuzIlk=&sandikRumuzSon=&secimCevresiId=&sandikId=&sorguTuru=";

                HttpClient client = new HttpClient();
                var provinceData = await client.GetFromJsonAsync<List<Province>>(urlProvinces);
                var candidateData = await client.GetFromJsonAsync<List<Candidate>>(urlCandidate);
                var resultData = await client.GetFromJsonAsync<List<Result>>(urlResult);
                TextInfo textInfo = new CultureInfo("tr", false).TextInfo;
                // Verileri oy oranına göre sırala

                foreach (var province in provinceData)
                {
                    richTextBox2.Clear();
                    count = 0;
                    countIndependent = 1;
                    mergedDatas.Clear();
                    urlTitle = $"https://sonuc.ysk.gov.tr/api/getSandikSecimSonucBaslikList?secimId=60882&secimCevresiId={province.secim_CEVRESI_ID}&ilId={i}&bagimsiz=1&secimTuru=2&yurtIciDisi=1";
                    var titleData = await client.GetFromJsonAsync<List<TitleOfParty>>(urlTitle);

                    foreach (var item in candidateData.Where(x => x.ilce_ADI == province.ilce_ADI))
                    {


                        var party = titleData.FirstOrDefault(x => x.ad == item.parti_KISA_ADI);
                        var filteredResults = resultData.Where(x => x.ilce_ADI.ToString() == item.ilce_ADI.ToString()).FirstOrDefault();
                        if (item.parti_KISA_ADI == "BAĞIMSIZ")
                        {
                            party = new TitleOfParty
                            {
                                ad = "BAĞIMSIZ",
                                column_NAME = $"bagimsiz{countIndependent}_ALDIGI_OY"
                            };
                            countIndependent++;
                        }
                        var vote = Convert.ToInt32(GetPropertyDynamically(filteredResults, party.column_NAME));

                        mergedDatas.Add(new MergedData
                        {
                            Candidate = item.adi_SOYADI.ToLower(),
                            PartyFullName = textInfo.ToTitleCase(item.parti_ADI.ToLower()).Replace(" Ve ", " ve "),
                            Party = textInfo.ToTitleCase(item.parti_ADI.ToLower())
                            .Replace("İyi Parti", "İYİ Parti")
                            .Replace(" Ve ", " ve ")
                            .Replace("Türkiye Komünist Partisi", "Türkiye Komünist Partisi (2001)")
                            .Replace("Yeni Türkiye Partisi", "Yeni Türkiye Partisi (2023)")
                            .Replace("Vatan Partisi", "Vatan Partisi (2015)")
                            .Replace("Millet Partisi", "Millet Partisi (1992)")
                            .Replace("Adalet Partisi", "Adalet Partisi (2015)")
                            .Replace("Demokrat Parti", "Demokrat Parti (2007)")
                            .Replace("Türkiye İşçi Partisi", "Türkiye İşçi Partisi (2017)")
                            .Replace("Sol Parti", "Sol Parti (Türkiye)")
                            ,
                            Votes = vote,
                            Percentage = Math.Round(((double)vote / filteredResults.gecerli_OY_TOPLAMI * 100), 2)

                        });
                        bottomData = $"|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | '''Toplam'''\r\n| {filteredResults.gecerli_OY_TOPLAMI:N0}\r\n| %100\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Geçersiz ya da boş''\r\n|  align=\"center\" colspan=\"5\" | {filteredResults.gecersiz_OY_TOPLAMI.ToString("N0")}\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Katılım oranı''\r\n| align=\"center\" colspan=\"5\" |  %{Math.Round(((double)filteredResults.oy_KULLANAN_SECMEN_SAYISI / filteredResults.secmen_SAYISI) * 100, 2).ToString()}";

                    }

                    mergedDatas = mergedDatas.OrderByDescending(x => x.Votes).ToList();
                    mergedDatas.ForEach(x =>
                    {
                        richTextBox2.AppendText($"{{{{Siyasi parti haritarenk|{x.Party}|{textInfo.ToTitleCase(x.Candidate)}|{x.Votes.ToString("N0")}|%{x.Percentage.ToString()}{(count == 0 ? "|1" : "|0")}{(x.Party != x.PartyFullName ? "|" + x.PartyFullName + "}}" : "}}")}\n");
                        count++;
                    });
                    richTextBox2.AppendText(bottomData);
                    File.WriteAllText(province.il_ADI + "-" + province.ilce_ADI + ".txt", richTextBox2.Text);
                }

            }
        }

        private async Task Town()
        {
            for (int i = 67; i < 68; i++)
            {

                urlProvinces = $"https://sonuc.ysk.gov.tr/api/getIlceList?secimId=60882&secimTuru=2&ilId={i}&secimCevresiId=0&sandikTuru=-1&yurtIciDisi=1";
                urlCandidate = $"https://acikveri.ysk.gov.tr/api/getBeldeBelediyeBaskanligiAdayListesi?secimId=20260&ilId={i}";
                urlResult = $"https://acikveri.ysk.gov.tr/api/getSecimSandikSonucList?secimId=20260&secimTuru=2&ilId={i}&ilceId=&beldeId=&birimId=&muhtarlikId=&cezaeviId=&sandikTuru=&sandikNoIlk=&sandikNoSon=&ulkeId=&disTemsilcilikId=&gumrukId=&yurtIciDisi=1&sandikRumuzIlk=&sandikRumuzSon=&secimCevresiId=&sandikId=&sorguTuru=";

                HttpClient client = new HttpClient();
                var provinceData = await client.GetFromJsonAsync<List<Province>>(urlProvinces);
                var candidateData = await client.GetFromJsonAsync<List<Candidate>>(urlCandidate);
                var resultData = await client.GetFromJsonAsync<List<Result>>(urlResult);
                TextInfo textInfo = new CultureInfo("tr", false).TextInfo;
                // Verileri oy oranına göre sırala

                foreach (var province in provinceData.Where(x => x.belde_ID != 0))
                {
                    richTextBox2.Clear();
                    count = 0;
                    countIndependent = 1;
                    mergedDatas.Clear();
                    urlTitle = $"https://sonuc.ysk.gov.tr/api/getSandikSecimSonucBaslikList?secimId=60882&secimCevresiId={province.secim_CEVRESI_ID}&ilId={i}&bagimsiz=1&secimTuru=2&yurtIciDisi=1";
                    var titleData = await client.GetFromJsonAsync<List<TitleOfParty>>(urlTitle);

                    foreach (var item in candidateData.Where(x => x.ilce_ADI + " " + x.belde_ADI == province.ilce_ADI))
                    {


                        var party = titleData.FirstOrDefault(x => x.ad == item.parti_KISA_ADI);
                        var filteredResults = resultData.Where(x => x.ilce_ADI.ToString() == item.ilce_ADI + " - " + item.belde_ADI).FirstOrDefault();
                        if (item.parti_KISA_ADI == "BAĞIMSIZ")
                        {
                            party = new TitleOfParty
                            {
                                ad = "BAĞIMSIZ",
                                column_NAME = $"bagimsiz{countIndependent}_ALDIGI_OY"
                            };
                            countIndependent++;
                        }
                        var vote = Convert.ToInt32(GetPropertyDynamically(filteredResults, party.column_NAME));

                        mergedDatas.Add(new MergedData
                        {
                            Candidate = item.adi_SOYADI.ToLower(),
                            PartyFullName = textInfo.ToTitleCase(item.parti_ADI.ToLower()).Replace(" Ve ", " ve "),
                            Party = textInfo.ToTitleCase(item.parti_ADI.ToLower())
                            .Replace("İyi Parti", "İYİ Parti")
                            .Replace(" Ve ", " ve ")
                            .Replace("Türkiye Komünist Partisi", "Türkiye Komünist Partisi (2001)")
                            .Replace("Yeni Türkiye Partisi", "Yeni Türkiye Partisi (2023)")
                            .Replace("Vatan Partisi", "Vatan Partisi (2015)")
                            .Replace("Millet Partisi", "Millet Partisi (1992)")
                            .Replace("Adalet Partisi", "Adalet Partisi (2015)")
                            .Replace("Demokrat Parti", "Demokrat Parti (2007)")
                            .Replace("Türkiye İşçi Partisi", "Türkiye İşçi Partisi (2017)")
                            ,
                            Votes = vote,
                            Percentage = Math.Round(((double)vote / filteredResults.gecerli_OY_TOPLAMI * 100), 2)

                        });
                        bottomData = $"|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | '''Toplam'''\r\n| {filteredResults.gecerli_OY_TOPLAMI:N0}\r\n| %100\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Geçersiz ya da boş''\r\n|  align=\"center\" colspan=\"5\" | {filteredResults.gecersiz_OY_TOPLAMI.ToString("N0")}\r\n|- style=\"background-color:#E9E9E9\" align=\"left\"\r\n! align=\"center\" colspan=\"4\" | ''Katılım oranı''\r\n| align=\"center\" colspan=\"5\" |  %{Math.Round(((double)filteredResults.oy_KULLANAN_SECMEN_SAYISI / filteredResults.secmen_SAYISI) * 100, 2).ToString()}";

                    }

                    mergedDatas = mergedDatas.OrderByDescending(x => x.Votes).ToList();
                    mergedDatas.ForEach(x =>
                    {
                        richTextBox2.AppendText($"{{{{Siyasi parti haritarenk|{x.Party}|{textInfo.ToTitleCase(x.Candidate)}|{x.Votes.ToString("N0")}|%{x.Percentage.ToString()}{(count == 0 ? "|1" : "|0")}{(x.Party != x.PartyFullName ? "|" + x.PartyFullName + "}}" : "}}")}\n");
                        count++;
                    });
                    richTextBox2.AppendText(bottomData);
                    File.WriteAllText(province.il_ADI + "-" + province.ilce_ADI + ".txt", richTextBox2.Text);
                }

            }
        }
        private async Task MetropolitanDataByProvinces()
        {
            richTextBox1.Clear();
            count = 0;
            countIndependent = 1;
            mergedDatas.Clear();

            urlTitle = $"https://acikveri.ysk.gov.tr/api/getSandikSecimSonucBaslikList?secimId=20260&secimCevresiId=&ilId={textBox1.Text}&bagimsiz=1&secimTuru=6&yurtIciDisi=1";
            urlCandidate = $"https://acikveri.ysk.gov.tr/api/getBuyuksehirBelediyeBaskanligiAdayListesi?secimId=20260&ilId={textBox1.Text}";
            urlResult = $"https://acikveri.ysk.gov.tr/api/getSecimSandikSonucList?secimId=20260&secimTuru=6&ilId={textBox1.Text}&ilceId=&beldeId=&birimId=&muhtarlikId=&cezaeviId=&sandikTuru=&sandikNoIlk=&sandikNoSon=&ulkeId=&disTemsilcilikId=&gumrukId=&yurtIciDisi=1&sandikRumuzIlk=&sandikRumuzSon=&secimCevresiId=&sandikId=&sorguTuru=";
            HttpClient client = new HttpClient();
            var titleData = await client.GetFromJsonAsync<List<TitleOfParty>>(urlTitle);
            var candidateData = await client.GetFromJsonAsync<List<Candidate>>(urlCandidate);
            var resultData = await client.GetFromJsonAsync<List<Result>>(urlResult);
            TextInfo textInfo = new CultureInfo("tr", false).TextInfo;
            // Verileri oy oranına göre sırala


            var allVotesOfCity = await MetropolitanDat();

         
            for (int i = 0; i < resultData.Count; i++)
            {
                countIndependent = 1;
                foreach (var item in candidateData)
                {

                    var party = titleData.FirstOrDefault(x => x.ad == item.parti_KISA_ADI);
                    var filteredResults = resultData.Where(x => x.il_ADI == item.secim_CEVRESI_ADI).ToArray();
                    if (item.parti_KISA_ADI == "BAĞIMSIZ")
                    {
                        party = new TitleOfParty
                        {
                            ad = "BAĞIMSIZ",
                            column_NAME = $"bagimsiz{countIndependent}_ALDIGI_OY"
                        };
                        countIndependent++;
                    }
                    var vote = Convert.ToInt32(GetPropertyDynamically(filteredResults[i], party.column_NAME));

                    mergedDatas.Add(new MergedData
                    {
                        Province = filteredResults[i].ilce_ADI,
                        Candidate = item.adi_SOYADI.ToLower(),
                        PartyFullName = textInfo.ToTitleCase(item.parti_ADI.ToLower()).Replace(" Ve ", " ve "),
                        Party = textInfo.ToTitleCase(item.parti_ADI.ToLower())
                        .Replace("İyi Parti", "İYİ Parti")
                        .Replace(" Ve ", " ve ")
                        .Replace("Türkiye Komünist Partisi", "Türkiye Komünist Partisi (2001)")
                        .Replace("Yeni Türkiye Partisi", "Yeni Türkiye Partisi (2023)")
                        .Replace("Vatan Partisi", "Vatan Partisi (2015)")
                        .Replace("Millet Partisi", "Millet Partisi (1992)")
                        .Replace("Adalet Partisi", "Adalet Partisi (2015)")
                                                    .Replace("Demokrat Parti", "Demokrat Parti (2007)")
                                .Replace("Türkiye İşçi Partisi", "Türkiye İşçi Partisi (2017)")
                        ,
                        Votes = vote,
                        Percentage = Math.Round(((double)vote / filteredResults[i].gecerli_OY_TOPLAMI * 100), 2)

                    });


                }

            }
            richTextBox1.AppendText("{{Seçim sonuç büyükşehir-ilçe-tablo-başlık|");
            resultData.ForEach(x => { richTextBox1.AppendText("[[" + textInfo.ToTitleCase(x.ilce_ADI.ToLower()) + "]]|"); });
            richTextBox1.AppendText("}}\n|-\n");

            for (int i = 0; i < allVotesOfCity.Count; i++)
            {
                var resultsofParty = mergedDatas.Where(x => x.Candidate == allVotesOfCity[i].Candidate).ToList();


                richTextBox1.AppendText($"{{{{Seçim sonuç büyükşehir-ilçe-tablo|{allVotesOfCity[i].Party}|");
                resultsofParty.ForEach(x => {
                    var resultsofProvince = mergedDatas.Where(t => t.Province == x.Province).ToList();
                    var winner = resultsofProvince.OrderByDescending(z => z.Votes).FirstOrDefault();

                    richTextBox1.AppendText(x.Votes.ToString("N0")+"|" + (x.Party == winner.Party ? "1" : "0" ) + "|");
                    richTextBox1.AppendText("");

                });
                richTextBox1.AppendText(resultsofParty.Sum(x => x.Votes).ToString("N0"));
                richTextBox1.AppendText("}}\n|-\n");

            }

            //mergedDatas.ForEach(x =>
            //{
            //    richTextBox1.AppendText($"{{{{Siyasi parti haritarenk|{x.Party}|{textInfo.ToTitleCase(x.Candidate)}|{x.Votes.ToString("N0")}|%{x.Percentage.ToString()}{(count == 0 ? "|1" : "|0")}{(x.Party != x.PartyFullName ? "|" + x.PartyFullName + "}}" : "}}")}\n");
            //    count++;
            //});
            //richTextBox1.AppendText(bottomData);
            //File.WriteAllText(textBox1.Text + ".txt", richTextBox1.Text);


        }
        public object GetPropertyDynamically(Result obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj);
            }

            return null;
        }
    }
}
