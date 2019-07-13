using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GoogleSearch
{
    public class Engine
    {
        public bool cacheResponses = true; //Indicates If A Cache Of All Search Results Should Be Saved To Increase Speed And Lower Actual Queries
        public WebClient client = new WebClient(); //WebClient Used To Fetch Google Search Results
        public Dictionary<string, Result[]> cache = new Dictionary<string, Result[]>(); //Cache Dictionary Containtion All Results Cached

        //Used To Send A Search Query
        public Result[] Search(string query, int start = 0)
        {
            //Try To Load The Cache If Caching Is Enabled And A Cache Has Not Been Loaded Yet
            try { if (cacheResponses && cache.Count == 0) LoadCache(); } catch { }

            try
            {
                string queryUrl = "https://www.google.com/search?q=" + HttpUtility.UrlEncode(query.ToLower()) + "&start=" + start; //Create The Query URL
                if (cache.ContainsKey(queryUrl)) //Check If This Query Has Already Been Sent
                    return cache[queryUrl]; //Return Directly From Cache
                string response = client.DownloadString(queryUrl); //Download The HTML From The Query URL

                var doc = new HtmlAgilityPack.HtmlDocument(); //Create An HTML Document From The Downloaded HTML
                doc.LoadHtml(response); //Load The Downloaded HTML
                var divs = doc.DocumentNode.Descendants("div"); //Get All The Divs In The Document
                List<Result> results = new List<Result>(); //Create A List For All The Results
                foreach (HtmlNode node in divs) //Loop Through All Nodes In The Div Array
                {
                    if (node.GetClasses().Contains("ZINbbc")) //Check If The Node Has The Class "ZINbbc"
                        if (node.FirstChild.GetClasses().Contains("jfp3ef")) //Check If The Node Has The Class "jfp3ef"
                            if (node.FirstChild.FirstChild.OuterHtml.StartsWith("<a href=\"/url?q=")) //Check If The Link(Inside The Node)'s Outer HTML Starts With "<a href="/url?q="
                            {
                                results.Add(new Result() //Add A New Result Class
                                {
                                    url = HttpUtility.HtmlDecode(node.FirstChild.FirstChild.GetAttributeValue("href", "").Substring(("/url?q=").Length).Split('&')[0]), //Set The URL To The One Found In The Link (Inside The Node)
                                    title = node.FirstChild.FirstChild.FirstChild.InnerText, //Set The Result Title
                                    description = node.LastChild.InnerText //Set The Result Description
                                });
                            }
                }

                //Save Cache If Enabled And The Response HTML Was Longer Than 1500 Characters (If It's Longer Than That, It Has Probably Gone Well)
                if (response.Length > 1500 && cacheResponses)
                {
                    cache[queryUrl] = results.ToArray(); //Set The Cache
                    SaveCache(); //Save The Cache
                }

                return results.ToArray(); //Return The Results
            }
            catch (Exception ex)
            {
                return null; //Return Null On Error
            }
        }

        //Search Multiple Pages
        public Result[] SearchPages(string query, int pages)
        {
            List<Result> results = new List<Result>(); //List To Save Found Results
            for (int i = 0; i < pages; i++) //Loop The Amount Of Times Specified By The Pages INT
                results.AddRange(Search(query, results.Count)); //Add Results Found
            return results.ToArray(); //Return The List As An Array
        }

        public void LoadCache() { if (File.Exists("GoogleSearchCache.json")) cache = JsonConvert.DeserializeObject<Dictionary<string, Result[]>>(File.ReadAllText("GoogleSearchCache.json")); } //Read The Cache File And Deserialize It Into A Dictionary
        public void SaveCache() { File.WriteAllText("GoogleSearchCache.json", JsonConvert.SerializeObject(cache)); } //Serialize The Cache Dictionary And Save It To The Cache File

        public class Result { public string url, title, description; } //Simple Class To Store Result URL, Title And Description
    }
}
