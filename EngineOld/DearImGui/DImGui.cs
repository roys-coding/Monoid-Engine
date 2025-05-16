
using MonoGame.ImGuiNET;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Hexa.NET.ImPlot;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui.Utilities;

namespace MyMonoGameApp.DearImGui;

public static class DImGui
{
    public const string MAIN_DOCKSPACE_ID = "main_dockspace";

    public static event EventHandler OnDraw;
    private static ImGuiRenderer _imGuiRenderer;
    private static ImGuiFontBuilder _fontBuilder;

    public static ImGuiRenderer Renderer => _imGuiRenderer;

    public static void Initialize(Game game)
    {
        _imGuiRenderer = new(game);
        _fontBuilder = new();

        ImGui.GetIO().ConfigDragClickToInputText = true;
        ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        SetStyle();
        LoadFonts();
    }

    public static void Terminate()
    {
        ImGuiRenderer.Terminate();
        _fontBuilder.Dispose();
    }

    public static void Render(Microsoft.Xna.Framework.GameTime gameTime)
    {
        const ImGuiDockNodeFlags DOCK_FLAGS = ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingOverCentralNode;

        _imGuiRenderer.BeginLayout(gameTime);
        uint dockspaceID = ImGui.GetID(MAIN_DOCKSPACE_ID);
        ImGui.DockSpaceOverViewport(dockspaceID, ImGui.GetMainViewport(), DOCK_FLAGS);

        OnDraw?.Invoke(null, EventArgs.Empty);

        _imGuiRenderer.EndLayout();
    }

    private static void LoadFonts()
    {
        const string FONTS_PATH = "Content/fonts/";
        const float FONT_SIZE = 13.5f;
        const float ICONS_FONT_SIZE = 16f;

        _fontBuilder.AddFontFromFileTTF($"{FONTS_PATH}{Fonts.Bfont.FileName}", FONT_SIZE);
        //_fontBuilder.AddDefaultFont();

        uint minGlyph = Fonts.Lucide.IconMin;
        uint maxGlyph = Fonts.Lucide.IconMax16;
        
        _fontBuilder.SetOption(config =>
        {
            config.MergeMode = true;
            config.FontDataOwnedByAtlas = true;
            config.GlyphOffset.Y = 3f;
        });
        _fontBuilder.AddFontFromFileTTF($"{FONTS_PATH}{Fonts.Lucide.FontIconFileNameLC}", ICONS_FONT_SIZE, new GlyphRanges(minGlyph, maxGlyph, 0));
        
        _imGuiRenderer.RebuildFontAtlas();
    }

