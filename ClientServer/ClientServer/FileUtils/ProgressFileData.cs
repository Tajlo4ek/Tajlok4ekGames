
namespace ClientServer.FileUtils
{
    public class ProgressFileData
    {
        public enum States
        {
            Start,
            Process,
            End,
            Error
        }

        public string Name { get; set; }

        public int Progress { get; set; }

        public States State { get; set; }
    }
}
