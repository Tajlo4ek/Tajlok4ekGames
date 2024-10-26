using ClientServer;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Tajlo4ekUtils;
using Utils;
using MessageType = LauncherUtils.Messages.MessageType;


namespace LauncherServer
{
    internal class Controller
    {
        private Config config;

        private readonly ClientServer.Server<MessageType> server;

        public Controller()
        {
            Load();
            Save();

            if (IPAddress.TryParse(config.ServerIp, out IPAddress ipAddr))
            {
                server = new ClientServer.Server<MessageType>(ipAddr, config.ServerPort);
                server.onGetMessage += OnGetMessageUser;
                server.SetWorkPath(config.ProgramPath);
                server.Start();
            }
            else
            {
                throw new System.Exception("bad config. cant start");
            }
        }

        private void OnGetMessageUser(ClientServer.Message<MessageType> message)
        {
            switch (message.Command)
            {
                case MessageType.GetFilesApplication:
                    {
                        var appName = message.GetData<string>("app");
                        var find = config.AvailableProgram.Find((item) =>
                        {
                            return item.Path == appName && item.Available == true;
                        });

                        if (find == null) { break; }

                        var infoMessage = message.GetReply()
                            .SetCommand(MessageType.SendFilesApplication)
                            .Add("app", appName)
                            .Add("files", FileUtils.GetFileWithHash(config.ProgramPath + "/" + appName));
                        server.SendMessage(infoMessage);
                    }
                    break;

                case MessageType.GetInfo:
                    {
                        var infoMessage = message.GetReply()
                            .SetCommand(MessageType.SendInfo)
                            .Add("info", config.AvailableProgram);
                        server.SendMessage(infoMessage);
                    }
                    break;

                case MessageType.AppUpdated:
                    {
                        var infoMessage = message.GetReply()
                           .SetCommand(MessageType.AppUpdated)
                           .Add("app", message.GetData<string>("app"));
                        server.SendMessage(infoMessage);
                    }
                    break;
            }
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
            else
            {
                if (ConfigSaver<List<ApplicationAvailable>>.Load(
                    Config.ApplicationSaveFile,
                    config.ProgramPath,
                    out List<ApplicationAvailable> applicationAvailable))
                {
                    config.AvailableProgram = applicationAvailable;
                }
                else
                {
                    config.AvailableProgram = new List<ApplicationAvailable> { };
                }
            }
        }

        private void Save()
        {
            ConfigSaver<Config>.Save(Config.ConfigName, config);
        }


    }
}
