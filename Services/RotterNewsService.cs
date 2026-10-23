using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewsWall.Services;

public class RotterNewsService : IRotterNewsService
{
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = true })
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36" },
            { "Accept", "application/rss+xml, application/xml, text/xml, */*" },
            { "Accept-Language", "he-IL,he;q=0.9,en;q=0.8" }
        }
    };

    public async Task<IReadOnlyList<string>> GetLatestHeadlinesAsync(int count = 6)
    {
        // Try HTTP first (more likely to work without SSL/redirect issues), then HTTPS
        var urls = new[]
        {
            "http://rotter.net/rss/rotternews.xml",
            "https://rotter.net/rss/rotternews.xml"
        };

        foreach (var url in urls)
        {
            try
            {
                var xml = await Http.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(xml))
                    continue;

                var doc = XDocument.Parse(xml);

                // Handle RSS with or without XML namespaces
                var channel = doc.Root?.Element("channel")
                              ?? doc.Root?.Descendants().FirstOrDefault(e => e.Name.LocalName == "channel");

                if (channel == null)
                    continue;

                var headlines = channel
                    .Elements()
                    .Where(e => e.Name.LocalName == "item")
                    .Select(item => item.Elements().FirstOrDefault(e => e.Name.LocalName == "title")?.Value)
                    .Where(title => !string.IsNullOrWhiteSpace(title))
                    .Select(title => System.Net.WebUtility.HtmlDecode(title!.Trim()))
                    .Take(count)
                    .ToList();

                if (headlines.Count > 0)
                    return headlines;
            }
            catch
            {
                // Try next URL
            }
        }

        return new List<string>();
    }
}
