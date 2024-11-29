using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace src.Models;

internal class RequestSerializer
{
    // possible values method, jsonrpc, params, id
    public static T? GetValueAs<T>(ref Span<byte> readBytes, string PropertyName)
    {
        var reader = new Utf8JsonReader(readBytes);

        while(reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(PropertyName))
                {
                    reader.Read();
                    return DecodedValue<T>(reader);
                }
            }
            reader.Read();
        }
        return default;
    }
    
    private static T? DecodedValue<T>(Utf8JsonReader propertyReader)
    {
        if (typeof(T) == typeof(string))
            return (T?)(object)(propertyReader.GetString() ?? string.Empty);
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
            var result = JsonSerializer.Deserialize<EstimateGas>(propertyReader.ValueSpan);
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<EstimateGas, T>(ref result), 1)[0];
        }
        throw new NotSupportedException();
    }

    //add method for array
}
