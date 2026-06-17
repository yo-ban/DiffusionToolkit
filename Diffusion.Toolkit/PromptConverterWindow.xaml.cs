using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Diffusion.Common;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit;

public partial class PromptConverterWindow : BorderlessWindow
{
    private readonly ImageViewModel _image;
    private readonly ImagePayloadService _imagePayloadService = new();
    private readonly LlmPromptConversionService _conversionService = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private ObservableCollection<SystemPromptPreset> _systemPromptPresets = [];

    public PromptConverterWindow(ImageViewModel image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        InitializeComponent();
        InitializeFields();
    }

    private void InitializeFields()
    {
        PreviewImage.Source = _image.Image;
        SourcePromptText.Text = _image.Prompt ?? "";
        SourceNegativeText.Text = _image.NegativePrompt ?? "";
        SourceParametersText.Text = BuildParametersText();
        SourceRawText.Text = _image.Workflow ?? "";

        var settings = ServiceLocator.Settings;
        ModelText.Text = string.IsNullOrWhiteSpace(settings?.LlmModel) ? "google/gemini-2.5-flash" : settings.LlmModel;

        var canSendImage = _image.Type == ImageType.Image;
        IncludeImageCheck.IsChecked = canSendImage && (settings?.LlmIncludeImage ?? true);
        IncludeImageCheck.IsEnabled = canSendImage;
        IncludeMetadataCheck.IsChecked = settings?.LlmIncludeMetadata ?? true;
        DownscaleCheck.IsChecked = settings?.LlmDownscaleImage ?? true;
        MaxEdgeText.Text = (settings?.LlmMaxImageEdge > 0 ? settings.LlmMaxImageEdge : 1280).ToString();

        _systemPromptPresets = new ObservableCollection<SystemPromptPreset>(NormalizeSystemPrompts(settings));
        SystemPromptBox.ItemsSource = _systemPromptPresets;
        SystemPromptBox.SelectedItem = _systemPromptPresets.FirstOrDefault(d => d.Id == settings?.LlmSelectedSystemPromptId) ?? _systemPromptPresets.FirstOrDefault();
    }

    private string BuildParametersText()
    {
        return $"""
                Model: {_image.ModelName}
                Model hash: {_image.ModelHash}
                Seed: {(_image.Seed == 0 ? "" : _image.Seed)}
                Sampler: {_image.Sampler}
                Steps: {(_image.Steps == 0 ? "" : _image.Steps)}
                CFG scale: {(_image.CFGScale == 0 ? "" : _image.CFGScale)}
                Size: {_image.Width}x{_image.Height}

                {_image.OtherParameters}
                """;
    }

