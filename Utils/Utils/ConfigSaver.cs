using System;
using System.IO;
using Utils;

namespace Tajlo4ekUtils
{

    public class ConfigSaver<SaveObj>
    {
        private static string DefaultConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\tajlo4ekGames\";

        public static void SetDefaultPath(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) == false)
            {
                path += Path.DirectorySeparatorChar;
            }
            DefaultConfigPath = path;
        }

        public static bool Save(string name, SaveObj obj)
        {
            try
            {
                if (!Directory.Exists(DefaultConfigPath))
                {
                    Directory.CreateDirectory(DefaultConfigPath);
                }

                var json = JsonUtils<SaveObj>.ToJson(obj, true);

                using (StreamWriter sw = new StreamWriter(DefaultConfigPath + name + ".json"))
                {
                    sw.Write(json);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static bool Load(string name, out SaveObj obj)
        {
            obj = default;

            try
            {
                var fileName = DefaultConfigPath + name + ".json";

                if (File.Exists(fileName) != true) { return false; }


                using (StreamReader sr = new StreamReader(fileName))
                {
                    var json = sr.ReadToEnd();
                    obj = JsonUtils<SaveObj>.FromJson(json);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}
