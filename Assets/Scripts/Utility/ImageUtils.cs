using System;
using System.Runtime.InteropServices;
using UnityEngine;

/**<summary> General utility functions for handling image data </summary>*/
public class ImageUtils {

    /**<summary> Flip image pixels Color32 array horizontally or vertically </summary>*/
	public static Color32[] Flip (Color32[] pixels, int width, int height, bool horizontal)
    {
        Color32[] result = new Color32[width*height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (horizontal)
                    result[width - 1 - x + y * width] = pixels[x + y * width];
                else
                    result[x + (height - 1 - y) * width] = pixels[x + y * width];
            }
        }
        return result;
    }

    /**<summary> Rotate image pixels Color32 array clockwise or counterclockwise 90 degrees </summary>*/
    public static Color32[] Rotate (Color32[] pixels, int width, int height, bool clockwise)
    {
        Color32[] result = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (clockwise)
                    result[y + (width - 1 - x) * height] = pixels[x + y * width];
                else
                    result[(height - 1) - y + x * height] = pixels[x + y * width];
            }
        }
        return result;
    }

    /**<summary> Get Color32 pixel array from RGBA byte data </summary>*/
    public static Color32[] GetPixels(byte[] imageBytes)
    {
        Color32[] pixels = new Color32[imageBytes.Length / 4];
        for (int i = 0; i < imageBytes.Length; i += 4)
        {
            pixels[i / 4] = new Color32(imageBytes[i], imageBytes[i + 1], imageBytes[i + 2], imageBytes[i + 3]);
        }
        return pixels;
    }

    /**<summary> Convert Color32 array to byte array </summary>*/
    public static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        int length = Marshal.SizeOf(typeof(Color32)) * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }

    /**<summary> Get image height based on device model to avoid strecth </summary>*/
    public static int ImageHeight
    {
        get
        {
            switch (SystemInfo.deviceModel)
            {
                case "OnePlus ONEPLUS A6003":
                    return 1080;
                default:
                    return 1080;
            }
        }
    }
}
