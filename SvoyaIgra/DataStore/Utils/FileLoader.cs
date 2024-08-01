using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace DataStore.Utils
{
    public static class FileLoader
    {
        public static bool TryLoad(string url, string path, out string fileLoadTo, int timeout = 10000)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = timeout;
                request.ReadWriteTimeout = timeout;
                var wresp = (HttpWebResponse)request.GetResponse();

                using (Stream file = File.OpenWrite(path))
                {
                    wresp.GetResponseStream().CopyTo(file);
                }

                string pathExtension = TryGetFileExtension(path);
                if (!pathExtension.Equals(""))
                {
                    fileLoadTo = path + pathExtension;
                    File.Copy(path, path + pathExtension);
                }
                else
                {
                    fileLoadTo = path;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                fileLoadTo = "";
                return false;
            }
        }

        private static string TryGetFileExtension(string filePath)
        {
            var image = Image.FromFile(filePath);
            var imageFormat = image.RawFormat;
            image.Dispose();

            if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
            {
                return ".gif";
            }
            else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
            {
                return ".png";
            }
            else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
            {
                return ".bmp";
            }
            else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
            {
                return ".jpg";
            }
            else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
            {
                return ".ico";
            }

            return "";
        }

    }
}
