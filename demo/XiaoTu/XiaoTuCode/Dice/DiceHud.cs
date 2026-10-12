using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>
/// Draws the player's dice above the character in combat (like the Defect's orbs): one die face per die with its pips,
/// temporary dice in blue (orange rim while borrowed), locked dice in gold, and pledged dice as grey blanks with the
/// number of turns left. Built from plain Godot nodes and generated textures, no custom Godot scripts needed.
/// It redraws on <see cref="DiceState.Changed"/>.
/// </summary>
public static class DiceHud
{
    private const int Face = 48;
    private const int Gap = 6;
    private const string RootName = "XiaoTuDiceHud";

    private static readonly ConditionalWeakTable<Player, Binding> Bindings = new();
    private static readonly Dictionary<string, ImageTexture> Textures = new();

    private sealed class Binding
    {
        public bool Scheduled;
        public Action? Handler;
    }

    /// <summary>Hooks the HUD to the player's dice (idempotent); call at combat start and turn start.</summary>
    public static void Bind(Player player)
    {
        var binding = Bindings.GetOrCreateValue(player);
        if (binding.Handler == null)
        {
            binding.Handler = () => Schedule(player, binding);
            DiceState.Of(player).Changed += binding.Handler;
        }
        Schedule(player, binding);
    }

    private static void Schedule(Player player, Binding binding)
    {
        if (binding.Scheduled) return;
        binding.Scheduled = true;
        Callable.From(() =>
        {
            binding.Scheduled = false;
            try
            {
                Refresh(player);
            }
            catch (Exception e)
            {
                MainFile.Logger.Warn($"[dice hud] refresh failed: {e.Message}");
            }
        }).CallDeferred();
    }

    private static void Refresh(Player player)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
        if (node == null || !GodotObject.IsInstanceValid(node)) return;

        var root = node.GetNodeOrNull<Control>(RootName);
        if (root == null)
        {
            root = new Control { Name = RootName, MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 20 };
            node.AddChild(root);
        }

        foreach (var child in root.GetChildren())
        {
            root.RemoveChild(child);
            child.QueueFree();
        }

        var state = DiceState.Of(player);
        var pledged = state.PledgeTurns;
        var count = state.Dice.Count + pledged.Count;
        if (count == 0)
        {
            root.Visible = false;
            return;
        }

        root.Visible = true;
        var x = 0;
        foreach (var die in state.Dice)
        {
            var kind = die.Fixed ? "fixed" : die.Temporary ? (die.Extra > 0 ? "borrowed" : "temp") : "die";
            root.AddChild(new TextureRect
            {
                Texture = FaceTexture(die.Value, kind),
                Position = new Vector2(x, 0),
                Size = new Vector2(Face, Face),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                MouseFilter = Control.MouseFilterEnum.Ignore
            });
            x += Face + Gap;
        }

        foreach (var turns in pledged)
        {
            root.AddChild(new TextureRect
            {
                Texture = FaceTexture(0, "pledged"),
                Position = new Vector2(x, 0),
                Size = new Vector2(Face, Face),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                MouseFilter = Control.MouseFilterEnum.Ignore
            });
            root.AddChild(MakeLabel(turns.ToString(), 26, new Vector2(x, 4), new Vector2(Face, Face - 8), center: true));
            x += Face + Gap;
        }

        var width = x - Gap;
        root.AddChild(MakeLabel($"总骰点 {state.Total}", 18, new Vector2(0, Face + 2), new Vector2(width, 24), center: true));

        // Centered above the character's hitbox.
        root.GlobalPosition = node.GetTopOfHitbox() + new Vector2(-width / 2f, -Face - 110);
    }

    private static Label MakeLabel(string text, int size, Vector2 pos, Vector2 boxSize, bool center)
    {
        var label = new Label
        {
            Text = text,
            Position = pos,
            Size = boxSize,
            HorizontalAlignment = center ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.97f, 0.88f));
        label.AddThemeColorOverride("font_outline_color", new Color(0.08f, 0.06f, 0.1f));
        label.AddThemeConstantOverride("outline_size", 6);
        return label;
    }

    // ---- procedural die faces ----
    private static ImageTexture FaceTexture(int value, string kind)
    {
        var key = $"{kind}:{value}";
        if (Textures.TryGetValue(key, out var cached) && GodotObject.IsInstanceValid(cached)) return cached;

        var (body, rim) = kind switch
        {
            "temp" => (new Color(0.62f, 0.82f, 1f), new Color(0.15f, 0.3f, 0.6f)),
            "borrowed" => (new Color(0.62f, 0.82f, 1f), new Color(0.95f, 0.55f, 0.1f)),
            "fixed" => (new Color(1f, 0.85f, 0.3f), new Color(0.55f, 0.35f, 0.05f)),
            "pledged" => (new Color(0.42f, 0.42f, 0.5f), new Color(0.15f, 0.15f, 0.2f)),
            _ => (new Color(0.97f, 0.95f, 0.88f), new Color(0.13f, 0.11f, 0.17f))
        };

        var img = Image.CreateEmpty(Face, Face, false, Image.Format.Rgba8);
        const float radius = 9f;
        for (var y = 0; y < Face; y++)
        {
            for (var px = 0; px < Face; px++)
            {
                var d = RoundedBoxDistance(px + 0.5f, y + 0.5f, Face / 2f, Face / 2f, Face / 2f - 2f, radius);
                if (d > 0.5f) continue;
                img.SetPixel(px, y, d > -2.5f ? rim : body);
            }
        }

        if (value > 0)
        {
            var pipColor = kind == "fixed" ? new Color(0.45f, 0.25f, 0f) : new Color(0.13f, 0.11f, 0.17f);
            foreach (var (gx, gy) in Pips(value))
                FillCircle(img, Face / 2f + gx * 12f, Face / 2f + gy * 12f, 4.2f, pipColor);
        }

        var tex = ImageTexture.CreateFromImage(img);
        Textures[key] = tex;
        return tex;
    }

    private static float RoundedBoxDistance(float px, float py, float cx, float cy, float half, float r)
    {
        var dx = Mathf.Abs(px - cx) - (half - r);
        var dy = Mathf.Abs(py - cy) - (half - r);
        var ox = Mathf.Max(dx, 0f);
        var oy = Mathf.Max(dy, 0f);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(dx, dy), 0f) - r;
    }

    private static void FillCircle(Image img, float cx, float cy, float r, Color c)
    {
        for (var y = (int)(cy - r - 1); y <= (int)(cy + r + 1); y++)
        for (var x = (int)(cx - r - 1); x <= (int)(cx + r + 1); x++)
        {
            if (x < 0 || y < 0 || x >= Face || y >= Face) continue;
            var dx = x + 0.5f - cx;
            var dy = y + 0.5f - cy;
            if (dx * dx + dy * dy <= r * r) img.SetPixel(x, y, c);
        }
    }

    private static (int, int)[] Pips(int v) => v switch
    {
        1 => [(0, 0)],
        2 => [(-1, -1), (1, 1)],
        3 => [(-1, -1), (0, 0), (1, 1)],
        4 => [(-1, -1), (1, -1), (-1, 1), (1, 1)],
        5 => [(-1, -1), (1, -1), (0, 0), (-1, 1), (1, 1)],
        _ => [(-1, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (1, 1)]
    };
}
