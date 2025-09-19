using System.Text.Json;
using System.Text.Json.Serialization;

namespace FintechPSP.CompanyService.Converters;

/// <summary>
/// Conversor que transforma strings vazias em null para DateTime?
/// </summary>
public class EmptyStringToNullDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            
            // Se for string vazia ou null, retorna null
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }
            
            // Tenta converter para DateTime
            if (DateTime.TryParse(stringValue, out var dateTime))
            {
                return dateTime;
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        // Se não conseguir converter, retorna null
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
