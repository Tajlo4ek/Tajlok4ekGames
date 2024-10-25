using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer.FileUtils
{
    internal class SendRecvController
    {
        private readonly int partSize = 2048;

        private readonly object _lock = new object();
        private readonly Dictionary<string, List<SendingFile>> sendFiles;
        private readonly Dictionary<string, List<ReceivingFile>> recvFiles;
        private readonly Action<ProgressFileData> onLoadCallback;

        public SendRecvController(Action<ProgressFileData> loadCallback = null)
        {
            sendFiles = new Dictionary<string, List<SendingFile>>();
            recvFiles = new Dictionary<string, List<ReceivingFile>>();
            onLoadCallback = loadCallback;
        }

        public string AddSendFile(string connectionToken, string path)
        {
            lock (_lock)
            {
                if (sendFiles.ContainsKey(connectionToken) == false)
                {
                    sendFiles[connectionToken] = new List<SendingFile>();
                }

                var newFile = new SendingFile(path, partSize);
                sendFiles[connectionToken].Add(newFile);
                return newFile.Token;
            }
        }

        public void AddRecvFile(string connectionToken, string path, string fileName, long totalSize, string fileToken)
        {
            lock (_lock)
            {
                if (recvFiles.ContainsKey(connectionToken) == false)
                {
                    recvFiles[connectionToken] = new List<ReceivingFile>();
                }

                var newFile = new ReceivingFile(path, fileName, totalSize, onLoadCallback, fileToken);
                recvFiles[connectionToken].Add(newFile);
            }
        }

        public bool TryGetSendingFile(string connectionToken, string fileToken, out SendingFile file)
        {
            lock (_lock)
            {
                if (sendFiles.ContainsKey(connectionToken) && sendFiles[connectionToken].Count > 0)
                {
                    file = sendFiles[connectionToken].Find((f) => { return f.Token == fileToken; });
                    if (file != default)
                    {
                        return true;
                    }
                }
            }
            file = default;
            return false;
        }

        public bool TryGetReceivingFile(string connectionToken, string fileToken, out ReceivingFile file)
        {
            lock (_lock)
            {
                if (recvFiles.ContainsKey(connectionToken) && recvFiles[connectionToken].Count > 0)
                {
                    file = recvFiles[connectionToken].Find((f) => { return f.Token == fileToken; });
                    if (file != default)
                    {
                        return true;
                    }
                }
            }
            file = default;
            return false;
        }

        public void RemoveSendingFile(string connectionToken, string fileToken)
        {
            lock (_lock)
            {
                if (sendFiles.TryGetValue(connectionToken, out List<SendingFile> val))
                {
                    sendFiles[connectionToken].RemoveAll((file) => { return file.Token == fileToken; });
                }
            }
        }

        public void RemoveRecvingFile(string connectionToken, string fileToken)
        {
            lock (_lock)
            {
                if (recvFiles.TryGetValue(connectionToken, out List<ReceivingFile> val))
                {
                    recvFiles[connectionToken].RemoveAll((file) => { return file.Token == fileToken; });
                }
            }
        }
    }
}
