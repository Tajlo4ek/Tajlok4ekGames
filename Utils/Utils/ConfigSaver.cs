using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tajlo4ekUtils
{

    public  class ConfigSaver<SaveObj>
    {
        private static readonly string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\tajlo4ekGames\";


        public static bool Save(string name, SaveObj obj)
        {
            try
            {
                if (!Directory.Exists(ConfigPath))
                {
                    Directory.CreateDirectory(ConfigPath);
                }

                var json = JsonSerializer.Serialize(obj, typeof(SaveObj));

                using (StreamWriter sw = new StreamWriter(ConfigPath + name + ".json"))
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
                var fileName = ConfigPath + name + ".json";

                if (File.Exists(fileName) != true) { return false; }


                using (StreamReader sr = new StreamReader(fileName))
                {
                    var json = sr.ReadToEnd();
                    obj = JsonSerializer.Deserialize<SaveObj>(json);
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
