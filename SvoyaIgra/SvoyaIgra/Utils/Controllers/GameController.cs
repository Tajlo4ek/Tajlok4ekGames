using ClientServer;
using ClientServer.FileUtils;
using DataStore;
using SvoyaIgra.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using static SvoyaIgra.Data.MessageTypes;

namespace SvoyaIgra.Utils.Controllers
{
    public class GameController
    {
        const int connectionPort = ClientServer.Utils.defaultPort + 1;

        public static readonly string MyImgName = "myImg";
        public static readonly string AdminImgName = "adminImg";


        public enum SkipType
        {
            Theme,
            Question,
            Round
        }

        public const int AuctionStep = 100;


        public static readonly Color MainColor = Color.FromArgb(255, 255, 70);

        private readonly Font mainFont;

        private readonly Forms.GameForm gameForm;

        private int nowRound;
        private int endQCount;

        private Package package;

        private readonly List<ChoiceRect> choiceRects;

        private int nowQuestionScenario;
        private int nowAnswerScenario;

        private ChoiceRect nowRectUnder;

        private Question nowQuestion;

        private readonly List<Data.User> users;

        private const int timeWaitAnswerFinal = 60;
        private const int timeWaitAnswerNormal = 8;

        private bool isFinal;

        private readonly string myName;

        public enum AnswerType
        {
            Full,
            Half,
            Fail
        }

        private enum State
        {
            WaitPlayer,
            Start,
            ShowPackageName,
            ShowAllThemes,
            ShowRoundName,
            ShowRoundThemes,
            ShowMain,
            ChoiseQuestion,
            ShowQuestion,
            WaitAnswer,
            Bagcat,
            Auction,
            ChoiseFirstPlayer,
            ShowAnswer,
            ShowFinalThemes,
            ChoiseFinalTheme,
            ShowText,
            FinalRate,
            CheckFinalAns,
            End,
        }

        private State state;
        private State nextState;

        private readonly bool isServer;

        private readonly ClientServer.Server<MessageTypes.MessageType> server;
        private readonly ClientServer.Client<MessageTypes.MessageType> client;
        private readonly DataStore.Utils.PackUtils.FileManager loader;

        private bool isGameStarted = false;
        private bool isGamePaused;

        private bool isAllready;

        private string userAnsToken = "";
        private string userChoiseToken = "";

        private bool isCanAnswer;
        private bool autoAnswer;

        private bool canChoise;

        private string nowShowText;

        private readonly MyQueue<string> userTokenQueue;


