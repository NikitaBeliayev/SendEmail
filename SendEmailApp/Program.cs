using System;
using System.Text;
using HtmlAgilityPack;


namespace SendEmailApp
{
    internal class Program
    {
        private const string InitialPageUrl = "https://park.by/residents/?q=&UNP=&save=%D0%9D%D0%B0%D0%B9%D1%82%D0%B8&search=Y&STAFF=&EXPER=&CITY%5B%5D=612&SFERA%5B%5D=651";
        private static int CurrentDivContainerNumber = 2;
        private static readonly string AjaxUrl =
            $"https://park.by/residents/?q=&UNP=&search=Y&STAFF=&EXPER=&CITY%5B0%5D=612&SFERA%5B0%5D=651&TARGET%5B0%5D=614&save=%D0%9D%D0%B0%D0%B9%D1%82%D0%B8&PAGEN_1=";
        private static List<string> CompaniesUrl = new List<string>();
        private const string UrlInitialPart = "https://park.by";
        private const short DivListCount = 68;
        private static List<string> CompaniesEmailList = new List<string>();
        static void Main(string[] args)
        {
            ParseListOfSites();
            ParseEachPage(CompaniesUrl);
        }


        private static void ParseListOfSites()
        {
            using HttpClient client = new HttpClient();
            HtmlDocument document = new HtmlDocument();
            string html = string.Empty;
            HtmlNodeCollection? collection = null;
            StringBuilder builder = new StringBuilder(AjaxUrl);
            builder.Append(CurrentDivContainerNumber.ToString());

            for (int i = 0; i < DivListCount; i++)
            {
                if (i == 0)
                {
                    html = client.GetStringAsync(InitialPageUrl).Result;
                }
                else
                {
                    html = client.GetStringAsync(builder.ToString()).Result;
                    builder.Clear();
                    builder.Append(AjaxUrl);
                    CurrentDivContainerNumber++;
                    builder.Append(CurrentDivContainerNumber);
                }
                document.LoadHtml(html);
                collection = document.DocumentNode.SelectNodes("//div[@class = 'news-list']//div[@class = 'news-item col-md-12']//a");
                builder.Clear();
                foreach (var node in collection)
                {
                    builder.Append(UrlInitialPart);
                    builder.Append(node.Attributes["href"].DeEntitizeValue);
                    CompaniesUrl.Add(builder.ToString());
                    builder.Clear();
                }

                builder.Clear();
                builder.Append(AjaxUrl);
                builder.Append(CurrentDivContainerNumber);

            }
        }

        private static void ParseEachPage(List<string> companiesUrlList)
        {
            using HttpClient client = new HttpClient();
            string html = string.Empty;
            string email = string.Empty;
            HtmlDocument document;
            foreach (var url in companiesUrlList)
            {
                html = client.GetStringAsync(url).Result;
                document = new HtmlDocument();
                document.LoadHtml(html);
                HtmlNode node = document.DocumentNode
                    .SelectSingleNode(
                        "//div[@class = 'block-unde']/div[text()[contains(., 'Адрес электронной почты:')]]");
                email = node is null ? string.Empty : node.InnerText.Substring(node.InnerText.IndexOf(": ") + 2);
                CompaniesEmailList.Add(email);
            }
            foreach (var url in CompaniesEmailList)
            {
                Console.WriteLine(url);
            }
        }
    }
}