using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Rendering;
using Core4.AvaloniaProbe.Services;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Top-level shell for the Avalonia probe. Categories choose the current family of demos, while the page row narrows
/// to the demos inside that family and remembers the last page visited for each category.
/// </summary>
public sealed class ProbeView : UserControl
{
    private readonly DominantOnlyPageView _dominantOnlyPage = new();
    private readonly InvertedPairPageView _invertedPairPage = new();
    private readonly FrozenSegmentPageView _frozenSegmentPage = new();
    private readonly OrthogonalOutputPageView _orthogonalOutputPage = new();
    private readonly OrthogonalInputPageView _orthogonalInputPage = new();
    private readonly PrimitiveBasesPageView _primitiveBasesPage = new();
    private readonly MultipleInputsPageView _multipleInputsPage = new();
    private readonly CarrierChainPageView _carrierChainPage = new();
    private readonly ContentControl _pageHost;
    private readonly StackPanel _pageButtonRow;
    private readonly TextBlock _sectionLabel;
    private readonly TextBlock _sectionTitle;
    private readonly TextBlock _sectionBody;
    private readonly Dictionary<ProbeCategory, Button> _categoryButtons = [];
    private readonly Dictionary<ProbePage, Button> _pageButtons = [];
    private readonly Dictionary<ProbeCategory, ProbePage> _pageSelections = new()
    {
        [ProbeCategory.Primitives] = ProbePage.DominantOnly,
        [ProbeCategory.Orthogonals] = ProbePage.OrthogonalOutput,
        [ProbeCategory.Topology] = ProbePage.CarrierChain
    };

    private readonly CategoryDefinition[] _categories;
    private readonly PageDefinition[] _pages;
    private ProbeCategory _activeCategory = ProbeCategory.Primitives;
    private ProbePage _activePage = ProbePage.DominantOnly;

