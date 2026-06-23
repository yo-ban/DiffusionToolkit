using System.Collections.Generic;
using System.Linq;

namespace Diffusion.Toolkit.Configuration;

/// <summary>
/// A single sidebar navigation entry persisted in config.json.
/// Only <see cref="Key"/> and <see cref="Visible"/> are stored; icon/url/tooltip
/// are resolved at runtime from <see cref="Models.NavItemViewModel"/>'s master table.
/// </summary>
public class NavEntry
{
    public string Key { get; set; }
    public bool Visible { get; set; }
}

/// <summary>
/// Persisted, reorderable/show-hide configuration for the main sidebar buttons.
/// Mirrors the <see cref="NavigationSectionSettings"/> pattern: a <see cref="SettingsContainer"/>
/// subclass whose <see cref="Attach"/> propagates dirtiness to the parent <see cref="Settings"/>.
/// </summary>
public class NavigationBarSettings : SettingsContainer
{
    /// <summary>Canonical keys in their default display order.</summary>
    public static readonly IReadOnlyList<string> CanonicalKeys = new[]
    {
        "folders", "search", "favorites", "fordeletion", "prompts", "models"
    };

    public List<NavEntry> Items
    {
        get;
        set => UpdateList(ref field, value);
    }

    public NavigationBarSettings()
    {
        Items = CanonicalKeys.Select(k => new NavEntry { Key = k, Visible = true }).ToList();
    }

    /// <summary>
    /// Reconciles the persisted <see cref="Items"/> with <see cref="CanonicalKeys"/>:
    /// drops unknown keys, appends any missing canonical keys (default Visible=true),
    /// and seeds all-visible defaults when the list is null/empty. Safe to call after
    /// deserialization of an older config that predates this feature.
    /// </summary>
    public void Normalize()
    {
        if (Items == null || Items.Count == 0)
        {
            Items = CanonicalKeys.Select(k => new NavEntry { Key = k, Visible = true }).ToList();
            return;
        }

        var known = new HashSet<string>(CanonicalKeys);
        var ordered = new List<NavEntry>();

        // Preserve persisted order, dropping unknown and duplicate keys.
        foreach (var entry in Items)
        {
            if (entry != null && entry.Key != null && known.Contains(entry.Key) &&
                ordered.All(e => e.Key != entry.Key))
            {
                ordered.Add(entry);
            }
        }

        // Append any canonical keys missing from the persisted list.
        foreach (var key in CanonicalKeys)
        {
            if (ordered.All(e => e.Key != key))
            {
                ordered.Add(new NavEntry { Key = key, Visible = true });
            }
        }

        Items = ordered;
    }

    public void Attach(Settings settings)
    {
        SettingChanged += (sender, args) =>
        {
            settings.SetDirty();
        };
    }
}
