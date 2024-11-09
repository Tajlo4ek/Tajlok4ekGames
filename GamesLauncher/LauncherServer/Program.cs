using System.Threading.Tasks;

namespace LauncherServer
{
    internal class Program
    {
        static async Task Main()
        {
            new Controller();

            while (true)
            {
                await Task.Delay(1000);
            }

        }
    }
}
