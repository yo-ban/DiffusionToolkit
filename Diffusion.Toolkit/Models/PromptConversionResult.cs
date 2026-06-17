using System.Text.Json.Serialization;

namespace Diffusion.Toolkit.Models;

public class PromptConversionResult
{
    [JsonPropertyName("positive_prompt")]
    public string PositivePrompt { get; set; } = "";

    [JsonPropertyName("negative_prompt")]
    public string NegativePrompt { get; set; } = "";

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = "";

    public string RawResponse { get; set; } = "";
    public string Model { get; set; } = "";

    [JsonIgnore]
    public string Text
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PositivePrompt) &&
                string.IsNullOrWhiteSpace(NegativePrompt) &&
                string.IsNullOrWhiteSpace(Notes))
            {
                return RawResponse;
            }

            return $"""
                    Positive prompt:
                    {PositivePrompt}

                    Negative prompt:
                    {NegativePrompt}

                    Notes:
                    {Notes}
                    """;
        }
        set
        {
            RawResponse = value;
            if (string.IsNullOrWhiteSpace(PositivePrompt))
            {
                PositivePrompt = value;
            }
        }
    }
}
