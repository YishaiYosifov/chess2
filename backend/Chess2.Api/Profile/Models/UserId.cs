namespace Chess2.Api.Profile.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Profile.Models.UserId")]
public readonly record struct UserId(string Value)
{
    public static implicit operator string(UserId id) => id.Value;

    public static implicit operator UserId(string value) => new(value);

    public override string ToString() => Value;
}
