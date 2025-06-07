using FluentResults;
using Newtonsoft.Json;

public class ResultValueOnlyConverter<T> : JsonConverter<Result<T>>
{
    public override void WriteJson(JsonWriter writer, Result<T> result, JsonSerializer serializer)
    {
        serializer.Serialize(writer, result.Value);
    }

    public override Result<T> ReadJson(JsonReader reader, Type objectType, Result<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = serializer.Deserialize<T>(reader);
        return Result.Ok(value);
    }
}