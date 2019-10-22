using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace UnitySourceEngine
{
    public class VMTData
    {
        private static Dictionary<string, VMTData> vmtCache = new Dictionary<string, VMTData>();

        public string vmtPath { get; private set; }

        public string includedVmtPath { get; private set; }
        private VMTData includedVmt;
        public string baseTexturePath { get; private set; }
        private SourceTexture baseTexture;
        public string bumpMapPath { get; private set; }
        private SourceTexture bumpMap;
        public string detailMapPath { get; private set; }
        private SourceTexture detailMap;

        private float glossiness;
        private bool hasTransparency;

        private Material material;
        private VMTDataWrapper wrappedData;

        private VMTData(string _vmtPath)
        {
            vmtPath = _vmtPath;
            vmtCache.Add(vmtPath, this);
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(vmtPath) && vmtCache.ContainsKey(vmtPath))
                vmtCache.Remove(vmtPath);
            if (material != null)
                Object.Destroy(material);

            includedVmt?.Dispose();
            baseTexture?.Dispose();
            bumpMap?.Dispose();
            detailMap?.Dispose();
        }

        public Material GetMaterial()
        {
            if (material == null)
            {
                if (includedVmt != null)
                    material = new Material(includedVmt.GetMaterial());
                else if (hasTransparency && bumpMap == null)
                    material = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
                else if (hasTransparency && bumpMap != null)
                    material = new Material(Shader.Find("Legacy Shaders/Transparent/Bumped Diffuse"));
                else
                    material = new Material(Shader.Find("Standard"));
                //else if (bumpMap != null)
                //    material = new Material(Shader.Find("Mobile/Bumped Diffuse Variant"));
                //else
                //    material = new Material(Shader.Find("Mobile/Diffuse"));

                if (baseTexture != null)
                    material.mainTexture = baseTexture.GetTexture();
                if (bumpMap != null && material.HasProperty("_BumpMap"))
                    material.SetTexture("_BumpMap", bumpMap.GetTexture());
                if (detailMap != null && material.HasProperty("_DetailMask"))
                    material.SetTexture("_DetailMask", detailMap.GetTexture());

                if (material.HasProperty("_Glossiness"))
                    material.SetFloat("_Glossiness", glossiness);
            }
            return material;
        }
        public static bool HasVMT(string key)
        {
            return vmtCache.ContainsKey(key);
        }

        /*public static VMTData GrabVMT(byte[] data, string rawPath)
        {
            VMTData vmtData = null;

            if (!string.IsNullOrEmpty(rawPath))
            {
                string vmtFilePath = rawPath.Replace("\\", "/").ToLower();

                if (vmtCache.ContainsKey(vmtFilePath))
                {
                    vmtData = vmtCache[vmtFilePath];
                }
                else
                {
                    vmtData = new VMTData(vmtFilePath);
                    vmtData.Parse(data);
                }
            }
            else
                Debug.LogError("VMTData: Given path is null or empty");

            return vmtData;
        }*/
        public static VMTData GrabVMT(BSPParser bspParser, VPKParser vpkParser, string rawPath, bool grabDependants = true)
        {
            VMTData vmtData = null;

            if (!string.IsNullOrEmpty(rawPath))
            {
                string vmtFilePath = FixLocation(bspParser, vpkParser, rawPath);
                if (vmtCache.ContainsKey(vmtFilePath))
                {
                    vmtData = vmtCache[vmtFilePath];
                    if (grabDependants)
                        vmtData.GrabDependants(bspParser, vpkParser);
                }
                else
                {
                    if (bspParser != null && bspParser.HasPakFile(vmtFilePath))
                    {
                        vmtData = new VMTData(vmtFilePath);
                        //Debug.Log("Loaded " + vmtFilePath + " from pakfile");
                        vmtData.Parse(bspParser.GetPakFile(vmtFilePath));
                        if (grabDependants)
                            vmtData.GrabDependants(bspParser, vpkParser);
                    }
                    else if (vpkParser != null && vpkParser.FileExists(vmtFilePath))
                    {
                        try
                        {
                            byte[] vmtByteData = null;
                            vpkParser.LoadFileAsStream(vmtFilePath, (stream, origOffset, fileLength) =>
                            {
                                vmtByteData = new byte[fileLength];
                                stream.Position = origOffset;
                                stream.Read(vmtByteData, 0, vmtByteData.Length);
                            });
                            vmtData = new VMTData(vmtFilePath);
                            vmtData.Parse(vmtByteData);
                            if (grabDependants)
                                vmtData.GrabDependants(bspParser, vpkParser);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("VMTData: " + e.ToString());
                        }
                    }
                    else
                    {
                        string vtfFilePath = SourceTexture.FixLocation(bspParser, vpkParser, rawPath);
                        if ((bspParser != null && bspParser.HasPakFile(vtfFilePath)) || (vpkParser != null && vpkParser.FileExists(vtfFilePath)))
                        {
                            vmtData = new VMTData(vmtFilePath);
                            vmtData.baseTexturePath = rawPath;
                            if (grabDependants)
                                vmtData.GrabDependants(bspParser, vpkParser);
                        }
                        else
                            Debug.LogError("VMTData: Could not find VMT file at FixedPath(" + vmtFilePath + ") RawPath(" + rawPath + ")");
                    }
                }
            }
            else
                Debug.LogError("VMTData: Given path is null or empty");

            return vmtData;
        }
        private void GrabDependants(BSPParser bspParser, VPKParser vpkParser)
        {
            if (!string.IsNullOrEmpty(includedVmtPath))
                includedVmt = VMTData.GrabVMT(bspParser, vpkParser, includedVmtPath);

            if (!string.IsNullOrEmpty(baseTexturePath))
                baseTexture = SourceTexture.GrabTexture(bspParser, vpkParser, baseTexturePath);
            if (!string.IsNullOrEmpty(bumpMapPath))
                bumpMap = SourceTexture.GrabTexture(bspParser, vpkParser, bumpMapPath);
            if (!string.IsNullOrEmpty(detailMapPath))
                detailMap = SourceTexture.GrabTexture(bspParser, vpkParser, detailMapPath);
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

        public void SetBaseTexture(SourceTexture texture)
        {
            baseTexture = texture;
        }
        public void SetBumpMap(SourceTexture texture)
        {
            bumpMap = texture;
        }
        private void Parse(byte[] byteData)
        {
            string originalVmtString = System.Text.Encoding.ASCII.GetString(byteData);
            wrappedData = VMTDataWrapper.WrapData(originalVmtString);

            
            if (wrappedData.IsOfType("patch"))
            {
                includedVmtPath = wrappedData.GetValue("include");
            }
            else//if (wrappedData.dataTitle.Equals("lightmappedgeneric"))
            {
                string surfaceProp = wrappedData.GetValue("$surfaceprop");
                if (!string.IsNullOrEmpty(surfaceProp) && surfaceProp.Equals("metal", System.StringComparison.OrdinalIgnoreCase))
                {
                    string tempBaseTexturePath = wrappedData.GetValue("$basetexture");
                    if (!string.IsNullOrEmpty(tempBaseTexturePath))
                        baseTexturePath = tempBaseTexturePath;

                    detailMapPath = wrappedData.GetValue("$detail");
                    //glossiness = 0.5f;
                }
                else
                {
                    baseTexturePath = wrappedData.GetValue("$basetexture");
                    //glossiness = 0;
                }

                glossiness = 0;
                bumpMapPath = wrappedData.GetValue("$bumpmap");
                hasTransparency = !string.IsNullOrEmpty(wrappedData.GetValue("$alphatest"));
            }
        }
    }

    public class VMTDataWrapper
    {
        public const string ENCAPSULATION_PATTERN = "[%\"\\$\\w\\d]+\\s+{"; //[%"\$\w\d]+\s+{
        public const string VALUE_PAIR_PATTERN = "^\\s*[%\"\\$\\w\\d]+\\s+[\\$\\-\\[\\] \\w\\d\"/\\.\\\\]+"; //^\s*[%"\$\w\d]+\s+[\$\-\[\] \w\d"/\.\\]+
        public const string TITLE_PATTERN = @"[%\$\w\d]+"; //[%\$\w\d]+
        public const string VALUE_PATTERN = @"[\$\-\[\] \w\d/\.\\]+"; //[\$\-\[\] \w\d/\.\\]+
        private static Regex encapsulatorRegex = new Regex(ENCAPSULATION_PATTERN, RegexOptions.Multiline);
        private static Regex valuePairsRegex = new Regex(VALUE_PAIR_PATTERN, RegexOptions.Multiline);
        private static Regex titleRegex = new Regex(TITLE_PATTERN, RegexOptions.Multiline);
        private static Regex valueRegex = new Regex(VALUE_PATTERN, RegexOptions.Multiline);

        public string dataTitle;
        public Dictionary<string, string> values;
        public List<VMTDataWrapper> children;

        public bool IsOfType(string typeName)
        {
            return !string.IsNullOrEmpty(dataTitle) && dataTitle.Equals(typeName);
        }
        public void SetValue(string name, string value)
        {
            if (values == null)
                values = new Dictionary<string, string>();
            values[name] = value;
        }
        public string GetValue(string name)
        {
            string returnedValue = null;
            if (values != null && values.ContainsKey(name))
                returnedValue = values[name];
            return returnedValue;
        }
        public string GetValueInChildren(string name)
        {
            string returnedValue = null;
            if (values != null && values.ContainsKey(name))
                returnedValue = values[name];
            else if (children != null)
            {
                foreach(var child in children)
                {
                    returnedValue = child?.GetValueInChildren(name);
                    if (!string.IsNullOrEmpty(returnedValue))
                        break;
                }
            }

            return returnedValue;
        }
        public void AddChild(VMTDataWrapper child)
        {
            if (children == null)
                children = new List<VMTDataWrapper>();
            children.Add(child);
        }
        public static VMTDataWrapper WrapData(string originalVmtString)
        {
            string modifiedVmtString = originalVmtString;
            VMTDataWrapper currentVmtWrapper = new VMTDataWrapper();

            var nextEncapsulator = encapsulatorRegex.Match(modifiedVmtString);
            if (nextEncapsulator.Success)
            {
                currentVmtWrapper.dataTitle = titleRegex.Match(nextEncapsulator.Value).Value.ToLower();
                modifiedVmtString = modifiedVmtString.Substring(nextEncapsulator.Index + nextEncapsulator.Length);
                int endEncapsulationIndex = modifiedVmtString.LastIndexOf("}");
                if (endEncapsulationIndex < 0)
                    endEncapsulationIndex = modifiedVmtString.Length;

                modifiedVmtString = modifiedVmtString.Substring(0, endEncapsulationIndex);

                string nextSubstring = "";
                Match peekEncapsulator = encapsulatorRegex.Match(modifiedVmtString);
                do
                {
                    peekEncapsulator = encapsulatorRegex.Match(modifiedVmtString);
                    if (peekEncapsulator.Success)
                    {
                        endEncapsulationIndex = FindEncapsulationEnd(modifiedVmtString, peekEncapsulator.Index + peekEncapsulator.Length);
                        if (endEncapsulationIndex < 0)
                            endEncapsulationIndex = modifiedVmtString.Length - 1;

                        int length = endEncapsulationIndex - peekEncapsulator.Index + 1;
                        if (length >= 0)
                        {
                            nextSubstring = modifiedVmtString.Substring(peekEncapsulator.Index, length);
                            modifiedVmtString = modifiedVmtString.Substring(0, peekEncapsulator.Index) + modifiedVmtString.Substring(endEncapsulationIndex + 1);
                            currentVmtWrapper.AddChild(WrapData(nextSubstring));
                        }
                        else
                            Debug.LogError("VMTData: Next substring's length was less than zero in..\n\n" + originalVmtString);
                    }
                }
                while (peekEncapsulator.Success);

                var valuePairs = valuePairsRegex.Matches(modifiedVmtString);
                for (int i = 0; i < valuePairs.Count; i++)
                {
                    var titleMatch = titleRegex.Match(valuePairs[i].Value);
                    string valueName = titleMatch.Value.ToLower();
                    int cutIndex = titleMatch.Index + titleMatch.Length + 1;
                    string secondHalfOfPair = "";
                    if (cutIndex < valuePairs[i].Value.Length)
                        secondHalfOfPair = valuePairs[i].Value.Substring(cutIndex).Trim();
                    string value = valueRegex.Match(secondHalfOfPair).Value;
                    currentVmtWrapper.SetValue(valueName, value);
                }
            }

            return currentVmtWrapper;
        }
        public static int FindEncapsulationEnd(string encapsulated, int startIndex, char encapsulationStarter = '{', char encapsulationEnder = '}')
        {
            int enderIndex = -1;
            int openersCount = 0;
            for (int i = startIndex + 1; i < encapsulated.Length; i++)
            {
                if (encapsulated[i] == encapsulationStarter)
                    openersCount++;
                if (encapsulated[i] == encapsulationEnder)
                {
                    if (openersCount == 0)
                    {
                        enderIndex = i;
                        break;
                    }
                    else
                        openersCount--;
                }
            }
            return enderIndex;
        }
        public static int FindInlineEncapsulationEndIndex(string encapsulated, string encapsulationStarter = "{", string encapsulationEnder = "}")
        {
            string modified = encapsulated;
            int foundIndex = -1;
            int amountRemoved = 0;
            int startIndex = modified.IndexOf(encapsulationStarter);
            if (startIndex >= 0)
            {
                amountRemoved += startIndex + 1;
                modified = modified.Substring(startIndex + 1);
                int openersCount = 0;
                while (modified.Contains(encapsulationEnder) && foundIndex < 0)
                {
                    startIndex = modified.IndexOf(encapsulationStarter);
                    int endIndex = modified.IndexOf(encapsulationEnder);
                    if (startIndex >= 0 && startIndex < endIndex)
                    {
                        amountRemoved += startIndex + 1;
                        modified = modified.Substring(startIndex + 1);
                        openersCount++;
                    }
                    else
                    {
                        if (openersCount == 0)
                        {
                            foundIndex = endIndex + amountRemoved;
                        }
                        else
                        {
                            amountRemoved += endIndex + 1;
                            modified = modified.Substring(endIndex + 1);
                            openersCount--;
                        }
                    }
                }
            }
            return foundIndex;
        }
    }
}