using MelonLoader;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace matechat.ui
{
    public static class EmbeddedAssets
    {
        /// <summary>
        /// Allows an Image Byte to Be Loaded into Unity Sprites
        /// </summary>
        /// <param name="img">Image Byte Array</param>
        /// <returns></returns>
        public static Sprite LoadButtonSprite(byte[] img)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(img))
            {
                MelonDebug.Msg($"Loaded Texture Dimensions: {texture.width}x{texture.height}");
                MelonDebug.Msg($"Texture Format: {texture.format}");

                Sprite sprite = Sprite.Create(texture,
                                              new Rect(0, 0, texture.width, texture.height),
                                              new Vector2(0.5f, 0.5f));

                MelonDebug.Msg($"Created Sprite: {sprite.rect.width}x{sprite.rect.height}");
                return sprite;
            }
            else
            {
                MelonDebug.Msg("Failed to load image into Texture2D");
            }
            return null;
        }

    }
}
