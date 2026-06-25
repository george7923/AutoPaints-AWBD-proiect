import numpy as np
from PIL import Image, ImageOps, ImageFilter
from flask import Flask, request, jsonify
from scipy.ndimage import laplace
from sklearn.cluster import KMeans
import traceback
import cv2
import webcolors
import colorsys


ENRICHED_COLOR_PALETTE = {
    "AliceBlue": (240, 248, 255),
    "AntiqueWhite": (250, 235, 215),
    "Aqua": (0, 255, 255),
    "Aquamarine": (127, 255, 212),
    "Azure": (240, 255, 255),
    "Beige": (245, 245, 220),
    "Bisque": (255, 228, 196),
    "Black": (0, 0, 0),
    "BlanchedAlmond": (255, 235, 205),
    "Blue": (0, 0, 255),
    "BlueViolet": (138, 43, 226),
    "Brown": (165, 42, 42),
    "BurlyWood": (222, 184, 135),
    "CadetBlue": (95, 158, 160),
    "Chartreuse": (127, 255, 0),
    "Chocolate": (210, 105, 30),
    "Coral": (255, 127, 80),
    "CornflowerBlue": (100, 149, 237),
    "Cornsilk": (255, 248, 220),
    "Crimson": (220, 20, 60),
    "Cyan": (0, 255, 255),
    "DarkBlue": (0, 0, 139),
    "DarkCyan": (0, 139, 139),
    "DarkGoldenrod": (184, 134, 11),
    "DarkGray": (169, 169, 169),
    "DarkGreen": (0, 100, 0),
    "DarkKhaki": (189, 183, 107),
    "DarkMagenta": (139, 0, 139),
    "DarkOliveGreen": (85, 107, 47),
    "Orange": (255, 140, 0),
    "DarkOrchid": (153, 50, 204),
    "DarkRed": (139, 0, 0),
    "DarkSalmon": (233, 150, 122),
    "DarkSeaGreen": (143, 188, 143),
    "DarkSlateBlue": (72, 61, 139),
    "DarkSlateGray": (47, 79, 79),
    "DarkTurquoise": (0, 206, 209),
    "DarkViolet": (148, 0, 211),
    "DeepPink": (255, 20, 147),
    "DeepSkyBlue": (0, 191, 255),
    "DimGray": (105, 105, 105),
    "DodgerBlue": (30, 144, 255),
    "FireBrick": (178, 34, 34),
    "FloralWhite": (255, 250, 240),
    "ForestGreen": (34, 139, 34),
    "Fuchsia": (255, 0, 255),
    "Gainsboro": (220, 220, 220),
    "GhostWhite": (248, 248, 255),
    "Gold": (255, 215, 0),
    "Goldenrod": (218, 165, 32),
    "Gray": (128, 128, 128),
    "Green": (0, 128, 0),
    "GreenYellow": (173, 255, 47),
    "Honeydew": (240, 255, 240),
    "HotPink": (255, 105, 180),
    "IndianRed": (205, 92, 92),
    "Indigo": (75, 0, 130),
    "Ivory": (255, 255, 240),
    "Khaki": (240, 230, 140),
    "Lavender": (230, 230, 250),
    "LavenderBlush": (255, 240, 245),
    "LawnGreen": (124, 252, 0),
    "LemonChiffon": (255, 250, 205),
    "LightBlue": (173, 216, 230),
    "LightCoral": (240, 128, 128),
    "LightCyan": (224, 255, 255),
    "LightGoldenrodYellow": (250, 250, 210),
    "LightGray": (211, 211, 211),
    "LightGreen": (144, 238, 144),
    "LightPink": (255, 182, 193),
    "LightSalmon": (255, 160, 122),
    "LightSeaGreen": (32, 178, 170),
    "LightSkyBlue": (135, 206, 250),
    "LightSlateGray": (119, 136, 153),
    "LightSteelBlue": (176, 196, 222),
    "LightYellow": (255, 255, 224),
    "Lime": (0, 255, 0),
    "LimeGreen": (50, 205, 50),
    "Linen": (250, 240, 230),
    "Magenta": (255, 0, 255),
    "Maroon": (128, 0, 0),
    "MediumAquamarine": (102, 205, 170),
    "MediumBlue": (0, 0, 205),
    "MediumOrchid": (186, 85, 211),
    "MediumPurple": (147, 112, 219),
    "MediumSeaGreen": (60, 179, 113),
    "MediumSlateBlue": (123, 104, 238),
    "MediumSpringGreen": (0, 250, 154),
    "MediumTurquoise": (72, 209, 204),
    "MediumVioletRed": (199, 21, 133),
    "MidnightBlue": (25, 25, 112),
    "MintCream": (245, 255, 250),
    "MistyRose": (255, 228, 225),
    "Moccasin": (255, 228, 181),
    "NavajoWhite": (255, 222, 173),
    "Navy": (0, 0, 128),
    "OldLace": (253, 245, 230),
    "Olive": (128, 128, 0),
    "OliveDrab": (107, 142, 35),
    "Orange": (255, 165, 0),
    "OrangeRed": (255, 69, 0),
    "Orchid": (218, 112, 214),
    "PaleGoldenrod": (238, 232, 170),
    "PaleGreen": (152, 251, 152),
    "PaleTurquoise": (175, 238, 238),
    "PaleVioletRed": (219, 112, 147),
    "PapayaWhip": (255, 239, 213),
    "PeachPuff": (255, 218, 185),
    "Peru": (205, 133, 63),
    "Pink": (255, 192, 203),
    "Plum": (221, 160, 221),
    "PowderBlue": (176, 224, 230),
    "Purple": (128, 0, 128),
    "RebeccaPurple": (102, 51, 153),
    "Red": (255, 0, 0),
    "RosyBrown": (188, 143, 143),
    "RoyalBlue": (65, 105, 225),
    "SaddleBrown": (139, 69, 19),
    "Salmon": (250, 128, 114),
    "SandyBrown": (244, 164, 96),
    "SeaGreen": (46, 139, 87),
    "SeaShell": (255, 245, 238),
    "Sienna": (160, 82, 45),
    "Silver": (192, 192, 192),
    "SkyBlue": (135, 206, 235),
    "SlateBlue": (106, 90, 205),
    "SlateGray": (112, 128, 144),
    "Snow": (255, 250, 250),
    "SpringGreen": (0, 255, 127),
    "SteelBlue": (70, 130, 180),
    "Tan": (210, 180, 140),
    "Teal": (0, 128, 128),
    "Thistle": (216, 191, 216),
    "Tomato": (255, 99, 71),
    "Turquoise": (64, 224, 208),
    "Violet": (238, 130, 238),
    "Wheat": (245, 222, 179),
    "White": (255, 255, 255),
    "WhiteSmoke": (245, 245, 245),
    "Yellow": (255, 255, 0),
    "YellowGreen": (154, 205, 50)
}



