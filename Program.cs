using System;
using System.Text;
using WebMarkupMin.Core;

namespace guardrex.com
{
    class Program
    {
        private HtmlMinifier _htmlMinifier = new HtmlMinifier();

        static void Main(string[] args)
        {
            
            // Cycle through the pages
            //   Read metadata from page
            //   Merge the content with the layout.html file
            //   Replace values
            //       !google_analytics_property UA-48425827-1
            //       !site_title the guardrex chew
            //       !page_description Read guardrex articles on IT topics.
            //       !page_title xxxxxxxxxx
            //       !blog_owner_name Luke Latham
            //       !blog_owner_twitter_username guardrex
            //       !content
            //       !copyright_year AUTOMATIC
            //       !domain www.guardrex.com
            //       !bootstrap_version 3.3.7
            //       !styles
            //       !scripts
            //   Minify and inject scripts and stylesheet
            // Generate RSS and sitemap files
            
            Console.WriteLine("Hello World!");
        }

                private string Minify(string markup)
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
