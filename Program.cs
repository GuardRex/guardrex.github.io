using System;
using System.Collections.Generic;
using System.Text;
using WebMarkupMin.Core;
using System.IO;
using static System.IO.SearchOption;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace guardrex.com
{
    class Program
    {
        private static HtmlMinifier _htmlMinifier = new HtmlMinifier();

        static void Main(string[] args)
        {
            // Setup a few general values for use in the layout, RSS, and sitmap
            var site_title = "the guardrex chew";
            var site_description = "Read guardrex articles on IT topics.";
            var domain = "www.guardrex.com";
            var cdn_domain = "rexsite.azureedge.net";

            // Set the path to the repo docs_debug folder
            var path = @"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug\";

            // Setup for the index page content
            StringBuilder indexContent = new StringBuilder();

            // Setup for the RSS file
            StringBuilder rssContent = new StringBuilder($@"<?xml version=""1.0"" encoding=""utf-8""?><rss version=""2.0""><channel><title>{site_title}</title><link>http://{domain}/</link><description>{site_description}</description>");

            // Setup for the sitemap file
            StringBuilder sitemapContent = new StringBuilder($@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd""><url><loc>http://{domain}/</loc><changefreq>weekly</changefreq><lastmod>{DateTime.Now.ToString("yyyy-MM-dd")}</lastmod><priority>1</priority></url><url><loc>http://{domain}/rss.xml</loc><changefreq>weekly</changefreq><lastmod>{DateTime.Now.ToString("yyyy-MM-dd")}</lastmod></url>");

            // Setup dict to hold page metadata
            var pageMetadataDict = new ConcurrentDictionary<string, string>();
            
            //Load the layout
            var layout = File.ReadAllText($"{path}layout.htm");

            // Replace tokens in the layout
            layout = layout
                .Replace("!styles", File.ReadAllText($"{path}styles.css"))
                .Replace("!scripts", File.ReadAllText($"{path}scripts.js"))
                .Replace("!copyright_year", DateTime.Now.Year.ToString())
                .Replace("!domain", domain)
                .Replace("!bootstrap_version", "3.3.7")
                .Replace("!google_analytics_property", "UA-48425827-1")
                .Replace("!site_title", site_title)
                .Replace("!blog_owner_name", "Luke Latham")
                .Replace("!blog_owner_twitter_username", "guardrex");

            // RegEx to grab metadata from pages
            Regex regExp = new Regex(@"^(.*?)\r\n---", RegexOptions.Compiled | RegexOptions.Singleline);
            
            // Work on the pages
            var files = Directory.EnumerateFiles(path, "*.html", AllDirectories);
            foreach (var file in files)
            {
                var fileText = File.ReadAllText(file);

                // Read metadata from page into the dict of replacement values
                var metadataCapture = regExp.Matches(fileText);
                var metadataCaptureLines = metadataCapture[0].Groups[1].ToString().Split("\r\n");
                foreach (var metadataLine in metadataCaptureLines)
                {
                    var key = metadataLine.Substring(0, metadataLine.IndexOf(":"));
                    var value = metadataLine.Substring(metadataLine.IndexOf(":") + 2);
                    pageMetadataDict.AddOrUpdate(key, value, (k, v) => value);
                }

                // Merge the content (sans metadata) into the layout and apply the CDN domcain where needed
                var outputMarkup = layout.Replace("!content", fileText.Substring(fileText.IndexOf("---") + 3))
                    .Replace("!cdn_domain", cdn_domain);

                // Replace tokens with page metadata from the dict
                foreach (var pageMetadataItem in pageMetadataDict)
                {
                    outputMarkup = outputMarkup.Replace($"!{pageMetadataItem.Key}", pageMetadataItem.Value);
                }

                // Minify page
                outputMarkup = Minify(outputMarkup);

                // Generate content for index, RSS, and sitemap files
                if (!file.EndsWith("index.html"))
                {
                    var fileName = file.Substring(file.LastIndexOf("\\") + 1);

                    indexContent.Append($@"<div><a class=""nostyle"" href=""post/{fileName}""><h2>{pageMetadataDict["page_title"]}</h2></a><p>{pageMetadataDict["publication_date"]}</p><p>{pageMetadataDict["page_description"]}</p><p><a class=""btn"" href=""post/{fileName}"">Read More</a></p></div>");

                    rssContent.Append($@"<item><title>{pageMetadataDict["page_title"]}</title><link>http://{domain}/post/{fileName}</link><guid>post/{fileName}</guid><pubDate>{pageMetadataDict["publication_date"]}</pubDate><description>{pageMetadataDict["page_description"]}</description></item>");

                    sitemapContent.Append($@"<url><loc>http://{domain}/post/{fileName}</loc><changefreq>monthly</changefreq><lastmod>{pageMetadataDict["last_modification_date"]}</lastmod></url>");
                }

                // Save it to the docs folder
                var saveFile = file.Replace("_debug", string.Empty);
                File.WriteAllText(saveFile, outputMarkup);
            }

            // Inject the index page content
            var indexFilePath = $@"{path.Replace("_debug", string.Empty)}\index.html";
            var indexFileText = File.ReadAllText(indexFilePath);
            File.WriteAllText(indexFilePath, indexFileText.Replace("!index_content", indexContent.ToString()));
            
            // Finish up the RSS file
            rssContent.Append(@"</channel></rss>");
            File.WriteAllText($@"{path.Replace("_debug", string.Empty)}rss.xml", rssContent.ToString());

            // Finish up the sitemap file
            sitemapContent.Append(@"</urlset>");
            File.WriteAllText($@"{path.Replace("_debug", string.Empty)}sitemap.xml", sitemapContent.ToString());
        }

        private static string Minify(string markup)
        {
            MarkupMinificationResult result = _htmlMinifier.Minify(markup, string.Empty, Encoding.UTF8, true);
            if (result.Errors.Count == 0)
            {
                MinificationStatistics statistics = result.Statistics;
                if (statistics != null)
                {
                    Console.WriteLine("Original size: {0:N0} Bytes", statistics.OriginalSize);
                    Console.WriteLine("Minified size: {0:N0} Bytes", statistics.MinifiedSize);
                    Console.WriteLine("Saved: {0:N2}%", statistics.SavedInPercent);
                }
                Console.WriteLine("Minified content:{0}{0}{1}", Environment.NewLine, result.MinifiedContent);

                return result.MinifiedContent;
            }
            else
            {
                IList<MinificationErrorInfo> errors = result.Errors;

                Console.WriteLine("Found {0:N0} error(s):", errors.Count);
                Console.WriteLine();

                foreach (var error in errors)
                {
                    Console.WriteLine("Line {0}, Column {1}: {2}", error.LineNumber, error.ColumnNumber, error.Message);
                    Console.WriteLine();
                }

                return markup;
            }
        }
    }
}
