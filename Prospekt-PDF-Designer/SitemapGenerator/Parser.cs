using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitemapGenerator.Sitemap
{
    public static class Parser
    {
        public static IEnumerable<string> GetAHrefs(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);
            var tags=document.DocumentNode.SelectNodes(".//*");
            foreach(var tag in tags)
            {
                if (tag.Name == "a" )
                {
                    string href = tag.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(href))
                        yield return href;
                }
            }
        }
        public static void Normalize(string domain,string current,ref List<string> url)
        {
            url = url.Select(x => {
            if (!x.EndsWith('/'))
            {
                if (x.Contains('/'))
                {
                    string end = x.Split('/').Last();
                    if ((end.Contains(".") || end.Contains("#")) && 
                        (!end.Contains(".html") && !end.Contains(".php")))
                    {
                            return string.Empty;
                    }
                }
                    else
                        if (x.Contains('.') || x.Contains("#"))
                        return string.Empty;
            }
                //if ((x.Contains("http://") || x.Contains("https://")) && !x.Contains(domain))
                if (x.Length >= 8 && ("http://" == x.Substring(0, 7) || "https://" == x.Substring(0, 8)))
                {
                    string[] splittet = x.Split('/');

                    if( 2<splittet.Length && !splittet[2].Contains(domain) )
                    {
                        return string.Empty;
                    }
                }
                //if (!x.Contains("http://") && !x.Contains("https://"))
                else
                {
                    if(x[0]=='/')
                    {
                        return "https://" + domain  + x;
                    }
                    else
                    {
                        if (x[0] == '#')
                            return string.Empty;
                        int last = current.LastIndexOf('/');
                        string meta = current.Substring(0, last);
                        return meta + "/" + x;
                    }
                }
                return x;
            }).ToList();

            url = url.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
