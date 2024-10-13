using LauncherUtils;
using System;
using System.Collections.Generic;
using System.IO;
using Tajlo4ekUtils;
using Utils;
using MessageType = LauncherUtils.Messages.MessageType;


namespace LauncherServer
{
    internal class Controller
    {
        private Config config;

        private readonly ClientServer.Server<MessageType> server;

        private readonly List<Connection> connections;

        public Controller()
        {
            connections = new List<Connection>();

            Load();
            Save();

            server = new ClientServer.Server<MessageType>(config.ServerIp, OnServerError, config.ServerPort);
            server.onGetMessage += OnGetMessage;
            server.GetFilePath += GetFilePath;
            server.Start();
        }

        private void OnServerError(Exception ex, string token)
        {

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

        private void OnGetMessage(ClientServer.Message<MessageType> message)
        {
            var messageToken = message.TokenFrom;

            connections.ForEach((connection) => { if (connection.MyToken.Equals(messageToken)) { connection.Update(); } });

            switch (message.MessageType)
            {
                case ClientServer.Message<MessageType>.GeneralMessageType.User:
                    {
                        OnGetMessageUser(message);
                    }
                    break;

                case ClientServer.Message<MessageType>.GeneralMessageType.GetReg:
                    {
                        var token = message.GetData<string>("token");
                        Connection newConnection = new Connection(token, server.ServerToken);
                        connections.Add(newConnection);
                    }
                    break;
            }
        }

        private string GetFilePath(string name)
        {
            return config.ProgramPath + "/" + name;
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
                ConfigSaver<List<ApplicationAvailable>>.SetDefaultPath(config.ProgramPath);
                if (ConfigSaver<List<ApplicationAvailable>>.Load(
                    ApplicationAvailable.DirName,
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
