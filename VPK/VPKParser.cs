using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;

namespace UnitySourceEngine
{
    public class VPKParser : IDisposable
    {
        private const ushort DIR_PAK = 0x7fff, NO_PAK = ushort.MaxValue;
        public string directoryLocation { get; private set; }
        public string vpkStartName { get; private set; }
        private VPKHeader header;
        private int headerSize;
        private Dictionary<string, Dictionary<string, Dictionary<string, VPKDirectoryEntry>>> tree = new Dictionary<string, Dictionary<string, Dictionary<string, VPKDirectoryEntry>>>();

        private List<ushort> archivesNotFound = new List<ushort>();

        private Stream preloadStream;
        private VPKStream[] openStreams = new VPKStream[7];
        private int nextStreamIndex;

        public VPKParser(string _directoryLocation) : this(_directoryLocation, "pak01") { }
        public VPKParser(string _directoryLocation, string _vpkPakPrefix)
        {
            vpkStartName = _vpkPakPrefix;
            directoryLocation = _directoryLocation;
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~VPKParser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                preloadStream?.Dispose();
                foreach (var streamWrapper in openStreams)
                    streamWrapper.stream?.Dispose();

                archivesNotFound.Clear();
                archivesNotFound = null;

                header = null;
                if (tree != null)
                {
                    foreach (var extPair in tree)
                        if (extPair.Value != null)
                        {
                            foreach (var dirPair in extPair.Value)
                                if (dirPair.Value != null)
                                {
                                    foreach (var entryPair in dirPair.Value)
                                        entryPair.Value.Dispose();
                                    dirPair.Value.Clear();
                                }
                            extPair.Value.Clear();
                        }
                    tree.Clear();
                }
                tree = null;
            }
        }

