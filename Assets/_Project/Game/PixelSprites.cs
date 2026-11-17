using System.Collections.Generic;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Flat-colour 32x32 sprites for placeholder art. Point filtered, 32 pixels per unit.</summary>
    internal static class PixelSprites
    {
        private const int Size = 32;

        private static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();

        public static Sprite Solid(Color color)
        {
            Color32 c = color;
            int key = (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;

            if (Cache.TryGetValue(key, out Sprite cached) && cached != null)
                return cached;

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;

            var pixels = new Color32[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = c;

            texture.SetPixels32(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), Size);
            sprite.hideFlags = HideFlags.HideAndDontSave;

            Cache[key] = sprite;
            return sprite;
        }
    }
}
