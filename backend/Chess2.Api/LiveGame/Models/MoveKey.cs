using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Chess2.Api.LiveGame.Models;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Models.MoveKey")]
[JsonConverter(typeof(StructStringJsonConverter<MoveKey>))]
[TypeConverter(typeof(StructStringTypeConverter<MoveKey>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct MoveKey(string Value)
{
    public MoveKey(Move move)
        : this(move.From, move.To, move.PromotesTo, move.IntermediateSquares) { }

    public MoveKey(
        AlgebraicPoint from,
        AlgebraicPoint to,
        PieceType? promotesTo = null,
        IEnumerable<AlgebraicPoint>? intermediateSquares = null
    )
        : this(FromParts(from, to, promotesTo, intermediateSquares)) { }

    public static implicit operator string(MoveKey id) => id.Value;

    public static implicit operator MoveKey(string value) => new(value);

    public override string ToString() => Value;

    private static string FromParts(
        AlgebraicPoint from,
        AlgebraicPoint to,
        PieceType? promotesTo,
        IEnumerable<AlgebraicPoint>? intermediateSquares
    )
    {
        StringBuilder sb = new();
        sb.Append(from);
        sb.Append('-');
        sb.Append(to);
        if (promotesTo.HasValue)
        {
            sb.Append('=');
            sb.Append(promotesTo);
        }
        foreach (var square in intermediateSquares ?? [])
        {
            sb.Append('~');
            sb.Append(square.ToString());
        }
        return sb.ToString();
    }
}
