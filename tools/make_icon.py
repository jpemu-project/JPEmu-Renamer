"""アプリアイコン（app.ico）を作る。
白ベースに筆記体（Dancing Script Bold）の「JPEmu」と、赤・黒・赤のドット3つ。
32px以下は頭文字「J」だけにする。
使い方: python make_icon.py <DancingScript-Bold.ttf> <出力.ico> [プレビューPNGの出力先]
同じフォルダに logo.png（画面左上のロゴ）も出力する。
"""
import sys
from PIL import Image, ImageDraw, ImageFont

INK = (26, 28, 34, 255)
ACCENT = (229, 72, 77, 255)


def fit_font(path, text, max_w, max_h, start):
    size = start
    while size > 6:
        f = ImageFont.truetype(path, size)
        l, t, r, b = f.getbbox(text)
        if r - l <= max_w and b - t <= max_h:
            return f
        size -= 1
    return ImageFont.truetype(path, 6)


def render(font_path, px, letter_only=None):
    s = 4  # 4倍で描いて縮小（なめらかにする）
    W = px * s
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    radius = int(W * 0.22)
    d.rounded_rectangle([0, 0, W - 1, W - 1], radius=radius, fill=(255, 255, 255, 255))
    if (px <= 32) if letter_only is None else letter_only:
        f = fit_font(font_path, "J", W * 0.62, W * 0.72, int(W * 0.9))
        l, t, r, b = f.getbbox("J")
        d.text(((W - (r - l)) / 2 - l, (W - (b - t)) / 2 - t), "J", font=f, fill=INK)
    else:
        text = "JPEmu"
        f = fit_font(font_path, text, W * 0.80, W * 0.40, int(W * 0.5))
        l, t, r, b = f.getbbox(text)
        tw, th = r - l, b - t
        dot = max(int(W * 0.045), 2 * s)
        gap = int(dot * 0.6)
        total_h = th + dot * 2.2
        y0 = (W - total_h) / 2
        d.text(((W - tw) / 2 - l, y0 - t), text, font=f, fill=INK)
        dy = y0 + th + dot * 1.2
        row_w = dot * 3 + gap * 2
        x = (W - row_w) / 2
        for c in (ACCENT, INK, ACCENT):
            d.rectangle([x, dy, x + dot, dy + dot], fill=c)
            x += dot + gap
    return img.resize((px, px), Image.LANCZOS)


def main():
    font_path, out = sys.argv[1], sys.argv[2]
    sizes = [256, 128, 64, 48, 32, 24, 16]
    images = [render(font_path, s) for s in sizes]
    images[0].save(out, format="ICO", sizes=[(s, s) for s in sizes], append_images=images[1:])
    # 画面左上のロゴ用（頭文字「J」のみ、128px）
    import os
    render(font_path, 128, letter_only=True).save(os.path.join(os.path.dirname(out), "logo.png"))
    if len(sys.argv) > 3:
        sheet = Image.new("RGBA", (256 + 128 + 64 + 48 + 32 + 24 + 16 + 8 * 8, 272), (15, 17, 21, 255))
        x = 8
        for im in images:
            sheet.paste(im, (x, 8), im)
            x += im.width + 8
        sheet.save(sys.argv[3])


if __name__ == "__main__":
    main()
