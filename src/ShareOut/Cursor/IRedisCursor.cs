using System.Threading.Tasks;

namespace ShareOut.Cursor
{
    public interface IRedisCursor
    {
        Task<IRedisCursor> Cursor(string key, string pattern);
    }
}
