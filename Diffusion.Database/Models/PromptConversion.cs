using SQLite;

namespace Diffusion.Database.Models;

public class PromptConversion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ImageId { get; set; }
    public string ImagePathAtCreation { get; set; } = "";
    public string SourceMetadataHash { get; set; } = "";
    public string ProviderBaseUrl { get; set; } = "";
    public string Model { get; set; } = "";
    public string SystemPromptPresetName { get; set; } = "";
    public string SystemPromptSnapshot { get; set; } = "";
    public string AdditionalRequest { get; set; } = "";
    public bool IncludeImage { get; set; }
    public bool IncludeMetadata { get; set; }
    public string PositivePrompt { get; set; } = "";
    public string NegativePrompt { get; set; } = "";
    public string Notes { get; set; } = "";
    public string RawResponse { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
