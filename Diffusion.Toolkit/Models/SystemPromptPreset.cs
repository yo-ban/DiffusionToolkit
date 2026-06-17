using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Diffusion.Toolkit.Models;

public class SystemPromptPreset : INotifyPropertyChanged
{
    private string _id = Guid.NewGuid().ToString("N");
    private string _name = "";
    private string _text = "";
    private DateTime _updatedAt = DateTime.UtcNow;

    public string Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => SetField(ref _updatedAt, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static List<SystemPromptPreset> CreateDefaults()
    {
        return
        [
            new()
            {
                Name = "Flux Prompt Converter",
                Text = """
                       You are a prompt conversion assistant for AI image generation.

                       Convert the selected generated image and its metadata into a prompt optimized for Flux-style image models.

                       Rules:
                       - Return only valid JSON.
                       - Do not wrap the JSON in markdown.
                       - Use natural descriptive language rather than excessive weighted tags.
                       - Preserve subject, composition, lighting, style, camera angle, clothing, environment, and mood.
                       - When metadata and image content conflict, prioritize visible image content and mention the conflict in notes.
                       - Avoid copying sampler, seed, CFG, or UI-specific parameters into the prompt.
                       - Keep negative_prompt short. If the target model normally does not need a negative prompt, keep it minimal.

                       JSON schema:
                       {
                         "positive_prompt": "...",
                         "negative_prompt": "...",
                         "notes": "..."
                       }
                       """
            },
            new()
            {
                Name = "SDXL Prompt Converter",
                Text = """
                       You are a prompt conversion assistant for AI image generation.

                       Convert the selected generated image and its metadata into a prompt optimized for SDXL.

                       Rules:
                       - Return only valid JSON.
                       - Do not wrap the JSON in markdown.
                       - Preserve important tags from the original prompt when they are still useful.
                       - Use comma-separated visual descriptors where appropriate.
                       - Include subject, composition, style, lighting, quality descriptors, camera/lens cues, and background.
                       - Build a practical negative prompt for common SDXL artifacts.
                       - Do not include seed, sampler, steps, CFG scale, file paths, or application-specific text.

                       JSON schema:
                       {
                         "positive_prompt": "...",
                         "negative_prompt": "...",
                         "notes": "..."
                       }
                       """
            },
            new()
            {
                Name = "Pony / Illustrious Tag Converter",
                Text = """
                       You are a prompt conversion assistant for anime-style diffusion models.

                       Convert the selected generated image and its metadata into a tag-oriented prompt.

                       Rules:
                       - Return only valid JSON.
                       - Do not wrap the JSON in markdown.
                       - Use concise comma-separated tags.
                       - Preserve character count, pose, outfit, expression, framing, style, background, and lighting.
                       - Use model-family-friendly quality tags only when appropriate.
                       - Keep negative_prompt useful and not excessively long.
                       - Remove tool-specific syntax that is unlikely to transfer.

                       JSON schema:
                       {
                         "positive_prompt": "...",
                         "negative_prompt": "...",
                         "notes": "..."
                       }
                       """
            }
        ];
    }
}
