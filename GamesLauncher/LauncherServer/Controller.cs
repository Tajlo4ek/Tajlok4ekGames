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

            server = new ClientServer.Server<MessageType>(config.ServerIp, GetMessageToSend, OnServerError, config.ServerPort);
            server.onGetMessage += OnGetMessage;
            server.GetFilePath += GetFilePath;
            server.Start();
        }

        private ClientServer.Message<MessageType> GetMessageToSend(string token)
        {
            lock (connections)
            {
                foreach (var connection in connections)
                {
                    if (connection.Token.Equals(token))
                    {
                        return connection.GetMessage();
                    }
                }
            }

            return new ClientServer.Message<MessageType>(
                ClientServer.Message<MessageType>.GeneralMessageType.Close);
        }

        private void OnServerError(Exception ex, string token)
        {

        }

        private void OnGetMessageUser(ClientServer.Message<MessageType> message)
        {
            var messageToken = message.Token;

            switch (message.Command)
            {
                case MessageType.GetFilesApplication:
                    {
                        var appName = message.GetData("app");
                        var find = config.AvailableProgram.Find((item) =>
                        {
                            return item.Path == appName && item.Available == true;
                        });

                        if (find == null) { break; }

                        var infoMessage = new ClientServer.Message<MessageType>()
                            .SetToken(messageToken)
                            .SetCommand(MessageType.SendFilesApplication)
                            .Add("app", appName)
                            .Add("files", FileUtils.GetFileWithHash(config.ProgramPath + Path.DirectorySeparatorChar + appName));

                        AddMessageForConnection(infoMessage);
                    }
                    break;

                case MessageType.GetInfo:
                    {
                        var infoMessage = new ClientServer.Message<MessageType>()
                            .SetToken(messageToken)
                            .SetCommand(MessageType.SendInfo)
                            .Add("info", config.AvailableProgram);
                        AddMessageForConnection(infoMessage);
                    }
                    break;

                case MessageType.AppUpdated:
                    {
                        var infoMessage = new ClientServer.Message<MessageType>()
                           .SetToken(messageToken)
                           .SetCommand(MessageType.AppUpdated)
                           .Add("app", message.GetData("app"));
                        AddMessageForConnection(infoMessage);
                    }
                    break;
            }
        }

        private void OnGetMessage(ClientServer.Message<MessageType> message)
        {
            var messageToken = message.Token;

            connections.ForEach((connection) => { if (connection.Token.Equals(messageToken)) { connection.Update(); } });

            switch (message.MessageType)
            {
                case ClientServer.Message<MessageType>.GeneralMessageType.User:
                    {
                        OnGetMessageUser(message);
                    }
                    break;

                case ClientServer.Message<MessageType>.GeneralMessageType.GetReg:
                    {
                        Connection newConnection = new Connection(messageToken, true);
                        connections.Add(newConnection);
                    }
                    break;
            }
        }

        private string GetFilePath(string name)
        {
            return config.ProgramPath + Path.DirectorySeparatorChar + name;
        }

        private void AddMessageForConnection(ClientServer.Message<MessageType> message)
        {
            lock (connections)
            {
                foreach (var connection in connections)
                {
                    if (connection.Token == message.Token)
                    {
                        message.SetToken(ClientServer.Server<MessageType>.ServerToken);
                        connection.AddDataToSend(message);
                    }
                }
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
