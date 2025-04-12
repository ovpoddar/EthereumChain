using API.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace API.Handlers;

internal class RequestSerializer
{
    // possible values method, jsonrpc, params, id
    public static T? GetValueAs<T>(ref Span<byte> readBytes, string propertyName) =>
        GetValueAs<T>(new Utf8JsonReader(readBytes), propertyName);
    public static T[] GetArrayAs<T>(ref Span<byte> readBytes, string propertyName, int itemsCount = 10)
    {
        var reader = new Utf8JsonReader(readBytes);
        var foundDepth = 0;
        while (reader.TokenType != JsonTokenType.EndObject || reader.CurrentDepth != 0)
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(propertyName))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartArray)
                    continue;
                foundDepth = reader.CurrentDepth;

                reader.Read();
                var result = new T[itemsCount];
                var writeIndex = 0;
                while (reader.TokenType != JsonTokenType.EndArray || reader.CurrentDepth != foundDepth)
                {
                    var item = DecodedValue<T>(ref reader);
                    result[writeIndex++] = item;
                    reader.Read();
                }
                return result;

            }
            reader.Read();
        }
        return [];
    }

    static T? GetValueAs<T>(Utf8JsonReader reader, string propertyName)
    {
        while (reader.TokenType != JsonTokenType.EndObject || reader.CurrentDepth != 0)
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(propertyName))
            {
                reader.Read();
                return DecodedValue<T>(ref reader);
            }
            reader.Read();
        }
        return default;
    }
    static T DecodedValue<T>(ref Utf8JsonReader propertyReader)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)(propertyReader.GetString() ?? string.Empty);
        else if (typeof(T) == typeof(int))
        {
            var result = propertyReader.GetInt32();
            return Unsafe.As<int, T>(ref result);
        }
        else if (typeof(T) == typeof(Range))
        {
            var result = new Range((int)propertyReader.TokenStartIndex, (int)propertyReader.BytesConsumed);
            return Unsafe.As<Range, T>(ref result);
        }
        else if (typeof(T) == typeof(EstimateGas))
        {
            var result = new EstimateGas
            {
                From = GetValueAs<string>(propertyReader, "from"),
                To = GetValueAs<string>(propertyReader, "to") ?? throw new Exception("Missing to"),
                Value = GetValueAs<string>(propertyReader, "value"),
                Gas = GetValueAs<string>(propertyReader, "gas"),
                GasPrice = GetValueAs<string>(propertyReader, "gasPrice"),
                Data = GetValueAs<string>(propertyReader, "data")
            };
            propertyReader.TrySkip();
            return Unsafe.As<EstimateGas, T>(ref result);
        }
        else if (typeof(T) == typeof(bool))
        {
            var result = propertyReader.GetBoolean();
            return Unsafe.As<bool, T>(ref result);
        }
        throw new NotImplementedException();
    }

    // this is a creepy method but so does the json
    public static T GetValueFromArray<T>(ref Span<byte> readBytes, string propertyName, int index)
    {
        var reader = new Utf8JsonReader(readBytes);
        var foundDepth = 0;
        while (reader.TokenType != JsonTokenType.EndObject || reader.CurrentDepth != 0)
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(propertyName))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartArray)
                    continue;
                foundDepth = reader.CurrentDepth;

                reader.Read();
                var writeIndex = 0;
                while (reader.TokenType != JsonTokenType.EndArray || reader.CurrentDepth != foundDepth)
                {
                    if (index != writeIndex) { reader.Read(); writeIndex++; continue; }
                    return DecodedValue<T>(ref reader);
                }

            }
            reader.Read();
        }
        return default(T);
    }
}
