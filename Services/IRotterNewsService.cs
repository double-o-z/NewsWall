using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsWall.Services;

public interface IRotterNewsService
{
    Task<IReadOnlyList<string>> GetLatestHeadlinesAsync(int count = 6);
}
