using System.Collections.Generic;
using FontAwesome5;

namespace Diffusion.Toolkit.Models;

/// <summary>
/// View model for a single main sidebar navigation button. Holds the runtime
/// metadata (url, icon, tooltip) that is NOT persisted — those come from
/// <see cref="Master"/>, keyed by the stable <see cref="Key"/>. Only the order
/// and visibility are user-configurable (see <c>NavigationBarSettings</c>).
/// </summary>
public class NavItemViewModel : BaseNotify
{
    /// <summary>Stable identifier matching a <c>NavEntry.Key</c>.</summary>
    public string Key { get; }

    /// <summary>Navigator target, e.g. "search/#folders".</summary>
    public string Url { get; }

    /// <summary>The <see cref="MainModel.ActiveView"/> value that selects this item.</summary>
    public string ActiveView { get; }

    /// <summary>FontAwesome icon enum (bindable directly to fa:ImageAwesome.Icon).</summary>
    public EFontAwesomeIcon Icon { get; }

    /// <summary>Localized tooltip text, resolved at build time via GetLocalizedText.</summary>
    public string ToolTipText { get; }

    private bool _isCurrent;
    /// <summary>Whether this item represents the currently active view (drives the selection bar).</summary>
    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetField(ref _isCurrent, value);
    }

    public NavItemViewModel(string key, string url, string activeView, EFontAwesomeIcon icon, string toolTipText)
    {
        Key = key;
        Url = url;
        ActiveView = activeView;
        Icon = icon;
        ToolTipText = toolTipText;
    }

    /// <summary>
    /// Master definition table: the single source of truth for each nav item's
    /// url / active-view / icon / tooltip-key. Keyed by the stable identifier.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string Url, string ActiveView, EFontAwesomeIcon Icon, string ToolTipKey)> Master =
        new Dictionary<string, (string, string, EFontAwesomeIcon, string)>
        {
            { "folders",     ("search/#folders",   "Folders",      EFontAwesomeIcon.Solid_FolderOpen, "MainWindow.Nav.Folders.ToolTip") },
            { "search",      ("search/#images",    "Diffusions",   EFontAwesomeIcon.Solid_Search,      "MainWindow.Nav.Search.ToolTip") },
            { "favorites",   ("search/#favorites", "Favorites",    EFontAwesomeIcon.Regular_Heart,     "MainWindow.Nav.Favorites.ToolTip") },
            { "fordeletion", ("search/#deleted",   "For Deletion", EFontAwesomeIcon.Regular_TrashAlt,  "MainWindow.Nav.ForDeletion.ToolTip") },
            { "prompts",     ("prompts",           "Prompts",      EFontAwesomeIcon.Regular_FileAlt,   "MainWindow.Nav.Prompts.ToolTip") },
            { "models",      ("models",            "Models",       EFontAwesomeIcon.Solid_PuzzlePiece, "MainWindow.Nav.Models.ToolTip") },
        };
}
