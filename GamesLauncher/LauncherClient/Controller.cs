using ClientServer;
using ClientServer.fileSend;
using LauncherServer;
using LauncherUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Tajlo4ekUtils;
using Utils;
using static LauncherUtils.Messages;

namespace LauncherClient
{
    internal class Controller
    {
        private Config config;

        private readonly ClientServer.Client<MessageType> client;
        private readonly string launcherDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private Connection connection;

        public Action<string, string> AddNewApplication;
        public Action<string> OnAppUpdated;
        public Action<int> SendCountNeedLoad;
        public Action<Exception> OnError;

        public Action<ProgressFileData> OnFileProcess
        {
            get { return client.OnFileLoadProgress; }
            set { client.OnFileLoadProgress += value; }
        }
        public Action<string> ShowMessage;

        private readonly Dictionary<string, HashSet<string>> needDownloadFiles;

        public Controller()
        {
            Load();
            Save();

            needDownloadFiles = new Dictionary<string, HashSet<string>>();

            client = new ClientServer.Client<MessageType>(config.ServerIp, "", OnServerError, config.ServerPort);
            client.OnGetMessage += OnGetMessage;
            client.GetFilePath += GetFilePath;

            client.SetWorkPath(config.ProgramPath);
            client.Start();

            OnFileProcess += FileLoadCallback;

            try
            {
                File.Delete(launcherDir + "/" + Config.LauncherName + ".exe.back");
            }
            catch (Exception)
            {
            }
        }

        private void OnServerError(Exception ex)
        {
            client.Stop();
            ShowMessage?.Invoke("Ошибка связи с сервером");
        }

        private void OnGetMessageUser(ClientServer.Message<MessageType> message)
        {
            switch (message.Command)
            {
                case MessageType.SendFilesApplication:
                    {
                        var appName = message.GetData<string>("app");
                        var files = message.GetData<List<FileUtils.FileData>>("files");

                        if (appName.Length == 0 || files.Count == 0)
                        {
                            break;
                        }

                        LoadApplication(appName, files);
                    }
                    break;

                case MessageType.SendInfo:
                    {
                        var apps = message.GetData<List<ApplicationAvailable>>("info");

                        if (apps.Count == 0) { break; }

                        foreach (var app in apps)
                        {
                            if (app.Path != Config.LauncherName)
                            {
                                AddNewApplication?.Invoke(app.Name, app.Path);
                                CheckUpdate(app.Path);
                            }
                        }
                    }
                    break;

                case MessageType.AppUpdated:
                    {
                        var app = message.GetData<string>("app");

                        if (app.Length != 0)
                        {
                            CheckUpdate(app);
                        }
                    }
                    break;
            }
        }

        private void OnGetMessage(ClientServer.Message<MessageType> message)
        {
            switch (message.MessageType)
            {
                case ClientServer.Message<MessageType>.GeneralMessageType.User:
                    {
                        OnGetMessageUser(message);
                    }
                    break;

                case ClientServer.Message<MessageType>.GeneralMessageType.SendReg:
                    {
                        var token = message.GetData<string>("token");
                        connection = new Connection(token, message.TokenFrom);

                        AddMessageForServer(ClientServer.Message<MessageType>.GeneralMessageType.User,
                           MessageType.GetInfo);

                        CheckUpdate(Config.LauncherName);
                    }
                    break;
            }
        }

        private string GetFilePath(string name)
        {
            return config.ProgramPath + "/" + name;
        }

        private void FileLoadCallback(ProgressFileData fileData)
        {
            if (fileData.State == ProgressFileData.States.End)
            {
                foreach (var key in needDownloadFiles.Keys)
                {
                    needDownloadFiles[key].Remove(fileData.Name);
                    if (needDownloadFiles[key].Count == 0)
                    {
                        CheckUpdate(key);
                    }
                }
            }
        }

