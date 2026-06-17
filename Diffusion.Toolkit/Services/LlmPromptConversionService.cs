using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class LlmPromptConversionService
{
    private static readonly HttpClient Client = new();

    public async Task<PromptConversionResult> Convert(PromptConversionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Provider.BaseUrl))
        {
            throw new InvalidOperationException("Base URL is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Provider.Model))
        {
            throw new InvalidOperationException("Model is required.");
        }

        var endpoint = request.Provider.BaseUrl.TrimEnd('/') + "/chat/completions";
        var systemPrompt = BuildSystemPrompt(request);
        var userPrompt = BuildUserPrompt(request);

        object content = request.IncludeImage && !string.IsNullOrWhiteSpace(request.ImageDataUrl)
            ? new object[]
            {
                new { type = "text", text = userPrompt },
                new { type = "image_url", image_url = new { url = request.ImageDataUrl } }
            }
            : new object[]
            {
                new { type = "text", text = userPrompt }
            };

        var body = new
        {
            model = request.Provider.Model.Trim(),
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content
                }
            },
            temperature = 0.4
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        if (!string.IsNullOrWhiteSpace(request.Provider.ApiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Provider.ApiKey.Trim());
        }

        if (request.Provider.BaseUrl.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase))
        {
            httpRequest.Headers.TryAddWithoutValidation("HTTP-Referer", "https://github.com/RupertAvery/DiffusionToolkit");
            httpRequest.Headers.TryAddWithoutValidation("X-OpenRouter-Title", "Diffusion Toolkit");
        }

        using var response = await Client.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"LLM request failed: {(int)response.StatusCode} {response.ReasonPhrase}\r\n{ReadError(responseText)}");
        }

        var contentText = ReadContent(responseText);
        var result = ParseResult(contentText);
        result.RawResponse = contentText;
        result.Model = request.Provider.Model.Trim();
        return result;
    }

    private static PromptConversionResult ParseResult(string responseText)
    {
        var json = ExtractJson(responseText);
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var result = JsonSerializer.Deserialize<PromptConversionResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    result.RawResponse = responseText;
                    return result;
                }
            }
            catch
            {
                // Fall back to raw text below.
            }
        }

        return new PromptConversionResult
        {
            PositivePrompt = responseText,
            RawResponse = responseText
        };
    }

    private static string ExtractJson(string responseText)
    {
        var trimmed = responseText.Trim();
        if (trimmed.StartsWith("```"))
        {
            trimmed = Regex.Replace(trimmed, "^```(?:json)?\\s*", "", RegexOptions.IgnoreCase);
            trimmed = Regex.Replace(trimmed, "\\s*```$", "");
        }

        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return trimmed[firstBrace..(lastBrace + 1)];
        }

        return trimmed;
    }

    private static string ReadContent(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        var message = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message");

        if (!message.TryGetProperty("content", out var content))
        {
            return responseText;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? "";
        }

        if (content.ValueKind == JsonValueKind.Array)
        {
            return string.Join("\r\n", content.EnumerateArray()
                .Where(d => d.TryGetProperty("type", out var type) && type.GetString() == "text")
                .Select(d => d.TryGetProperty("text", out var text) ? text.GetString() : null)
                .Where(d => !string.IsNullOrWhiteSpace(d)));
        }

        return responseText;
    }

    private static string ReadError(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.String) return error.GetString() ?? responseText;
                if (error.TryGetProperty("message", out var message)) return message.GetString() ?? responseText;
            }
        }
        catch
        {
            // Return the raw server response below.
        }

        return responseText;
    }

    private static string BuildSystemPrompt(PromptConversionRequest request)
    {
        var basePrompt = string.IsNullOrWhiteSpace(request.SystemPrompt)
            ? "You convert AI image metadata and visual references into practical prompts for other image generation models. Return only valid JSON with positive_prompt, negative_prompt, and notes."
            : request.SystemPrompt.Trim();

        var imageContext = request.IncludeImage && !string.IsNullOrWhiteSpace(request.ImageDataUrl)
            ? "An image is attached in the user message. Use it as the visual ground truth. Treat metadata as helpful context, but prefer the visible image if metadata and image conflict."
            : "No image is attached. Use only the supplied source metadata.";

        var metadataContext = request.IncludeMetadata
            ? $"""

               Source context for this conversion:

               Source positive prompt:
               {Trim(request.Prompt, 6000)}

               Source negative prompt:
               {Trim(request.NegativePrompt, 3000)}

               Source parameters:
               {Trim(request.OtherParameters, 4000)}

               Source workflow / raw metadata:
               {Trim(request.Workflow, 6000)}
               """
            : "Source metadata was intentionally omitted.";

        return $"""
                {basePrompt}

                Runtime context:
                {imageContext}

                {metadataContext}
                """;
    }

    private static string BuildUserPrompt(PromptConversionRequest request)
    {
        const string basePrompt = "Convert this AI-generated image asset into a prompt. Use the attached image and source context according to the system instructions.";
        if (string.IsNullOrWhiteSpace(request.AdditionalRequest))
        {
            return basePrompt;
        }

        return $"""
                {basePrompt}

                Additional request:
                {request.AdditionalRequest.Trim()}
                """;
    }

    private static string Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return "(none)";
        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength] + "\r\n...(truncated)";
    }
}