        public bool IsValid()
        {
            CheckHeader();
            return header != null && header.TreeSize > 0;
        }
        private void CheckHeader()
        {
            if (header == null)
                ParseHeader();
        }
        private void ParseHeader()
        {
            string archivePath = Path.Combine(directoryLocation, GetArchiveName(DIR_PAK) + ".vpk");

            if (File.Exists(archivePath))
            {
                header = new VPKHeader();

                preloadStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);

                uint signature = DataParser.ReadUInt(preloadStream);
                if (signature != VPKHeader.Signature)
                    return;

                header.Version = DataParser.ReadUInt(preloadStream);
                header.TreeSize = DataParser.ReadUInt(preloadStream);
                headerSize = 12;

                if (header.Version > 1)
                {
                    header.FileDataSectionSize = DataParser.ReadUInt(preloadStream);
                    header.ArchiveMD5SectionSize = DataParser.ReadUInt(preloadStream);
                    header.OtherMD5SectionSize = DataParser.ReadUInt(preloadStream);
                    header.SignatureSectionSize = DataParser.ReadUInt(preloadStream);
                    headerSize += 16;
                }

                ParseTree(preloadStream);
            }
        }
        private void ParseTree(Stream currentStream)
        {
            while (currentStream.Position < header.TreeSize)
            {
                string extension = DataParser.ReadNullTerminatedString(currentStream).ToLower();
                if (extension.Length <= 0)
                    extension = tree.Keys.ElementAt(tree.Count - 1);
                else
                {
                    if (!tree.ContainsKey(extension))
                    {
                        tree.Add(extension, new Dictionary<string, Dictionary<string, VPKDirectoryEntry>>());
                    }
                }

                while (true)
                {
                    string directory = DataParser.ReadNullTerminatedString(currentStream).ToLower();
                    if (directory.Length <= 0)
                        break;
                    if (!tree[extension].ContainsKey(directory))
                        tree[extension].Add(directory, new Dictionary<string, VPKDirectoryEntry>());

                    string fileName;
                    do
                    {
                        fileName = DataParser.ReadNullTerminatedString(currentStream).ToLower();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            VPKDirectoryEntry dirEntry = new VPKDirectoryEntry();
                            dirEntry.CRC = DataParser.ReadUInt(currentStream);
                            dirEntry.PreloadBytes = DataParser.ReadUShort(currentStream);
                            dirEntry.ArchiveIndex = DataParser.ReadUShort(currentStream);
                            dirEntry.EntryOffset = DataParser.ReadUInt(currentStream);
                            dirEntry.EntryLength = DataParser.ReadUInt(currentStream);
                            ushort terminator = DataParser.ReadUShort(currentStream);

                            if (dirEntry.EntryOffset == 0 && dirEntry.ArchiveIndex == DIR_PAK)
                                dirEntry.EntryOffset = Convert.ToUInt32(currentStream.Position);
                            if (dirEntry.EntryLength == 0)
                                dirEntry.EntryLength = dirEntry.PreloadBytes;

                            currentStream.Position += dirEntry.PreloadBytes;

                            if (!tree[extension][directory].ContainsKey(fileName))
                                tree[extension][directory].Add(fileName, dirEntry);
                        }
                    }
                    while (!string.IsNullOrEmpty(fileName));
                }
            }
        }

        public string LocateInArchive(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension.Length > 0)
                extension = extension.Substring(1);
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            return LocateInArchive(extension, directory, fileName);
        }
        public string LocateInArchive(string extension, string directory, string fileName)
        {
            string archiveName = null;

            string extFixed = extension.ToLower();
            string dirFixed = directory.Replace("\\", "/").ToLower();
            string fileNameFixed = fileName.ToLower();

            if (extFixed.IndexOf(".") == 0)
                extFixed = extFixed.Substring(1);
            if (dirFixed.IndexOf("/") == 0)
                dirFixed = dirFixed.Substring(1);
            if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
                dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

            VPKDirectoryEntry entry;
            if (GetEntry(extFixed, dirFixed, fileNameFixed, out entry))
            {
                archiveName = GetArchiveName(entry.ArchiveIndex);
            }

            return archiveName;
        }
        public void LoadFileAsStream(string path, Action<Stream, uint, uint> streamActions)
        {
            string fixedPath = path.Replace("\\", "/");
            string extension = fixedPath.Substring(fixedPath.LastIndexOf(".") + 1);
            string directory = fixedPath.Substring(0, fixedPath.LastIndexOf("/"));
            string fileName = fixedPath.Substring(fixedPath.LastIndexOf("/") + 1);
            fileName = fileName.Substring(0, fileName.LastIndexOf("."));

            LoadFileAsStream(extension, directory, fileName, streamActions);
        }
        public void LoadFileAsStream(string extension, string directory, string fileName, Action<Stream, uint, uint> streamActions)
        {
            CheckHeader();

            string extFixed = extension.ToLower();
            string dirFixed = directory.Replace("\\", "/").ToLower();
            string fileNameFixed = fileName.ToLower();

            if (extFixed.IndexOf(".") == 0)
                extFixed = extFixed.Substring(1);
            if (dirFixed.IndexOf("/") == 0)
                dirFixed = dirFixed.Substring(1);
            if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
                dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

            VPKDirectoryEntry entry;
            if (GetEntry(extFixed, dirFixed, fileNameFixed, out entry))
            {
                Stream currentStream = GetStream(entry.ArchiveIndex);
                if (currentStream != null)
                    streamActions(currentStream, entry.EntryOffset, entry.EntryLength);
            }
            else
                UnityEngine.Debug.LogError("VPKParser: Could not find entry " + dirFixed + "/" + fileNameFixed + "." + extFixed);
        }

        private Stream GetStream(ushort archiveIndex)
        {
            Stream currentStream = null;

            if (archiveIndex == DIR_PAK)
            {
                currentStream = preloadStream;
            }
            else
            {
                for (int i = 0; i < openStreams.Length; i++)
                {
                    if (openStreams[i].pakIndex == archiveIndex)
                    {
                        currentStream = openStreams[i].stream;
                        break;
                    }
                }
            }

            if (currentStream == null)
            {
                string archiveName = GetArchiveName(archiveIndex);
                string archivePath = Path.Combine(directoryLocation, archiveName + ".vpk");
                bool archiveExists = File.Exists(archivePath);
                if (archiveExists)
                {
                    openStreams[nextStreamIndex].stream?.Dispose();

                    currentStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
                    openStreams[nextStreamIndex].stream = currentStream;
                    openStreams[nextStreamIndex].pakIndex = archiveIndex;

                    nextStreamIndex = (nextStreamIndex + 1) % openStreams.Length;
                }
                else if (!archivesNotFound.Contains(archiveIndex))
                {
                    archivesNotFound.Add(archiveIndex);
                    UnityEngine.Debug.LogError("VPKParser: Could not find archive " + archiveName + ", full path = '" + archivePath + "'");
                }
            }

            return currentStream;
        }

        private string GetArchiveName(ushort archiveIndex)
        {
            string vpkPakDir = "_";
            if (archiveIndex == DIR_PAK)
            {
                vpkPakDir += "dir";
            }
            else if (archiveIndex < 1000)
            {
                if (archiveIndex >= 0 && archiveIndex < 10)
                    vpkPakDir += "00" + archiveIndex;
                else if (archiveIndex >= 10 && archiveIndex < 100)
                    vpkPakDir += "0" + archiveIndex;
                else
                    vpkPakDir += archiveIndex;
            }

            return vpkStartName + vpkPakDir;
        }
        private bool GetEntry(string ext, string dir, string fileName, out VPKDirectoryEntry entry)
        {
            CheckHeader();

            string extFixed = ext.ToLower();
            string dirFixed = dir.ToLower();
            string fileNameFixed = fileName.ToLower();

            if (tree != null && tree.ContainsKey(extFixed) && tree[extFixed].ContainsKey(dirFixed) && tree[extFixed][dirFixed].ContainsKey(fileNameFixed))
            {
                entry = tree[extFixed][dirFixed][fileNameFixed];
                return true;
            }
            else
            {
                entry = new VPKDirectoryEntry();
                return false;
            }
        }
        public bool FileExists(string path)
        {
            string fixedPath = path.Replace("\\", "/").ToLower();
            string extension = Path.GetExtension(fixedPath);
            string directory = Path.GetDirectoryName(fixedPath);
            string fileName = Path.GetFileNameWithoutExtension(fixedPath);

            return FileExists(extension, directory, fileName);
        }
        public bool FileExists(string extension, string directory, string fileName)
        {
            CheckHeader();

            string extFixed = extension.ToLower();
            string dirFixed = directory.Replace("\\", "/").ToLower();
            string fileNameFixed = fileName.ToLower();

            if (extFixed.IndexOf(".") == 0)
                extFixed = extFixed.Substring(1);

            if (dirFixed.IndexOf("/") == 0)
                dirFixed = dirFixed.Substring(1);
            if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
                dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

            if (tree != null && tree.ContainsKey(extFixed) && tree[extFixed].ContainsKey(dirFixed) && tree[extFixed][dirFixed].ContainsKey(fileNameFixed))
                return true;
            else
                return false;
        }
        public bool DirectoryExists(string directory)
        {
            CheckHeader();

            string dirFixed = directory.Replace("\\", "/");

            if (dirFixed.IndexOf("/") == 0)
                dirFixed = dirFixed.Substring(1);
            if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
                dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

            if (tree != null)
            {
                for (int i = 0; i < tree.Count; i++)
                {
                    if (tree.ContainsKey(tree.Keys.ElementAt(i)) && tree[tree.Keys.ElementAt(i)].ContainsKey(dirFixed))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public struct VPKStream
    {
        public ushort pakIndex;
        public Stream stream;
    }
}