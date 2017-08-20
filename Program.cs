using System;
using System.Collections.Generic;
using System.Text;
using WebMarkupMin.Core;
using System.IO;
using static System.IO.SearchOption;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Linq;

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
            string sourcePath;
            string destinationPath;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                sourcePath = @"c:\projects\guardrex-com\docs_debug\";
                destinationPath = @"c:\projects\guardrex-com\docs\";
            }
            else
            {
                sourcePath = @"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug\";
                destinationPath = @"C:\Users\guard\Documents\GitHub\guardrex.com\docs_staging\";
            }

            // Clear the posts folder
            System.IO.DirectoryInfo di = new DirectoryInfo($@"{destinationPath}\post");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete(); 
            }

            // Setup for the index page content
            var indexContent = new StringBuilder();
            var indexContentPosts = new List<KeyValuePair<string, string>>();

            // Setup for the RSS file
            var rssContent = new StringBuilder($@"<?xml version=""1.0"" encoding=""utf-8""?><rss version=""2.0""><channel><title>{site_title}</title><link>http://{domain}/</link><description>{site_description}</description>");

            // Setup for the sitemap file
            var sitemapContent = new StringBuilder($@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd""><url><loc>http://{domain}/</loc><changefreq>weekly</changefreq><lastmod>{DateTime.Now.ToString("yyyy-MM-dd")}</lastmod><priority>1</priority></url><url><loc>http://{domain}/rss.xml</loc><changefreq>weekly</changefreq><lastmod>{DateTime.Now.ToString("yyyy-MM-dd")}</lastmod></url>");

            // Setup dict to hold page metadata
            var pageMetadataDict = new ConcurrentDictionary<string, string>();
            
            //Load the layout
            var layout = File.ReadAllText($"{sourcePath}layout.htm");

            // Replace tokens in the layout
            layout = layout
                .Replace("!styles", File.ReadAllText($"{sourcePath}styles.css"))
                .Replace("!scripts", File.ReadAllText($"{sourcePath}scripts.js"))
                .Replace("!copyright_year", DateTime.Now.Year.ToString())
                .Replace("!bootstrap_version", "3.3.7")
                .Replace("!google_analytics_property", "UA-48425827-1")
                .Replace("!site_title", site_title)
                .Replace("!blog_owner_name", "Luke Latham")
                .Replace("!blog_owner_twitter_username", "guardrex");

            // Work on the pages
            var files = Directory.EnumerateFiles(sourcePath, "*", AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(".md") || file.EndsWith(".html"))
                {
                    var fileText = File.ReadAllText(file);
                    var filename = file.Substring(file.LastIndexOf("\\") + 1);

                    Console.WriteLine();
                    Console.WriteLine(filename);

                    var breakPoint = fileText.IndexOf("---");
                    var metadataSection = fileText.Substring(0, breakPoint);
                    var metadataLines = metadataSection.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < metadataLines.Count() - 1; i++)
                    {
                        var colonIndex = metadataLines[i].IndexOf(":");
                        var key = metadataLines[i].Substring(0, colonIndex);
                        var value = metadataLines[i].Substring(colonIndex + 2);
                        pageMetadataDict.AddOrUpdate(key, value, (k, v) => value);
                    }

                    string outputMarkup;

                    // Convert markdown into HTML
                    if (filename.EndsWith(".md"))
                    {
                        outputMarkup = CommonMark.CommonMarkConverter.Convert(fileText.Substring(fileText.IndexOf("---") + 3));
                        filename = $"{filename.Substring(0, filename.IndexOf("."))}.html";
                    }
                    else
                    {
                        outputMarkup = fileText.Substring(fileText.IndexOf("---") + 3);
                    }

                    // Merge the content (sans metadata) into the layout and apply the CDN domain where needed
                    outputMarkup = layout.Replace("!content", outputMarkup)
                        .Replace("!cdn_domain", cdn_domain)
                        .Replace("!domain", domain)
                        .Replace("!filename", filename);

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
                        indexContentPosts.Add(new KeyValuePair<string, string>(pageMetadataDict["last_modification_date"], $@"<div><a class=""nostyle"" href=""post/{filename}""><h2>{pageMetadataDict["page_title"]}</h2></a><p>{pageMetadataDict["publication_date"]}</p><p>{pageMetadataDict["page_description"]}</p><p><a class=""btn"" href=""post/{filename}"">Read More</a></p></div>"));

                        rssContent.Append($@"<item><title>{pageMetadataDict["page_title"]}</title><link>http://{domain}/post/{filename}</link><guid>{pageMetadataDict["guid"]}</guid><pubDate>{pageMetadataDict["publication_date"]}</pubDate><description>{pageMetadataDict["page_description"]}</description></item>");

                        sitemapContent.Append($@"<url><loc>http://{domain}/post/{filename}</loc><changefreq>monthly</changefreq><lastmod>{pageMetadataDict["last_modification_date"]}</lastmod></url>");

                        File.WriteAllText($@"{destinationPath}post\{filename}", outputMarkup);
                    }
                    else
                    {
                        File.WriteAllText($@"{destinationPath}index.html", outputMarkup);
                    }
                }
            }

            // Inject the index page content
            var sortedPosts = indexContentPosts.OrderByDescending(_ => _.Key);
            foreach (var post in sortedPosts)
            {
                indexContent.Append(post.Value);
            }
            // The index file is already in output
            var indexFilePath = $@"{destinationPath}index.html";
            var indexFileText = File.ReadAllText(indexFilePath);
            File.WriteAllText(indexFilePath, indexFileText.Replace("!index_content", indexContent.ToString()));
            
            // Finish up the RSS file
            rssContent.Append(@"</channel></rss>");
            File.WriteAllText($@"{destinationPath}rss.xml", rssContent.ToString());

            // Finish up the sitemap file
            sitemapContent.Append(@"</urlset>");
            File.WriteAllText($@"{destinationPath}sitemap.xml", sitemapContent.ToString());

            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.WriteLine();
        }

        private static string Minify(string markup)
        {
            MarkupMinificationResult result = _htmlMinifier.Minify(markup, string.Empty, Encoding.UTF8, true);
            if (result.Errors.Count == 0)
            {
                MinificationStatistics statistics = result.Statistics;
                if (statistics != null)
                {
                    Console.WriteLine("Original size: {0:N0} Bytes | Minified size: {1:N0} | Bytes Saved: {2:N2}%", statistics.OriginalSize, statistics.MinifiedSize, statistics.SavedInPercent);
                }
                //Console.WriteLine("Minified content:{0}{0}{1}", Environment.NewLine, result.MinifiedContent);

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
