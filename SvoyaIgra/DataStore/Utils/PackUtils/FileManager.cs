using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DataStore.Utils.PackUtils
{
    public class FileManager
    {
        private readonly Dictionary<string, string> files;

        readonly PackManager packManager;

        public string WorkDirectory { get { return packManager.WorkDirectory; } }

        public FileManager()
        {
            packManager = new PackManager();
            files = new Dictionary<string, string>();
        }

        public Package LoadPack(string path)
        {
            return packManager.LoadPack(path);
        }

        public Package LoadPackFromLocal(string localName)
        {
            return LoadPack(WorkDirectory + "/" + localName);
        }

        public void LoadImg(string url, string name)
        {
            var newPathImg = WorkDirectory + "/" + name;

            bool isFind = true;

            try
            {
                if (url.StartsWith("http"))
                {
                    if (FileLoader.TryLoad(url, newPathImg, out string imgPath))
                    {
                        files.Add(name, imgPath);
                    }
                }
                else
                {
                    if (File.Exists(url))
                    {
                        if (!System.Drawing.Imaging.ImageFormat.Gif.Equals(Image.FromFile(url).RawFormat))
                        {
                            File.Copy(url, newPathImg);
                            files.Add(name, newPathImg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isFind = false;
            }

            if (!isFind)
            {
                newPathImg += ".png";
                Properties.Resources.NoImg.Save(newPathImg);
                files.Add(name, newPathImg);
            }
        }

        public void AddFile(string path, string name)
        {
            files[name] = path;
        }

        public void AddLocalFile(string name)
        {
            AddFile(WorkDirectory + "/" + name, name);
        }

        public string GetRealPath(string name)
        {
            return files.TryGetValue(name, out string path) ? path : "";
        }

        public void RenameFile(string oldName, string newName)
        {
            var oldFullName = WorkDirectory + "/" + oldName;
            var newFullName = WorkDirectory + "/" + newName;

            File.Copy(oldFullName, newFullName);
            files.Remove(oldName);
            AddLocalFile(newName);
        }

        public void Dispose()
        {
            packManager.Dispose();
        }

    }
}
