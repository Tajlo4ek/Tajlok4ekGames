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
        public Action<string> OnFileProcess
        {
            get { return client.OnFileLoadProcess; }
            set { client.OnFileLoadProcess += value; }
        }
        public Action<string> ShowMessage;

        public Controller()
        {
            Load();
            Save();

            client = new ClientServer.Client<MessageType>(config.ServerIp, "", OnServerError, config.ServerPort);
            client.OnGetMessage += OnGetMessage;
            client.GetFilePath += GetFilePath;
            client.GetMessageForServer += GetMessageToSend;

            client.SetWorkPath(config.ProgramPath);
            client.Start();

            try
            {
                File.Delete(launcherDir + Path.DirectorySeparatorChar + Config.LauncherName + ".exe.back");
            }
            catch (Exception)
            {
            }
        }

        private ClientServer.Message<MessageType> GetMessageToSend(string token)
        {
            if (connection.Token.Equals(token))
            {
                return connection.GetMessage();
            }

            return new ClientServer.Message<MessageType>(
                ClientServer.Message<MessageType>.GeneralMessageType.Close);
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
                        var appName = message.GetData("app");
                        var files = JsonUtils<List<FileUtils.FileData>>.FromJson(message.GetData("files"));

                        if (appName.Length == 0 || files.Count == 0)
                        {
                            break;
                        }

                        LoadApplication(appName, files);
                    }
                    break;

                case MessageType.SendInfo:
                    {
                        var apps = JsonUtils<List<ApplicationAvailable>>.FromJson(message.GetData("info"));

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
                        var app = message.GetData("app");

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
                        connection = new Connection(message.GetData("token"), false);
                        CheckUpdate(Config.LauncherName);
                    }
                    break;
            }
        }

        private string GetFilePath(string name)
        {
            return config.ProgramPath + Path.DirectorySeparatorChar + name;
        }

        private void AddMessageForServer(ClientServer.Message<MessageType> message)
        {
            message.SetToken(connection.Token);
            connection.AddDataToSend(message);
        }

        public void CheckUpdate(string appName)
        {
            var infoMessage = new ClientServer.Message<MessageType>()
                            .SetCommand(MessageType.GetFilesApplication)
                            .Add("app", appName);
            AddMessageForServer(infoMessage);
        }

        private void LoadApplication(string appName, List<FileUtils.FileData> files)
        {
            if (Directory.Exists(config.ProgramPath) == false)
            {
                Directory.CreateDirectory(config.ProgramPath);
            }

            int countNeedLoad = 0;

            foreach (var file in files)
            {
                var fullFilePath = config.ProgramPath + Path.DirectorySeparatorChar + appName + Path.DirectorySeparatorChar + file.Name;

                if (FileUtils.GetSHA256(fullFilePath) != file.Hash)
                {
                    countNeedLoad++;

                    var message = new ClientServer.Message<MessageType>(
                        ClientServer.Message<MessageType>.GeneralMessageType.GetFile)
                            .Add("fileName", appName + Path.DirectorySeparatorChar + file.Name);

                    AddMessageForServer(message);
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
                AddMessageForServer(new ClientServer.Message<MessageType>().SetCommand(MessageType.AppUpdated).Add("app", appName));
            }
        }
        private void CheckNeedUpdateLauncher()
        {

#if !DEBUG
            bool needUpdate = false;

            
            var updateDir = config.ProgramPath + Path.DirectorySeparatorChar + Config.LauncherName;

            var files = FileUtils.GetFileWithHash(updateDir);

            foreach (var file in files)
            {
                var origHash = file.Hash;
                var nowHash = FileUtils.GetSHA256(launcherDir + Path.DirectorySeparatorChar + file.Name);

                if (origHash != nowHash || nowHash == "")
                {
                    needUpdate = true;
                    break;
                }
            }


            if (needUpdate)
            {
                var exeFile = $"{launcherDir}{Path.DirectorySeparatorChar}{Config.LauncherName}.exe";
                var exeFileBack = $"{launcherDir}{Path.DirectorySeparatorChar}{Config.LauncherName}.exe.back";

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
            Process.Start(config.ProgramPath + Path.DirectorySeparatorChar + path + Path.DirectorySeparatorChar + path + ".exe");
        }

    }
}
