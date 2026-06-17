using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public partial class SystemPromptPresetDialog : BorderlessWindow
{
    private bool _isUpdating;

    public ObservableCollection<SystemPromptPreset> Presets { get; }

    public SystemPromptPreset? SelectedPreset => PresetsList.SelectedItem as SystemPromptPreset;

    public SystemPromptPresetDialog(ObservableCollection<SystemPromptPreset> presets, SystemPromptPreset? selectedPreset)
    {
        Presets = presets;
        InitializeComponent();
        PresetsList.ItemsSource = Presets;
        PresetsList.SelectedItem = selectedPreset ?? Presets.FirstOrDefault();
        UpdateEditor();
    }

    private void PresetsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateEditor();
    }

    private void UpdateEditor()
    {
        _isUpdating = true;
        try
        {
            NameText.Text = SelectedPreset?.Name ?? "";
            PromptText.Text = SelectedPreset?.Text ?? "";
            NameText.IsEnabled = SelectedPreset != null;
            PromptText.IsEnabled = SelectedPreset != null;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void NameText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || SelectedPreset == null) return;
        SelectedPreset.Name = NameText.Text;
        SelectedPreset.UpdatedAt = DateTime.UtcNow;
    }

    private void PromptText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || SelectedPreset == null) return;
        SelectedPreset.Text = PromptText.Text;
        SelectedPreset.UpdatedAt = DateTime.UtcNow;
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var preset = new SystemPromptPreset
        {
            Name = "New Preset",
            Text = """
                   You are a prompt conversion assistant for AI image generation.

                   Return only valid JSON. Do not wrap the JSON in markdown.

                   JSON schema:
                   {
                     "positive_prompt": "...",
                     "negative_prompt": "...",
                     "notes": "..."
                   }
                   """
        };

        Presets.Add(preset);
        PresetsList.SelectedItem = preset;
    }

    private void DuplicateButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedPreset == null) return;

        var copy = Clone(SelectedPreset);
        copy.Id = Guid.NewGuid().ToString("N");
        copy.Name = $"{copy.Name} Copy";
        copy.UpdatedAt = DateTime.UtcNow;

        Presets.Add(copy);
        PresetsList.SelectedItem = copy;
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedPreset == null || Presets.Count <= 1) return;

        var index = Presets.IndexOf(SelectedPreset);
        Presets.Remove(SelectedPreset);
        PresetsList.SelectedIndex = Math.Max(0, index - 1);
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static SystemPromptPreset Clone(SystemPromptPreset source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<SystemPromptPreset>(json) ?? new SystemPromptPreset();
    }
}