    private async void GenerateButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null) return;

        try
        {
            SaveSettings();
            PersistSettings();

            _cancellationTokenSource = new CancellationTokenSource();
            SetBusy(true, "Preparing request...");

            var includeImage = IncludeImageCheck.IsChecked == true;
            var imageDataUrl = includeImage
                ? await _imagePayloadService.CreateDataUrl(_image.Path, DownscaleCheck.IsChecked == true, GetMaxEdge(), _cancellationTokenSource.Token)
                : null;

            SetBusy(true, "Calling LLM API...");
            var request = BuildRequest(imageDataUrl);
            var result = await _conversionService.Convert(request, _cancellationTokenSource.Token);
            SavePromptConversion(request, result);
            DisplayResult(result);
            StatusText.Text = $"Generated with {result.Model}";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Cancelled.";
        }
        catch (Exception exception)
        {
            StatusText.Text = "Generation failed.";
            RawResponseText.Text = exception.Message;
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            SetBusy(false);
        }
    }

    private PromptConversionRequest BuildRequest(string? imageDataUrl)
    {
        return new PromptConversionRequest
        {
            Provider = new LlmProviderSettings
            {
                BaseUrl = GetConfiguredBaseUrl(),
                ApiKey = LlmApiKeyProtector.Unprotect(ServiceLocator.Settings?.LlmApiKeyProtected),
                Model = ModelText.Text.Trim()
            },
            SystemPrompt = (SystemPromptBox.SelectedItem as SystemPromptPreset)?.Text ?? "",
            SystemPromptPresetName = (SystemPromptBox.SelectedItem as SystemPromptPreset)?.Name ?? "",
            AdditionalRequest = AdditionalRequestText.Text,
            ImagePath = _image.Path,
            ImageDataUrl = imageDataUrl,
            Prompt = _image.Prompt,
            NegativePrompt = _image.NegativePrompt,
            OtherParameters = BuildParametersText(),
            Workflow = _image.Workflow,
            IncludeImage = IncludeImageCheck.IsChecked == true,
            IncludeMetadata = IncludeMetadataCheck.IsChecked == true
        };
    }

    private int GetMaxEdge()
    {
        return int.TryParse(MaxEdgeText.Text, out var value) ? Math.Clamp(value, 256, 4096) : 1280;
    }

    private void SaveSettings()
    {
        var settings = ServiceLocator.Settings;
        if (settings == null) return;

        settings.LlmModel = ModelText.Text.Trim();
        if (IncludeImageCheck.IsEnabled)
        {
            settings.LlmIncludeImage = IncludeImageCheck.IsChecked == true;
        }
        settings.LlmIncludeMetadata = IncludeMetadataCheck.IsChecked == true;
        settings.LlmDownscaleImage = DownscaleCheck.IsChecked == true;
        settings.LlmMaxImageEdge = GetMaxEdge();
        settings.LlmSystemPromptPresets = _systemPromptPresets.ToList();

        if (SystemPromptBox.SelectedItem is SystemPromptPreset selectedPrompt)
        {
            settings.LlmSelectedSystemPromptId = selectedPrompt.Id;
        }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        GenerateButton.IsEnabled = !busy;
        if (status != null) StatusText.Text = status;
    }

    private void ModelText_OnLostFocus(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        PersistSettings();
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        var text = BuildResultText();
        if (string.IsNullOrWhiteSpace(text)) return;
        Clipboard.SetDataObject(text, true);
        StatusText.Text = "Copied result to clipboard.";
    }

    private void CopyPositiveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PositivePromptText.Text)) return;
        Clipboard.SetDataObject(PositivePromptText.Text, true);
        StatusText.Text = "Copied positive prompt to clipboard.";
    }

    private void EditPromptsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var clones = new ObservableCollection<SystemPromptPreset>(_systemPromptPresets.Select(ClonePreset));
        var selectedClone = clones.FirstOrDefault(d => d.Id == (SystemPromptBox.SelectedItem as SystemPromptPreset)?.Id) ?? clones.FirstOrDefault();

        var dialog = new SystemPromptPresetDialog(clones, selectedClone)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true)
        {
            _systemPromptPresets = new ObservableCollection<SystemPromptPreset>(dialog.Presets);
            SystemPromptBox.ItemsSource = _systemPromptPresets;
            SystemPromptBox.SelectedItem = dialog.SelectedPreset != null
                ? _systemPromptPresets.FirstOrDefault(d => d.Id == dialog.SelectedPreset.Id)
                : _systemPromptPresets.FirstOrDefault();
            SaveSettings();
            PersistSettings();
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }

    private void DisplayResult(PromptConversionResult result)
    {
        PositivePromptText.Text = result.PositivePrompt;
        NegativePromptText.Text = result.NegativePrompt;
        NotesText.Text = result.Notes;
        RawResponseText.Text = result.RawResponse;
    }

    private string BuildResultText()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(PositivePromptText.Text))
        {
            builder.AppendLine("[Positive]");
            builder.AppendLine(PositivePromptText.Text);
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(NegativePromptText.Text))
        {
            builder.AppendLine("[Negative]");
            builder.AppendLine(NegativePromptText.Text);
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(NotesText.Text))
        {
            builder.AppendLine("[Notes]");
            builder.AppendLine(NotesText.Text);
            builder.AppendLine();
        }

        if (builder.Length == 0 && !string.IsNullOrWhiteSpace(RawResponseText.Text))
        {
            builder.AppendLine(RawResponseText.Text);
        }

        return builder.ToString().TrimEnd();
    }

    private static List<SystemPromptPreset> NormalizeSystemPrompts(Configuration.Settings? settings)
    {
        if (settings?.LlmSystemPromptPresets is { Count: > 0 })
        {
            return settings.LlmSystemPromptPresets;
        }

        return SystemPromptPreset.CreateDefaults();
    }

    private static SystemPromptPreset ClonePreset(SystemPromptPreset source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<SystemPromptPreset>(json) ?? new SystemPromptPreset();
    }

    private static string GetConfiguredBaseUrl()
    {
        var baseUrl = ServiceLocator.Settings?.LlmBaseUrl;
        return string.IsNullOrWhiteSpace(baseUrl) ? "https://openrouter.ai/api/v1" : baseUrl.Trim();
    }

    private void SavePromptConversion(PromptConversionRequest request, PromptConversionResult result)
    {
        var conversion = new PromptConversion
        {
            ImageId = _image.Id,
            ImagePathAtCreation = _image.Path,
            SourceMetadataHash = Hash(BuildMetadataText(_image)),
            ProviderBaseUrl = request.Provider.BaseUrl,
            Model = request.Provider.Model,
            SystemPromptPresetName = request.SystemPromptPresetName,
            SystemPromptSnapshot = request.SystemPrompt,
            AdditionalRequest = request.AdditionalRequest,
            IncludeImage = request.IncludeImage,
            IncludeMetadata = request.IncludeMetadata,
            PositivePrompt = result.PositivePrompt,
            NegativePrompt = result.NegativePrompt,
            Notes = result.Notes,
            RawResponse = result.RawResponse,
            CreatedAt = DateTime.Now
        };

        ServiceLocator.DataStore.AddPromptConversion(conversion);
        _image.PromptConversions = ServiceLocator.DataStore.GetPromptConversions(_image.Id);
        _image.SelectedPromptConversion = _image.PromptConversions.FirstOrDefault();

        if (ServiceLocator.MainModel.CurrentImageEntry?.Id == _image.Id)
        {
            ServiceLocator.MainModel.CurrentImageEntry.HasPromptConversions = true;
        }
    }

    private static string BuildMetadataText(ImageViewModel image)
    {
        return $"""
                {image.Prompt}
                {image.NegativePrompt}
                {image.OtherParameters}
                {image.Workflow}
                {image.ModelName}
                {image.ModelHash}
                {image.Seed}
                {image.Sampler}
                {image.Steps}
                {image.CFGScale}
                {image.Width}x{image.Height}
                """;
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static void PersistSettings()
    {
        var settings = ServiceLocator.Settings;
        if (settings == null) return;

        var configuration = new Diffusion.Common.Configuration<Configuration.Settings>(AppInfo.SettingsPath, AppInfo.IsPortable);
        configuration.Save(settings);
        settings.SetPristine();
    }
}
