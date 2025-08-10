namespace Chess2.Api.Shared.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.ConnectionId")]
public readonly record struct ConnectionId(string Value)
{
    public static implicit operator string(ConnectionId id) => id.Value;

    public static implicit operator ConnectionId(string value) => new(value);

    public override string ToString() => Value;
}
