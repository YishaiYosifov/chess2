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
        var hash = BitConverter.ToInt64(bytes, 0);
        return (int)(hash % 5);
    }
}
