using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    public class FileUtils
    {
        public class FileData
        {
            public string Name { get; set; }
            public string Hash { get; set; }
        }

        public static List<FileData> GetFileWithHash(string dirPath)
        {
            var res = new List<FileData>();
            var files = GetFilesInDir(dirPath);

            foreach (var file in files)
            {
                res.Add(new FileData
                {
                    Name = file.Replace(dirPath + Path.DirectorySeparatorChar, ""),
                    Hash = GetSHA256(file)
                });
            }

            return res;
        }

        public static List<string> GetFilesInDir(string targetDirectory)
        {
            List<string> fileEntries = new List<string>();
            fileEntries.AddRange(Directory.GetFiles(targetDirectory));

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                fileEntries.AddRange(GetFilesInDir(subdirectory));
            }

            return fileEntries;
        }

        public static string GetSHA256(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        FileInfo log = new FileInfo(filePath);
                        using (var streamReader = new StreamReader(log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            byte[] hashValue = mySHA256.ComputeHash(streamReader.BaseStream);

                            return ArrayToHex(ref hashValue);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        private static string ArrayToHex(ref byte[] array)
        {
            StringBuilder stringBuilder = new StringBuilder();


            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(($"{array[i]:X2}"));
            }

            return stringBuilder.ToString();
        }
    }

}
