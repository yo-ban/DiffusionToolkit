namespace Diffusion.Toolkit.Models;

/// <summary>
/// Editable representation of a sidebar navigation entry shown in the Settings
/// "Navigation" tab. <see cref="Key"/> and <see cref="DisplayName"/> are fixed;
/// only <see cref="Visible"/> (and collection order) are user-editable.
/// </summary>
public class NavEditItem : BaseNotify
{
    public string Key { get; }

    public string DisplayName { get; }

    private bool _visible;
    public bool Visible
    {
        get => _visible;
        set => SetField(ref _visible, value);
    }

    public NavEditItem(string key, string displayName, bool visible)
    {
        Key = key;
        DisplayName = displayName;
        _visible = visible;
    }
}
