using DataStore.Exceptions;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Xml;

namespace DataStore.Utils.PackUtils
{
    public class PackManager : IDisposable
    {
        public const string MyPackExtension = ".tjk";
        public const string SiqPackExtension = ".siq";
        public const string BasePackName = "pack" + MyPackExtension;

        public readonly string WorkDirectory;
        private bool isDisposed;

        private static string GetUniqFolder()
        {
            var folder = FindDirectory();
            while (Directory.Exists(folder))
            {
                folder = FindDirectory();
            }
            return folder;
        }

        private static string FindDirectory()
        {
            return Path.GetTempPath() + @"tjkGame\Content\" + Guid.NewGuid().ToString();
        }

        public PackManager()
        {
            WorkDirectory = GetUniqFolder();

            PackUtils.PackConverter.RecreateDirectory(WorkDirectory);
            isDisposed = false;
        }

        public Package LoadPack(string path, Action<string> process = null)
        {
            try
            {
                if (path.EndsWith(SiqPackExtension))
                {
                    return LoadSiq(path, process);
                }
                else if (path.EndsWith(MyPackExtension))
                {
                    return LoadMyPack(path, process);
                }
                else
                {
                    throw new BadFileException("unknow extension: " + path);
                }
            }
            catch (Exception ex)
            {
                throw new BadFileException(ex.Message);
            }
        }

        private Package LoadSiq(string path, Action<string> process = null)
        {
            process?.Invoke("start convert");
            PackConverter.Convert(path, this, BasePackName, process);
            process?.Invoke("converted");
            return LoadMyPack(WorkDirectory + @"\" + BasePackName, process);
        }

        private Package LoadMyPack(string path, Action<string> process = null)
        {
            process?.Invoke("start load");

            if (!path.StartsWith(WorkDirectory))
            {
                File.Copy(path, WorkDirectory + @"\" + BasePackName);
            }

            process?.Invoke("start unzip");

            ZipFile.ExtractToDirectory(path, WorkDirectory);

            process?.Invoke("unzip end");

            using (FileStream fs = new FileStream(WorkDirectory + @"\content.xml", FileMode.Open))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                DataContractSerializer ser = new DataContractSerializer(typeof(Package));

                process?.Invoke("start parse");

                var pack = (Package)ser.ReadObject(reader);

                process?.Invoke("parsed. finishing...");

                pack.Update(WorkDirectory);

                return pack;
            }

        }

        public string AddToPackFolder(string contentPath)
        {
            var isLocal = contentPath.StartsWith("@");

            long id = Utils.IdUtil.GetId();
            var newName = id.ToString();
            var newPath = WorkDirectory + @"\" + newName;

            if (isLocal)
            {
                try
                {
                    var oldName = contentPath.Substring(1);
                    var ind = oldName.LastIndexOf(".");
                    if (ind != -1)
                    {
                        newPath += oldName.Substring(ind);
                        newName += oldName.Substring(ind);
                    }

                    File.Copy(oldName, newPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                Console.WriteLine("try load: " + contentPath);
                Utils.FileLoader.TryLoad(contentPath, newPath, out string path);

                var ind = path.LastIndexOf(@"\");
                if (ind != 0)
                {
                    newName = path.Substring(ind + 1);
                }
            }
            return newName;
        }

        public void Dispose()
        {
            try
            {
                PackConverter.DeleteDirectory(WorkDirectory);
                isDisposed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        ~PackManager()
        {
            if (!isDisposed)
            {
                Dispose();
            }
        }

    }

}