        public GameController(bool isServer, IPAddress ip, string name, string imgUrl, string packPath = null)
        {
            mainFont = new Font("Arial", 40, FontStyle.Regular, GraphicsUnit.Point);
            choiceRects = new List<ChoiceRect>();
            userTokenQueue = new MyQueue<string>();

            gameForm = new Forms.GameForm(800, 500, OnEndAct, GetCurrentImage, isServer);
            gameForm.StartStopAction += StartStop;
            gameForm.OnMouseMoveAction += OnRectOver;
            gameForm.OnMouseClickAction += OnRectClick;
            gameForm.OnUserClick += OnChoiseUser;
            gameForm.OnAnswerClick += OnTryAnswerClick;
            gameForm.OnCheckUserAns += OnUserAnswer;
            gameForm.OnSkipAction += NextUserMove;
            gameForm.OnCloseAction += OnClose;
            gameForm.OnCloseAction += SvoyaIgra.Forms.MainMenu.ShowMain;
            gameForm.OnKickUser += Kick;
            gameForm.OnConfigUserMoney += StartConfigMoney;
            gameForm.OnAcceptUserMoney += AcceptConfigUser;
            gameForm.SetChoiceUser += SetChoiceUser;
            gameForm.AuctionMove += OnAuctionMove;
            gameForm.FinalAnsClick += FinalAnswer;
            gameForm.OnSkipClick += SkipQuestion;

            gameForm.Show();

            users = new List<Data.User>();

            this.isServer = isServer;
            isGamePaused = true;
            userAnsToken = "";
            userChoiseToken = "";
            nowShowText = "";
            myName = name;

            if (isServer)
            {
                loader = new DataStore.Utils.PackUtils.FileManager();

                try
                {
                    package = loader.LoadPack(packPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    gameForm.ShowMessageBox("ошибка при загрузке пака");

                    state = State.End;
                    nextState = State.End;

                    gameForm.EndGame();
                    OnClose();

                    return;
                }

                loader.AddLocalFile(DataStore.Utils.PackUtils.PackManager.BasePackName);

                loader.LoadImg(imgUrl, AdminImgName);
                gameForm.SetAdminImage(loader.GetRealPath(AdminImgName));


                server = new ClientServer.Server<MessageTypes.MessageType>(ip, connectionPort);
                server.OnGetMessage += OnGetUserMassage;
                server.OnErrorAction += OnServerError;
                server.NewConnectAction += ServerNewUserConnected;
                server.OnFileLoadProgress += FileLoadProgress;
                server.SetWorkPath(loader.WorkDirectory);

                gameForm.SetAdminName(name);

                server.Start();
            }
            else
            {
                loader = new DataStore.Utils.PackUtils.FileManager();
                loader.LoadImg(imgUrl, MyImgName);

                client = new ClientServer.Client<MessageTypes.MessageType>(ip, connectionPort);
                client.OnErrorAction += OnClientError;
                client.SetWorkPath(loader.WorkDirectory);
                client.OnGetMessage += OnGetUserMassage;
                client.OnFileLoadProgress += FileLoadProgress;
                client.NewConnectAction += ClientToServerConnected;

                client.Start();
            }

            nowRound = 0;
            nextState = State.WaitPlayer;

            canChoise = false;
            isFinal = false;
        }

        private void ServerNewUserConnected(string token)
        {

            var connectedUser = new Data.User(token);

            users.Add(connectedUser);
            ServerSendToUser(MessageType.SendAdminData, connectedUser.Token, new Dictionary<string, object> { { "name", myName } });


            server.SendFile(token, DataStore.Utils.PackUtils.PackManager.BasePackName);
            server.SendFile(token, AdminImgName);
        }

        private void ClientToServerConnected(string token)
        {
            loader.RenameFile(MyImgName, client.MyToken);

            ClientSendToServer(MessageType.SendUserData, new Dictionary<string, object> { { "name", myName } });
            client.SendFile(client.ServerToken, client.MyToken);
        }

        private void FileLoadProgress(ProgressFileData data)
        {
            switch (data.State)
            {
                case ProgressFileData.States.Start:
                    {
                        gameForm.AddToChat("загрузка: " + data.Name);
                    }
                    break;

                case ProgressFileData.States.End:
                    {
                        if (data.Name == DataStore.Utils.PackUtils.PackManager.BasePackName)
                        {
                            if (!isServer)
                            {
                                FileLoad(MessageTypes.FileType.Pack, data.Name);
                            }
                        }
                        else if (data.Name == AdminImgName)
                        {
                            FileLoad(MessageTypes.FileType.AdminImg, data.Name);
                        }
                        else
                        {
                            FileLoad(MessageTypes.FileType.UserImg, data.Name);
                        }
                    }
                    break;

                case ProgressFileData.States.Error:
                    {
                        if (data.Name == DataStore.Utils.PackUtils.PackManager.BasePackName)
                        {
                            throw new Exception("error get pack from server");
                        }
                    }
                    break;

                case ProgressFileData.States.Process:
                    {
                        gameForm.AddToChat(data.Progress + "%");
                    }
                    break;
            }
        }

        private void OnClientError(Exception ex, string token)
        {
            if (state != State.End)
            {
                gameForm.EndGame();
                gameForm.ShowMessageBox("lost host\n ");
                OnClose();
            }
        }

        private void OnServerError(Exception ex, string token)
        {
            lock (users)
            {
                var user = users.Find((x) => x.Token.Equals(token));

                if (user == null || token == "")
                    return;

                ServerBroadcastMessage(MessageType.Kick, new Dictionary<string, object> { { "token", token } });
            }
        }

        private void OnClose()
        {
            loader.Dispose();

            client?.Stop();
            server?.Stop();
        }

        private void FileLoad(MessageTypes.FileType type, string fileName)
        {
            switch (type)
            {
                case MessageTypes.FileType.Pack:
                    {
                        if (!isServer)
                        {
                            package = loader.LoadPackFromLocal(fileName);
                            ClientSendToServer(MessageTypes.MessageType.PackLoaded);
                            ClientSendToServer(MessageTypes.MessageType.Ready);
                        }
                    }
                    break;

                case MessageTypes.FileType.AdminImg:
                    {
                        if (!isServer)
                        {
                            loader.AddLocalFile(AdminImgName);
                            gameForm.SetAdminImage(loader.GetRealPath(AdminImgName));
                        }
                    }
                    break;

                case MessageTypes.FileType.UserImg:
                    {
                        loader.AddLocalFile(fileName);
                        gameForm.AddUserImage(fileName, loader.GetRealPath(fileName));

                        if (isServer)
                        {
                            lock (users)
                            {
                                foreach (var user in users)
                                {
                                    server.SendFile(user.Token, fileName);
                                }
                            }
                        }

                    }
                    break;
            }
        }

        private void OnGetUserMassage(ClientServer.Message<MessageTypes.MessageType> message)
        {
            if (state == State.End)
            {
                return;
            }

            switch (message.Command)
            {
                case MessageTypes.MessageType.AddToChat:
                    {
                        gameForm.AddToChat(message.GetData<string>("data"));
                    }
                    break;

                case MessageTypes.MessageType.SendUserData:
                    {
                        if (isServer)
                        {
                            if (TryGetUser(message.TokenFrom, out User user) == false)
                            {
                                return;
                            }
                            var name = message.GetData<string>("name");

                            user.SetName(name);
                            gameForm.AddUserData(name, 0, user.Token);

                            ServerBroadcastMessage(
                                MessageType.SendUserData,
                                new Dictionary<string, object> {
                                    { "token", user.Token },
                                    { "name", name },
                                    { "money", 0 },
                                });

                            lock (users)
                            {
                                foreach (var uData in users)
                                {
                                    ServerSendToUser(
                                        MessageType.SendUserData,
                                        user.Token,
                                        new Dictionary<string, object> {
                                            { "token", uData.Token },
                                            { "name", uData.Name },
                                            { "money", 0 },
                                        });

                                    server.SendFile(user.Token, uData.Token);
                                }
                            }

                            ServerBroadcastMessage(
                                MessageType.AddToChat,
                                new Dictionary<string, object> {
                                    { "data", name + " connect"}
                                });
                        }
                        else
                        {
                            var userToken = message.GetData<string>("token");

                            if (TryGetUser(userToken, out User user) != false)
                            {
                                return;
                            }

                            var userName = message.GetData<string>("name");
                            var userMoney = message.GetData<int>("money");

                            lock (users)
                            {
                                users.Add(new Data.User(userToken, userName));
                            }
                            gameForm.AddUserData(userName, userMoney, userToken);
                        }
                    }
                    break;

                case MessageType.PackLoaded:
                    {
                        if (isGameStarted)
                        {
                            if (TryGetUser(message.TokenFrom, out User user) == false)
                            {
                                return;
                            }

                            ServerSendToUser(MessageType.SendUsedQuestion, user.Token, CurrentQuestionMessageData());
                            ServerSendToUser(MessageType.ForseShowMain, user.Token, new Dictionary<string, object> { { "round", nowRound } });
                        }
                    }
                    break;

                case MessageTypes.MessageType.StartGame:
                    {

                        if (!isGameStarted)
                        {
                            nowRound = message.GetData<int>("round");

                            isGamePaused = false;
                            isGameStarted = true;
                            nextState = State.Start;
                            OnEndAct();
                        }
                        else
                        {
                            if (isGamePaused)
                            {
                                gameForm.Start();
                            }
                        }
                        isGamePaused = false;
                    }
                    break;

                case MessageTypes.MessageType.Ready:
                    {
                        users.Find((user) => user.Token.Equals(message.TokenFrom))?.SetReady();

                        bool bufAllReady = true;
                        users.ForEach((user) => bufAllReady &= user.IsReady);
                        isAllready = bufAllReady;
                    }
                    break;

                case MessageTypes.MessageType.ChoiseQuestion:
                    {
                        if (isServer)
                        {
                            int round = message.GetData<int>("round");
                            int theme = message.GetData<int>("theme");
                            int question = message.GetData<int>("question");

                            ShowQuestion(round, theme, question);

                            ServerBroadcastMessage(
                                MessageType.ShowQuestion,
                                new Dictionary<string, object> {
                                    {"round", round },
                                    {"theme", theme },
                                    {"question", question }
                                });

                            userChoiseToken = message.TokenFrom;
                        }
                        else
                        {
                            canChoise = true;
                            nextState = State.ShowMain;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.ShowQuestion:
                    {
                        int round = message.GetData<int>("round");
                        int theme = message.GetData<int>("theme");
                        int question = message.GetData<int>("question");

                        ShowQuestion(round, theme, question);

                        if (isFinal)
                        {
                            gameForm.SetCanAnswer(false);
                        }

                    }
                    break;

                case MessageTypes.MessageType.ChoiseTheme:
                    {
                        if (!isServer)
                        {
                            canChoise = true;
                            nextState = State.ShowFinalThemes;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.SetPause:
                    {
                        gameForm.Pause();
                        isGamePaused = true;
                    }
                    break;

                case MessageTypes.MessageType.TryAnswer:
                    {
                        if (!userAnsToken.Equals(""))
                            return;

                        if (isServer)
                        {
                            if (TryGetUser(message.TokenFrom, out User findUser) == false)
                            {
                                return;
                            }

                            if (findUser == null)
                            {
                                userAnsToken = "";
                                return;
                            }
                            else
                            {
                                if (findUser.CanAnswer)
                                {
                                    userAnsToken = findUser.Token;
                                }
                                else
                                {
                                    userAnsToken = "";
                                    return;
                                }
                            }

                            //TODO: 
                            if (isCanAnswer || true)
                            {
                                ServerBroadcastMessage(MessageType.SetPause);

                                gameForm.Pause();
                                gameForm.ShowAnsMenu(true);

                                findUser.SetCanAnswer(false);

                                AdminSay(findUser.Name + " отвечайте");
                            }
                            else if (state == State.ShowQuestion && nowQuestion.IsNormal)
                            {
                                var text = findUser.Name + " фальстарт";

                                gameForm.AddToChat(text);

                                findUser.SetCanAnswer(false);
                                userAnsToken = "";

                                ServerBroadcastMessage(
                                    MessageType.AddToChat,
                                    new Dictionary<string, object> {
                                        { "data", text }
                                    });
                            }
                            else
                            {
                                userAnsToken = "";
                            }
                        }
                    }
                    break;

                case MessageTypes.MessageType.ShowAnswer:
                    {
                        nowAnswerScenario = 0;
                        nextState = State.ShowAnswer;
                        OnEndAct();
                    }
                    break;

                case MessageTypes.MessageType.AdminSay:
                    {
                        gameForm.SetAdminSay(message.GetData<string>("text"));
                    }
                    break;

                case MessageTypes.MessageType.UpdateMoney:
                    {
                        var userToken = message.GetData<string>("token");

                        if (TryGetUser(userToken, out User user) == false)
                        {
                            return;
                        }

                        user.Money = message.GetData<int>("money");
                        gameForm.UpdateMoney(userToken, user.Money);

                    }
                    break;

                case MessageTypes.MessageType.SendAdminData:
                    {
                        gameForm.SetAdminName(message.GetData<string>("name"));
                    }
                    break;

                case MessageTypes.MessageType.Kick:
                    {
                        var token = message.GetData<string>("token");
                        if (!isServer)
                        {
                            if (client.MyToken.Equals(token))
                            {
                                state = State.End;
                                nextState = State.End;
                                gameForm.EndGame();
                                gameForm.ShowMessageBox("Вас кикнули");
                                OnClose();
                            }
                            else
                            {
                                for (int userId = 0; userId < users.Count; userId++)
                                {
                                    if (users[userId].Token.Equals(token))
                                    {
                                        users.RemoveAt(userId);
                                        gameForm.Kick(token);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case MessageTypes.MessageType.ForseShowMain:
                    {
                        if (!isServer)
                        {
                            gameForm.SetCanChoise(false);
                            isGameStarted = true;
                            nowRound = message.GetData<int>("round");
                            nowQuestion = null;
                            gameForm.FinishMedia(false);
                            state = State.ShowAnswer;
                            nextState = package.GetRound(nowRound).IsFinal ? State.ShowFinalThemes : State.ShowMain;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.SendUsedQuestion:
                    {
                        int count = message.GetData<int>("count");
                        nowRound = message.GetData<int>("round");

                        for (int i = 0; i < count; i++)
                        {
                            var str = message.GetData<string>(i.ToString()).Split(' ');

                            var roundId = int.Parse(str[0]);
                            var themeId = int.Parse(str[1]);
                            var questionId = int.Parse(str[2]);

                            package.GetRound(roundId).GetTheme(themeId).GetQuestion(questionId).SetEnd();
                        }

                        endQCount = CalcUsedQuestion();
                        OnEndAct();
                    }
                    break;

                case MessageTypes.MessageType.StartCanAnswer:
                    {
                        if (!isServer)
                        {
                            if (nowQuestion.IsNormal)
                            {
                                int time = message.GetData<int>("timeSec");
                                gameForm.WaitAnswer(time, "");
                                if (isFinal)
                                {
                                    gameForm.ShowFinalAnswer();
                                }
                            }

                            if (autoAnswer)
                            {
                                OnTryAnswerClick();
                            }
                        }
                    }
                    break;

                case MessageTypes.MessageType.UserClickUser:
                    {
                        string token = message.GetData<string>("token");

                        if (isServer)
                        {
                            gameForm.SetCanChoise(false);

                            if (TryGetUser(token, out User user) == false)
                            {
                                return;
                            }

                            foreach (var userCheck in users)
                            {
                                userCheck.SetCanAnswer(false);
                            }
                            user.SetCanAnswer(true);

                            AdminSay(user.Name + ", вопрос для вас");

                            ServerSendToUser(MessageType.StartAutoAnswer, user.Token);
                            ServerBroadcastMessage(MessageType.ForceShowQuestion);

                            userChoiseToken = token;

                            nextState = State.ShowQuestion;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.CanChoiceUser:
                    {
                        if (!isServer)
                        {
                            gameForm.SetCanChoise(true);
                        }
                    }
                    break;

                case MessageTypes.MessageType.StartAutoAnswer:
                    {
                        if (!isServer)
                        {
                            autoAnswer = true;
                        }
                    }
                    break;

                case MessageTypes.MessageType.ForceShowQuestion:
                    {
                        if (!isServer)
                        {
                            gameForm.SetCanChoise(false);
                            nextState = State.ShowQuestion;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.AuctionChoice:
                    {
                        if (isServer)
                        {
                            if (TryGetUser(message.TokenFrom, out User user) == false)
                            {
                                return;
                            }

                            int rate = 0;

                            if (user != null)
                            {
                                rate = message.GetData<int>("rate");
                                if (rate <= 0)
                                {
                                    rate = -1;
                                }
                                user.SetRate(rate);


                                if (rate == -1)
                                {
                                    AdminSay(user.Name + ": пас");
                                }
                                else
                                {
                                    AdminSay(user.Name + ": " + rate);
                                }
                            }


                            int maxRate = 0;
                            bool isSomeAllIn = false;

                            foreach (var checkUser in users)
                            {
                                if (checkUser.Rate > maxRate)
                                {
                                    maxRate = checkUser.Rate;
                                }
                            }

                            foreach (var checkUser in users)
                            {
                                if (rate != -1)
                                {
                                    if (checkUser.Money <= maxRate && user != checkUser)
                                    {
                                        checkUser.SetRate(-1);
                                    }
                                }
                                isSomeAllIn |= checkUser.IsAllIn;
                            }

                            NextUserQueue();

                            if (userTokenQueue.Count == 1)
                            {
                                if (TryGetUser(userTokenQueue.Dequeue(), out user) == false)
                                {
                                    return;
                                }

                                foreach (var checkUser in users)
                                {
                                    checkUser.SetCanAnswer(false);
                                }
                                user.SetCanAnswer(true);

                                ServerSendToUser(MessageType.StartAutoAnswer, user.Token);
                                ServerBroadcastMessage(MessageType.ForceShowQuestion);
                                AdminSay(user.Name + " вопрос для вас");

                                userChoiseToken = user.Token;

                                nextState = State.ShowQuestion;
                                OnEndAct();
                            }
                            else
                            {
                                if (TryGetUser(userTokenQueue.Peek(), out User nextUser) == false)
                                {
                                    return;
                                }

                                var minValue = maxRate + AuctionStep;
                                var maxValue = nextUser.Money;

                                if (minValue > nextUser.Money)
                                {
                                    minValue = nextUser.Money;
                                    maxValue = nextUser.Money;
                                    isSomeAllIn = true;
                                }
                                ServerSendToUser(
                                    MessageType.AuctionChoice,
                                    nextUser.Token,
                                    new Dictionary<string, object> {
                                        { "minValue", minValue },
                                        { "maxValue", maxValue },
                                        { "canPass", true },
                                        { "canAllIn", true },
                                        { "canSet", !isSomeAllIn }
                                    });
                            }

                        }
                        else
                        {
                            var minValue = message.GetData<int>("minValue");
                            var maxValue = message.GetData<int>("maxValue");

                            var canPass = message.GetData<bool>("canPass");
                            var canAllIn = message.GetData<bool>("canAllIn");
                            var canSet = message.GetData<bool>("canSet");

                            gameForm.ShowAuction(minValue, maxValue, canPass, canAllIn, canSet);
                        }
                    }
                    break;

                case MessageTypes.MessageType.AddTextToMainScreen:
                    {
                        if (!isServer)
                        {
                            nowShowText = message.GetData<string>("text");
                            bool show = message.GetData<bool>("show");
                            if (show)
                            {
                                nextState = State.ShowText;
                                OnEndAct();
                            }
                        }
                    }
                    break;

                case MessageTypes.MessageType.EndGame:
                    {
                        if (!isServer)
                        {
                            state = State.End;
                            nextState = State.End;
                            OnClose();
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.FinalKickTheme:
                    {
                        if (message.TokenFrom.Equals(userTokenQueue.Peek()) != true)
                        {
                            break;
                        }

                        int roundId = message.GetData<int>("round");
                        int themeId = message.GetData<int>("theme");

                        package.GetRound(roundId).GetTheme(themeId).SetUsed();

                        if (isServer)
                        {
                            int countThemes = 0;
                            var round = package.GetRound(roundId);
                            int playTheme = 0;

                            for (int id = 0; id < round.CountThemes; id++)
                            {
                                if (!round.GetTheme(id).IsUsed)
                                {
                                    countThemes++;
                                    playTheme = id;
                                }
                            }

                            if (countThemes == 1)
                            {
                                nowShowText = "Играем тему " + round.GetTheme(playTheme).Name + "\n\n Делайте Ваши ставки";

                                ServerBroadcastMessage(
                                    MessageType.AddTextToMainScreen,
                                    new Dictionary<string, object> {
                                        { "text", nowShowText },
                                        { "show", true }
                                    });

                                foreach (var checkUser in users)
                                {
                                    if (checkUser.Money > 0)
                                    {
                                        ServerSendToUser(MessageType.FinalRate, checkUser.Token);
                                        checkUser.SetRate(0);
                                    }
                                    else
                                    {
                                        checkUser.SetRate(-1);
                                    }

                                }

                                isFinal = true;
                                nextState = State.FinalRate;
                            }
                            else
                            {
                                ServerBroadcastMessage(
                                    MessageType.FinalKickTheme,
                                    new Dictionary<string, object> {
                                        { "round", roundId },
                                        { "theme", themeId }
                                    });

                                NextUserMove();
                            }
                        }

                        OnEndAct();
                    }
                    break;

                case MessageTypes.MessageType.FinalRate:
                    {
                        isFinal = true;

                        if (!isServer)
                        {
                            gameForm.ShowAuction(AuctionStep, users[0].Money, false, true, users[0].Money >= AuctionStep);
                            state = State.FinalRate;
                        }
                        else
                        {
                            if (TryGetUser(message.TokenFrom, out User user) != false)
                            {
                                int rate = message.GetData<int>("rate");
                                if (rate > user.Money || rate <= 0)
                                {
                                    rate = -1;
                                }
                                user.SetRate(rate);
                            }

                            int countRate = 0;
                            foreach (var checkUser in users)
                            {
                                if (checkUser.IsPass || checkUser.Rate > 0)
                                {
                                    countRate++;
                                }
                            }

                            if (countRate == users.Count)
                            {
                                NextUserMove();
                            }

                        }
                    }
                    break;

                case MessageTypes.MessageType.FinalAnswer:
                    {
                        if (TryGetUser(message.TokenFrom, out User user) == false)
                        {
                            return;
                        }

                        user.SetFinalAns(message.GetData<string>("data"));
                    }
                    break;
            }
        }

        private Image GenerateMain()
        {
            choiceRects.Clear();

            var size = gameForm.GetViewSize();
            var bmp = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(bmp);

            g.Clear(Color.Empty);

            var round = package.GetRound(nowRound);

            float dx = size.Width * 0.3f;
            float dy = (float)size.Height / round.CountThemes;

            Pen pen = new Pen(MainColor, 2);
            Brush brush = new SolidBrush(MainColor);

            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            Font font = new Font("Arial", 20, FontStyle.Regular, GraphicsUnit.Pixel);

            g.DrawLine(pen, dx, 0, dx, size.Height);
            for (int i = 1; i < round.CountThemes; i++)
            {
                g.DrawLine(pen, 0, i * dy, size.Width, i * dy);
            }

            int maxQCount = 0;

            for (int i = 0; i < round.CountThemes; i++)
            {
                var theme = round.GetTheme(i);

                Rectangle rect = new Rectangle(0, (int)(i * dy), (int)dx, (int)dy);

                font = Utils.DrawUtils.GetAdjustedFont(g, theme.Name, font, new SizeF(rect.Width, rect.Height), 60, 5);

                g.DrawString(theme.Name, font, brush, rect, stringFormat);

                if (theme.CountQuestions > maxQCount)
                    maxQCount = theme.CountQuestions;
            }

            var offset = dx;
            dx = (size.Width - offset) / maxQCount;


            for (int tId = 0; tId < round.CountThemes; tId++)
            {
                var theme = round.GetTheme(tId);

                for (int qId = 0; qId < maxQCount; qId++)
                {
                    if (qId < theme.CountQuestions)
                    {
                        var quest = theme.GetQuestion(qId);

                        if (!quest.IsUsed)
                        {
                            Rectangle rect = new Rectangle((int)(offset + qId * dx) + 2, (int)(tId * dy) + 2, (int)dx - 4, (int)dy - 4);

                            choiceRects.Add(new ChoiceRect(rect, tId, qId));

                            var text = quest.Cost.ToString();

                            font = Utils.DrawUtils.GetAdjustedFont(g, text, font, new SizeF(rect.Width, rect.Height), 25, 5);

                            g.DrawString(text, font, brush, rect, stringFormat);
                        }
                    }

                    if (tId == 0)
                    {
                        g.DrawLine(pen, offset + qId * dx, 0, offset + qId * dx, size.Height);
                    }
                }
            }

            if (state == State.ChoiseQuestion)
            {
                if (nowRectUnder != null)
                {
                    pen = new Pen(Color.Red, 3);
                    g.DrawRectangle(pen, nowRectUnder.GetRect());
                }
            }
            else
            {
                nowRectUnder = null;
            }

            return bmp;
        }

        private Image GenerateFinal()
        {
            choiceRects.Clear();

            var size = gameForm.GetViewSize();
            var bmp = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(bmp);

            g.Clear(Color.Empty);

            var round = package.GetRound(nowRound);
            float dy = (float)size.Height / round.CountThemes;

            Pen pen = new Pen(Color.Red, 2);
            Brush brush = new SolidBrush(MainColor);

            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            Font font = new Font("Arial", 20, FontStyle.Regular, GraphicsUnit.Pixel);

            for (int themeNum = 0; themeNum < round.CountThemes; themeNum++)
            {
                var theme = round.GetTheme(themeNum);

                if (!theme.IsUsed)
                {
                    Rectangle rect = new Rectangle(0, (int)(themeNum * dy), size.Width, (int)dy);
                    font = Utils.DrawUtils.GetAdjustedFont(g, theme.Name, font, new SizeF(rect.Width, rect.Height), 60, 5);
                    g.DrawString(theme.Name, font, brush, rect, stringFormat);
                    choiceRects.Add(new ChoiceRect(rect, themeNum));
                }
            }

            if (nowRectUnder != null)
            {
                g.DrawRectangle(pen, nowRectUnder.GetRect());
            }

            return bmp;
        }

        private void SkipRound()
        {
            nextState = State.ShowRoundName;
            endQCount = 0;
            nowRound++;
        }

        private void OnEndAct()
        {
            switch (state)
            {
                case State.ShowRoundThemes:
                case State.ShowAnswer:
                    {
                        if (nowQuestion != null && nowAnswerScenario < nowQuestion.CountAnswer)
                            break;

                        if (endQCount == package.GetRound(nowRound).CountQuestions)
                        {
                            SkipRound();

                            if (nowRound < package.CountRounds)
                            {
                                AdminSay("следующий раунд");
                                nowAnswerScenario = 0;
                                nowQuestion = null;
                            }
                            else
                            {
                                if (isServer)
                                {
                                    nextState = State.End;
                                    AdminSay("");

                                    var data = "Спасибо за игру\n";
                                    lock (users)
                                    {
                                        int maxId = 0;
                                        int maxMoney = 0;
                                        for (int userId = 0; userId < users.Count; userId++)
                                        {
                                            if (users[userId].Money > maxMoney)
                                            {
                                                maxId = userId;
                                                maxMoney = users[userId].Money;
                                            }
                                        }
                                        data += users[maxId].Name + " победил\n";
                                    }

                                    EndGame(data);

                                }
                            }
                        }
                        else
                        {
                            if (isServer)
                            {
                                lock (users)
                                {
                                    isFinal = package.GetRound(nowRound).IsFinal;

                                    if (isFinal)
                                    {
                                        bool isFind = false;
                                        foreach (var checkUser in users)
                                        {
                                            if (checkUser.Money > 0)
                                            {
                                                isFind = true;
                                                checkUser.SetRate(0);
                                            }
                                            else
                                            {
                                                checkUser.SetRate(-1);
                                            }
                                        }

                                        if (!isFind)
                                        {
                                            nextState = State.End;
                                            AdminSay("");
                                            EndGame("Финала не будет\n расходимся");
                                            break;
                                        }

                                        CreateQueueByMoney();
                                        userChoiseToken = userTokenQueue.Peek();
                                    }

                                    if (userChoiseToken.Equals(""))
                                    {
                                        gameForm.SetAdminSay("кто выбирает");
                                        gameForm.SetCanChoise(true);
                                        break;
                                    }

                                    gameForm.SetCanChoise(false);

                                    ServerBroadcastMessage(
                                        isFinal ? MessageTypes.MessageType.ChoiseTheme : MessageTypes.MessageType.ChoiseQuestion,
                                        new Dictionary<string, object> { { "token", userChoiseToken } });


                                    if (TryGetUser(userChoiseToken, out User findUser) == true)
                                    {
                                        if (isFinal)
                                        {
                                            AdminSay(findUser.Name + " уберите 1 тему");
                                        }
                                        else
                                        {
                                            AdminSay(findUser.Name + " выбирайте вопрос");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            state = nextState;

            if (canChoise)
            {
                if (state == State.ShowMain)
                {
                    state = State.ChoiseQuestion;
                }
                else if (state == State.ShowFinalThemes)
                {
                    state = State.ChoiseFinalTheme;
                }
            }

            Image img = GetCurrentImage();

            switch (state)
            {
                case State.Start:
                    gameForm.ShowImageBackground(img, 2);
                    nextState = State.ShowPackageName;
                    break;
                case State.ShowPackageName:
                    gameForm.ShowImageBackground(img, 2);
                    nextState = State.ShowAllThemes;
                    break;
                case State.ShowAllThemes:
                    gameForm.ShowMoveToUpImage(img);
                    nextState = State.ShowRoundName;
                    break;
                case State.ShowRoundName:
                    gameForm.ShowImageBackground(img, 3);
                    nextState = State.ShowRoundThemes;
                    break;
                case State.ShowRoundThemes:
                    gameForm.ShowMoveToUpImage(img);
                    nextState = package.GetRound(nowRound).IsFinal ? State.ShowFinalThemes : State.ShowMain;
                    break;
                case State.ShowMain:
                    nowRectUnder = null;
                    gameForm.ShowImageBackground(img, -1);
                    if (!isServer)
                    {
                        ClientSendToServer(MessageTypes.MessageType.Ready);
                    }
                    else
                    {
                        state = State.ChoiseQuestion;
                        lock (users)
                        {
                            foreach (var user in users)
                            {
                                user.SetCanAnswer(true);
                            }
                        }
                    }
                    break;
                case State.Bagcat:
                    gameForm.ShowImageBackground(img, -1);
                    break;
                case State.Auction:
                    gameForm.ShowImageBackground(img, -1);
                    break;
                case State.ChoiseQuestion:
                    gameForm.ShowImageBackground(img, -1);
                    gameForm.ShowAnsMenu(false);
                    break;
                case State.ShowQuestion:
                    gameForm.SetCanAnswer(true);
                    PlayScenarioQuestion();
                    break;
                case State.WaitAnswer:
                    if (isServer)
                    {
                        gameForm.WaitAnswer(isFinal ? timeWaitAnswerFinal : timeWaitAnswerNormal + 1, nowQuestion.StrAnswer);

                        if (isFinal)
                        {
                            nextState = State.CheckFinalAns;
                            CreateQueueByMoney();
                        }
                        else
                        {
                            nextState = State.ShowAnswer;
                            userTokenQueue.Clear();
                        }
                    }
                    break;
                case State.ShowAnswer:
                    gameForm.SetCanAnswer(false);
                    gameForm.ShowAnsMenu(false);
                    if (isServer)
                    {
                        lock (users)
                        {
                            foreach (var user in users)
                            {
                                user.SetCanAnswer(true);
                            }
                        }
                    }
                    PlayScenarioAnswer();
                    break;
                case State.End:
                    gameForm.ShowImageBackground(img, -1);
                    gameForm.EndGame();
                    break;
                case State.ShowFinalThemes:
                    nowRectUnder = null;
                    if (isServer)
                    {
                        state = State.ChoiseFinalTheme;
                    }
                    gameForm.ShowImageBackground(img, -1);
                    break;
                case State.ChoiseFinalTheme:
                    gameForm.ShowImageBackground(img, -1);
                    break;
                case State.FinalRate:
                case State.ShowText:
                    gameForm.ShowImageBackground(img, -1);
                    break;
                case State.CheckFinalAns:
                    CheckUserFinalAns();
                    break;
            }


            if (state == State.WaitAnswer)
            {
                isCanAnswer = true;
            }
            else
            {
                isCanAnswer = false;
            }

        }

        private void CheckUserFinalAns()
        {
            if (!isServer)
            {
                return;
            }

            lock (users)
            {
                if (!userTokenQueue.IsEmpty)
                {
                    userAnsToken = userTokenQueue.Peek();

                    if (TryGetUser(userAnsToken, out User user) == false)
                    {
                        return;
                    }

                    nowShowText = user.Name;
                    nowShowText += "\nОтвет: " + user.FinalAns;
                    nowShowText += "\nСтавка: " + user.Rate;
                    gameForm.ShowImageBackground(GetCurrentImage(), -1);

                    ServerBroadcastMessage(
                        MessageType.AddTextToMainScreen,
                        new Dictionary<string, object> {
                            {"text", nowShowText },
                            {"show", true }
                        });

                    gameForm.ShowAnsMenu(true);
                }
                else
                {
                    gameForm.ShowAnsMenu(false);

                    nextState = State.End;
                    AdminSay("");

                    var data = "Спасибо за игру\n";
                    lock (users)
                    {
                        int maxId = 0;
                        int maxMoney = int.MinValue;
                        for (int userId = 0; userId < users.Count; userId++)
                        {
                            if (users[userId].Money > maxMoney)
                            {
                                maxId = userId;
                                maxMoney = users[userId].Money;
                            }
                        }
                        data += users[maxId].Name + " победил\n";
                    }

                    EndGame(data);
                    OnEndAct();

                }

            }
        }

        private void PlayScenarioQuestion()
        {
            if (nowQuestionScenario < nowQuestion.CountScenarios)
            {
                isCanAnswer = false;
                var scenario = nowQuestion.GetScenario(nowQuestionScenario);

                gameForm.SetAnswer(nowQuestion.StrAnswer);

                nowShowText = "";

                switch (scenario.Type)
                {
                    case Scenario.ScenarioType.Text:
                        nowShowText = scenario.Data;
                        gameForm.ShowImageBackground(
                            Utils.DrawUtils.GenerateShowText(
                                nowShowText,
                                true,
                                gameForm.GetViewSize(),
                                mainFont,
                                MainColor),
                            3 + (int)(scenario.Data.Length * 0.05),
                            true);
                        break;
                    case Scenario.ScenarioType.Video:
                    case Scenario.ScenarioType.Audio:
                        gameForm.PlayMedia(scenario.Data, scenario.Time);
                        break;
                    case Scenario.ScenarioType.Image:
                        gameForm.ShowImage(scenario.Data, 5);
                        break;
                }

                nowQuestionScenario++;
            }
            else
            {
                if (isServer)
                {
                    isCanAnswer = true;
                    nextState = State.WaitAnswer;

                    ServerBroadcastMessage(MessageType.StartCanAnswer,
                        new Dictionary<string, object> {
                            { "timeSec", isFinal ? timeWaitAnswerFinal : timeWaitAnswerNormal }
                        });

                    OnEndAct();
                }
            }

            if (package.GetRound(nowRound).IsFinal)
            {
                gameForm.SetCanAnswer(false);
            }

        }

        private void PlayScenarioAnswer()
        {
            if (isServer && nowAnswerScenario == 0)
            {
                ServerBroadcastMessage(MessageTypes.MessageType.ShowAnswer);
            }

            autoAnswer = false;

            if (nowAnswerScenario < nowQuestion.CountAnswer)
            {
                isCanAnswer = false;
                var scenario = nowQuestion.GetAnswer(nowAnswerScenario);

                nowShowText = "";

                switch (scenario.Type)
                {
                    case Scenario.ScenarioType.Text:
                        nowShowText = scenario.Data;
                        gameForm.ShowImageBackground(
                            Utils.DrawUtils.GenerateShowText(
                                nowShowText,
                                true,
                                gameForm.GetViewSize(),
                                mainFont,
                                MainColor),
                            3 + (int)(scenario.Data.Length * 0.05),
                            false);
                        break;
                    case Scenario.ScenarioType.Video:
                    case Scenario.ScenarioType.Audio:
                        gameForm.PlayMedia(scenario.Data, scenario.Time, false);
                        break;
                    case Scenario.ScenarioType.Image:
                        gameForm.ShowImage(scenario.Data, 5, false);
                        break;
                }

                nowAnswerScenario++;
            }
            else
            {
                if (isServer)
                {
                    ServerBroadcastMessage(MessageType.ForseShowMain, new Dictionary<string, object> { { "round", nowRound } });

                    nowAnswerScenario = 0;
                    nextState = State.ShowMain;
                    OnEndAct();
                }
            }
        }

        private Image GetCurrentImage()
        {
            var size = gameForm.GetViewSize();

            switch (state)
            {
                case State.Start:
                    return Utils.DrawUtils.GenerateShowText("Добро пожаловать", true, size, mainFont, MainColor);
                case State.ShowPackageName:
                    return Utils.DrawUtils.GenerateShowText(package.GetInfoString(), true, size, mainFont, MainColor);
                case State.ShowRoundName:
                    return Utils.DrawUtils.GenerateShowText(package.GetRound(nowRound).Name, true, size, mainFont, MainColor);
                case State.ShowRoundThemes:
                    return Utils.DrawUtils.GenerateShowText(package.GetRound(nowRound).GetThemesName(), false, size, mainFont, MainColor);
                case State.ShowMain:
                case State.ChoiseQuestion:
                    return GenerateMain();
                case State.ShowAnswer:
                case State.ShowQuestion:
                    return Utils.DrawUtils.GenerateShowText(nowShowText, true, size, mainFont, MainColor);
                case State.ShowAllThemes:
                    return Utils.DrawUtils.GenerateShowText(package.GetAllThemes(), false, size, mainFont, MainColor);
                case State.Bagcat:
                    var text = "Кот в мешке!\nВопрос нужно отдать\n\nТема: " + nowQuestion.ThemeName + "\nСтоимость: " + nowQuestion.SpecialCost;
                    return Utils.DrawUtils.GenerateShowText(text, true, size, mainFont, MainColor);
                case State.Auction:
                    return Utils.DrawUtils.GenerateShowText("Вопрос-аукцион", true, size, mainFont, MainColor);
                case State.End:
                case State.ShowText:
                case State.FinalRate:
                case State.CheckFinalAns:
                    return Utils.DrawUtils.GenerateShowText(nowShowText, true, gameForm.GetViewSize(), mainFont, MainColor);
                case State.ChoiseFinalTheme:
                case State.ShowFinalThemes:
                    return GenerateFinal();
            }

            return null;
        }

        private void OnRectOver(int x, int y)
        {
            if (state != State.ChoiseQuestion && state != State.ChoiseFinalTheme)
            {
                return;
            }

            var find = GetRectUnderMouse(x, y);

            if (find == null)
            {
                if (nowRectUnder != null)
                {
                    nowRectUnder = null;
                    nextState = state;
                    OnEndAct();
                }
                return;
            }

            if (!find.Equals(nowRectUnder))
            {
                nowRectUnder = find;
                nextState = state;
                OnEndAct();
            }

        }

        private int CalcUsedQuestion()
        {
            int count = 0;
            var round = package.GetRound(nowRound);

            for (int i = 0; i < round.CountThemes; i++)
            {
                var theme = round.GetTheme(i);
                for (int j = 0; j < theme.CountQuestions; j++)
                {
                    if (theme.GetQuestion(j).IsUsed)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private void OnRectClick(Point location, MouseButtons button)
        {
            if (state != State.ChoiseQuestion && state != State.ChoiseFinalTheme)
            {
                return;
            }

            var lastClickedRect = GetRectUnderMouse(location.X, location.Y);

            if (lastClickedRect == null)
            {
                return;
            }

            if (!isServer)
            {
                canChoise = false;

                if (state == State.ChoiseQuestion)
                {
                    nextState = State.ShowMain;
                    OnEndAct();

                    ClientSendToServer(
                        MessageType.ChoiseQuestion,
                        new Dictionary<string, object> {
                            {"round", nowRound },
                            {"theme", lastClickedRect.ThemeId },
                            {"question", lastClickedRect.QuestionId}
                        });
                }
                else
                {
                    nextState = State.ShowFinalThemes;

                    ClientSendToServer(
                        MessageType.FinalKickTheme,
                        new Dictionary<string, object> {
                            {"round", nowRound },
                            {"theme", lastClickedRect.ThemeId }
                        });

                }
            }
            else
            {
                if (button == MouseButtons.Left)
                {
                    if (state == State.ChoiseQuestion)
                    {
                        userChoiseToken = "";

                        ServerBroadcastMessage(
                            MessageType.ShowQuestion,
                            new Dictionary<string, object> {
                                {"round", nowRound },
                                {"theme", lastClickedRect.ThemeId },
                                {"question", lastClickedRect.QuestionId }
                            });

                        ShowQuestion(nowRound, lastClickedRect.ThemeId, lastClickedRect.QuestionId);
                    }
                    else
                    {
                        userChoiseToken = "";

                        ServerBroadcastMessage(
                            MessageType.FinalKickTheme,
                            new Dictionary<string, object> {
                                {"round", nowRound },
                                {"theme", lastClickedRect.ThemeId },
                            });
                    }
                }
                else if (button == MouseButtons.Right)
                {
                    gameForm.OnQuestionForDeleteClick(lastClickedRect, location);
                }
            }

            nowRectUnder = null;
        }

        private void ShowQuestion(int roundId, int themeId, int questionId)
        {
            var nowTheme = package.GetRound(roundId).GetTheme(themeId);
            nowQuestion = nowTheme.GetQuestion(questionId);

            if (nowQuestion.IsBagcat)
            {
                nextState = State.Bagcat;
                if (isServer)
                {
                    if (TryGetUser(userChoiseToken, out User user) == false)
                    {
                        return;
                    }

                    AdminSay("Кому?");
                    if (user != null)
                    {
                        ServerSendToUser(MessageType.CanChoiceUser, user.Token);
                    }
                    gameForm.SetCanChoise(true);
                }
            }
            else if (nowQuestion.IsAuction)
            {
                nextState = State.Auction;
                if (isServer)
                {
                    lock (users)
                    {
                        if (TryGetUser(userChoiseToken, out User user) == false)
                        {
                            user = users[new Random().Next(users.Count)];
                        }

                        foreach (var checkUser in users)
                        {
                            checkUser.SetRate(0);
                        }

                        CreateQueueByMoney(user.Token);

                        ServerSendToUser(
                            MessageType.AuctionChoice,
                            user.Token,
                            new Dictionary<string, object> {
                                {"minValue", nowQuestion.Cost},
                                {"maxValue", user.Money },
                                {"canPass", false },
                                {"canAllIn", user.Money >= nowQuestion.Cost },
                                {"canSet", true },
                            });
                    }
                }
            }
            else
            {
                if (isServer)
                {
                    if (isFinal)
                    {
                        AdminSay("Отвечайте");
                    }
                    else
                    {
                        var adminText = nowTheme.Name + " " + nowQuestion.Cost;
                        if (nowQuestion.IsNoRisk)
                        {
                            adminText += "\nВопрос без риска.";
                        }

                        AdminSay(adminText);
                    }
                }
                nextState = State.ShowQuestion;
            }

            nowQuestion.SetEnd();
            endQCount++;
            nowQuestionScenario = 0;
            OnEndAct();
        }

        private ChoiceRect GetRectUnderMouse(int x, int y)
        {
            return choiceRects.Find((rect) => rect.IsInside(x, y));
        }

        private bool StartStop(bool forse)
        {
            lock (users)
            {
                if (!isGameStarted)
                {
                    if (!isAllready && !forse)
                        return true;

                    ServerBroadcastMessage(MessageType.StartGame, new Dictionary<string, object> { { "round", nowRound } });

                    isGameStarted = true;
                    isGamePaused = false;
                    nextState = State.Start;
                    OnEndAct();
                }
                else
                {
                    if (!isGamePaused)
                    {
                        gameForm.Pause();
                        isGamePaused = true;

                        ServerBroadcastMessage(MessageType.SetPause);
                    }
                    else
                    {
                        gameForm.Start();
                        isGamePaused = false;

                        ServerBroadcastMessage(MessageType.StartGame, new Dictionary<string, object> { { "round", nowRound } });
                    }
                }
            }
            return isGamePaused;
        }

        private void ServerSendToUser(MessageType command, string token, Dictionary<string, object> data = default)
        {
            var message = new Message<MessageType>(server.MyToken, token)
                        .SetCommand(command);

            if (data != default)
            {
                foreach (var d in data)
                {
                    message.Add(d.Key, d.Value);
                }
            }

            server.SendMessage(message);
        }

        private void ServerBroadcastMessage(MessageType command, Dictionary<string, object> data = default)
        {
            lock (users)
            {
                foreach (var user in users)
                {
                    ServerSendToUser(command, user.Token, data);
                }
            }
        }

        private void ClientSendToServer(MessageType command, Dictionary<string, object> data = default)
        {
            var message = new Message<MessageType>(client.MyToken, client.ServerToken)
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

        private void OnChoiseUser(string token)
        {
            lock (users)
            {
                if (TryGetUser(token, out User user) == false)
                {
                    return;
                }

                if (isServer)
                {
                    gameForm.SetCanChoise(false);

                    if (state != State.Bagcat)
                    {
                        userChoiseToken = token;
                        AdminSay(user.Name + " выбирайте вопрос");
                        ServerSendToUser(MessageType.ChoiseQuestion, user.Token);
                    }
                    else
                    {
                        AdminSay(user.Name + ", вопрос для вас");

                        userChoiseToken = user.Token;
                        nextState = State.ShowQuestion;

                        ServerSendToUser(MessageType.StartAutoAnswer, user.Token);
                        ServerBroadcastMessage(MessageType.ForceShowQuestion);

                        OnEndAct();
                    }
                }
                else
                {
                    if (token.Equals(client.MyToken) == false)
                    {
                        gameForm.SetCanChoise(false);
                        ClientSendToServer(MessageType.UserClickUser, new Dictionary<string, object> { { "token", token } });
                    }
                }

            }

        }

        private void AdminSay(string data)
        {
            lock (users)
            {
                if (isServer)
                {
                    gameForm.SetAdminSay(data);
                    ServerBroadcastMessage(MessageType.AdminSay, new Dictionary<string, object> { { "text", data } });
                }
            }
        }

        private void OnTryAnswerClick()
        {
            if (!isServer)
            {
                ClientSendToServer(MessageType.TryAnswer);
            }
        }

        private void OnUserAnswer(AnswerType type)
        {
            lock (users)
            {
                if (!isServer) { return; }

                if (TryGetUser(userAnsToken, out User findUser) == false)
                {
                    return;
                }

                if (type != AnswerType.Fail)
                {
                    AdminSay("верно!");
                    isCanAnswer = false;

                    if (!isFinal)
                    {
                        nowAnswerScenario = 0;
                        var count = nowQuestion.IsNormal ? nowQuestion.Cost : nowQuestion.IsAuction ? findUser.Rate : nowQuestion.SpecialCost;
                        findUser.Money += (type == AnswerType.Full) ? count : count / 2;
                        UpdateUser(findUser);
                        nextState = State.ShowAnswer;
                    }
                    else
                    {
                        findUser.Money += findUser.Rate;
                        UpdateUser(findUser);
                        userTokenQueue.Remove(findUser.Token);
                    }
                    userChoiseToken = userAnsToken;
                    OnEndAct();
                }
                else
                {
                    if (isFinal == false)
                    {
                        switch (nowQuestion.questionType)
                        {
                            case Question.QuestionType.Normal:
                                findUser.Money -= nowQuestion.Cost;
                                break;
                            case Question.QuestionType.Bagcat:
                                findUser.Money -= nowQuestion.SpecialCost;
                                break;
                            case Question.QuestionType.Auction:
                                findUser.Money -= findUser.Rate;
                                break;
                            case Question.QuestionType.NoRisk:
                                findUser.Money -= 0;
                                break;
                        }

                        UpdateUser(findUser);

                        gameForm.Start();
                        AdminSay("нет");

                        if (nowQuestion.IsNormal == false)
                        {
                            nextState = State.ShowAnswer;
                            OnEndAct();
                        }
                        else
                        {
                            gameForm.ShowAnsMenu(false);
                            ServerBroadcastMessage(MessageType.StartGame, new Dictionary<string, object> { { "round", nowRound } });
                            userAnsToken = "";
                        }
                    }
                    else
                    {
                        AdminSay("нет");
                        findUser.Money -= findUser.Rate;
                        UpdateUser(findUser);
                        userTokenQueue.Remove(findUser.Token);
                        OnEndAct();
                    }
                }
            }
        }

        private void UpdateUser(Data.User user)
        {
            lock (users)
            {
                gameForm.UpdateMoney(user.Token, user.Money);

                ServerBroadcastMessage(
                    MessageType.UpdateMoney,
                    new Dictionary<string, object> {
                        {"token", user.Token },
                        {"money", user.Money }
                    });
            }
        }

        public void Kick(string token)
        {
            lock (users)
            {
                if (TryGetUser(token, out User user) == false)
                {
                    return;
                }
                Kick(user);
            }
        }

        private void Kick(Data.User user)
        {
            lock (users)
            {
                gameForm.AddToChat(user.Name + " leave");
                gameForm.AddToChat("last money: " + user.Money);

                ServerBroadcastMessage(MessageType.AddToChat, new Dictionary<string, object> { { "data", user.Name + " leave" } });

                gameForm.Kick(user.Token);
                users.Remove(user);

                ServerBroadcastMessage(MessageType.Kick, new Dictionary<string, object> { { "token", user.Token } });

                userTokenQueue.Remove(user.Token);
            }
        }

        private void ForseShowMain()
        {
            gameForm.SetCanChoise(false);

            ServerBroadcastMessage(MessageType.ForseShowMain, new Dictionary<string, object> { { "round", nowRound } });

            gameForm.FinishMedia(false);

            nowQuestion = null;
            userAnsToken = "";
            autoAnswer = false;

            state = State.ShowAnswer;
            nextState = package.GetRound(nowRound).IsFinal ? State.ChoiseFinalTheme : State.ShowMain;
            OnEndAct();
        }

        private Dictionary<string, object> CurrentQuestionMessageData()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "round", nowRound }
            };

            int count = 0;

            for (int roundId = 0; roundId < package.CountRounds; roundId++)
            {
                var round = package.GetRound(roundId);
                for (int themeId = 0; themeId < round.CountThemes; themeId++)
                {
                    var theme = round.GetTheme(themeId);
                    for (int questionId = 0; questionId < theme.CountQuestions; questionId++)
                    {
                        if (theme.GetQuestion(questionId).IsUsed)
                        {
                            dict.Add(count.ToString(), roundId + " " + themeId + " " + questionId);
                            count++;
                        }
                    }
                }
            }

            dict.Add("count", count.ToString());
            return dict;
        }

        private bool TryGetUser(string token, out Data.User user)
        {
            lock (users)
            {
                foreach (var uData in users)
                {
                    if (uData.Token.Equals(token))
                    {
                        user = uData;
                        return true;
                    }
                }
            }

            user = default;
            return false;
        }

        private void StartConfigMoney(string token)
        {
            if (TryGetUser(token, out User user) == false)
            {
                return;
            }
            gameForm.StartConfigUser(user.Name, user.Money);
        }

        private void AcceptConfigUser(string token, int money)
        {
            if (TryGetUser(token, out User user) == false)
            {
                return;
            }

            lock (users)
            {
                user.Money = money;
                UpdateUser(user);
            }

        }

        private void SetChoiceUser(string token)
        {
            if (TryGetUser(token, out User user) == false)
            {
                return;
            }

            userChoiseToken = token;

            ServerSendToUser(MessageType.ChoiseQuestion, user.Token);
            AdminSay(user.Name + " выбирайте вопрос");
            ForseShowMain();
        }

        private void OnAuctionMove(int rate)
        {

            if (isServer)
                return;

            lock (users)
            {
                if (rate == -1)
                {
                    rate = users[0].Money;
                }

                ClientSendToServer(
                    state == State.FinalRate ?
                        MessageTypes.MessageType.FinalRate :
                        MessageTypes.MessageType.AuctionChoice,
                    new Dictionary<string, object> { { "rate", rate } });
            }
        }

        private void NextUserQueue()
        {
            lock (users)
            {
                userTokenQueue.Enqueue(userTokenQueue.Dequeue());
                userTokenQueue.RemoveAll((token) => TryGetUser(token, out User user) && user.IsPass);
            }
        }

        private void CreateQueueByMoney(string first = "")
        {
            lock (users)
            {
                userTokenQueue.Clear();

                if (!first.Equals(""))
                {
                    userTokenQueue.Enqueue(first);
                }

                for (int j = 0; j < users.Count; j++)
                {
                    int maxMoney = 0;
                    int maxId = 0;
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (!userTokenQueue.Contains(users[i].Token))
                        {
                            if (users[i].Money > maxMoney)
                            {
                                maxMoney = users[i].Money;
                                maxId = i;
                            }
                        }
                    }
                    if (maxMoney != 0)
                    {
                        userTokenQueue.Enqueue(users[maxId].Token);
                    }

                }
            }
        }

        private void FinalAnswer(string data)
        {
            ClientSendToServer(MessageType.FinalAnswer, new Dictionary<string, object> { { "data", data } });
        }

        private void EndGame(string data)
        {
            nowShowText = data;

            ServerBroadcastMessage(
                MessageType.AddTextToMainScreen,
                new Dictionary<string, object> {
                    {"text", data },
                    {"show", false}
                });

            ServerBroadcastMessage(MessageType.EndGame);
        }

        private void NextUserMove()
        {
            lock (users)
            {
                if (state == State.ChoiseFinalTheme)
                {
                    NextUserQueue();

                    if (TryGetUser(userTokenQueue.Peek(), out User user) == false)
                    {
                        return;
                    }
                    ServerSendToUser(MessageType.ChoiseTheme, user.Token);
                    AdminSay(user.Name + " уберите 1 тему");
                }
                else if (state == State.FinalRate)
                {
                    foreach (var user in users)
                    {
                        if (user.Rate <= 0)
                        {
                            user.SetRate(-1);
                        }
                    }

                    nowQuestionScenario = 0;

                    var round = package.GetRound(nowRound);

                    for (int id = 0; id < round.CountThemes; id++)
                    {
                        if (!round.GetTheme(id).IsUsed)
                        {
                            nowQuestion = round.GetTheme(id).GetQuestion(0);

                            ServerBroadcastMessage(
                                MessageType.ShowQuestion,
                                new Dictionary<string, object> {
                                    {"round", nowRound },
                                    {"theme", id },
                                    {"question", 0}
                                });

                            ShowQuestion(nowRound, id, 0);
                            break;
                        }
                    }
                }
                else
                {
                    ForseShowMain();
                }
            }

        }

        private void SkipQuestion(ChoiceRect questrion, SkipType type)
        {
            switch (type)
            {
                case SkipType.Theme:
                    package.GetRound(nowRound).GetTheme(questrion.ThemeId).SetEnd();
                    break;
                case SkipType.Question:
                    package.GetRound(nowRound).GetTheme(questrion.ThemeId).GetQuestion(questrion.QuestionId).SetEnd();
                    break;
                case SkipType.Round:
                    SkipRound();
                    break;
            }

            endQCount = CalcUsedQuestion();

            ServerBroadcastMessage(MessageType.SendUsedQuestion, CurrentQuestionMessageData());
            ForseShowMain();
        }
    }
}