    private static void SetStyle()
    {
        System.Numerics.Vector4 Black = new(0f, 0f, 0f, 1.0f);
        System.Numerics.Vector4 White = new(1f, 1f, 1f, 1.0f);
        System.Numerics.Vector4 Dark0 = new(15 / 255f, 15 / 255f, 15 / 255f, 1.0f);
        System.Numerics.Vector4 Dark1 = new(26 / 255f, 26 / 255f, 26 / 255f, 1.0f);
        System.Numerics.Vector4 Dark2 = new(38 / 255f, 38 / 255f, 38 / 255f, 1.0f);
        System.Numerics.Vector4 Medium0 = new(53 / 255f, 53 / 255f, 53 / 255f, 1.0f);
        System.Numerics.Vector4 Medium1 = new(64 / 255f, 64 / 255f, 64 / 255f, 1.0f);
        System.Numerics.Vector4 Medium2 = new(77 / 255f, 77 / 255f, 77 / 255f, 1.0f);
        System.Numerics.Vector4 Light0 = new(89 / 255f, 89 / 255f, 89 / 255f, 1.0f);
        System.Numerics.Vector4 Light1 = new(102 / 255f, 102 / 255f, 102 / 255f, 1.0f);
        System.Numerics.Vector4 Light2 = new(115 / 255f, 115 / 255f, 115 / 255f, 1.0f);
        System.Numerics.Vector4 Lighter = new(128 / 255f, 128 / 255f, 128 / 255f, 1.0f);
        System.Numerics.Vector4 Lightest = new(153 / 255f, 153 / 255f, 153 / 255f, 1.0f);
        System.Numerics.Vector4 Highlight0 = new(151 / 255f, 71 / 255f, 255 / 255f, 1.0f);
        System.Numerics.Vector4 Highlight1 = new(166 / 255f, 98 / 255f, 255 / 255f, 1.0f);
        System.Numerics.Vector4 Highlight2 = new(183 / 255f, 128 / 255f, 255 / 255f, 1.0f);

        var style = ImGui.GetStyle();
        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = White;
        colors[(int)ImGuiCol.TextDisabled] = White.SetAlphaARGB(0.6f);
        colors[(int)ImGuiCol.WindowBg] = Dark2.SetAlphaARGB(0.9f);
        colors[(int)ImGuiCol.ChildBg] = Dark1.SetAlphaARGB(0.9f);
        colors[(int)ImGuiCol.PopupBg] = Dark1.SetAlphaARGB(0.9f);
        colors[(int)ImGuiCol.Border] = Medium0;
        colors[(int)ImGuiCol.BorderShadow] = new(0.00f);
        colors[(int)ImGuiCol.FrameBg] = Dark1.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.FrameBgHovered] = Dark2.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.FrameBgActive] = Medium0.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.TitleBg] = Black.SetAlphaARGB(0.9f);
        colors[(int)ImGuiCol.TitleBgActive] = Dark0;
        colors[(int)ImGuiCol.TitleBgCollapsed] = Dark0.SetAlphaARGB(0.5f);
        colors[(int)ImGuiCol.MenuBarBg] = Dark1;
        colors[(int)ImGuiCol.ScrollbarBg] = Dark1;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = Medium0;
        colors[(int)ImGuiCol.ScrollbarGrab] = Dark2;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = Medium2;
        colors[(int)ImGuiCol.CheckMark] = Medium2;
        colors[(int)ImGuiCol.SliderGrab] = Medium1;
        colors[(int)ImGuiCol.SliderGrabActive] = Light0;
        colors[(int)ImGuiCol.Button] = Medium1.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.ButtonHovered] = Medium2.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.ButtonActive] = Light1.SetAlphaARGB(0.8f);
        colors[(int)ImGuiCol.Header] = Medium1.SetAlphaARGB(0.7f);
        colors[(int)ImGuiCol.HeaderHovered] = Medium2.SetAlphaARGB(0.7f);
        colors[(int)ImGuiCol.HeaderActive] = Light0.SetAlphaARGB(0.7f);
        colors[(int)ImGuiCol.Separator] = new(1f, 1f, 1f, 0.2f);
        colors[(int)ImGuiCol.SeparatorHovered] = new(1f, 1f, 1f, 0.25f);
        colors[(int)ImGuiCol.SeparatorActive] = new(1f, 1f, 1f, 0.35f);
        colors[(int)ImGuiCol.Tab] = Dark0;
        colors[(int)ImGuiCol.TabHovered] = Dark1;
        colors[(int)ImGuiCol.TabSelected] = Dark2;
        colors[(int)ImGuiCol.TabSelectedOverline] = Highlight1;
        colors[(int)ImGuiCol.TabDimmed] = Black;
        colors[(int)ImGuiCol.TabDimmedSelected] = Dark0;
        colors[(int)ImGuiCol.TabDimmedSelectedOverline] = Dark2;
        colors[(int)ImGuiCol.DockingPreview] = Medium2;
        colors[(int)ImGuiCol.DockingEmptyBg] = Medium2;
        colors[(int)ImGuiCol.TableHeaderBg] = Medium1;
        colors[(int)ImGuiCol.TableBorderStrong] = Medium2;
        colors[(int)ImGuiCol.TableBorderLight] = Medium1;
        colors[(int)ImGuiCol.ResizeGrip] = Medium1;
        colors[(int)ImGuiCol.ResizeGripHovered] = Medium2;
        colors[(int)ImGuiCol.ResizeGripActive] = Light1;
        colors[(int)ImGuiCol.PlotLines] = new(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new(1.00f, 0.60f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableRowBg] = new(0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextLink] = new(0.53f, 0.53f, 0.87f, 0.80f);
        colors[(int)ImGuiCol.TextSelectedBg] = new(0.00f, 0.00f, 1.00f, 0.35f);
        colors[(int)ImGuiCol.DragDropTarget] = new(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int)ImGuiCol.NavCursor] = new(0.45f, 0.45f, 0.90f, 0.80f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.20f, 0.20f, 0.20f, 0.35f);

        // Padding.
        style.WindowPadding = new System.Numerics.Vector2(12f, 10f);
        style.FramePadding = new System.Numerics.Vector2(8f, 4f);
        style.CellPadding = new System.Numerics.Vector2(4f, 4f);

        // Spacing.
        style.ItemSpacing = new System.Numerics.Vector2(6f, 6f);
        style.ItemInnerSpacing = new System.Numerics.Vector2(4f, 4f);
        style.SeparatorTextPadding = new System.Numerics.Vector2(12f, 4f);
        style.IndentSpacing = 8f;

        // Sizes.
        style.WindowBorderSize = 1f;
        style.ChildBorderSize = 1f;
        style.PopupBorderSize = 1f;
        style.FrameBorderSize = 1f;
        style.TabBarBorderSize = 0f;
        style.TabBarOverlineSize = 2f;
        style.SeparatorTextBorderSize = 1;
        style.DockingSeparatorSize = 1;
        style.ScrollbarSize = 12f;
        style.GrabMinSize = 12f;

        // Rounding.
        style.WindowRounding = 5f;
        style.ChildRounding = 5f;
        style.ScrollbarRounding = 3f;
        style.GrabRounding = 3f;

        // These look like rounding with value "5"
        style.FrameRounding = 3f;
        style.PopupRounding = 2f;
        // .

        style.TabRounding = 0f;

        // Other options.
        style.WindowMenuButtonPosition = ImGuiDir.Right;
        //style.AntiAliasedLines = false;
    }
}
