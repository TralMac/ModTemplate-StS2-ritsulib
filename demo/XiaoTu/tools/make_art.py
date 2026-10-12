#!/usr/bin/env python3
"""
Generates simple flat art for the XiaoTu demo: card portraits, relic icons and power icons.

    pip install pillow
    python tools/make_art.py            # write everything that is missing
    python tools/make_art.py --force    # overwrite

Everything is drawn from the card / relic / power class names and the zhs localization text (no image service):
the colour comes from the card type, the motif from the effect text (dice, coin, shield, sword, lock, up-arrow,
odd/even dice, star), and the card name is printed on the lower band. Output paths follow the base classes:
  XiaoTu/images/cards/<Class>.png            250x190 (250x351 for Ancient cards)
  XiaoTu/images/relics/<Class>.png           85x85, <Class>_outline.png 85x85, big/<Class>.png 256x256
  XiaoTu/images/powers/<Class>.png           64x64,  big/<Class>.png 256x256
  XiaoTu/images/potions/<Class>.png + outline/<Class>.png
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

ROOT = Path(__file__).resolve().parent.parent
CODE = ROOT / "XiaoTuCode"
RES = ROOT / "XiaoTu"
IMG = RES / "images"

TYPE_COLORS = {
    "Attack": ((150, 44, 52), (226, 112, 78)),
    "Skill": ((38, 112, 96), (104, 190, 150)),
    "Power": ((52, 76, 154), (120, 156, 232)),
    "Status": ((88, 88, 96), (140, 140, 150)),
    "Curse": ((72, 40, 92), (128, 70, 150)),
}
FONTS = ["C:/Windows/Fonts/msyhbd.ttc", "C:/Windows/Fonts/msyh.ttc", "C:/Windows/Fonts/simhei.ttf",
         "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc", "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc"]
WHITE = (250, 247, 238)
INK = (34, 30, 44)


def font(size):
    for f in FONTS:
        if Path(f).exists():
            return ImageFont.truetype(f, size)
    sys.exit("No CJK font found; edit FONTS in tools/make_art.py")


def lerp(a, b, t):
    return tuple(int(x + (y - x) * t) for x, y in zip(a, b))


def gradient(w, h, c1, c2):
    img = Image.new("RGB", (w, h))
    px = img.load()
    for y in range(h):
        for x in range(w):
            px[x, y] = lerp(c1, c2, min(1.0, (y / h) * 0.8 + (x / w) * 0.2))
    return img


def snake(name):
    s = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1_\2", name)
    s = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", s)
    return s.upper()


# ------------------------------------------------------------------ motifs
def pips(value):
    g = {1: [(0, 0)], 2: [(-1, -1), (1, 1)], 3: [(-1, -1), (0, 0), (1, 1)], 4: [(-1, -1), (1, -1), (-1, 1), (1, 1)],
         5: [(-1, -1), (1, -1), (0, 0), (-1, 1), (1, 1)], 6: [(-1, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (1, 1)]}
    return g[value]


def draw_die(d, cx, cy, r, value, body=WHITE, tilt=0.0):
    pts = [(cx + r * math.cos(math.radians(a + tilt)) * 1.0, cy + r * math.sin(math.radians(a + tilt)) * 1.0)
           for a in (45, 135, 225, 315)]
    d.polygon(pts, fill=body, outline=INK)
    d.line(pts + [pts[0]], fill=INK, width=max(2, int(r * 0.08)))
    side = r * 1.414 * 0.5
    for px, py in pips(value):
        x = cx + px * side * 0.55
        y = cy + py * side * 0.55
        rr = side * 0.14
        d.ellipse([x - rr, y - rr, x + rr, y + rr], fill=INK)


def draw_coin(d, cx, cy, r):
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(246, 200, 70), outline=INK, width=max(2, int(r * 0.08)))
    d.ellipse([cx - r * .72, cy - r * .72, cx + r * .72, cy + r * .72], outline=(190, 140, 30), width=max(2, int(r * .06)))
    f = font(int(r * 1.1))
    d.text((cx, cy), "$", fill=(150, 100, 20), font=f, anchor="mm")


def draw_shield(d, cx, cy, r):
    pts = [(cx - r * .8, cy - r * .85), (cx + r * .8, cy - r * .85), (cx + r * .8, cy), (cx, cy + r), (cx - r * .8, cy)]
    d.polygon(pts, fill=(168, 196, 226), outline=INK)
    d.line(pts + [pts[0]], fill=INK, width=max(2, int(r * .07)))
    d.line([(cx, cy - r * .85), (cx, cy + r)], fill=INK, width=max(2, int(r * .05)))


def draw_sword(d, cx, cy, r):
    d.polygon([(cx - r * .12, cy - r), (cx + r * .12, cy - r), (cx + r * .12, cy + r * .45), (cx - r * .12, cy + r * .45)],
              fill=(226, 232, 240), outline=INK)
    d.rectangle([cx - r * .45, cy + r * .45, cx + r * .45, cy + r * .6], fill=(190, 150, 60), outline=INK)
    d.rectangle([cx - r * .1, cy + r * .6, cx + r * .1, cy + r * 1.0], fill=(120, 76, 40), outline=INK)


def draw_lock(d, cx, cy, r):
    d.arc([cx - r * .5, cy - r * 1.0, cx + r * .5, cy + r * .1], 180, 360, fill=INK, width=max(3, int(r * .16)))
    d.rounded_rectangle([cx - r * .75, cy - r * .1, cx + r * .75, cy + r * .9], radius=int(r * .15), fill=(236, 190, 70), outline=INK, width=2)
    d.ellipse([cx - r * .12, cy + r * .2, cx + r * .12, cy + r * .44], fill=INK)


def draw_arrow(d, cx, cy, r):
    d.polygon([(cx, cy - r), (cx + r * .8, cy), (cx + r * .3, cy), (cx + r * .3, cy + r * .9), (cx - r * .3, cy + r * .9),
               (cx - r * .3, cy), (cx - r * .8, cy)], fill=(110, 224, 140), outline=INK)


def draw_star(d, cx, cy, r):
    pts = []
    for i in range(10):
        rad = r if i % 2 == 0 else r * .45
        a = math.radians(-90 + i * 36)
        pts.append((cx + rad * math.cos(a), cy + rad * math.sin(a)))
    d.polygon(pts, fill=(250, 226, 120), outline=INK)


def draw_cards(d, cx, cy, r):
    for k, ang in enumerate((-14, 0, 14)):
        x = cx + (k - 1) * r * .45
        d.rounded_rectangle([x - r * .38, cy - r * .62, x + r * .38, cy + r * .62], radius=int(r * .08), fill=WHITE, outline=INK, width=2)


def motifs_for(text, ctype, key):
    m = []
    if re.search(r"奇数|偶数|奇偶|Odd|Even", text):
        m.append("parity")
    if "金币" in text or "gold" in text.lower():
        m.append("coin")
    if "质押" in text or "pledge" in text.lower():
        m.append("lock")
    if "押大" in text or "bet high" in text.lower():
        m.append("arrow")
    if "格挡" in text:
        m.append("shield")
    if "伤害" in text and ctype == "Attack":
        m.append("sword")
    if "抽" in text and not m:
        m.append("cards")
    if ctype == "Power" and not m:
        m.append("star")
    if not m or "骰" in text or "dice" in text.lower() or "die" in text.lower():
        m.insert(0, "die")
    seen, out = set(), []
    for x in m:
        if x not in seen:
            seen.add(x)
            out.append(x)
    return out[:3]


def draw_motif(d, name, cx, cy, r, key):
    value = int(hashlib.md5(key.encode()).hexdigest(), 16) % 6 + 1
    if name == "die":
        draw_die(d, cx, cy, r, value, tilt=((value * 7) % 20) - 10)
    elif name == "parity":
        draw_die(d, cx - r * .5, cy, r * .7, 3, tilt=-8)
        draw_die(d, cx + r * .5, cy, r * .7, 4, body=(206, 226, 250), tilt=8)
    elif name == "coin":
        draw_coin(d, cx, cy, r * .85)
    elif name == "shield":
        draw_shield(d, cx, cy, r)
    elif name == "sword":
        draw_sword(d, cx, cy, r)
    elif name == "lock":
        draw_lock(d, cx, cy, r)
    elif name == "arrow":
        draw_arrow(d, cx, cy, r)
    elif name == "star":
        draw_star(d, cx, cy, r)
    elif name == "cards":
        draw_cards(d, cx, cy, r)


def draw_scene(img, motifs, key, box):
    d = ImageDraw.Draw(img, "RGBA")
    x0, y0, x1, y1 = box
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    r = min(x1 - x0, y1 - y0) * .34
    # soft backdrop disc
    d.ellipse([cx - r * 1.5, cy - r * 1.4, cx + r * 1.5, cy + r * 1.4], fill=(255, 255, 255, 38))
    n = len(motifs)
    if n == 1:
        draw_motif(d, motifs[0], cx, cy, r * 1.1, key)
    else:
        spread = r * 1.25
        for i, m in enumerate(motifs):
            ox = (i - (n - 1) / 2) * spread
            draw_motif(d, m, cx + ox, cy + (i % 2) * r * .12, r * (.82 if n == 2 else .7), key + m)


# ------------------------------------------------------------------ discovery
CARD_RE = re.compile(r"class\s+(\w+)\s*\(\s*\)\s*:\s*XiaoTuCard\(\s*[^,]+,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)")


def loc(lang, fname):
    p = RES / "localization" / lang / fname
    return json.loads(p.read_text(encoding="utf-8-sig")) if p.exists() else {}


def lookup(table, prefix, cls, suffix):
    return table.get(f"XIAO_TU_{prefix}_{snake(cls)}.{suffix}")


def save(img, path, force):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and not force:
        return False
    img.save(path)
    return True


def make_card(cls, ctype, rarity, title, desc, force):
    w, h = (250, 351) if rarity == "Ancient" else (250, 190)
    c1, c2 = TYPE_COLORS.get(ctype, TYPE_COLORS["Skill"])
    if rarity == "Rare":
        c1, c2 = lerp(c1, (60, 40, 20), .25), lerp(c2, (250, 200, 80), .35)
    img = gradient(w, h, c1, c2)
    band = 44 if w == 250 and h == 190 else 60
    draw_scene(img, motifs_for(desc, ctype, cls), cls, (0, 0, w, h - band))
    d = ImageDraw.Draw(img, "RGBA")
    d.rectangle([0, h - band, w, h], fill=(20, 16, 30, 190))
    size = 26 if len(title) <= 7 else 20
    d.text((w / 2, h - band / 2), title, fill=WHITE, font=font(size), anchor="mm")
    return img


def make_icon(size, motifs, key, colors, label=None):
    img = gradient(size, size, *colors)
    draw_scene(img, motifs, key, (0, 0, size, size))
    return img


def outline_of(img):
    """White ring silhouette used as the relic outline."""
    mask = Image.new("L", img.size, 0)
    d = ImageDraw.Draw(mask)
    d.ellipse([2, 2, img.size[0] - 3, img.size[1] - 3], outline=255, width=3)
    out = Image.new("RGBA", img.size, (0, 0, 0, 0))
    out.paste((255, 255, 255, 255), mask=mask)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--force", action="store_true")
    args = ap.parse_args()
    zhs_cards, zhs_powers, zhs_relics, zhs_potions = (loc("zhs", f) for f in ("cards.json", "powers.json", "relics.json", "potions.json"))
    n = 0

    for cs in sorted((CODE / "Cards").glob("*.cs")):
        for cls, ctype, rarity in CARD_RE.findall(cs.read_text(encoding="utf-8")):
            title = lookup(zhs_cards, "CARD", cls, "title") or cls
            desc = lookup(zhs_cards, "CARD", cls, "description") or ""
            n += save(make_card(cls, ctype, rarity, title, desc, args.force), IMG / "cards" / f"{cls}.png", args.force)

    relic_colors = ((84, 66, 120), (190, 150, 230))
    for cs in sorted((CODE / "Relics").glob("*.cs")):
        m = re.search(r"class\s+(\w+)\s*(?::|\n)", cs.read_text(encoding="utf-8"))
        if not m or m.group(1) == "XiaoTuRelic":
            continue
        cls = m.group(1)
        desc = lookup(zhs_relics, "RELIC", cls, "description") or ""
        motifs = motifs_for(desc, "Skill", cls)[:2] or ["die"]
        small = make_icon(85, motifs[:1] if len(motifs) > 1 else motifs, cls, relic_colors)
        big = make_icon(256, motifs, cls, relic_colors)
        n += save(small, IMG / "relics" / f"{cls}.png", args.force)
        n += save(outline_of(small), IMG / "relics" / f"{cls}_outline.png", args.force)
        n += save(big, IMG / "relics" / "big" / f"{cls}.png", args.force)

    power_colors = ((52, 76, 154), (150, 180, 240))
    for cs in sorted((CODE / "Powers").glob("*Power.cs")):
        cls = cs.stem
        if cls == "XiaoTuPower":
            continue
        desc = lookup(zhs_powers, "POWER", cls, "description") or ""
        motifs = motifs_for(desc, "Power", cls)[:2]
        debuff = "PowerType.Debuff" in cs.read_text(encoding="utf-8")
        colors = ((120, 44, 60), (230, 120, 120)) if debuff else power_colors
        n += save(make_icon(64, motifs[:1], cls, colors), IMG / "powers" / f"{cls}.png", args.force)
        n += save(make_icon(256, motifs, cls, colors), IMG / "powers" / "big" / f"{cls}.png", args.force)

    for cs in sorted((CODE / "Potions").glob("*.cs")):
        cls = cs.stem
        if cls == "XiaoTuPotion":
            continue
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.ellipse([12, 22, 52, 62], fill=(246, 214, 110), outline=INK, width=2)
        d.rectangle([26, 6, 38, 26], fill=(200, 230, 210), outline=INK, width=2)
        d.rectangle([24, 4, 40, 10], fill=(150, 90, 50), outline=INK, width=2)
        n += save(img, IMG / "potions" / f"{cls}.png", args.force)
        ring = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        ImageDraw.Draw(ring).ellipse([10, 20, 54, 63], outline=(255, 255, 255, 255), width=3)
        n += save(ring, IMG / "potions" / "outline" / f"{cls}.png", args.force)

    print(f"wrote {n} images")


if __name__ == "__main__":
    main()