    public ProbeView()
    {
        Background = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0d, 1d, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#fbf8f3"), 0d),
                new GradientStop(Color.Parse("#f1eadf"), 1d)
            ]
        };

        _categories =
        [
            new CategoryDefinition(
                ProbeCategory.Primitives,
                "Primitives",
                "Core4 units, inversion, freezing, and primitive supports."),
            new CategoryDefinition(
                ProbeCategory.Orthogonals,
                "Orthogonals",
                "Orthogonal publication, input, and multi-branch settlement."),
            new CategoryDefinition(
                ProbeCategory.Topology,
                "Topology",
                "Connected sites, carrier chains, and connector structure built from live Core4 relations.")
        ];

        _pages =
        [
            new PageDefinition(ProbePage.DominantOnly, ProbeCategory.Primitives, "Dominant Only", _dominantOnlyPage),
            new PageDefinition(ProbePage.InvertedPair, ProbeCategory.Primitives, "Inverted Pair", _invertedPairPage),
            new PageDefinition(ProbePage.FrozenSegment, ProbeCategory.Primitives, "Frozen Segment", _frozenSegmentPage),
            new PageDefinition(ProbePage.PrimitiveBases, ProbeCategory.Primitives, "Primitive Bases", _primitiveBasesPage),
            new PageDefinition(ProbePage.OrthogonalOutput, ProbeCategory.Orthogonals, "Orthogonal Output", _orthogonalOutputPage),
            new PageDefinition(ProbePage.OrthogonalInput, ProbeCategory.Orthogonals, "Orthogonal Input", _orthogonalInputPage),
            new PageDefinition(ProbePage.MultipleInputs, ProbeCategory.Orthogonals, "Multiple Inputs", _multipleInputsPage),
            new PageDefinition(ProbePage.CarrierChain, ProbeCategory.Topology, "Carrier Chain", _carrierChainPage)
        ];

        _sectionLabel = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);
        _sectionTitle = ProbeUi.DisplayTitle(string.Empty, 18d);
        _sectionBody = ProbeUi.Body(string.Empty, 10d);

        _pageButtonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        _pageHost = new ContentControl();

        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(8),
                Children =
                {
                    Header(),
                    NavigationCard(),
                    _pageHost
                }
            }
        };

        SetCategory(_activeCategory);
    }

    private Border Header()
    {
        var titleStack = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                ProbeUi.DisplayTitle("Numeric Explorer", 22d),
                ProbeUi.Body("Browse Core4 demos by family and drill into one page at a time.", 10d)
            }
        };

        var sectionCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                _sectionLabel,
                _sectionTitle,
                _sectionBody
            }
        }, 20d, new Thickness(12, 10));

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 10,
            Children =
            {
                titleStack,
                sectionCard,
                UtilityCluster()
            }
        };
        Grid.SetColumn(sectionCard, 1);
        Grid.SetColumn(grid.Children[2], 2);

        return new Border
        {
            CornerRadius = new CornerRadius(24),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1d, 1d, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.Parse("#f5f0e4"), 0d),
                    new GradientStop(Color.Parse("#f8f4ec"), 0.55d),
                    new GradientStop(Color.Parse("#f3dfc6"), 1d)
                ]
            },
            BorderBrush = new SolidColorBrush(Color.Parse("#dfd7c8")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 10),
            Child = grid
        };
    }

    private Border NavigationCard()
    {
        return ProbeUi.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                ProbeUi.Mono("NAVIGATION", 10d, ProbePalette.Muted),
                CategoryRow(),
                new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.Parse("#ded6ca"))
                },
                _pageButtonRow
            }
        }, 20d, new Thickness(12, 10));
    }

    private StackPanel CategoryRow()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        foreach (var category in _categories)
        {
            var button = ProbeUi.SectionButton(category.Label);
            ToolTip.SetTip(button, category.Blurb);
            button.Click += (_, _) => SetCategory(category.Category);
            button.PointerEntered += (_, _) => PreviewCategory(category.Category);
            button.PointerExited += (_, _) => RestoreCategoryPreview();
            _categoryButtons[category.Category] = button;
            row.Children.Add(button);
        }

        return row;
    }

    private Control UtilityCluster()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        if (!OperatingSystem.IsBrowser())
        {
            var webButton = ProbeUi.PillButton("Web");
            webButton.Click += async (_, _) =>
            {
                webButton.IsEnabled = false;
                try
                {
                    await WebPreviewLauncher.LaunchAsync();
                }
                catch (Exception ex)
                {
                    webButton.Content = "Web failed";
                    ToolTip.SetTip(webButton, ex.Message);
                }
                finally
                {
                    webButton.IsEnabled = true;
                }
            };

            stack.Children.Add(webButton);
        }

        return stack;
    }

    private void SetCategory(ProbeCategory category)
    {
        _activeCategory = category;
        ApplyCategoryStyles();
        RebuildPageRow();
        ShowCategory(category, preview: false);
        SetPage(_pageSelections[category]);
    }

    private void PreviewCategory(ProbeCategory category) => ShowCategory(category, preview: category != _activeCategory);

    private void RestoreCategoryPreview() => ShowCategory(_activeCategory, preview: false);

    private void ShowCategory(ProbeCategory category, bool preview)
    {
        var definition = CategoryFor(category);
        _sectionLabel.Text = preview ? "SECTION PREVIEW" : "ACTIVE SECTION";
        _sectionTitle.Text = definition.Label;
        _sectionBody.Text = definition.Blurb;
    }

    private void RebuildPageRow()
    {
        _pageButtonRow.Children.Clear();

        foreach (var page in PagesFor(_activeCategory))
        {
            if (!_pageButtons.TryGetValue(page.Page, out var button))
            {
                button = ProbeUi.PillButton(page.Label);
                var target = page.Page;
                button.Click += (_, _) => SetPage(target);
                _pageButtons[target] = button;
            }

            _pageButtonRow.Children.Add(button);
        }

        ApplyPageStyles();
    }

    private void SetPage(ProbePage page)
    {
        var definition = PageFor(page);
        _activePage = page;
        _pageSelections[definition.Category] = page;
        _pageHost.Content = definition.View;
        ApplyPageStyles();
    }

    private void ApplyCategoryStyles()
    {
        foreach (var category in _categories)
        {
            ProbeUi.ApplySectionStyle(_categoryButtons[category.Category], category.Category == _activeCategory);
        }
    }

    private void ApplyPageStyles()
    {
        foreach (var page in _pages)
        {
            if (_pageButtons.TryGetValue(page.Page, out var button))
            {
                ProbeUi.ApplyPillStyle(button, page.Page == _activePage);
            }
        }
    }

    private CategoryDefinition CategoryFor(ProbeCategory category)
    {
        foreach (var definition in _categories)
        {
            if (definition.Category == category)
            {
                return definition;
            }
        }

        return _categories[0];
    }

    private List<PageDefinition> PagesFor(ProbeCategory category)
    {
        var pages = new List<PageDefinition>();

        foreach (var definition in _pages)
        {
            if (definition.Category == category)
            {
                pages.Add(definition);
            }
        }

        return pages;
    }

    private PageDefinition PageFor(ProbePage page)
    {
        foreach (var definition in _pages)
        {
            if (definition.Page == page)
            {
                return definition;
            }
        }

        return _pages[0];
    }

    private readonly record struct CategoryDefinition(ProbeCategory Category, string Label, string Blurb);

    private readonly record struct PageDefinition(ProbePage Page, ProbeCategory Category, string Label, Control View);

    private enum ProbeCategory
    {
        Primitives,
        Orthogonals,
        Topology
    }

    private enum ProbePage
    {
        DominantOnly,
        InvertedPair,
        FrozenSegment,
        OrthogonalOutput,
        OrthogonalInput,
        PrimitiveBases,
        MultipleInputs,
        CarrierChain
    }
}
