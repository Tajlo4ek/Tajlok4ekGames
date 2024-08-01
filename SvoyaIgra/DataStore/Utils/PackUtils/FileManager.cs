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

        public void LoadImg(string path, string name)
        {
            var newPathImg = WorkDirectory + @"\" + name;

            bool isFind = false;

            try
            {
                if (path.StartsWith("http"))
                {
                    if (FileLoader.TryLoad(path, newPathImg, out string imgPath))
                    {
                        //if (!System.Drawing.Imaging.ImageFormat.Gif.Equals(Image.FromFile(imgPath).RawFormat))
                        {
                            files.Add(name, imgPath);
                            isFind = true;
                        }
                    }
                }
                else
                {
                    newPathImg += path.Substring(path.LastIndexOf("."));
                    if (File.Exists(path))
                    {
                        if (!System.Drawing.Imaging.ImageFormat.Gif.Equals(Image.FromFile(path).RawFormat))
                        {
                            File.Copy(path, newPathImg);
                            files.Add(name, newPathImg);
                            isFind = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            files.Add(name, path);
        }

        public string GetFilePath(string name)
        {
            return files.TryGetValue(name, out string path) ? path : "";
        }

        public void RenameFile(string oldName, string newName)
        {
            if (files.TryGetValue(oldName, out string path))
            {
                var ind = path.LastIndexOf(@"\");
                var buf = path.Substring(0, ind);
                string newPath = buf + @"\" + newName;
                File.Copy(GetFilePath(oldName), newPath);
                files.Remove(oldName);
                AddFile(newPath, newName);
            }
        }

        public void Dispose()
        {
            packManager.Dispose();
        }

    }
}
