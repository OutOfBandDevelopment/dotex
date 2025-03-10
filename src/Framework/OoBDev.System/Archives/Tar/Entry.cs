﻿using System;
using System.Text;
using System.IO;

namespace OoBDev.System.Archives.Tar;

internal class Entry
{
    public static void Sample(string[] args)
    {
        var file = @"XXX.tar.gz";

        if (string.IsNullOrEmpty(file))
        {
            Console.WriteLine("provide files name to decompress");
            return;
        }
        if (!File.Exists(file))
        {
            Console.WriteLine("file \"{0}\" not found", file);
            return;
        }
        var basePath = Path.GetDirectoryName(file);
        var volumePath = basePath;

        Stream? infile = File.OpenRead(file);
        try
        {
            var ext = Path.GetExtension(file).ToUpper();
            if (ext == ".GZ" || ext == ".TGZ")
                infile = infile.Decompress();

            byte[] buffer = new byte[512];
            TarHeader? header = null;
            bool getHeader = true;
            Stream? newFile = null;
            int lengthWrote = 0;
            string? longName = null;
            bool writingFile = false;

            int getLength = 1;

            while (getLength > 0)
            {
                getLength = infile.Read(buffer, 0, 512);

                if (getHeader)
                {
                    header = buffer.ToHeader();

                    if (!string.IsNullOrEmpty(header.FileName))
                    {
                        Console.WriteLine("{0} \"{1}\" ({2})",
                                          header.FileType,
                                          header.FileName,
                                          header.FileSize);
                        getHeader = false;
                    }
                    else
                        continue;
                }

                switch (header?.FileType)
                {
                    case TarFileType.File:
                    case TarFileType.FileOld:
                    case TarFileType.ContiguousFile:
                    case TarFileType.SparseFile:
                    case TarFileType.LongName:
                        {
                            if (header.FileSize == 0)
                                getHeader = true;
                            else
                            {
                                if (!writingFile)
                                {
                                    lengthWrote = 0;
                                    writingFile = true;

                                    var innerFile = Path.Combine(volumePath ?? throw new NotSupportedException($"{nameof(volumePath)} must be set"), longName ?? header.FileName);
                                    if (!File.Exists(innerFile) || new FileInfo(innerFile).Length != header.FileSize)
                                    {
                                        if (header.FileType == TarFileType.LongName)
                                            newFile = new MemoryStream();
                                        else
                                            newFile = IOUtilities.OpenFileStream(innerFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                                    }
                                }
                                else
                                {
                                    if (header.FileType != TarFileType.LongName)
                                        longName = null;
                                    newFile?.Write(buffer,
                                                      0,
                                                      Math.Min(buffer.Length,
                                                               header.FileSize - lengthWrote));
                                    lengthWrote += 512;

                                    if (lengthWrote >= header.FileSize && writingFile)
                                    {
                                        if (newFile != null)
                                        {
                                            if (newFile is MemoryStream ms)
                                                longName = string.Format("{0}{1}", longName, Encoding.ASCII.GetString(ms.ToArray()).TrimEnd('\0'));
                                            newFile.Flush();
                                            newFile.Close();
                                        }
                                        writingFile = false;
                                        newFile = null;
                                        getHeader = true;
                                        lengthWrote = 0;
                                    }
                                }
                            }

                            break;
                        }

                    default:
                    case TarFileType.HardLink:
                    case TarFileType.SymbolicLink:
                    case TarFileType.LongSymbolicLink:
                    case TarFileType.CharacterDevice:
                    case TarFileType.BlockDevice:
                    case TarFileType.NamedPipe:
                        Console.WriteLine("Windows doesn't really care about these: {0}", header?.FileType);
                        break;

                    case TarFileType.Volume:
                        var cleanName = header.FileName.Replace(':', '\\');
                        volumePath = Path.Combine(basePath ?? throw new NotSupportedException($"{nameof(basePath)} must be set"), cleanName);
                        break;

                    case TarFileType.Directory:
                        {
                            var tarPath = Path.Combine(volumePath ?? throw new NotSupportedException($"{nameof(volumePath)} must be set"), longName ?? header.FileName);
                            longName = null;
                            if (!Directory.Exists(tarPath))
                                Directory.CreateDirectory(tarPath);
                            getHeader = true;
                            break;
                        }
                }
            }
            Console.WriteLine("Decompress Complete");
        }
        finally
        {
            infile?.Dispose();
        }
    }
}
