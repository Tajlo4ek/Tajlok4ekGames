using System;
using System.IO;

namespace ClientServer.FileUtils
{
    internal class BaseFile : IDisposable
    {
        public string Token { get; private set; }

        public ProgressFileData ProgressData { get; private set; }

        protected FileStream stream;

        public BaseFile(string name) : this(TokenGenerator.Generate(), name) { }

        public BaseFile(string token, string fileName)
        {
            Token = token;

            ProgressData = new ProgressFileData()
            {
                Name = fileName,
                State = ProgressFileData.States.Start,
                Progress = 0
            };
        }

        public void Dispose()
        {
            stream?.Close();
            stream = null;
        }
    }
}
