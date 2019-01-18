using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace IG.Downloader.Console
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            if (args.Length == 0 && !Debugger.IsAttached)
            {
                var versionString = Assembly.GetEntryAssembly()
                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                .InformationalVersion
                                .ToString();

                System.Console.WriteLine($"Instagram Image Downloader v{versionString}");
                System.Console.WriteLine("Usage: IG.Downloader.Exe <url>");
                return;
            }

            var inputString = args.FirstOrDefault() ?? "https://www.instagram.com/p/BsAs16YBxKN/";

            if (Uri.TryCreate(inputString, UriKind.RelativeOrAbsolute, out Uri igUrl) && inputString.Contains("www.instagram.com", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var client = new HttpClient())
                {
                    var page = await client.GetAsync(igUrl);
                    var html = await page.Content.ReadAsStringAsync();

                    // HTML Query
                    var pageDocument = new HtmlDocument();
                    pageDocument.LoadHtml(html);
                    var element = pageDocument.DocumentNode.SelectSingleNode("(//meta[contains(@property,'og:image')])[1]");

                    if (element != null)
                    {
                        var imageUrl = element.Attributes["content"].Value;
                        var uri = new Uri(imageUrl);

                        var imageStream = await client.GetStreamAsync(uri);
                        using (var fileStream = new FileStream(uri.Segments.Last(), FileMode.Create, FileAccess.Write))
                        {
                            imageStream.CopyTo(fileStream);
                        }
                    }
                }
            }
            else
            {
                System.Console.WriteLine("Invalid Instagram URL. Please input a valid instagram.com URL");
            }
        }
    }
}
