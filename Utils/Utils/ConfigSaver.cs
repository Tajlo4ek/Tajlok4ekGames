using System;
using System.IO;

namespace Tajlo4ekUtils
{

    public class ConfigSaver<SaveObj>
    {
        private static readonly string DefaultConfigPath = Environment.CurrentDirectory + "/config/";

        public static bool Save(string name, SaveObj obj)
        {
            return Save(name, DefaultConfigPath, obj);
        }

        public static bool Save(string name, string dir, SaveObj obj)
        {
            try
            {
                if (dir.EndsWith("/") == false)
                {
                    dir += "/";
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonUtils<SaveObj>.ToJson(obj, true);

                using (StreamWriter sw = new StreamWriter(dir + name + ".json"))
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
            return Load(name, DefaultConfigPath, out obj);
        }

        public static bool Load(string name, string dir, out SaveObj obj)
        {
            obj = default;

            try
            {
                if (dir.EndsWith("/") == false)
                {
                    dir += "/";
                }

                var fileName = dir + name + ".json";

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
