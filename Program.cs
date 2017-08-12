using System;
using System.Collections.Generic;
using System.Text;
using WebMarkupMin.Core;
using System.IO;
using static System.IO.SearchOption;
using System.Text.RegularExpressions;

namespace guardrex.com
{
    class Program
    {
        private static HtmlMinifier _htmlMinifier = new HtmlMinifier();

        static void Main(string[] args)
        {
            // Default values
            var pageMetadataDict = new Dictionary<string, string>() 
            {
                { "copyright_year", DateTime.Now.Year.ToString() },
                { "domain", "www.guardrex.com" },
                { "bootstrap_version", "3.3.7" },
                { "google_analytics_property", "UA-48425827-1" },
                { "site_title", "the guardrex chew" },
                { "page_title", string.Empty },
                { "page_description", string.Empty },
                { "blog_owner_name", "Luke Latham" },
                { "blog_owner_twitter_username", "guardrex" },
                { "content", string.Empty },
                { "styles", string.Empty },
                { "scripts", string.Empty }
            };
            
            // Load styles and scripts
            pageMetadataDict["styles"] = File.ReadAllText(@"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug\styles.css");
            pageMetadataDict["scripts"] = File.ReadAllText(@"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug\scripts.js");

            //Load the layout
            var layout = File.ReadAllText(@"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug\layout.htm");

            // RegEx
            Regex regExp = new Regex(@"---\r\n(.*?)\r\n---", RegexOptions.Compiled | RegexOptions.Singleline);
            
            // Cycle through the pages
            var files = Directory.EnumerateFiles(@"C:\Users\guard\Documents\GitHub\guardrex.com\docs_debug", "*.html", AllDirectories);
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
                    pageMetadataDict[key] = value;
                }

                // Merge the content (sans metadata) with the layout.html file
                var outputMarkup = layout.Replace("!content", fileText.Substring(fileText.IndexOf("---") + 3));

                // Replace values
                foreach (var pageMetadataItem in pageMetadataDict)
                {
                    var replaceKey = $"!{pageMetadataItem.Key}";
                    var replaceValue = pageMetadataItem.Value;
                    outputMarkup = outputMarkup.Replace(replaceKey, replaceValue);
                }

                // Minify page
                outputMarkup = Minify(outputMarkup);

                // Save it to docs
                var saveFile = file.Replace("docs_debug", "docs");
                File.WriteAllText(saveFile, outputMarkup);
            }
            
            // Generate RSS and sitemap files
            
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
