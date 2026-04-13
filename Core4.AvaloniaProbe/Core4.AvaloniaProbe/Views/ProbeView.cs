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
    private readonly GlossaryClaimPageView _glossaryClaimPage = new();
    private readonly GlossaryRelationPageView _glossaryRelationPage = new();
    private readonly GlossaryBasisPageView _glossaryBasisPage = new();
    private readonly GlossaryMeasurementPageView _glossaryMeasurementPage = new();
    private readonly GlossaryTensionPageView _glossaryTensionPage = new();
    private readonly GlossaryFusionPageView _glossaryFusionPage = new();
    private readonly DominantOnlyPageView _dominantOnlyPage = new();
    private readonly InvertedPairPageView _invertedPairPage = new();
    private readonly FrozenSegmentPageView _frozenSegmentPage = new();
    private readonly OrthogonalOutputPageView _orthogonalOutputPage = new();
    private readonly OrthogonalInputPageView _orthogonalInputPage = new();
    private readonly PrimitiveBasesPageView _primitiveBasesPage = new();
    private readonly LockedInvariantPageView _lockedInvariantPage = new();
    private readonly MultipleInputsPageView _multipleInputsPage = new();
    private readonly CalibrationPageView _calibrationPage = new();
    private readonly CarrierChainPageView _carrierChainPage = new();
    private readonly TopologyTrolleyPageView _topologyTrolleyPage = new();
    private readonly BoundaryLoopPageView _boundaryLoopPage = new();
    private readonly CircuitPageView _circuitPage = new();
    private readonly ContentControl _pageHost;
    private readonly StackPanel _pageButtonRow;
    private readonly TextBlock _categoryBlurb;
    private readonly Border _glossaryOverlay;
    private readonly Dictionary<ProbeCategory, Button> _categoryButtons = [];
    private readonly Dictionary<ProbePage, Button> _pageButtons = [];
    private readonly Dictionary<ProbeCategory, ProbePage> _pageSelections = new()
    {
        [ProbeCategory.Glossary] = ProbePage.GlossaryClaim,
        [ProbeCategory.Primitives] = ProbePage.DominantOnly,
        [ProbeCategory.Orthogonals] = ProbePage.OrthogonalOutput,
        [ProbeCategory.Calibration] = ProbePage.CalibrationRolling,
        [ProbeCategory.Topology] = ProbePage.CarrierChain
    };

    private readonly CategoryDefinition[] _categories;
    private readonly PageDefinition[] _pages;
    private ProbeCategory _activeCategory = ProbeCategory.Glossary;
    private ProbePage _activePage = ProbePage.GlossaryClaim;

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
                ProbeCategory.Glossary,
                "Glossary",
                "Foundational Core4 vocabulary shown as small interactive scenes, starting with ordered support, site, and measure."),
            new CategoryDefinition(
                ProbeCategory.Primitives,
                "Primitives",
                "Core4 units, inversion, freezing, and primitive supports."),
            new CategoryDefinition(
                ProbeCategory.Orthogonals,
                "Orthogonals",
                "Orthogonal publication, input, and multi-branch settlement."),
            new CategoryDefinition(
                ProbeCategory.Calibration,
                "Calibration",
                "Small rolling calibraters that turn repeated orthogonal input into a first local origin, unit, and projected value before richer tuning exists."),
            new CategoryDefinition(
                ProbeCategory.Topology,
                "Topology",
                "Connected sites, carrier chains, traversal, and first real-world relaxation scenes built from live Core4 relations.")
        ];

        _pages =
        [
            new PageDefinition(ProbePage.GlossaryClaim, ProbeCategory.Glossary, "Carrier / Site / Measure", "glossary-claim", _glossaryClaimPage),
            new PageDefinition(ProbePage.GlossaryRelation, ProbeCategory.Glossary, "Pin / Origin / Relation", "glossary-relation", _glossaryRelationPage),
            new PageDefinition(ProbePage.GlossaryBasis, ProbeCategory.Glossary, "Freeze / Basis / Attachment", "glossary-basis", _glossaryBasisPage),
            new PageDefinition(ProbePage.GlossaryMeasurement, ProbeCategory.Glossary, "Unit / Ratio / Placement / Resolution", "glossary-measurement", _glossaryMeasurementPage),
            new PageDefinition(ProbePage.GlossaryTension, ProbeCategory.Glossary, "Tension / Fusion / Orthogonal", "glossary-tension", _glossaryTensionPage),
            new PageDefinition(ProbePage.GlossaryFusion, ProbeCategory.Glossary, "Fuse Pairs Into Bases", "glossary-fusion", _glossaryFusionPage),
            new PageDefinition(ProbePage.DominantOnly, ProbeCategory.Primitives, "Dominant Only", "dominant-only", _dominantOnlyPage),
            new PageDefinition(ProbePage.InvertedPair, ProbeCategory.Primitives, "Inverted Pair", "inverted-pair", _invertedPairPage),
            new PageDefinition(ProbePage.FrozenSegment, ProbeCategory.Primitives, "Frozen Segment", "frozen-segment", _frozenSegmentPage),
            new PageDefinition(ProbePage.PrimitiveBases, ProbeCategory.Primitives, "Primitive Bases", "primitive-bases", _primitiveBasesPage),
            new PageDefinition(ProbePage.LockedInvariants, ProbeCategory.Primitives, "Locked Invariants", "locked-invariants", _lockedInvariantPage),
            new PageDefinition(ProbePage.OrthogonalOutput, ProbeCategory.Orthogonals, "Orthogonal Output", "orthogonal-output", _orthogonalOutputPage),
            new PageDefinition(ProbePage.OrthogonalInput, ProbeCategory.Orthogonals, "Orthogonal Input", "orthogonal-input", _orthogonalInputPage),
            new PageDefinition(ProbePage.MultipleInputs, ProbeCategory.Orthogonals, "Multiple Inputs", "multiple-inputs", _multipleInputsPage),
            new PageDefinition(ProbePage.CalibrationRolling, ProbeCategory.Calibration, "Rolling Calibrater", "calibration-rolling", _calibrationPage),
            new PageDefinition(ProbePage.CarrierChain, ProbeCategory.Topology, "Carrier Chain", "carrier-chain", _carrierChainPage),
            new PageDefinition(ProbePage.TopologyTrolley, ProbeCategory.Topology, "Trolley", "trolley", _topologyTrolleyPage),
            new PageDefinition(ProbePage.BoundaryLoop, ProbeCategory.Topology, "Boundaries", "boundaries", _boundaryLoopPage),
            new PageDefinition(ProbePage.Circuit, ProbeCategory.Topology, "Circuit", "circuit", _circuitPage)
        ];

        RestoreStartupRoute();

        _categoryBlurb = ProbeUi.Body(string.Empty, 10d);
        _categoryBlurb.VerticalAlignment = VerticalAlignment.Center;
        _categoryBlurb.MaxWidth = 560;

        _pageButtonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6
        };

        _pageHost = new ContentControl();
        _glossaryOverlay = BuildGlossaryOverlay();

        Content = new Grid
        {
            Children =
            {
                new ScrollViewer
                {
                    Content = new StackPanel
                    {
                        Spacing = 6,
                        Margin = new Thickness(6),
                        Children =
                        {
                            Header(),
                            NavigationCard(),
                            _pageHost
                        }
                    }
                },
                _glossaryOverlay
            }
        };

        SetCategory(_activeCategory);
    }

    private Border Header()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 10,
                Children =
                {
                    ProbeUi.DisplayTitle("Numeric Explorer", 18d),
                    UtilityCluster()
                }
            };
        Grid.SetColumn(grid.Children[1], 1);

        return new Border
        {
            CornerRadius = new CornerRadius(20),
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
            Padding = new Thickness(12, 5),
            Child = grid
        };
    }

    private Border NavigationCard()
    {
        return ProbeUi.Card(new StackPanel
        {
            Spacing = 5,
            Children =
            {
                CategoryRow(),
                new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.Parse("#ded6ca"))
                },
                _pageButtonRow
            }
        }, 18d, new Thickness(10, 6));
    }

    private Control CategoryRow()
    {
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        foreach (var category in _categories)
        {
            var button = ProbeUi.SectionButton(category.Label);
            ToolTip.SetTip(button, category.Blurb);
            button.Click += (_, _) => SetCategory(category.Category);
            button.PointerEntered += (_, _) => PreviewCategory(category.Category);
            button.PointerExited += (_, _) => RestoreCategoryPreview();
            _categoryButtons[category.Category] = button;
            buttons.Children.Add(button);
        }

        var separator = new Border
        {
            Width = 1,
            Margin = new Thickness(0, 2),
            Background = new SolidColorBrush(Color.Parse("#d4cabb"))
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            ColumnSpacing = 8,
            Children =
            {
                buttons,
                separator,
                _categoryBlurb
            }
        };
        Grid.SetColumn(separator, 1);
        Grid.SetColumn(_categoryBlurb, 2);
        return grid;
    }

    private Control UtilityCluster()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var glossaryButton = ProbeUi.PillButton("Glossary");
        glossaryButton.Click += (_, _) => ToggleGlossary(true);
        stack.Children.Add(glossaryButton);

        if (!OperatingSystem.IsBrowser())
        {
            var webButton = ProbeUi.PillButton("Web");
            webButton.Click += async (_, _) =>
            {
                webButton.IsEnabled = false;
                try
                {
                    await WebPreviewLauncher.LaunchAsync(PageFor(_activePage).Route);
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
        _categoryBlurb.Text = definition.Blurb;
        _categoryBlurb.Foreground = preview
            ? ProbePalette.Ink
            : ProbePalette.Muted;
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
        ProbePageState.RememberRoute(definition.Route);
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

    private void RestoreStartupRoute()
    {
        var route = ProbePageState.TryRestoreRoute();

        if (string.IsNullOrWhiteSpace(route) || !TryPageForRoute(route, out var definition))
        {
            return;
        }

        _activeCategory = definition.Category;
        _activePage = definition.Page;
        _pageSelections[definition.Category] = definition.Page;
    }

    private bool TryPageForRoute(string route, out PageDefinition definition)
    {
        foreach (var page in _pages)
        {
            if (string.Equals(page.Route, route, StringComparison.OrdinalIgnoreCase))
            {
                definition = page;
                return true;
            }
        }

        definition = default;
        return false;
    }

    private Border BuildGlossaryOverlay()
    {
        var closeButton = ProbeUi.PillButton("Close");
        closeButton.Click += (_, _) => ToggleGlossary(false);

        var sectionStack = new StackPanel
        {
            Spacing = 14
        };

        foreach (var section in ProbeGlossary.Sections)
        {
            var entryStack = new StackPanel
            {
                Spacing = 8
            };

            foreach (var entry in section.Entries)
            {
                if (entry.DividerBefore)
                {
                    entryStack.Children.Add(new Border
                    {
                        Height = 1,
                        Margin = new Thickness(0, 3, 0, 1),
                        Background = new SolidColorBrush(Color.Parse("#ddd4c7"))
                    });
                }

                var heading = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                    ColumnSpacing = 8
                };
                var term = ProbeUi.Header(entry.Term, 11d);
                var summary = ProbeUi.Mono(entry.Summary, 10d, ProbePalette.Muted);
                heading.Children.Add(term);
                heading.Children.Add(summary);
                Grid.SetColumn(summary, 1);

                entryStack.Children.Add(new StackPanel
                {
                    Spacing = 3,
                    Children =
                    {
                        heading,
                        ProbeUi.Body(entry.Description, 11d)
                    }
                });
            }

            sectionStack.Children.Add(ProbeUi.Card(new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    ProbeUi.Mono(section.Title, 10d, ProbePalette.Muted),
                    ProbeUi.Body(section.Blurb, 11d),
                    entryStack
                }
            }, 22d, new Thickness(14, 12)));
        }

        var titleRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 10,
            Children =
            {
                new StackPanel
                {
                    Spacing = 2,
                    Children =
                    {
                        ProbeUi.DisplayTitle("Glossary", 20d),
                        ProbeUi.Body("Core4 terms grouped by how they are created, measured, transformed, and inspected.", 11d)
                    }
                },
                closeButton
            }
        };
        Grid.SetColumn(closeButton, 1);

        var bodyScroll = new ScrollViewer
        {
            Content = sectionStack
        };
        Grid.SetRow(bodyScroll, 1);

        var card = new Border
        {
            Width = 780,
            MaxWidth = 780,
            MaxHeight = 720,
            Background = ProbePalette.Surface,
            BorderBrush = new SolidColorBrush(Color.Parse("#ddd6ca")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(26),
            Padding = new Thickness(16, 14),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 12,
                Children =
                {
                    titleRow,
                    bodyScroll
                }
            }
        };
        card.PointerPressed += (_, args) => args.Handled = true;

        var overlay = new Border
        {
            IsVisible = false,
            Background = new SolidColorBrush(Color.FromArgb(150, 33, 26, 20)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                Children = { card }
            }
        };
        overlay.PointerPressed += (_, _) => ToggleGlossary(false);
        return overlay;
    }

    private void ToggleGlossary(bool visible) => _glossaryOverlay.IsVisible = visible;

    private readonly record struct CategoryDefinition(ProbeCategory Category, string Label, string Blurb);

    private readonly record struct PageDefinition(ProbePage Page, ProbeCategory Category, string Label, string Route, Control View);

    private enum ProbeCategory
    {
        Glossary,
        Primitives,
        Orthogonals,
        Calibration,
        Topology
    }

    private enum ProbePage
    {
        GlossaryClaim,
        GlossaryRelation,
        GlossaryBasis,
        GlossaryMeasurement,
        GlossaryTension,
        GlossaryFusion,
        DominantOnly,
        InvertedPair,
        FrozenSegment,
        PrimitiveBases,
        LockedInvariants,
        OrthogonalOutput,
        OrthogonalInput,
        MultipleInputs,
        CalibrationRolling,
        CarrierChain,
        TopologyTrolley,
        BoundaryLoop,
        Circuit
    }
}
