using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;

public class VPKParser : IDisposable
{
    private const ushort DIR_PAK = 0x7fff, NO_PAK = ushort.MaxValue;
    public string directoryLocation { get; private set; }
    public string vpkStartName { get; private set; }
    private VPKHeader header;
    private int headerSize;
    private Dictionary<string, Dictionary<string, Dictionary<string, VPKDirectoryEntry>>> tree = new Dictionary<string, Dictionary<string, Dictionary<string, VPKDirectoryEntry>>>();

    public VPKParser(string _directoryLocation = "", string _vpkPakName = "pak01")
    {
        vpkStartName = _vpkPakName;
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
        string archivePath = GetArchivePath(DIR_PAK);

        if (File.Exists(archivePath))
        {
            header = new VPKHeader();

            using (FileStream currentStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            {
                uint signature = DataParser.ReadUInt(currentStream);
                if (signature != VPKHeader.Signature)
                    return;

                header.Version = DataParser.ReadUInt(currentStream);
                header.TreeSize = DataParser.ReadUInt(currentStream);
                headerSize = 12;

                if (header.Version > 1)
                {
                    header.FileDataSectionSize = DataParser.ReadUInt(currentStream);
                    header.ArchiveMD5SectionSize = DataParser.ReadUInt(currentStream);
                    header.OtherMD5SectionSize = DataParser.ReadUInt(currentStream);
                    header.SignatureSectionSize = DataParser.ReadUInt(currentStream);
                    headerSize += 16;
                }

                ParseTree(currentStream);
            }
        }
    }
    private void ParseTree(Stream currentStream)
    {
        //long currentPosition = currentStream.Position;
        //int bytesRead;

        while (currentStream.Position < header.TreeSize)
        {
            string extension = DataParser.ReadNullTerminatedString(currentStream);
            //currentPosition += bytesRead;
            if (extension.Length <= 0)
                extension = tree.Keys.ElementAt(tree.Count - 1);
            else
            {
                if (!tree.ContainsKey(extension))
                {
                    tree.Add(extension, new Dictionary<string, Dictionary<string, VPKDirectoryEntry>>());
                }
                //currentPosition += extension.Length;
            }

            while (true)
            {
                string directory = DataParser.ReadNullTerminatedString(currentStream);
                //currentPosition += bytesRead;
                if (directory.Length <= 0)
                    break;
                if (!tree[extension].ContainsKey(directory))
                    tree[extension].Add(directory, new Dictionary<string, VPKDirectoryEntry>());
                //currentPosition += directory.Length;

                while (true)
                {
                    string fileName = DataParser.ReadNullTerminatedString(currentStream);
                    //currentPosition += bytesRead;
                    if (fileName.Length <= 0)
                        break;
                    //currentPosition += file.Length;

                    VPKDirectoryEntry dirEntry = new VPKDirectoryEntry();
                    dirEntry.CRC = DataParser.ReadUInt(currentStream);
                    dirEntry.PreloadBytes = DataParser.ReadUShort(currentStream);
                    dirEntry.ArchiveIndex = DataParser.ReadUShort(currentStream);
                    dirEntry.EntryOffset = DataParser.ReadUInt(currentStream);
                    dirEntry.EntryLength = DataParser.ReadUInt(currentStream);
                    ushort terminator = DataParser.ReadUShort(currentStream);
                    //currentPosition += (4 + 2 + 2 + 4 + 4 + 2);

                    long currentStreamPosition = currentStream.Position;
                    if (dirEntry.PreloadBytes > 0)
                    {
                        //UnityEngine.Debug.Log("Found preload data for (" + directory + "/" + fileName + "." + extension + ")");
                        //dirEntry.ArchiveIndex = DIR_PAK;
                        //dirEntry.EntryOffset = (uint)currentStreamPosition;

                        if (extension.ToLower().Equals("vmt"))
                        {
                            //byte[] vmtData = new byte[dirEntry.PreloadBytes];
                            //currentStream.Read(vmtData, 0, dirEntry.PreloadBytes);
                            //UnityEngine.Debug.Log("Preloading " + directory + "/" + fileName + "." + extension);
                            VMTData.ReadAndCache(currentStream, currentStream.Position + dirEntry.PreloadBytes, directory + "/" + fileName + "." + extension);
                        }
                    }
                    //dirEntry.PreloadData = new byte[dirEntry.PreloadBytes];
                    //currentStream.Read(dirEntry.PreloadData, 0, dirEntry.PreloadBytes);
                    //for (int i = 0; i < dirEntry.PreloadData.Length; i++)
                    //{
                    //    dirEntry.PreloadData[i] = DataParser.ReadByte(currentStream);
                    //}
                    currentStream.Position = currentStreamPosition + dirEntry.PreloadBytes;
                    //currentPosition += dirEntry.PreloadBytes;

                    if (!tree[extension][directory].ContainsKey(fileName))
                        tree[extension][directory].Add(fileName, dirEntry);
                }
            }
        }
    }
    
    public byte[] LoadFile(string path)
    {
        string fixedPath = path.Replace("\\", "/");
        string extension = fixedPath.Substring(fixedPath.LastIndexOf(".") + 1);
        string directory = fixedPath.Substring(0, fixedPath.LastIndexOf("/"));
        string fileName = fixedPath.Substring(fixedPath.LastIndexOf("/") + 1);
        fileName = fileName.Substring(0, fileName.LastIndexOf("."));

        return LoadFile(extension, directory, fileName);
    }
    public byte[] LoadFile(string extension, string directory, string fileName)
    {
        CheckHeader();

        byte[] file = null;

        string extFixed = extension;
        string dirFixed = directory.Replace("\\", "/");
        string fileNameFixed = fileName;

        if (extFixed.IndexOf(".") == 0)
            extFixed = extFixed.Substring(1);
        if (dirFixed.IndexOf("/") == 0)
            dirFixed = dirFixed.Substring(1);
        if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
            dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

        VPKDirectoryEntry entry;
        if (GetEntry(extFixed, dirFixed, fileNameFixed, out entry))
        {
            if (entry.EntryLength <= 0)
                return entry.PreloadData;

            string archivePath = GetArchivePath(entry.ArchiveIndex);
            using (var currentStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            {
                #region Set Position in Stream
                if (entry.ArchiveIndex == DIR_PAK)
                    currentStream.Position = headerSize + header.TreeSize;
                else currentStream.Position = 0;
                    currentStream.Position += entry.EntryOffset;
                #endregion

                #region Read File Bytes
                file = new byte[(int)entry.EntryLength];
                currentStream.Read(file, 0, file.Length);
                //file = DataParser.ReadBytes(currentStream, (int)entry.EntryLength);
                #endregion
            }
        }

        return file;
    }
    public void LoadFileAsStream(string path, Action<Stream, int, int> streamActions)
    {
        string fixedPath = path.Replace("\\", "/");
        string extension = fixedPath.Substring(fixedPath.LastIndexOf(".") + 1);
        string directory = fixedPath.Substring(0, fixedPath.LastIndexOf("/"));
        string fileName = fixedPath.Substring(fixedPath.LastIndexOf("/") + 1);
        fileName = fileName.Substring(0, fileName.LastIndexOf("."));

        LoadFileAsStream(extension, directory, fileName, streamActions);
    }

    public void LoadFileAsStream(string extension, string directory, string fileName, Action<Stream, int, int> streamActions)
    {
        CheckHeader();

        string extFixed = extension;
        string dirFixed = directory.Replace("\\", "/");
        string fileNameFixed = fileName;

        if (extFixed.IndexOf(".") == 0)
            extFixed = extFixed.Substring(1);
        if (dirFixed.IndexOf("/") == 0)
            dirFixed = dirFixed.Substring(1);
        if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
            dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

        VPKDirectoryEntry entry;
        if (GetEntry(extFixed, dirFixed, fileNameFixed, out entry))
        {
            #region Get Position in Stream
            int fileOffset = 0;
            if (entry.ArchiveIndex == DIR_PAK)
                fileOffset = (int)(headerSize + header.TreeSize);

            if (entry.EntryLength <= 0)
                fileOffset = (int)entry.EntryOffset;
            else
                fileOffset += (int)entry.EntryOffset;
            #endregion

            string archivePath = GetArchivePath(entry.ArchiveIndex);
            using (FileStream currentStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            {
                currentStream.Position = fileOffset;
                streamActions(currentStream, fileOffset, (int)entry.EntryLength);
            }
        }
    }

    private string GetArchivePath(ushort archiveIndex)
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
        vpkPakDir += ".vpk";

        return Path.Combine(directoryLocation, vpkStartName + vpkPakDir);
    }
    private bool GetEntry(string ext, string dir, string fileName, out VPKDirectoryEntry entry)
    {
        CheckHeader();

        if (tree != null && tree.ContainsKey(ext) && tree[ext].ContainsKey(dir) && tree[ext][dir].ContainsKey(fileName))
        {
            entry = tree[ext][dir][fileName];
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
        string fixedPath = path.Replace("\\", "/");
        string extension = fixedPath.Substring(fixedPath.LastIndexOf(".") + 1);
        string directory = fixedPath.Substring(0, fixedPath.LastIndexOf("/"));
        string fileName = fixedPath.Substring(fixedPath.LastIndexOf("/") + 1);
        fileName = fileName.Substring(0, fileName.LastIndexOf("."));

        return FileExists(extension, directory, fileName);
    }
    public bool FileExists(string extension, string directory, string fileName)
    {
        CheckHeader();

        string extFixed = extension;
        string dirFixed = directory.Replace("\\", "/");
        string fileNameFixed = fileName;

        if (extFixed.IndexOf(".") == 0)
            extFixed = extFixed.Substring(1);

        if (dirFixed.IndexOf("/") == 0)
            dirFixed = dirFixed.Substring(1);
        if (dirFixed.LastIndexOf("/") == dirFixed.Length - 1)
            dirFixed = dirFixed.Substring(0, dirFixed.Length - 1);

        if (tree != null && tree.ContainsKey(extFixed) && tree[extFixed].ContainsKey(dirFixed) && tree[extFixed][dirFixed].ContainsKey(fileNameFixed))
        {
            return true;
        }

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
