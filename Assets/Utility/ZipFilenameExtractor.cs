using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public static class ZipFilenameExtractor
{
    public static List<string> GetNames(string zipName)
    {
        List<string> fileNames = new List<string>();
        using (FileStream zipFile = new FileStream(zipName, FileMode.Open))
        {

            int offset = 0;
            byte[] headerByte = new byte[4];

            zipFile.Read(headerByte, 0, headerByte.Length);

            int newByte = 0;

            while(newByte != -1)
            {
                int header = BitConverter.ToInt32(headerByte, 0);

                if (header == 0x04034b50)
                {
                    byte[] nameLengthByte = new byte[2];
                    zipFile.Seek(offset + 26, SeekOrigin.Begin);
                    zipFile.Read(nameLengthByte, 0, 2);
                    int filenameLength = BitConverter.ToInt16(nameLengthByte, 0);
              
                    byte[] filenameByte = new byte[filenameLength];
                    zipFile.Seek(offset + 30, SeekOrigin.Begin);
                    zipFile.Read(filenameByte, 0, filenameLength);
                    AddFilename(fileNames, filenameByte);

                    offset += (30 + filenameLength);
                    zipFile.Seek(offset, SeekOrigin.Begin);
                    zipFile.Read(headerByte, 0, 4);
                }
                else
                {
                    offset++;
                    newByte = zipFile.ReadByte();

                    headerByte[0] = headerByte[1];
                    headerByte[1] = headerByte[2];
                    headerByte[2] = headerByte[3];
                    headerByte[3] = (byte)newByte;
                }

                if (header == 0x06054b50)
                {
                    return fileNames;
                }
            }
        }

        return fileNames;
    }

    static void AddFilename(List<string> filenames, byte[] filenameByte)
    {
        string fullFilename = System.Text.Encoding.ASCII.GetString(filenameByte);

        if(!fullFilename.EndsWith("/"))
        {
            filenames.Add(fullFilename.Substring(fullFilename.LastIndexOf('/') + 1));
        }
    }

}