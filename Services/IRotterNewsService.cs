using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsWall.Services;

/// <summary>
/// Represents a single Rotter news headline with its source URL.
/// </summary>
public record RotterHeadline(string Title, string Url);

public interface IRotterNewsService
{
    Task<IReadOnlyList<RotterHeadline>> GetLatestHeadlinesAsync(int count = 6);
}
