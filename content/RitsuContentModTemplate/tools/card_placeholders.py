#!/usr/bin/env python3
"""
Generates placeholder card portraits: big color blocks plus the card's name.

Usage (from the project folder):
    pip install pillow
    python tools/card_placeholders.py              # only cards that have no image yet
    python tools/card_placeholders.py --force      # overwrite every card image
    python tools/card_placeholders.py --lang eng   # use English card names

Cards are read from <Mod>Code/Cards/*.cs, names from <Mod>/localization/<lang>/cards.json,
and images are written to <Mod>/images/cards/<ClassName>.png (250x190, 250x351 for ancient cards).
The color depends on keywords in the name (fire, ice, shield, ... see THEMES, which also draw a small
symbol), otherwise on the card type: Attack red, Skill green, Power blue, Status gray, Curse purple.
"""
import argparse
import hashlib
import json
import math
import re
import sys
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    sys.exit("Pillow is required: pip install pillow")

PROJECT_DIR = Path(__file__).resolve().parent.parent

# (base, accent) colors per card type.
TYPE_COLORS = {
    "Attack": ((156, 46, 46), (214, 98, 70)),
    "Skill": ((44, 118, 78), (96, 176, 110)),
    "Power": ((44, 82, 150), (92, 142, 214)),
    "Status": ((88, 88, 96), (140, 140, 150)),
    "Curse": ((72, 40, 92), (128, 70, 150)),
}
DEFAULT_COLORS = ((90, 90, 90), (150, 150, 150))

# Name keywords -> theme. The first theme with a matching keyword wins; cards without a match
# fall back to the card type colors. Add your own words here.
THEMES = {
    "fire": ("火 焰 炎 燃 烈 灼 烧 炽 fire flame burn blaze ember inferno scorch", ((176, 64, 28), (244, 160, 52))),
    "ice": ("冰 寒 霜 雪 冻 凛 ice frost freeze snow chill cold glacier", ((38, 98, 150), (140, 210, 240))),
    "thunder": ("雷 电 霆 闪 击穿 thunder lightning shock spark bolt storm", ((72, 58, 140), (244, 212, 70))),
    "shield": ("盾 护 御 壁 守 坚 挡 铁 shield guard block defend fortify bulwark wall", ((60, 84, 110), (150, 180, 206))),
    "poison": ("毒 瘴 腐 雾 蚀 poison toxic venom plague rot", ((70, 60, 120), (150, 200, 70))),
    "blood": ("血 嗜 狂 饮 blood bleed rage frenzy fury", ((110, 16, 28), (214, 52, 62))),
    "wind": ("风 疾 迅 旋 飞 步 wind gale swift dash whirl", ((40, 120, 124), (150, 224, 210))),
    "heal": ("愈 生 复 净 疗 春 heal regen restore cleanse life renew", ((40, 128, 90), (180, 240, 170))),
    "shadow": ("影 暗 夜 幽 魂 厄 诅 shadow dark night ghost soul curse doom", ((44, 30, 70), (140, 100, 190))),
    "magic": ("魔 法 力 涌 气 聚 能 magic mana energy surge arcane charge", ((120, 44, 140), (240, 130, 220))),
    "star": ("星 光 圣 辉 日 月 star light holy radiant sun moon", ((60, 70, 140), (250, 226, 130))),
    "blade": ("斩 刺 剑 刀 刃 锋 割 slash stab blade sword cut strike pierce", ((72, 78, 90), (210, 220, 232))),
    "fist": ("拳 打 碎 锤 砸 重 punch smash crush hammer bash", ((120, 76, 40), (230, 168, 96))),
}

# Fonts tried in order. Pass --font to use another one.
FONT_CANDIDATES = [
    "C:/Windows/Fonts/msyhbd.ttc",
    "C:/Windows/Fonts/msyh.ttc",
    "C:/Windows/Fonts/simhei.ttf",
    "/System/Library/Fonts/PingFang.ttc",
    "/System/Library/Fonts/STHeiti Medium.ttc",
    "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc",
    "/usr/share/fonts/noto-cjk/NotoSansCJK-Bold.ttc",
    "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc",
]

CARD_PATTERN = re.compile(
    r"class\s+(\w+)\s*\(\s*\)\s*:\s*\w+\(\s*[^,]+,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)"
)


def entry_stem(name):
    """Same transform RitsuLib uses for entries: StrikeRitsuChar -> STRIKE_RITSU_CHAR."""
    s = re.sub(r"[^A-Za-z0-9]+", "_", name)
    s = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1_\2", s)
    s = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", s)
    return re.sub(r"_+", "_", s).strip("_").upper()


