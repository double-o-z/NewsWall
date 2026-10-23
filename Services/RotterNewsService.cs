using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewsWall.Services;

public class RotterNewsService : IRotterNewsService
{
    private static readonly HttpClient _httpClient;
    private static readonly Encoding _hebrewEncoding = GetHebrewEncoding();

    private static Encoding GetHebrewEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1255); // Windows-1255 Hebrew
    }

    static RotterNewsService()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseDefaultCredentials = false,
            PreAuthenticate = false,
        };

        _httpClient = new HttpClient(handler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/rss+xml, application/xml, text/xml, */*");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "he-IL,he;q=0.9,en-US;q=0.8,en;q=0.7");
    }

    public async Task<IReadOnlyList<RotterHeadline>> GetLatestHeadlinesAsync(int count = 6)
    {
        var urls = new[]
        {
            "https://rotter.net/rss/rotternews.xml",
            "http://rotter.net/rss/rotternews.xml"
        };

        foreach (var url in urls)
        {
            try
            {
                var rawBytes = await _httpClient.GetByteArrayAsync(url);

                var xml = _hebrewEncoding.GetString(rawBytes);
                var doc = XDocument.Parse(xml);

                var channel = doc.Root?.Element("channel")
                              ?? doc.Root?.Descendants().FirstOrDefault(e => e.Name.LocalName == "channel");

                if (channel == null)
                    continue;

                var headlines = channel
                    .Elements()
                    .Where(e => e.Name.LocalName == "item")
                    .Select(item => new
                    {
                        Title = item.Elements().FirstOrDefault(e => e.Name.LocalName == "title")?.Value,
                        Url = item.Elements().FirstOrDefault(e => e.Name.LocalName == "guid")?.Value
                              ?? item.Elements().FirstOrDefault(e => e.Name.LocalName == "link")?.Value
                    })
                    .Where(h => !string.IsNullOrWhiteSpace(h.Title))
                    .Select(h => new RotterHeadline(
                        System.Net.WebUtility.HtmlDecode(h.Title!.Trim()),
                        System.Net.WebUtility.HtmlDecode(h.Url?.Trim() ?? string.Empty)))
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

        return new List<RotterHeadline>();
    }
}