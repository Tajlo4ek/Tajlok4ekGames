using ClientServer.FileUtils;
using System.IO;

namespace ClientServer
{
    internal class SendingFile : BaseFile
    {
        public bool Sended { get; private set; }

        private readonly FilePart filePart;

        public SendingFile(string path, int readPerOneCall)
            : base(path)
        {
            Sended = false;
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            filePart = new FilePart(readPerOneCall);
        }


        public FilePart GetNextPart()
        {
            if (Sended == true)
            {
                filePart.Available = 0;
                return filePart;
            }

            filePart.Available = stream.Read(filePart.Data, 0, filePart.MaxSize);
            if (filePart.Available == 0)
            {
                Sended = true;
                stream.Close();
            }

            return filePart;
        }
    }
}