def find_mod_dirs():
    for code_dir in sorted(PROJECT_DIR.glob("*Code")):
        res_dir = PROJECT_DIR / code_dir.name[: -len("Code")]
        if (code_dir / "Cards").is_dir() and (res_dir / "localization").is_dir():
            return code_dir, res_dir
    sys.exit(f"Could not find <Mod>Code/Cards and <Mod>/localization in {PROJECT_DIR}")


def find_cards(code_dir):
    for cs in sorted((code_dir / "Cards").rglob("*.cs")):
        for name, card_type, rarity in CARD_PATTERN.findall(cs.read_text(encoding="utf-8")):
            yield name, card_type, rarity


def load_titles(res_dir, lang):
    path = res_dir / "localization" / lang / "cards.json"
    if not path.exists():
        return {}
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    return {k[: -len(".title")]: v for k, v in data.items() if k.endswith(".title")}


def find_title(titles, class_name):
    suffix = "_CARD_" + entry_stem(class_name)
    for key, title in titles.items():
        if key.endswith(suffix):
            return title
    return class_name


def load_font(path, size):
    candidates = [path] if path else FONT_CANDIDATES
    for candidate in candidates:
        if candidate and Path(candidate).exists():
            return ImageFont.truetype(candidate, size)
    sys.exit("No usable font found (Chinese names need a CJK font). Pass one with --font <path>.")


def lerp(a, b, t):
    return tuple(int(x + (y - x) * t) for x, y in zip(a, b))


def find_theme(title):
    for name, (words, colors) in THEMES.items():
        if any(w in title.lower() for w in words.split()):
            return name, colors
    return None, None


def draw_glyph(d, theme, cx, cy, r, color, bg):
    """Very simple flat symbol for a theme, centered on (cx, cy) with radius r."""
    if theme == "fire":
        d.polygon([(cx, cy - r), (cx + r * .7, cy + r * .1), (cx + r * .45, cy + r), (cx - r * .45, cy + r), (cx - r * .7, cy + r * .1)], fill=color)
    elif theme == "ice":
        for dx, dy in [(0, 1), (.87, .5), (.87, -.5)]:
            d.line([(cx - dx * r, cy - dy * r), (cx + dx * r, cy + dy * r)], fill=color, width=max(3, int(r * .16)))
    elif theme == "thunder":
        d.polygon([(cx + r * .2, cy - r), (cx - r * .6, cy + r * .1), (cx - r * .05, cy + r * .1), (cx - r * .3, cy + r), (cx + r * .6, cy - r * .2), (cx + r * .05, cy - r * .2)], fill=color)
    elif theme == "shield":
        d.polygon([(cx - r * .8, cy - r * .8), (cx + r * .8, cy - r * .8), (cx + r * .8, cy), (cx, cy + r), (cx - r * .8, cy)], fill=color)
    elif theme == "poison":
        for dx, dy, k in [(-.45, .25, .5), (.4, .35, .4), (0, -.3, .65)]:
            d.ellipse([cx + dx * r - k * r, cy + dy * r - k * r, cx + dx * r + k * r, cy + dy * r + k * r], fill=color)
    elif theme == "blood":
        d.ellipse([cx - r * .7, cy - r * .35, cx + r * .7, cy + r * .95], fill=color)
        d.polygon([(cx, cy - r), (cx + r * .6, cy), (cx - r * .6, cy)], fill=color)
    elif theme == "wind":
        for i, k in enumerate((-.55, 0, .55)):
            d.arc([cx - r + i * r * .15, cy + k * r - r * .5, cx + r, cy + k * r + r * .5], 200, 360, fill=color, width=max(3, int(r * .16)))
    elif theme == "heal":
        w = r * .32
        d.rectangle([cx - w, cy - r, cx + w, cy + r], fill=color)
        d.rectangle([cx - r, cy - w, cx + r, cy + w], fill=color)
    elif theme == "shadow":
        d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=color)
        d.ellipse([cx - r * .35, cy - r * .95, cx + r * 1.15, cy + r * .55], fill=bg)
    elif theme == "magic":
        d.polygon([(cx, cy - r), (cx + r * .6, cy), (cx, cy + r), (cx - r * .6, cy)], fill=color)
        d.ellipse([cx - r, cy - r * .2, cx + r, cy + r * .2], fill=color)
    elif theme == "star":
        pts = []
        for i in range(10):
            a = -math.pi / 2 + i * math.pi / 5
            k = r if i % 2 == 0 else r * .42
            pts.append((cx + k * math.cos(a), cy + k * math.sin(a)))
        d.polygon(pts, fill=color)
    elif theme == "blade":
        d.polygon([(cx + r, cy - r), (cx + r * .75, cy - r * 1.05), (cx - r * .8, cy + r * .5), (cx - r * .5, cy + r * .8)], fill=color)
        d.line([(cx - r * .9, cy + r * .1), (cx - r * .1, cy + r * .9)], fill=color, width=max(3, int(r * .2)))
    elif theme == "fist":
        d.rounded_rectangle([cx - r * .8, cy - r * .6, cx + r * .8, cy + r * .6], radius=int(r * .3), fill=color)
        for i in range(4):
            x0 = cx - r * .8 + i * r * .4
            d.rectangle([x0 + 2, cy - r * .85, x0 + r * .36, cy - r * .4], fill=color)


