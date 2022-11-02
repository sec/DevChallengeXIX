using System.Text.Json.Serialization;

namespace DevChallengeXIX.Requests;

public record MessageRequest(
    string Text,
    string[] Topics,
    [property: JsonPropertyName("from_person_id")] string FromPersonId,
    [property: JsonPropertyName("min_trust_level")] int MinTrustLevel);