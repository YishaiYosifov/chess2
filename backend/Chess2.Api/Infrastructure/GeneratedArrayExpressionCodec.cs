using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using Orleans.Serialization.Buffers;
using Orleans.Serialization.Codecs;
using Orleans.Serialization.GeneratedCodeHelpers;
using Orleans.Serialization.Serializers;
using Orleans.Serialization.WireProtocol;

namespace Chess2.Api.Infrastructure;

/// <summary>
/// Add orleans serialization support for IReadOnlyList
/// https://github.com/dotnet/orleans/issues/8934#issuecomment-2483228855
/// </summary>
public class GeneratedArrayExpressionCodec(ICodecProvider codecProvider) : ISpecializableCodec
{
    private sealed class InnerCodec<T> : IFieldCodec<IReadOnlyCollection<T>>
    {
        private readonly IFieldCodec _fieldCodec;
        private readonly Type _codecElementType = typeof(T);

        public InnerCodec(IFieldCodec fieldCodec)
        {
            if (fieldCodec is IFieldCodec<T> typed)
            {
                _fieldCodec = OrleansGeneratedCodeHelper.UnwrapService(this, typed);
            }
            else
            {
                _fieldCodec = new UntypedFieldCodecAdapter<T>(fieldCodec);
            }
        }

        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            IReadOnlyCollection<T> value
        )
            where TBufferWriter : IBufferWriter<byte>
        {
            if (
                ReferenceCodec.TryWriteReferenceField(ref writer, fieldIdDelta, expectedType, value)
            )
            {
                return;
            }

            writer.WriteFieldHeader(
                fieldIdDelta,
                expectedType,
                _codecElementType.MakeArrayType(),
                WireType.TagDelimited
            );

            if (value.Count > 0)
            {
                UInt32Codec.WriteField(ref writer, 0, (uint)value.Count);
                uint innerFieldIdDelta = 1;
                foreach (var element in value)
                {
                    _fieldCodec.WriteField(
                        ref writer,
                        innerFieldIdDelta,
                        _codecElementType,
                        element
                    );
                    innerFieldIdDelta = 0;
                }
            }

            writer.WriteEndObject();
        }

        public IReadOnlyCollection<T> ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            // No need, the ArrayCodec will take care of reading :-)
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bridges untyped codecs (like AbstractTypeSerializer) into typed IFieldCodec{T}.
    /// </summary>
    private sealed class UntypedFieldCodecAdapter<T>(IFieldCodec inner) : IFieldCodec<T>
    {
        private readonly IFieldCodec _inner = inner;

        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            T value
        )
            where TBufferWriter : IBufferWriter<byte> =>
            _inner.WriteField(ref writer, fieldIdDelta, expectedType, value);

        public T ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            object? result = _inner.ReadValue(ref reader, field);
            return (T)result!;
        }
    }

    public bool IsSupportedType(Type type) =>
        type.Name.StartsWith("<>z", StringComparison.Ordinal)
        && type.GetCustomAttribute<CompilerGeneratedAttribute>() is not null
        && type.IsGenericType
        && type.GetGenericArguments() is [var elementType]
        && type.IsAssignableTo(typeof(IReadOnlyList<>).MakeGenericType(elementType));

    public IFieldCodec GetSpecializedCodec(Type type)
    {
        if (type.GetGenericArguments() is [var elementType])
        {
            var codecType = typeof(InnerCodec<>).MakeGenericType(elementType);
            var codec = Activator.CreateInstance(codecType, codecProvider.GetCodec(elementType));
            return (IFieldCodec)codec!;
        }

        throw new NotSupportedException();
    }
}