def draw_card(title, card_type, size, font_path, seed):
    width, height = size
    base, accent = TYPE_COLORS.get(card_type, DEFAULT_COLORS)
    theme, theme_colors = find_theme(title)
    if theme:
        base, accent = theme_colors
    rnd = hashlib.md5(seed.encode("utf-8")).digest()
    dark = lerp(base, (0, 0, 0), 0.35)
    light = lerp(accent, (255, 255, 255), 0.25)

    img = Image.new("RGB", size, base)
    d = ImageDraw.Draw(img)

    # A few big blocks whose placement depends on the card, so cards of the same type still differ.
    x = width * (0.25 + rnd[0] / 255 * 0.5)
    d.polygon([(x, 0), (width, 0), (width, height), (x - width * 0.35, height)], fill=accent)
    y = height * (0.15 + rnd[1] / 255 * 0.3)
    d.polygon([(0, y), (width * (0.3 + rnd[2] / 255 * 0.3), 0), (0, 0)], fill=light)
    r = min(width, height) * (0.25 + rnd[3] / 255 * 0.2)
    cx, cy = width * (rnd[4] / 255), height * (0.6 + rnd[5] / 255 * 0.4)
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=dark)

    if theme:
        r = min(width, height) * 0.22
        bright = sum(accent) / 3 > 170
        color = lerp(base, (0, 0, 0), 0.45) if bright else lerp(accent, (255, 255, 255), 0.6)
        draw_glyph(d, theme, width / 2, height * 0.3, r, color, accent)

    # Name band: lower part when there is a symbol above it, otherwise the middle.
    band_h = int(height * 0.26) if height < width * 1.2 else int(height * 0.16)
    top = int(height * 0.6) if theme else (height - band_h) // 2
    band = Image.new("RGBA", (width, band_h), (0, 0, 0, 150))
    img.paste(band, (0, top), band)

    font_size = int(band_h * 0.62)
    while True:
        font = load_font(font_path, font_size)
        left, t, right, b = d.textbbox((0, 0), title, font=font, stroke_width=2)
        if right - left <= width - 16 or font_size <= 10:
            break
        font_size -= 1
    d.text(
        ((width - (right - left)) / 2 - left, top + (band_h - (b - t)) / 2 - t),
        title,
        font=font,
        fill=(255, 255, 255),
        stroke_width=2,
        stroke_fill=(0, 0, 0),
    )
    return img


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--lang", default="zhs", help="localization folder for card names (default: zhs)")
    parser.add_argument("--font", help="path to a .ttf/.ttc/.otf font")
    parser.add_argument("--force", action="store_true", help="overwrite existing card images")
    args = parser.parse_args()

    code_dir, res_dir = find_mod_dirs()
    titles = load_titles(res_dir, args.lang)
    out_dir = res_dir / "images" / "cards"
    out_dir.mkdir(parents=True, exist_ok=True)

    written = skipped = 0
    for name, card_type, rarity in find_cards(code_dir):
        out = out_dir / f"{name}.png"
        if out.exists() and not args.force:
            skipped += 1
            continue
        size = (250, 351) if rarity == "Ancient" else (250, 190)
        draw_card(find_title(titles, name), card_type, size, args.font, name).save(out)
        print(f"wrote {out.relative_to(PROJECT_DIR)}")
        written += 1
    print(f"{written} written, {skipped} skipped (already exist, use --force to overwrite)")


if __name__ == "__main__":
    main()
