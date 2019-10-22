﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace UnitySourceEngine
{
    public class SourceTexture
    {
        public static bool averageTextures = false;
        public static int maxTextureSize = 1024;

        private static Dictionary<string, SourceTexture> loadedTextures = new Dictionary<string, SourceTexture>();
        public string location { get; private set; }

        private Texture2D texture;
        private Color[] pixels;
        public int width { get; private set; }
        public int height { get; private set; }

        private SourceTexture(string textureLocation)
        {
            location = textureLocation;
            loadedTextures.Add(location, this);
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(location) && loadedTextures.ContainsKey(location))
                loadedTextures.Remove(location);
            pixels = null;
            if (texture != null)
                UnityEngine.Object.Destroy(texture);
            texture = null;
        }
        public static void ClearCache()
        {
            foreach (var texPair in loadedTextures)
                texPair.Value.Dispose();
            loadedTextures.Clear();
            loadedTextures = new Dictionary<string, SourceTexture>();
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);
                texture.Apply();
            }
            return texture;
        }
        public static SourceTexture ReadAndCache(byte[] vtfData, string location)
        {
            SourceTexture vtf;
            using (MemoryStream ms = new MemoryStream(vtfData))
            {
                vtf = ReadAndCache(ms, 0, location);
            }
            return vtf;
        }
        public static SourceTexture ReadAndCache(Stream stream, long origOffset, string location)
        {
            string fixedLocation = location.Replace("\\", "/").ToLower();

            SourceTexture srcTexture = null;
            if (loadedTextures.ContainsKey(fixedLocation))
            {
                srcTexture = loadedTextures[fixedLocation];
            }
            else
            {
                srcTexture = new SourceTexture(fixedLocation);

                Color[] pixels = null;
                int width = 0;
                int height = 0;

                pixels = LoadVTFFile(stream, origOffset, out width, out height);

                if (pixels != null)
                {
                    if (averageTextures)
                    {
                        pixels = MakePlain(AverageTexture(pixels), 4, 4);
                        width = 4;
                        height = 4;
                    }
                    else
                        pixels = DecreaseTextureSize(pixels, width, height, maxTextureSize, out width, out height);
                }

                srcTexture.pixels = pixels;
                srcTexture.width = width;
                srcTexture.height = height;
            }

            return srcTexture;
        }
        public static SourceTexture GrabTexture(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            SourceTexture srcTexture = null;

            if (!string.IsNullOrEmpty(rawPath))
            {
                string vtfFilePath = FixLocation(bspParser, vpkParser, rawPath);
                if (!string.IsNullOrEmpty(vtfFilePath))
                {
                    if (loadedTextures.ContainsKey(vtfFilePath))
                    {
                        srcTexture = loadedTextures[vtfFilePath];
                    }
                    else
                    {
                        if (bspParser != null && bspParser.HasPakFile(vtfFilePath))
                        {
                            //Debug.Log("Loaded " + vtfFilePath + " from pakfile");
                            srcTexture = ReadAndCache(bspParser.GetPakFile(vtfFilePath), vtfFilePath);
                        }
                        else if (vpkParser != null && vpkParser.FileExists(vtfFilePath))
                        {
                            try
                            {
                                vpkParser.LoadFileAsStream(vtfFilePath, (stream, origOffset, fileLength) => { srcTexture = ReadAndCache(stream, origOffset, vtfFilePath); });
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("SourceTexture: " + e.ToString());
                            }
                        }
                        else
                            Debug.LogError("SourceTexture: Could not find Texture FixedPath(" + vtfFilePath + ") RawPath(" + rawPath + ")");
                    }
                }
                else
                    Debug.LogError("SourceTexture: Could not find texture at " + rawPath);
            }
            else
                Debug.LogError("SourceTexture: Texture string path is null or empty");

            return srcTexture;
        }

        public static string FixLocation(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            string fixedLocation = rawPath.Replace("\\", "/").ToLower();

            if (!Path.GetExtension(fixedLocation).Equals(".vtf") && (bspParser == null || !bspParser.HasPakFile(fixedLocation)) && (vpkParser == null || !vpkParser.FileExists(fixedLocation)))
                fixedLocation += ".vtf";
            if ((bspParser == null || !bspParser.HasPakFile(fixedLocation)) && (vpkParser == null || !vpkParser.FileExists(fixedLocation)))
                fixedLocation = Path.Combine("materials", fixedLocation).Replace("\\", "/");

            return fixedLocation;
        }

        public static Color[] DecreaseTextureSize(Color[] pixels, int origWidth, int origHeight, int maxSize, out int decreasedWidth, out int decreasedHeight)
        {
            Color[] decreased = pixels;
            decreasedWidth = origWidth;
            decreasedHeight = origHeight;
            if (Mathf.Max(origWidth, origHeight) > maxSize)
            {
                float ratio = Mathf.Max(origWidth, origHeight) / (float)maxSize;
                decreasedWidth = (int)(origWidth / ratio);
                decreasedHeight = (int)(origHeight / ratio);

                decreased = DecreaseTextureSize(pixels, origWidth, origHeight, decreasedWidth, decreasedHeight);
            }
            return decreased;
        }
        public static Color[] DecreaseTextureSize(Color[] pixels, int origWidth, int origHeight, int newWidth, int newHeight)
        {
            Color[] scaledTexture = null;
            if (newWidth < origWidth && newHeight < origHeight)
            {
                int divX = Mathf.FloorToInt((float)Mathf.Max(origWidth, newWidth) / Mathf.Min(origWidth, newWidth));
                int divY = Mathf.FloorToInt((float)Mathf.Max(origHeight, newHeight) / Mathf.Min(origHeight, newHeight));

                scaledTexture = new Color[newWidth * newHeight];
                for (int col = 0; col < newWidth; col++)
                {
                    for (int row = 0; row < newHeight; row++)
                    {
                        float red = 0, green = 0, blue = 0, alpha = 0;
                        int pixelCount = 0;
                        for (int x = -(divX - 1); x <= (divX - 1); x++)
                        {
                            for (int y = -(divY - 1); y <= (divY - 1); y++)
                            {
                                int mappedCol = (col + 0) * divX;
                                int mappedRow = (row + 0) * divY;
                                int mappedIndex = ((mappedRow + y) * origWidth) + mappedCol + x;
                                if (mappedIndex >= 0 && mappedIndex < pixels.Length)
                                {
                                    Color currentColor = pixels[mappedIndex];
                                    red += currentColor.r;
                                    green += currentColor.g;
                                    blue += currentColor.b;
                                    alpha += currentColor.a;
                                    pixelCount++;
                                }
                            }
                        }
                        Color avgColor = new Color(red / pixelCount, green / pixelCount, blue / pixelCount, alpha / pixelCount);
                        scaledTexture[(row * newWidth) + col] = avgColor;
                    }
                }
            }
            return scaledTexture;
        }
        public static Color AverageTexture(Color[] pixels)
        {
            Color allColorsInOne = new Color();
            foreach (Color color in pixels)
            {
                allColorsInOne.r += color.r;
                allColorsInOne.g += color.g;
                allColorsInOne.b += color.b;
                allColorsInOne.a += color.a;
            }

            allColorsInOne.r /= pixels.Length;
            allColorsInOne.g /= pixels.Length;
            allColorsInOne.b /= pixels.Length;
            allColorsInOne.a /= pixels.Length;

            return allColorsInOne;
        }
        public static Color[] MakePlain(Color mainColor, int width, int height)
        {
            Color[] plain = new Color[width * height];
            for (int i = 0; i < plain.Length; i++)
                plain[i] = mainColor;
            return plain;
        }

        public static Color[] LoadVTFFile(Stream stream, long vtfBytePosition, out int width, out int height)
        {
            //Texture2D extracted = null;
            Color[] extracted = null;
            width = 0; height = 0;
            if (stream != null)
            {
                stream.Position = vtfBytePosition;
                int signature = DataParser.ReadInt(stream);
                if (signature == VTFHeader.signature)
                {
                    #region Read Header
                    VTFHeader vtfHeader;
                    uint[] version = new uint[] { DataParser.ReadUInt(stream), DataParser.ReadUInt(stream) };
                    vtfHeader.version = (version[0]) + (version[1] / 10f);
                    vtfHeader.headerSize = DataParser.ReadUInt(stream);
                    vtfHeader.width = DataParser.ReadUShort(stream);
                    vtfHeader.height = DataParser.ReadUShort(stream);
                    vtfHeader.flags = DataParser.ReadUInt(stream);
                    vtfHeader.frames = DataParser.ReadUShort(stream);
                    vtfHeader.firstFrame = DataParser.ReadUShort(stream);
                    vtfHeader.padding0 = new byte[4];
                    stream.Read(vtfHeader.padding0, 0, 4);
                    vtfHeader.reflectivity = new float[] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                    vtfHeader.padding1 = new byte[4];
                    stream.Read(vtfHeader.padding1, 0, 4);
                    vtfHeader.bumpmapScale = DataParser.ReadFloat(stream);
                    vtfHeader.highResImageFormat = (VTFImageFormat)DataParser.ReadUInt(stream);
                    vtfHeader.mipmapCount = DataParser.ReadByte(stream);
                    vtfHeader.lowResImageFormat = (VTFImageFormat)DataParser.ReadUInt(stream);
                    vtfHeader.lowResImageWidth = DataParser.ReadByte(stream);
                    vtfHeader.lowResImageHeight = DataParser.ReadByte(stream);

                    vtfHeader.depth = 1;
                    vtfHeader.resourceCount = 0;
                    vtfHeader.resources = new VTFResource[0];

                    if (vtfHeader.version >= 7.2f)
                    {
                        vtfHeader.depth = DataParser.ReadUShort(stream);

                        if (vtfHeader.version >= 7.3)
                        {
                            vtfHeader.padding2 = new byte[3];
                            stream.Read(vtfHeader.padding2, 0, 3);
                            vtfHeader.resourceCount = DataParser.ReadUInt(stream);

                            if (vtfHeader.version >= 7.4)
                            {
                                vtfHeader.padding3 = new byte[8];
                                stream.Read(vtfHeader.padding3, 0, 8);
                                vtfHeader.resources = new VTFResource[vtfHeader.resourceCount];
                                for (int i = 0; i < vtfHeader.resources.Length; i++)
                                {
                                    vtfHeader.resources[i].type = DataParser.ReadUInt(stream);
                                    vtfHeader.resources[i].data = DataParser.ReadUInt(stream);
                                }
                            }
                        }
                    }
                    #endregion

                    int thumbnailBufferSize = 0;
                    int imageBufferSize = (int)ComputeImageBufferSize(vtfHeader.width, vtfHeader.height, vtfHeader.depth, vtfHeader.mipmapCount, vtfHeader.highResImageFormat) * vtfHeader.frames;
                    if (vtfHeader.lowResImageFormat != VTFImageFormat.IMAGE_FORMAT_NONE)
                        thumbnailBufferSize = (int)ComputeImageBufferSize(vtfHeader.lowResImageWidth, vtfHeader.lowResImageHeight, 1, vtfHeader.lowResImageFormat);

                    int thumbnailBufferOffset = 0, imageBufferOffset = 0;

                    #region Read Resource Directories
                    if (vtfHeader.resources.Length > 0)
                    {
                        for (int i = 0; i < vtfHeader.resources.Length; i++)
                        {
                            if ((VTFResourceEntryType)vtfHeader.resources[i].type == VTFResourceEntryType.VTF_LEGACY_RSRC_LOW_RES_IMAGE)
                                thumbnailBufferOffset = (int)vtfHeader.resources[i].data;
                            if ((VTFResourceEntryType)vtfHeader.resources[i].type == VTFResourceEntryType.VTF_LEGACY_RSRC_IMAGE)
                                imageBufferOffset = (int)vtfHeader.resources[i].data;
                        }
                    }
                    else
                    {
                        thumbnailBufferOffset = (int)vtfHeader.headerSize;
                        imageBufferOffset = thumbnailBufferOffset + thumbnailBufferSize;
                    }
                    #endregion

                    if (vtfHeader.highResImageFormat != VTFImageFormat.IMAGE_FORMAT_NONE)
                    {
                        int mipmapBufferOffset = 0;
                        for (uint i = 1; i <= vtfHeader.mipmapCount; i++)
                        {
                            mipmapBufferOffset += (int)ComputeMipmapSize(vtfHeader.width, vtfHeader.height, vtfHeader.depth, i, vtfHeader.highResImageFormat);
                        }
                        stream.Position = vtfBytePosition + imageBufferOffset + mipmapBufferOffset;
                        byte[] imageData = new byte[imageBufferSize];
                        stream.Read(imageData, 0, imageData.Length);
                        //DataParser.ReadBytes(ms, imageBufferOffset + mipmapBufferOffset, imageBufferSize);

                        extracted = DecompressImage(imageData, vtfHeader.width, vtfHeader.height, vtfHeader.highResImageFormat);
                        width = vtfHeader.width;
                        height = vtfHeader.height;
                        imageData = null;
                    }
                    else
                        Debug.LogError("SourceTexture: Image format given was none");
                }
                else
                    Debug.LogError("SourceTexture: Signature mismatch " + signature + " != " + VTFHeader.signature);
            }
            else
                Debug.LogError("SourceTexture: Missing VTF data");

            return extracted;
        }
        private static Color[] DecompressImage(byte[] data, ushort width, ushort height, VTFImageFormat imageFormat)
        {
            Color[] vtfColors = new Color[width * height];

            if (imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT1 || imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA)
                vtfColors = DecompressDXT1(data, width, height);
            else if (imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT3)
                vtfColors = DecompressDXT3(data, width, height);
            else if (imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT5)
                vtfColors = DecompressDXT5(data, width, height);
            else if (imageFormat == VTFImageFormat.IMAGE_FORMAT_BGR888)
                vtfColors = DecompressBGR888(data, width, height);
            else if (imageFormat == VTFImageFormat.IMAGE_FORMAT_BGRA8888)
                vtfColors = DecompressBGRA8888(data, width, height);
            else
                Debug.LogError("SourceTexture: Unsupported format " + imageFormat);

            vtfColors = RotateProperly(vtfColors, width, height);

            return vtfColors;
        }
        private static Color[] DecompressBGR888(byte[] data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            int currentDataIndex = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte blue = data[currentDataIndex];
                    byte green = data[currentDataIndex + 1];
                    byte red = data[currentDataIndex + 2];
                    currentDataIndex += 3;

                    int flattenedIndex = row * width + col;
                    texture2DColors[flattenedIndex] = new Color(((float)red) / byte.MaxValue, ((float)green) / byte.MaxValue, ((float)blue) / byte.MaxValue);
                }
            }

            return texture2DColors;
        }
        private static Color[] DecompressBGRA8888(byte[] data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            int currentDataIndex = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte blue = data[currentDataIndex];
                    byte green = data[currentDataIndex + 1];
                    byte red = data[currentDataIndex + 2];
                    byte alpha = data[currentDataIndex + 3];
                    currentDataIndex += 4;

                    int flattenedIndex = row * width + col;
                    texture2DColors[flattenedIndex] = new Color(((float)red) / byte.MaxValue, ((float)green) / byte.MaxValue, ((float)blue) / byte.MaxValue, ((float)alpha) / byte.MaxValue);
                }
            }

            return texture2DColors;
        }
        private static Color[] DecompressDXT1(byte[] data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            int currentDataIndex = 0;
            bool exceededArray = false;
            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    if (currentDataIndex + 7 < data.Length)
                    {
                        color0Data = BitConverter.ToUInt16(data, currentDataIndex);
                        color1Data = BitConverter.ToUInt16(data, currentDataIndex + 2);
                        bitmask = BitConverter.ToUInt32(data, currentDataIndex + 4);
                    }
                    else
                    {
                        Debug.LogError("DXT1: " + currentDataIndex + " > " + data.Length);
                        exceededArray = true;
                        break;
                    }
                    currentDataIndex += 8;

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                        new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                        new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                        new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                        new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };

                    if (color0Data < color1Data)
                    {
                        colorPalette[2] = new Color(((colors0[0] + colors1[0]) / 2) / 255f, ((colors0[1] + colors1[1]) / 2) / 255f, ((colors0[2] + colors1[2]) / 2) / 255f);
                        colorPalette[3] = new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f);
                    }

                    int blockIndex = 0;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                    }
                }
                if (exceededArray)
                    break;
            }

            return texture2DColors.ToArray();
        }
        private static Color[] DecompressDXT3(byte[] data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            int currentDataIndex = 0;
            bool exceededArray = false;
            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    currentDataIndex += 8;
                    if (currentDataIndex + 7 < data.Length)
                    {
                        color0Data = BitConverter.ToUInt16(data, currentDataIndex);
                        color1Data = BitConverter.ToUInt16(data, currentDataIndex + 2);
                        bitmask = BitConverter.ToUInt32(data, currentDataIndex + 4);
                    }
                    else
                    {
                        Debug.LogError("DXT3: " + currentDataIndex + " > " + data.Length);
                        exceededArray = true;
                        break;
                    }
                    currentDataIndex += 8;

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                    new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                    new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                    new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                    new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };

                    if (color0Data < color1Data)
                    {
                        colorPalette[2] = new Color(((colors0[0] + colors1[0]) / 2) / 255f, ((colors0[1] + colors1[1]) / 2) / 255f, ((colors0[2] + colors1[2]) / 2) / 255f);
                        colorPalette[3] = new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f);
                    }

                    int blockIndex = 0;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                    }
                }
                if (exceededArray) break;
            }

            return texture2DColors.ToArray();
        }
        private static Color[] DecompressDXT5(byte[] data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            int currentDataIndex = 0;
            bool exceededArray = false;
            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    #region Alpha Information
                    byte alpha0Data = 0;
                    byte alpha1Data = 0;
                    uint alphamask = 0;

                    if (currentDataIndex + 7 < data.Length)
                    {
                        alpha0Data = data[currentDataIndex];
                        alpha1Data = data[currentDataIndex + 1];
                        alphamask = BitConverter.ToUInt32(new byte[] { data[currentDataIndex + 2], data[currentDataIndex + 3], data[currentDataIndex + 4], data[currentDataIndex + 5], data[currentDataIndex + 6], data[currentDataIndex + 7] }, 0);
                    }
                    else
                    {
                        Debug.LogError("DXT5: " + currentDataIndex + " > " + data.Length);
                        exceededArray = true;
                        break;
                    }
                    currentDataIndex += 8;

                    float[] alphaPalette = new float[]
                    {
                    alpha0Data / 255f,
                    alpha1Data / 255f,
                    ((6 * alpha0Data + 1 * alpha1Data + 3) / 7) / 255f,
                    ((5 * alpha0Data + 2 * alpha1Data + 3) / 7) / 255f,
                    ((4 * alpha0Data + 3 * alpha1Data + 3) / 7) / 255f,
                    ((3 * alpha0Data + 4 * alpha1Data + 3) / 7) / 255f,
                    ((2 * alpha0Data + 5 * alpha1Data + 3) / 7) / 255f,
                    ((1 * alpha0Data + 6 * alpha1Data + 3) / 7) / 255f
                    };

                    if (alpha0Data <= alpha1Data)
                    {
                        alphaPalette[2] = (4 * alpha0Data + 1 * alpha1Data + 2) / 5;
                        alphaPalette[3] = (3 * alpha0Data + 2 * alpha1Data + 2) / 5;
                        alphaPalette[4] = (2 * alpha0Data + 3 * alpha1Data + 2) / 5;
                        alphaPalette[5] = (1 * alpha0Data + 4 * alpha1Data + 2) / 5;
                        alphaPalette[6] = 0;
                        alphaPalette[7] = 1;
                    }
                    #endregion

                    #region Color Information
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    if (currentDataIndex + 7 < data.Length)
                    {
                        color0Data = BitConverter.ToUInt16(data, currentDataIndex);
                        color1Data = BitConverter.ToUInt16(data, currentDataIndex + 2);
                        bitmask = BitConverter.ToUInt32(data, currentDataIndex + 4);
                    }
                    else { Debug.Log("Error: " + currentDataIndex + " > " + data.Length); exceededArray = true; break; }
                    currentDataIndex += 8;

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                    new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                    new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                    new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                    new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };
                    #endregion

                    #region Place All Information
                    int blockIndex = 0;
                    uint alphaBlockIndex1 = alphamask & 0x07, alphaBlockIndex2 = alphamask & 0x38;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            if (blockY < 2) colorInBlock.a = alphaPalette[alphaBlockIndex1 & 0x07];
                            else colorInBlock.a = alphaPalette[alphaBlockIndex2 & 0x07];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                        alphaBlockIndex1 >>= 3;
                        alphaBlockIndex2 >>= 3;
                    }
                    #endregion
                }
                if (exceededArray) break;
            }

            return texture2DColors.ToArray();
        }
        private static uint ComputeImageBufferSize(uint width, uint height, uint depth, VTFImageFormat imageFormat)
        {
            uint tempWidth = width, tempHeight = height;

            if (imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT1 || imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT1_ONEBITALPHA)
            {
                if (tempWidth < 4 && tempWidth > 0)
                    tempWidth = 4;

                if (tempHeight < 4 && tempHeight > 0)
                    tempHeight = 4;

                return ((tempWidth + 3) / 4) * ((tempHeight + 3) / 4) * 8 * depth;
            }
            else if (imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT3 || imageFormat == VTFImageFormat.IMAGE_FORMAT_DXT5)
            {
                if (tempWidth < 4 && tempWidth > 0)
                    tempWidth = 4;

                if (tempHeight < 4 && tempHeight > 0)
                    tempHeight = 4;

                return ((tempWidth + 3) / 4) * ((tempHeight + 3) / 4) * 16 * depth;
            }
            else return (uint)(tempWidth * tempHeight * depth * VTFImageConvertInfo[(int)imageFormat, (int)VTFImageConvertInfoIndex.bytesPerPixel]);
        }
        private static uint ComputeImageBufferSize(uint width, uint height, uint depth, uint mipmaps, VTFImageFormat imageFormat)
        {
            uint uiImageSize = 0, tempWidth = width, tempHeight = height;

            if (tempWidth > 0 && tempHeight > 0 && depth > 0)
            {
                for (int i = 0; i < mipmaps; i++)
                {
                    uiImageSize += ComputeImageBufferSize(tempWidth, tempHeight, depth, imageFormat);

                    tempWidth >>= 1;
                    tempHeight >>= 1;
                    depth >>= 1;

                    if (tempWidth < 1)
                        tempWidth = 1;

                    if (tempHeight < 1)
                        tempHeight = 1;

                    if (depth < 1)
                        depth = 1;
                }
            }

            return uiImageSize;
        }
        private static void ComputeMipmapDimensions(uint width, uint height, uint depth, uint mipmapLevel, out uint mipmapWidth, out uint mipmapHeight, out uint mipmapDepth)
        {
            // work out the width/height by taking the orignal dimension
            // and bit shifting them down uiMipmapLevel times
            mipmapWidth = width >> (int)mipmapLevel;
            mipmapHeight = height >> (int)mipmapLevel;
            mipmapDepth = depth >> (int)mipmapLevel;

            // stop the dimension being less than 1 x 1
            if (mipmapWidth < 1)
                mipmapWidth = 1;

            if (mipmapHeight < 1)
                mipmapHeight = 1;

            if (mipmapDepth < 1)
                mipmapDepth = 1;
        }
        private static uint ComputeMipmapSize(uint width, uint height, uint depth, uint mipmapLevel, VTFImageFormat ImageFormat)
        {
            // figure out the width/height of this MIP level
            uint uiMipmapWidth, uiMipmapHeight, uiMipmapDepth;
            ComputeMipmapDimensions(width, height, depth, mipmapLevel, out uiMipmapWidth, out uiMipmapHeight, out uiMipmapDepth);

            // return the memory requirements
            return ComputeImageBufferSize(uiMipmapWidth, uiMipmapHeight, uiMipmapDepth, ImageFormat);
        }
        public static Color[] RotateProperly(Color[] vtfColors, uint width, uint height)
        {
            Color[] proper = vtfColors.Reverse().ToArray();
            for (uint row = 0; row < height; row++)
            {
                for (uint col = 0; col < (width / 2); col++)
                {
                    uint currentRowIndex = row * width;
                    Color tempColor = proper[currentRowIndex + col];
                    proper[currentRowIndex + col] = proper[currentRowIndex + (width - col - 1)];
                    proper[currentRowIndex + (width - col - 1)] = tempColor;
                }
            }
            return proper;
        }

        #region Image Convert Info
        enum VTFImageConvertInfoIndex
        {
            bitsPerPixel, // Format bits per color.
            bytesPerPixel, // Format bytes per pixel.
            redBitsPerPixel, // Format conversion red bits per pixel.  0 for N/A.
            greenBitsPerPixel, // Format conversion green bits per pixel.  0 for N/A.
            blueBitsPerPixel, // Format conversion blue bits per pixel.  0 for N/A.
            alphaBitsPerPixel, // Format conversion alpha bits per pixel.  0 for N/A.
            redIndex, // "Red" index.
            greenIndex, // "Green" index.
            blueIndex, // "Blue" index.
            alphaIndex, // "Alpha" index.
        }

        static short[,] VTFImageConvertInfo = new short[,] {
        { 32,  4,  8,  8,  8,  8, 0,  1,  2,  3 },
        { 32,  4,  8,  8,  8,  8, 3,  2,  1,  0 },
        { 24,  3,  8,  8,  8,  0, 0,  1,  2, -1 },
        { 24,  3,  8,  8,  8,  0, 2,  1,  0, -1 },
        { 16,  2,  5,  6,  5,  0, 0,  1,  2, -1 },
        { 8,  1,  8,  8,  8,  0, 0, -1, -1, -1 },
        { 16,  2,  8,  8,  8,  8, 0, -1, -1,  1 },
        { 8,  1,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 8,  1,  0,  0,  0,  8, -1, -1, -1,  0 },
        { 24,  3,  8,  8,  8,  8, 0,  1,  2, -1 },
        { 24,  3,  8,  8,  8,  8, 2,  1,  0, -1 },
        { 32,  4,  8,  8,  8,  8, 3,  0,  1,  2 },
        { 32,  4,  8,  8,  8,  8, 2,  1,  0,  3 },
        { 4,  0,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 8,  0,  0,  0,  0,  8, -1, -1, -1, -1 },
        { 8,  0,  0,  0,  0,  8, -1, -1, -1, -1 },
        { 32,  4,  8,  8,  8,  0, 2,  1,  0, -1 },
        { 16,  2,  5,  6,  5,  0, 2,  1,  0, -1 },
        { 16,  2,  5,  5,  5,  0, 2,  1,  0, -1 },
        { 16,  2,  4,  4,  4,  4, 2,  1,  0,  3 },
        { 4,  0,  0,  0,  0,  1, -1, -1, -1, -1 },
        { 16,  2,  5,  5,  5,  1, 2,  1,  0,  3 },
        { 16,  2,  8,  8,  0,  0, 0,  1, -1, -1 },
        { 32,  4,  8,  8,  8,  8, 0,  1,  2,  3 },
        { 64,  8, 16, 16, 16, 16, 0,  1,  2,  3 },
        { 64,  8, 16, 16, 16, 16, 0,  1,  2,  3 },
        { 32,  4,  8,  8,  8,  8, 0,  1,  2,  3 },
        { 32,  4, 32,  0,  0,  0, 0, -1, -1, -1 },
        { 96, 12, 32, 32, 32,  0, 0,  1,  2, -1 },
        { 128, 16, 32, 32, 32, 32, 0,  1,  2,  3 },
        { 16,  2, 16,  0,  0,  0, 0, -1, -1, -1 },
        { 24,  3, 24,  0,  0,  0, 0, -1, -1, -1 },
        { 32,  4,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 24,  3,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 16,  2, 16,  0,  0,  0, 0, -1, -1, -1 },
        { 24,  3, 24,  0,  0,  0, 0, -1, -1, -1 },
        { 32,  4,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 4,  0,  0,  0,  0,  0, -1, -1, -1, -1 },
        { 8,  0,  0,  0,  0,  0, -1, -1, -1, -1 }
    };
        #endregion
        #region Texture Flags
        [System.Flags]
        enum VTFImageFlag
        {
            TEXTUREFLAGS_POINTSAMPLE = 0x00000001,
            TEXTUREFLAGS_TRILINEAR = 0x00000002,
            TEXTUREFLAGS_CLAMPS = 0x00000004,
            TEXTUREFLAGS_CLAMPT = 0x00000008,
            TEXTUREFLAGS_ANISOTROPIC = 0x00000010,
            TEXTUREFLAGS_HINT_DXT5 = 0x00000020,
            TEXTUREFLAGS_SRGB = 0x00000040, // Originally internal to VTex as TEXTUREFLAGS_NOCOMPRESS.
            TEXTUREFLAGS_DEPRECATED_NOCOMPRESS = 0x00000040,
            TEXTUREFLAGS_NORMAL = 0x00000080,
            TEXTUREFLAGS_NOMIP = 0x00000100,
            TEXTUREFLAGS_NOLOD = 0x00000200,
            TEXTUREFLAGS_MINMIP = 0x00000400,
            TEXTUREFLAGS_PROCEDURAL = 0x00000800,
            TEXTUREFLAGS_ONEBITALPHA = 0x00001000, //!< Automatically generated by VTex.
            TEXTUREFLAGS_EIGHTBITALPHA = 0x00002000, //!< Automatically generated by VTex.
            TEXTUREFLAGS_ENVMAP = 0x00004000,
            TEXTUREFLAGS_RENDERTARGET = 0x00008000,
            TEXTUREFLAGS_DEPTHRENDERTARGET = 0x00010000,
            TEXTUREFLAGS_NODEBUGOVERRIDE = 0x00020000,
            TEXTUREFLAGS_SINGLECOPY = 0x00040000,
            TEXTUREFLAGS_UNUSED0 = 0x00080000, //!< Originally internal to VTex as TEXTUREFLAGS_ONEOVERMIPLEVELINALPHA.
            TEXTUREFLAGS_DEPRECATED_ONEOVERMIPLEVELINALPHA = 0x00080000,
            TEXTUREFLAGS_UNUSED1 = 0x00100000, //!< Originally internal to VTex as TEXTUREFLAGS_PREMULTCOLORBYONEOVERMIPLEVEL.
            TEXTUREFLAGS_DEPRECATED_PREMULTCOLORBYONEOVERMIPLEVEL = 0x00100000,
            TEXTUREFLAGS_UNUSED2 = 0x00200000, //!< Originally internal to VTex as TEXTUREFLAGS_NORMALTODUDV.
            TEXTUREFLAGS_DEPRECATED_NORMALTODUDV = 0x00200000,
            TEXTUREFLAGS_UNUSED3 = 0x00400000, //!< Originally internal to VTex as TEXTUREFLAGS_ALPHATESTMIPGENERATION.
            TEXTUREFLAGS_DEPRECATED_ALPHATESTMIPGENERATION = 0x00400000,
            TEXTUREFLAGS_NODEPTHBUFFER = 0x00800000,
            TEXTUREFLAGS_UNUSED4 = 0x01000000, //!< Originally internal to VTex as TEXTUREFLAGS_NICEFILTERED.
            TEXTUREFLAGS_DEPRECATED_NICEFILTERED = 0x01000000,
            TEXTUREFLAGS_CLAMPU = 0x02000000,
            TEXTUREFLAGS_VERTEXTEXTURE = 0x04000000,
            TEXTUREFLAGS_SSBUMP = 0x08000000,
            TEXTUREFLAGS_UNUSED5 = 0x10000000, //!< Originally TEXTUREFLAGS_UNFILTERABLE_OK.
            TEXTUREFLAGS_DEPRECATED_UNFILTERABLE_OK = 0x10000000,
            TEXTUREFLAGS_BORDER = 0x20000000,
            TEXTUREFLAGS_DEPRECATED_SPECVAR_RED = 0x40000000,
            //TEXTUREFLAGS_DEPRECATED_SPECVAR_ALPHA = 0x80000000,
            TEXTUREFLAGS_LAST = 0x20000000,
            TEXTUREFLAGS_COUNT = 30
        }
        #endregion
    }
}