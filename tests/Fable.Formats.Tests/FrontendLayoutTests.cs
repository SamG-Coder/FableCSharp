using Fable.Core;
using Fable.Formats.Defs;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class FrontendLayoutTests
{
    private static readonly FrontendViewport FirstSeen =
        FrontendLayout.FirstSeenFrontend(1024f, 768f);

    [Fact]
    public void Root_Width_0_stays_dest_0_0_0_0()
    {
        var root = new FrontendWidgetLayout(
            PositionX: 0f,
            PositionY: 0f,
            PersistWidth: 0,
            PersistHeight: 0,
            LeftoverW: 0f,
            LeftoverH: 0f);
        var dest = FrontendLayout.Compute(root, parent: null, FirstSeen);
        Assert.Equal(0f, dest.X0);
        Assert.Equal(0f, dest.Y0);
        Assert.Equal(0f, dest.X1);
        Assert.Equal(0f, dest.Y1);
        Assert.Equal(0x0041AFA0u, FrontendLayout.SubmitDestFn);
        Assert.Equal(360, FrontendLayout.SizeWOffset);
        Assert.Equal(204, FrontendLayout.DestWOffset);
    }

    [Fact]
    public void Scale_init_writes_1_when_plus_280_is_0()
    {
        var one = FrontendLayout.InitInheritedScale(0);
        Assert.Equal(1f, one.ScaleX);
        Assert.Equal(1f, one.ScaleY);
        var left = FrontendLayout.InitInheritedScale(1, 2f, 3f);
        Assert.Equal(2f, left.ScaleX);
        Assert.Equal(3f, left.ScaleY);
        Assert.Equal(272, FrontendLayout.InheritScaleXOffset);
        Assert.Equal(280, FrontendLayout.InheritScaleFlagOffset);
        Assert.Equal(0x005339B0u, FrontendLayout.ScaleWriteFn);
    }

    [Fact]
    public void Press_Start_child_dest_is_nonempty_after_layout()
    {
        var root = FrontendLayout.Compute(
            new FrontendWidgetLayout(0f, 0f),
            parent: null,
            FirstSeen);
        Assert.Equal(0f, root.X0);
        Assert.Equal(0f, root.X1);

        var text = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 16f,
            LeftoverH: 16f);
        var dest = FrontendLayout.Compute(text, root, FirstSeen);
        Assert.True(dest.X1 > dest.X0, $"text dest {dest.X0},{dest.Y0},{dest.X1},{dest.Y1}");
        Assert.True(dest.Y1 > dest.Y0);
        Assert.Equal(320f, dest.OriginX);
        Assert.Equal(240f, dest.OriginY);
        Assert.Equal(320f, dest.X0);
        Assert.Equal(240f, dest.Y0);
        Assert.Equal(336f, dest.X1);
        Assert.Equal(256f, dest.Y1);
    }

    [Fact]
    public void First_seen_does_not_invent_640_480_scale()
    {
        Assert.True(FirstSeen.ResolutionScaleEnabled);
        Assert.Equal(1024f, FirstSeen.Width);
        Assert.Equal(768f, FirstSeen.Height);
        Assert.False(FirstSeen.GamePresent);
        Assert.Equal((1f, 1f), FrontendLayout.GlobalUiScale(FirstSeen));

        var identity = FrontendLayout.ApplyResolutionScale(320f, 240f, FirstSeen);
        Assert.Equal(512f, identity.X);
        Assert.Equal(384f, identity.Y);

        var text = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 10f,
            LeftoverH: 10f);
        var dest = FrontendLayout.Compute(text, parent: null, FirstSeen);
        Assert.Equal(320f, dest.OriginX);
        Assert.Equal(240f, dest.Y0);
        Assert.NotEqual(512f, dest.OriginX);
        Assert.Equal(640f, FrontendLayout.AuthoredWidth);
        Assert.Equal(480f, FrontendLayout.AuthoredHeight);
    }

    [Fact]
    public void Persist_remap_flags_scale_authored_640_480_to_viewport()
    {
        var remapped = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 10f,
            LeftoverH: 10f,
            ScaleOriginToViewport: true,
            ScaleSizeToViewport: true);
        var dest = FrontendLayout.Compute(remapped, parent: null, FirstSeen);
        Assert.Equal(512f, dest.OriginX);
        Assert.Equal(384f, dest.OriginY);
        Assert.Equal(512f, dest.X0);
        Assert.Equal(384f, dest.Y0);
        Assert.Equal(528f, dest.X1);
        Assert.Equal(400f, dest.Y1);
    }

    [Fact]
    public void Children_inherit_parent_dest_scale()
    {
        var parent = new FrontendDest(100f, 40f, 2f, 2f, 100f, 40f, 100f, 40f);
        var child = new FrontendWidgetLayout(
            PositionX: 10f,
            PositionY: 5f,
            LeftoverW: 8f,
            LeftoverH: 4f);
        var dest = FrontendLayout.Compute(child, parent, FirstSeen);
        Assert.Equal(120f, dest.OriginX);
        Assert.Equal(50f, dest.OriginY);
        Assert.Equal(2f, dest.ScaleX);
        Assert.Equal(2f, dest.ScaleY);
        Assert.Equal(120f, dest.X0);
        Assert.Equal(50f, dest.Y0);
        Assert.Equal(136f, dest.X1);
        Assert.Equal(58f, dest.Y1);
    }

    [Fact]
    public void Submit_uses_leftover_when_persist_size_is_0()
    {
        var leftover = FrontendLayout.ComputeSubmitDest(
            0, 0, 64f, 32f, 0f, 0f, 1f, 1f, center: false);
        Assert.Equal((0f, 0f, 64f, 32f), leftover);
        var persist = FrontendLayout.ComputeSubmitDest(
            20, 10, 64f, 32f, 0f, 0f, 1f, 1f, center: false);
        Assert.Equal((0f, 0f, 20f, 10f), persist);
        var centered = FrontendLayout.ComputeSubmitDest(
            10, 10, 0f, 0f, 100f, 100f, 1f, 1f, center: true);
        Assert.Equal((95f, 95f, 105f, 105f), centered);
        Assert.Equal(0.5f, FrontendLayout.Half);
        Assert.Equal(0x0052F1E0u, FrontendLayout.CenterFn);
    }

    [Fact]
    public void Press_Start_persist_positions_are_640_480_pixels()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        var rootPos = FirstPos(bin.FindEntry("UI_FRONTEND_PRESS_START_MENU")!.Raw);
        var titlePos = FirstPos(bin.FindEntry("UI_TITLE")!.Raw);
        var textPos = FirstPos(bin.FindEntry("UI_PRESS_START_TEXT")!.Raw);
        Assert.Equal((0f, 0f), rootPos);
        Assert.Equal((70f, 30f), titlePos);
        Assert.Equal((320f, 240f), textPos);

        var rootDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(rootPos.X, rootPos.Y),
            parent: null,
            FirstSeen);
        Assert.Equal(0f, rootDest.X0);
        Assert.Equal(0f, rootDest.X1);

        var titleDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                titlePos.X, titlePos.Y,
                LeftoverW: 256f, LeftoverH: 128f),
            rootDest,
            FirstSeen);
        Assert.True(titleDest.X1 > titleDest.X0);
        Assert.Equal(70f, titleDest.OriginX);
        Assert.Equal(30f, titleDest.OriginY);
        Assert.True(titleDest.OriginY < 240f);

        var textDest = FrontendLayout.Compute(
            new FrontendWidgetLayout(
                textPos.X, textPos.Y,
                LeftoverW: 16f, LeftoverH: 16f),
            rootDest,
            FirstSeen);
        Assert.True(textDest.X1 > textDest.X0);
        Assert.Equal(320f, textDest.OriginX);
        Assert.Equal(240f, textDest.OriginY);
    }

    private static (float X, float Y) FirstPos(byte[] raw)
    {
        float x = 0f;
        float y = 0f;
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            var crc = BitConverter.ToUInt32(raw, i);
            if (crc == FrontendUiDef.PositionXCrc)
            {
                var value = BitConverter.ToSingle(raw, i + 4);
                if (x == 0f && value != 0f)
                    x = value;
            }
            else if (crc == FrontendUiDef.PositionYCrc)
            {
                var value = BitConverter.ToSingle(raw, i + 4);
                if (y == 0f && value != 0f)
                    y = value;
            }
        }

        return (x, y);
    }

    [Fact]
    public void Y_increases_down_with_no_flip()
    {
        var dest = FrontendLayout.ComputeSubmitDest(
            0, 0, 8f, 8f, 0f, 30f, 1f, 1f, center: false);
        Assert.Equal(30f, dest.Y0);
        Assert.Equal(38f, dest.Y1);
        Assert.True(dest.Y1 > dest.Y0);
    }
}
