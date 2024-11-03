using ClientServer.FileUtils;
using System;
using System.IO;

namespace ClientServer
{
    //TODO: very slow send file

    internal class SendingFile : BaseFile
    {
        public bool Sended { get; private set; }

        private readonly FileStream stream;
        private readonly FilePart filePart;

        public SendingFile(string path, int maxSize)
            : base(path)
        {
            Sended = false;
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            filePart = new FilePart(maxSize);
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
