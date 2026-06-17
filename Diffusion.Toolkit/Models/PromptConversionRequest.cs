namespace Diffusion.Toolkit.Models;

public class PromptConversionRequest
{
    public LlmProviderSettings Provider { get; set; } = new();
    public string SystemPrompt { get; set; } = "";
    public string SystemPromptPresetName { get; set; } = "";
    public string AdditionalRequest { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public string? ImageDataUrl { get; set; }
    public string? Prompt { get; set; }
    public string? NegativePrompt { get; set; }
    public string? OtherParameters { get; set; }
    public string? Workflow { get; set; }
    public bool IncludeImage { get; set; }
    public bool IncludeMetadata { get; set; }
}
