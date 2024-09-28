using System.IO;
using Tajlo4ekUtils;

namespace LauncherServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ConfigSaver<Config>.SetDefaultPath(path);

            new Controller();
        }
    }
}
