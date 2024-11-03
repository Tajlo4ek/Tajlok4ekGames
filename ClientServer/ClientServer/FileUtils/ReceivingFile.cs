using ClientServer.FileUtils;
using System;
using System.IO;

namespace ClientServer
{
    internal class ReceivingFile : BaseFile
    {
        private readonly Action<ProgressFileData> onLoadCallaback;
        private readonly FileStream _stream;
        private readonly long totalSize;
        private long nowSize;

        public bool IsWrited { get { return nowSize == totalSize; } }

        private readonly string path;
        const string tempNameAdd = ".temp";

        public ReceivingFile(string path, string fileName, long totalSize, Action<ProgressFileData> onLoadCallaback, string fileToken)
            : base(fileToken, fileName)
        {
            this.path = path;
            onLoadCallaback?.Invoke(ProgressData);

            var dirName = System.IO.Path.GetDirectoryName(path);
            if (Directory.Exists(dirName) == false)
            {
                Directory.CreateDirectory(dirName);
            }

            _stream = new FileStream(path + tempNameAdd, FileMode.Create, FileAccess.Write);
            this.totalSize = totalSize;
            nowSize = 0;

            this.onLoadCallaback = onLoadCallaback;
        }

        public void AddBytes(byte[] bytes, int size)
        {
            if (nowSize >= totalSize)
            {
                return;
            }

            _stream.Write(bytes, 0, size);
            nowSize += size;

            int buf = (int)(nowSize * 100 / totalSize);
            if (buf > ProgressData.Progress)
            {
                ProgressData.State = ProgressFileData.States.Process;
                ProgressData.Progress = buf;
                onLoadCallaback?.Invoke(ProgressData);
            }

            if (nowSize >= totalSize)
            {
                ProgressData.State = ProgressFileData.States.End;
                _stream.Flush();
                _stream.Close();
                Tajlo4ekUtils.FileUtils.MoveWithReplace(path + tempNameAdd, path);
            }
        }


    }
}
