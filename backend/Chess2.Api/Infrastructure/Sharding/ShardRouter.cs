using System.Text;

namespace Chess2.Api.Infrastructure.Sharding;

public interface IShardRouter
{
    int GetShardNumber(string id, int shardCount);
}

public class ShardRouter : IShardRouter
{
    public int GetShardNumber(string id, int shardCount)
    {
        var bytes = Encoding.UTF8.GetBytes(id);
        long hash = 0;

        foreach (var b in bytes)
        {
            hash = (hash * 31) + b;
        }

        return (int)(Math.Abs(hash) % shardCount);
    }
}
