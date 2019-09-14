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

        public static VMTData ReadAndCache(Stream stream, long endOfVMT, string location)
        {
            string fixedLocation = location.ToLower();
            if (fixedLocation.LastIndexOf(".") > 0)
                fixedLocation = fixedLocation.Substring(0, fixedLocation.LastIndexOf("."));
            if (fixedLocation.IndexOf("materials/") >= 0)
                fixedLocation = fixedLocation.Substring(fixedLocation.IndexOf("materials/") + "materials/".Length);

            //UnityEngine.Debug.Log("Storing " + fixedLocation);

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

        public static VMTData GrabVMT(VPKParser vpkParser, string location)
        {
            VMTData vmtData = null;

            Debug.Assert(!string.IsNullOrEmpty(location));

            if (!string.IsNullOrEmpty(location))
            {
                string fixedLocation = location.Replace("\\", "/").ToLower();

                if (vmtCache.ContainsKey(fixedLocation))
                {
                    vmtData = vmtCache[fixedLocation];
                }
                else
                {
                    string vmtFilePath = "/materials/" + fixedLocation + ".vmt";
                    if (vpkParser.FileExists(vmtFilePath))
                    {
                        try
                        {
                            vpkParser.LoadFileAsStream(vmtFilePath, (stream, origOffset, fileLength) => { vmtData = ReadAndCache(stream, origOffset + fileLength, vmtFilePath); });
                        }
                        catch (System.Exception) { }
                    }
                    else
                    {
                        Debug.LogError("VMT: Could not find VMT file at FixedPath(" + vmtFilePath + ") RawPath(" + location + ")");
                    }
                }
            }
            return vmtData;
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