        private void AddMessageForServer(ClientServer.Message<MessageType>.GeneralMessageType type,
            MessageType command,
            Dictionary<string, object> data = default)
        {
            if (connection != default)
            {
                var message = new Message<MessageType>(connection.MyToken, connection.RemoteToken, type)
                    .SetCommand(command);

                if (data != default)
                {
                    foreach (var d in data)
                    {
                        message.Add(d.Key, d.Value);
                    }
                }
                client.SendMessage(message);
            }
        }

        public void CheckUpdate(string appName)
        {
            AddMessageForServer(
                ClientServer.Message<MessageType>.GeneralMessageType.User,
                MessageType.GetFilesApplication,
                new Dictionary<string, object> { { "app", appName } });
        }

        private void LoadApplication(string appName, List<FileUtils.FileData> files)
        {
            if (Directory.Exists(config.ProgramPath) == false)
            {
                Directory.CreateDirectory(config.ProgramPath);
            }

            int countNeedLoad = 0;
            files.Remove(files.Find((file) => { return file.Name == Config.ConfigName + ".json"; }));

            foreach (var file in files)
            {
                var fileName = appName + "/" + file.Name;
                var fullFilePath = config.ProgramPath + "/" + fileName;

                if (FileUtils.GetSHA256(fullFilePath) != file.Hash)
                {
                    if (needDownloadFiles.ContainsKey(appName) == false)
                    {
                        needDownloadFiles[appName] = new HashSet<string>();
                    }

                    needDownloadFiles[appName].Add(fileName);

                    countNeedLoad++;
                    AddMessageForServer(
                        ClientServer.Message<MessageType>.GeneralMessageType.GetFile,
                        MessageType.None,
                        new Dictionary<string, object> { { "fileName", fileName } });
                }
            }

            if (countNeedLoad == 0)
            {
                if (appName == Config.LauncherName)
                {
                    CheckNeedUpdateLauncher();
                }
                OnAppUpdated(appName);
            }
            else
            {
                SendCountNeedLoad?.Invoke(countNeedLoad);
            }
        }

        private void CheckNeedUpdateLauncher()
        {
#if !DEBUG
            bool needUpdate = false;


            var updateDir = config.ProgramPath + "/" + Config.LauncherName;

            var files = FileUtils.GetFileWithHash(updateDir);

            foreach (var file in files)
            {
                var origHash = file.Hash;
                var nowHash = FileUtils.GetSHA256(launcherDir + "/" + file.Name);

                if (origHash != nowHash || nowHash == "")
                {
                    needUpdate = true;
                    break;
                }
            }


            if (needUpdate)
            {
                var exeFile = $"{launcherDir}/{Config.LauncherName}.exe";
                var exeFileBack = $"{launcherDir}/{Config.LauncherName}.exe.back";

                var args = $"/c echo \"restart\" & timeout 5 & del \"{exeFileBack}\"" +
                    $" & ren \"{exeFile}\" \"{Config.LauncherName}.exe.back\"" +
                    $" & xcopy /e /k /h /i /y \"{updateDir}\" \"{launcherDir}\" " +
                    $" & cd \"{launcherDir}\" " +
                    $" & start {Config.LauncherName}.exe";

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                    }
                };
                proc.Start();
                Stop();
                Environment.Exit(0);
            }
#endif

        }

        private void Load()
        {
            if (ConfigSaver<Config>.Load(Config.ConfigName, out config) == false)
            {
                config = new Config();
            }

            if (Directory.Exists(config.ProgramPath) == false)
            {
                Directory.CreateDirectory(config.ProgramPath);
            }
        }

        private void Save()
        {
            ConfigSaver<Config>.Save(Config.ConfigName, config);
        }

        public void Stop()
        {
            client.Stop();
        }

        public void RunApplication(string path)
        {
            Process.Start(config.ProgramPath + "/" + path + "/" + path + ".exe");
        }

    }
}