IMG_SIZE = (128, 128)

# -----------------------------
# Funcții de analiză
# -----------------------------

def get_dominant_color(image):
    """
    Returnează (best_color, dominant) pentru imagine, folosind ENRICHED_COLOR_PALETTE,
    dar cu filtru inteligent pentru a nu returna "Brown" la roșu/roz/magenta.
    """


    # Preprocesare imagine
    image = image.convert("RGB").resize((64, 64))
    pixels = np.array(image).reshape(-1, 3)

    # K-means
    kmeans = KMeans(n_clusters=3, random_state=0).fit(pixels)
    counts = np.bincount(kmeans.labels_)
    dominant = tuple(int(x) for x in kmeans.cluster_centers_[np.argmax(counts)])

    r, g, b = [v / 255 for v in dominant]
    h, s, v = colorsys.rgb_to_hsv(r, g, b)
    h_deg = h * 360

    # Daca hue-ul este pe zona rosu/roz/magenta, selecteaza doar culori din acea familie!
    def in_red_family(name):
        return any(word in name.lower() for word in ["red", "crimson", "maroon", "pink", "magenta", "rose"])

    if (h_deg < 25 or h_deg > 325):
        candidate_palette = {name: rgb for name, rgb in ENRICHED_COLOR_PALETTE.items() if in_red_family(name)}
    else:
        candidate_palette = ENRICHED_COLOR_PALETTE

    def color_distance(c1, c2):
        return np.sqrt(sum((a - b) ** 2 for a, b in zip(c1, c2)))

    best_color = None
    best_distance = float('inf')
    for color_name, rgb in candidate_palette.items():
        d = color_distance(dominant, rgb)
        if d < best_distance:
            best_distance = d
            best_color = color_name

    return best_color, dominant

"""
def is_metallic(image):

    gray_image = ImageOps.grayscale(image)
    blurred = gray_image.filter(ImageFilter.GaussianBlur(radius=2))
    np_gray = np.array(blurred)
    bright_pixels = np.sum(np_gray > 230)
    total_pixels = np_gray.size
    ratio = bright_pixels / total_pixels
    return ratio > 0.01
"""
def is_metallic(image):
    """
    Detectează dacă vopseaua unei mașini este metalizată pe baza granulației vizuale.
    - Imaginea se convertește în tonuri de gri și se redimensionează pentru consistență.
    - Se aplică filtrul Laplacian pentru a evidenția variațiile locale de luminozitate (granulația).
    - Se calculează deviația standard a imaginii filtrate (mare = metalizat).
    - Pragul (0.08) poate fi ajustat în funcție de testele reale.
    """
    # Conversie la gri și resize pentru uniformitate
    gray = ImageOps.grayscale(image).resize((128, 128))
    arr = np.array(gray).astype(np.float32) / 255.0

    # Filtru Laplacian (scoate în evidență "sclipiciul" metalizat)
    lap = laplace(arr)
    stddev = np.std(lap)

    # Prag de metalizare 
    metallic_threshold = 0.08
    return stddev > metallic_threshold
# -----------------------------
# Creare API REST cu Flask
# -----------------------------
from flask import Flask, request, jsonify
app = Flask(__name__)

@app.route('/analyze-paint', methods=['POST'])
def analyze_paint():
    if 'image' not in request.files:
        return jsonify({"error": "Nu a fost furnizată nicio imagine"}), 400
    file = request.files['image']
    try:
        image = Image.open(file.stream)
    except Exception as e:
        return jsonify({"error": "Imaginea nu a putut fi procesată", "detalii": str(e)}), 400

    try:
        dominant_color, avg_color = get_dominant_color(image)
        metallic = is_metallic(image)
    except Exception as e:
        return jsonify({"error": "Eroare la analiză", "detalii": str(e), "trace": traceback.format_exc()}), 500
    
    # Convertim tipurile NumPy la tipuri native Python
    result = {
        "dominantColor": dominant_color,
        "avgColor": [int(x) for x in avg_color],
        "metallic": bool(metallic)
    }
    return jsonify(result)

if __name__ == '__main__':
    app.run(debug=True, port=5001, use_reloader=False)
