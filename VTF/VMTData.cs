using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySourceEngine
{
    public class VMTData
    {
        private static Dictionary<string, VMTData> vmtCache = new Dictionary<string, VMTData>();
        public string vtfPath { get; private set; }
        public string vmtPath { get; private set; }

        private VMTData(string _vmtPath)
        {
            vmtPath = _vmtPath;
            vmtCache.Add(vmtPath, this);
        }

        public static VMTData ReadAndCache(byte[] vmtData, string location)
        {
            VMTData vmt;
            using (MemoryStream ms = new MemoryStream(vmtData))
            {
                vmt = ReadAndCache(ms, ms.Length, location);
                Debug.Log("Read vmt from byte data and got " + vmt.vtfPath);
            }
            return vmt;
        }
        public static VMTData ReadAndCache(Stream stream, long endOfVMT, string location)
        {
            string fixedLocation = location.Replace("\\", "/").ToLower();

            VMTData vmtData = null;
            if (vmtCache.ContainsKey(fixedLocation))
            {
                vmtData = vmtCache[fixedLocation];
            }
            else
            {
                vmtData = new VMTData(fixedLocation);
                vmtData.vtfPath = GetLocationFromVMT(stream, endOfVMT);
            }

            return vmtData;
        }

        public static VMTData GrabVMT(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            VMTData vmtData = null;

            if (!string.IsNullOrEmpty(rawPath))
            {
                string vmtFilePath = FixLocation(bspParser, vpkParser, rawPath);

                if (vmtCache.ContainsKey(vmtFilePath))
                {
                    vmtData = vmtCache[vmtFilePath];
                }
                else
                {
                    if (bspParser != null && bspParser.HasPakFile(vmtFilePath))
                    {
                        //Debug.Log("Loaded " + vmtFilePath + " from pakfile");
                        vmtData = ReadAndCache(bspParser.GetPakFile(vmtFilePath), vmtFilePath);
                    }
                    else if (vpkParser != null && vpkParser.FileExists(vmtFilePath))
                    {
                        try
                        {
                            vpkParser.LoadFileAsStream(vmtFilePath, (stream, origOffset, fileLength) => { vmtData = ReadAndCache(stream, origOffset + fileLength, vmtFilePath); });
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("VMTData: " + e.ToString());
                        }
                    }
                    else
                    {
                        Debug.LogError("VMTData: Could not find VMT file at FixedPath(" + vmtFilePath + ") RawPath(" + rawPath + ")");
                    }
                }
            }
            else
                Debug.LogError("VMTData: Texture string path is null or empty");

            return vmtData;
        }

        public static string FixLocation(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            string fixedLocation = rawPath.Replace("\\", "/").ToLower();

            if ((bspParser == null || !bspParser.HasPakFile(fixedLocation)) && (vpkParser == null || !vpkParser.FileExists(fixedLocation)))
                fixedLocation += ".vmt";
            if ((bspParser == null || !bspParser.HasPakFile(fixedLocation)) && (vpkParser == null || !vpkParser.FileExists(fixedLocation)))
                fixedLocation = Path.Combine("materials", fixedLocation).Replace("\\", "/");

            return fixedLocation;
        }

        private static string GetLocationFromVMT(Stream stream, long endOfVMT)
        {
            string textureLocation = "";

            if (stream != null)
            {
                string baseTexture = "";

                while (stream.Position < endOfVMT)
                {
                    string line = DataParser.ReadNewlineTerminatedString(stream);
                    if (!string.IsNullOrEmpty(line) && line.IndexOf("$") > -1)
                    {
                        if (line.IndexOf("//") < 0 || line.IndexOf("$") < line.IndexOf("//"))
                        {
                            string materialInfo = line.Substring(line.IndexOf("$") + 1);
                            if (materialInfo.ToLower().IndexOf("basetexture") > -1)
                            {
                                int currentIndex = materialInfo.ToLower().IndexOf("basetexture");
                                if (currentIndex + "basetexture".Length < materialInfo.Length)
                                {
                                    currentIndex += "basetexture".Length;
                                    char charAtPoint = materialInfo[currentIndex];

                                    if (charAtPoint >= char.MinValue && charAtPoint <= char.MaxValue && !char.IsLetter(materialInfo, currentIndex) && !char.IsNumber(materialInfo, currentIndex))
                                    {
                                        if (materialInfo.Length - materialInfo.Replace("\"", "").Length == 3)
                                        {
                                            baseTexture = materialInfo.Substring(materialInfo.IndexOf("\"") + 1);
                                            baseTexture = baseTexture.Substring(baseTexture.IndexOf("\"") + 1);
                                            baseTexture = baseTexture.Substring(0, baseTexture.IndexOf("\""));
                                        }
                                        else if (materialInfo.Length - materialInfo.Replace("\"", "").Length == 2)
                                        {
                                            baseTexture = materialInfo.Substring(materialInfo.IndexOf("\"") + 1);
                                            baseTexture = baseTexture.Substring(0, baseTexture.IndexOf("\""));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                baseTexture = SourceTexture.RemoveMisleadingPath(baseTexture.Replace("\\", "/").ToLower());
                if (baseTexture.LastIndexOf(".") == baseTexture.Length - 4)
                    baseTexture = baseTexture.Substring(0, baseTexture.LastIndexOf("."));
                if (baseTexture.Length > 0)
                    textureLocation = baseTexture;
            }

            return textureLocation;
        }
    }
}