using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class MyExtensions
{
    /// <summary>
    /// Returns whether the operation was successful.
    /// It may fail if the graphics hardware is old enough.
    /// </summary>
    public static bool CopyTo(this Texture2D src, Texture2D dest)
    {
        if ((SystemInfo.copyTextureSupport | UnityEngine.Rendering.CopyTextureSupport.Basic) == 0)
            return false;

        if (dest.width != src.width || dest.height != src.height)
            dest.Resize(src.width, src.height);

        Graphics.CopyTexture(src, dest);
        return true;
    }

    /// <summary>
    /// Applies the given filter to every channel in every pixel in this texture.
    /// </summary>
    public static void Filter(this Texture2D tex, Func<float, float> filter,
                              bool updateMipmaps = true, bool makeNoLongerReadable = false)
    {
        var pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; ++i)
        {
            var color = pixels[i];
            color = new Color(filter(color.r), filter(color.g), filter(color.b), filter(color.a));
            pixels[i] = color;
        }

        tex.SetPixels(pixels);
        tex.Apply(updateMipmaps, makeNoLongerReadable);
    }
    /// <summary>
    /// Applies the given filter to every channel in every pixel in this texture.
    /// </summary>
    public static void Filter32(this Texture2D tex, Func<byte, byte> filter,
                                bool updateMipmaps = true, bool makeNoLongerReadable = false)
    {
        var pixels = tex.GetPixels32();
        for (int i = 0; i < pixels.Length; ++i)
        {
            var color = pixels[i];
            color = new Color32(filter(color.r), filter(color.g), filter(color.b), filter(color.a));
            pixels[i] = color;
        }

        tex.SetPixels32(pixels);
        tex.Apply(updateMipmaps, makeNoLongerReadable);
    }

    /// <summary>
    /// Applies the given filter to every pixel in this texture.
    /// </summary>
    /// <param name="filterPixelAtPos">
    /// The filter.
    /// The first two arguments are the XY position of the pixel,
    ///     the third argument is the pixel color,
    ///     and the fourth argument is the array of pixel data before filtering.
    /// </param>
    public static void Filter(this Texture2D tex,
                              Func<int, int, Color, Color[], Color> filterPixelAtPos,
                              bool updateMipmaps = true, bool makeNoLongerReadable = false)
    {
        var pixels = tex.GetPixels();
        var newPixels = new Color[pixels.Length];

        for (int y = 0; y < tex.height; ++y)
            for (int x = 0; x < tex.width; ++x)
            {
                int i = x + (y * tex.width);
                newPixels[i] = filterPixelAtPos(x, y, pixels[i], pixels);
            }

        tex.SetPixels(newPixels);
        tex.Apply(updateMipmaps, makeNoLongerReadable);
    }
    /// <summary>
    /// Applies the given filter to every pixel in this texture.
    /// </summary>
    /// <param name="filterPixelAtPos">
    /// The filter.
    /// The first two arguments are the XY position of the pixel,
    ///     the third argument is the pixel color,
    ///     and the fourth argument is the array of pixel data before filtering.
    /// </param>
    public static void Filter32(this Texture2D tex,
                                Func<int, int, Color32, Color32[], Color32> filterPixelAtPos,
                                bool updateMipmaps = true, bool makeNoLongerReadable = false)
    {
        var pixels = tex.GetPixels32();
        var newPixels = new Color32[pixels.Length];

        for (int y = 0; y < tex.height; ++y)
            for (int x = 0; x < tex.width; ++x)
            {
                int i = x + (y * tex.width);
                newPixels[i] = filterPixelAtPos(x, y, pixels[i], pixels);
            }

        tex.SetPixels32(newPixels);
        tex.Apply(updateMipmaps, makeNoLongerReadable);
    }
}