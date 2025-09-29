namespace Chess2.Api.Challenges.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Models.ChallengeId")]
public readonly record struct ChallengeId(string Value)
{
    public static implicit operator string(ChallengeId id) => id.Value;

    public static implicit operator ChallengeId(string value) => new(value);

    public override string ToString() => Value;
}
