namespace Diffusion.Toolkit.Models;

public class LlmProviderSettings
{
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "google/gemini-2.5-flash";
}
