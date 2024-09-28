using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace LauncherServer
{

    public class ApplicationAvailable
    {
        public static readonly string DirName = @"stor";

        public string Path { get; set; }

        public string Name { get; set; }

        public bool Available { get; set; }
    }

    public class Config
    {
        public static readonly string LauncherName = "LauncherClient";
        public static readonly string ConfigName = @"config";

        public string ServerIp { get; set; } = "127.0.0.1";

        public int ServerPort { get; set; } = 35125;

        public string ProgramPath { get; set; } =
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            + Path.DirectorySeparatorChar + ApplicationAvailable.DirName;

        [JsonIgnore]
        public List<ApplicationAvailable> AvailableProgram { get; set; } = new List<ApplicationAvailable>();
    }

}
