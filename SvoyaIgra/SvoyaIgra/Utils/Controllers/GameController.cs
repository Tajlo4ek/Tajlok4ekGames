using System;
using System.Collections.Generic;
using System.Drawing;
using ClientServer;
using DataStore;
using SvoyaIgra.Data;

namespace SvoyaIgra.Utils.Controllers
{
    public class GameController
    {
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

        private const int timeToCheck = 10000;

        private const int timeWaitAnswerFinal = 60;
        private const int timeWaitAnswerNormal = 8;

        private bool isFinal;

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

        private readonly string adminName;

        private bool isCanAnswer;
        private bool autoAnswer;

        private readonly System.Timers.Timer checkTimer;

        private bool canChoise;

        private string nowShowText;

        private readonly MyQueue<string> userTokenQueue;

        public GameController(bool isServer, string ip, string name, string imgUrl, string packPath = null)
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

            gameForm.Show();

            users = new List<Data.User>();

            this.isServer = isServer;
            isGamePaused = true;
            userAnsToken = "";
            userChoiseToken = "";
            nowShowText = "";

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

                loader.AddFile(loader.WorkDirectory + @"\" + DataStore.Utils.PackUtils.PackManager.BasePackName, DataStore.Utils.PackUtils.PackManager.BasePackName);

                loader.LoadImg(imgUrl, Data.User.AdminImgName);
                gameForm.SetAdminImage(loader.GetFilePath(Data.User.AdminImgName));


                server = new ClientServer.Server<MessageTypes.MessageType>(ip, GetMessageToSend, OnServerError);
                server.onGetMessage += OnGetMessage;
                server.SetWorkPath(loader.WorkDirectory);
                server.GetFilePath += GetFilePath;

                this.adminName = name;

                gameForm.SetAdminData(name);

                checkTimer = new System.Timers.Timer();
                checkTimer.Elapsed += CheckUsers;
                checkTimer.Interval = timeToCheck;
                checkTimer.Start();

                server.Start();
            }
            else
            {
                loader = new DataStore.Utils.PackUtils.FileManager();
                loader.LoadImg(imgUrl, Data.User.MyImgName);

                client = new ClientServer.Client<MessageTypes.MessageType>(ip, name, OnClientError);
                client.SetWorkPath(loader.WorkDirectory);
                client.GetMessageForServer += GetMessageToSend;
                client.OnGetMessage += OnGetMessage;
                client.OnFileLoadProcess += gameForm.AddToChat;
                client.GetFilePath += GetFilePath;

                client.Start();
            }

            nowRound = 0;
            nextState = State.WaitPlayer;

            canChoise = false;
            isFinal = false;
        }

        private void CheckUsers(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (users)
            {
                for (int userId = users.Count - 1; userId >= 0; userId--)
                {
                    if (!users[userId].IsActive)
                    {
                        Kick(users[userId]);
                    }
                }
            }
        }

        private void OnClientError(Exception ex)
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

                var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                    .SetCommand(MessageTypes.MessageType.Kick)
                    .Add("token", token);

                AddMessageForAll(message);
            }
        }

        private void OnClose()
        {
            loader.Dispose();
            checkTimer?.Stop();
            checkTimer?.Dispose();

            client?.Stop();
            server?.Stop();
        }

        private void FileLoad(MessageTypes.FileType type, string filePath)
        {
            switch (type)
            {
                case MessageTypes.FileType.Pack:
                    {
                        if (!isServer)
                        {
                            package = loader.LoadPack(filePath);

                            var message = new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(client.Token)
                                .SetCommand(MessageTypes.MessageType.Ready);

                            users[0].AddDataToSend(message);
                        }
                    }
                    break;

                case MessageTypes.FileType.AdminImg:
                    {
                        if (!isServer)
                        {
                            loader.AddFile(filePath, Data.User.AdminImgName);
                            gameForm.SetAdminImage(filePath);
                        }
                    }
                    break;

                case MessageTypes.FileType.UserImg:
                    {
                        var imgName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);
                        loader.AddFile(filePath, imgName);

                        var ind = imgName.LastIndexOf(".");
                        if (ind != -1)
                        {
                            imgName = imgName.Substring(0, ind);
                        }

                        gameForm.AddUserImage(imgName, filePath);

                        if (isServer)
                        {
                            var message = new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                .SetCommand(MessageTypes.MessageType.AvailableImage)
                                .Add("imageName", imgName);

                            AddMessageForAll(message);

                            GetUser(imgName)?.SetImgAvailable(true);
                        }

                    }
                    break;
            }
        }

        private void OnGetUserMassage(ClientServer.Message<MessageTypes.MessageType> message)
        {
            string messageToken = message.Token;

            switch (message.Command)
            {
                case MessageTypes.MessageType.AddToChat:
                    {
                        if (isServer)
                        {
                            AddMessageForAll(message);
                        }

                        gameForm.AddToChat(message.GetData("data"));
                    }
                    break;

                case MessageTypes.MessageType.SendUserData:
                    {
                        var userToken = message.GetData("token");

                        if (users.Find((user) => user.Token.Equals(userToken)) != null)
                            break;

                        var userName = message.GetData("name");
                        var userMoney = message.GetData("money");
                        if (userMoney.Equals(""))
                        {
                            userMoney = "0";
                        }

                        var userSend = new Data.User(userToken, userName, isServer);
                        users.Add(userSend);

                        gameForm.AddUserData(userName, userMoney, userToken);
                        if (userToken.Equals(client.Token))
                        {
                            loader.RenameFile(Data.User.MyImgName, userToken);
                            gameForm.AddUserImage(userToken, loader.GetFilePath(userToken));

                            var imageMessage = new ClientServer.Message<MessageTypes.MessageType>(
                                ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.SendFile)
                                    .SetToken(client.Token)
                                    .Add("fileName", client.Token);

                            users[0].AddDataToSend(imageMessage);
                        }

                    }
                    break;

                case MessageTypes.MessageType.StartGame:
                    {

                        if (!isGameStarted)
                        {
                            nowRound = int.Parse(message.GetData("round"));

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
                        users.Find((user) => user.Token.Equals(messageToken))?.SetReady();

                        isAllready = true;
                        users.ForEach((user) => isAllready &= user.IsReady);

                    }
                    break;

                case MessageTypes.MessageType.ChoiseQuestion:
                    {
                        if (isServer)
                        {
                            int round = int.Parse(message.GetData("round"));
                            int theme = int.Parse(message.GetData("theme"));
                            int question = int.Parse(message.GetData("question"));

                            ShowQuestion(round, theme, question);

                            var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                .SetCommand(MessageTypes.MessageType.ShowQuestion)
                                .Add("round", message.GetData("round"))
                                .Add("theme", message.GetData("theme"))
                                .Add("question", message.GetData("question"));

                            AddMessageForAll(sendMessage);

                            userChoiseToken = "";
                            for (int userId = 0; userId < users.Count; userId++)
                            {
                                if (users[userId].Token.Equals(message.Token))
                                {
                                    userChoiseToken = users[userId].Token;
                                }
                            }

                        }
                        else
                        {
                            canChoise = message.GetData("token").Equals(client.Token);
                            nextState = State.ShowMain;
                            OnEndAct();
                        }
                    }
                    break;

                case MessageTypes.MessageType.ShowQuestion:
                    {
                        int round = int.Parse(message.GetData("round"));
                        int theme = int.Parse(message.GetData("theme"));
                        int question = int.Parse(message.GetData("question"));

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
                            canChoise = message.GetData("token").Equals(client.Token);
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
                            Data.User findUser = GetUser(messageToken);

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
                                var messageSend = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.SetPause);

                                AddMessageForAll(messageSend);

                                gameForm.Pause();
                                gameForm.ShowAnsMenu(true);

                                findUser.SetCanAnswer(false);

                                AdminSay(findUser.Name + " отвечайте");
                            }
                            else if (state == State.ShowQuestion && nowQuestion.IsNormal)
                            {
                                gameForm.AddToChat(findUser.Name + " фальстарт");

                                var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.AddToChat)
                                    .Add("data", findUser.Name + " фальстарт");

                                findUser.SetCanAnswer(false);
                                userAnsToken = "";

                                AddMessageForAll(sendMessage);
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
                        gameForm.SetAdminSay(message.GetData("text"));
                    }
                    break;

                case MessageTypes.MessageType.UpdateMoney:
                    {
                        var userToken = message.GetData("token");

                        var user = GetUser(userToken);
                        if (user != null)
                        {
                            var userMoney = message.GetData("money");
                            user.Money = int.Parse(userMoney);
                            gameForm.UpdateMoney(userToken, userMoney);
                        }
                    }
                    break;

                case MessageTypes.MessageType.SendAdminData:
                    {
                        gameForm.SetAdminData(message.GetData("name"));
                    }
                    break;

                case MessageTypes.MessageType.Kick:
                    {
                        var token = message.GetData("token");
                        if (!isServer)
                        {
                            if (client.Token.Equals(token))
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
                            nowRound = int.Parse(message.GetData("round"));
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
                        int count = int.Parse(message.GetData("count"));
                        nowRound = int.Parse(message.GetData("round"));

                        for (int i = 0; i < count; i++)
                        {
                            var str = message.GetData(i.ToString()).Split(' ');

                            var roundId = int.Parse(str[0]);
                            var themeId = int.Parse(str[1]);
                            var questionId = int.Parse(str[2]);

                            package.GetRound(roundId).GetTheme(themeId).GetQuestion(questionId).SetEnd();
                        }

                        endQCount = 0;

                        var round = package.GetRound(nowRound);

                        for (int i = 0; i < round.CountThemes; i++)
                        {
                            var theme = round.GetTheme(i);
                            for (int j = 0; j < theme.CountQuestions; j++)
                            {
                                if (theme.GetQuestion(j).IsUsed)
                                {
                                    endQCount++;
                                }
                            }
                        }

                        OnEndAct();
                    }
                    break;

                case MessageTypes.MessageType.StartCanAnswer:
                    {
                        if (!isServer)
                        {
                            if (nowQuestion.IsNormal)
                            {
                                int time = int.Parse(message.GetData("timeSec"));
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

                case MessageTypes.MessageType.AvailableImage:
                    {
                        var imgName = message.GetData("imageName");

                        if (!client.Token.Equals(imgName))
                        {
                            var sendMessage = new ClientServer.Message<MessageTypes.MessageType>(
                                ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.GetFile)
                                    .SetToken(client.Token)
                                    .Add("fileName", imgName);

                            users[0].AddDataToSend(sendMessage);
                        }
                    }
                    break;

                case MessageTypes.MessageType.UserClickUser:
                    {
                        string token = message.GetData("token");

                        if (isServer)
                        {
                            gameForm.SetCanChoise(false);

                            var user = GetUser(token);

                            foreach (var userCheck in users)
                            {
                                userCheck.SetCanAnswer(false);
                            }
                            user.SetCanAnswer(true);

                            AdminSay(user.Name + ", вопрос для вас");

                            user.AddDataToSend(
                                new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                .SetCommand(MessageTypes.MessageType.StartAutoAnswer));

                            AddMessageForAll(
                                new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                .SetCommand(MessageTypes.MessageType.ForceShowQuestion));

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
                            var user = GetUser(messageToken);

                            int rate = 0;

                            if (user != null)
                            {
                                rate = int.Parse(message.GetData("rate"));
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
                                user = GetUser(userTokenQueue.Dequeue());

                                foreach (var checkUser in users)
                                {
                                    checkUser.SetCanAnswer(false);
                                }
                                user.SetCanAnswer(true);

                                user.AddDataToSend(
                                    new ClientServer.Message<MessageTypes.MessageType>()
                                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                        .SetCommand(MessageTypes.MessageType.StartAutoAnswer));

                                AddMessageForAll(
                                    new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.ForceShowQuestion));

                                AdminSay(user.Name + " вопрос для вас");

                                userChoiseToken = user.Token;

                                nextState = State.ShowQuestion;
                                OnEndAct();
                            }
                            else
                            {
                                var nextUser = GetUser(userTokenQueue.Peek());

                                var minValue = maxRate + AuctionStep;
                                var maxValue = nextUser.Money;

                                if (minValue > nextUser.Money)
                                {
                                    minValue = nextUser.Money;
                                    maxValue = nextUser.Money;
                                    isSomeAllIn = true;
                                }

                                var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(nextUser.Token)
                                    .SetCommand(MessageTypes.MessageType.AuctionChoice)
                                    .Add("minValue", minValue.ToString())
                                    .Add("maxValue", maxValue.ToString())
                                    .Add("canPass", true.ToString())
                                    .Add("canAllIn", true.ToString())
                                    .Add("canSet", (!isSomeAllIn).ToString());

                                nextUser.AddDataToSend(sendMessage);
                            }

                        }
                        else
                        {
                            var minValue = int.Parse(message.GetData("minValue"));
                            var maxValue = int.Parse(message.GetData("maxValue"));

                            var canPass = bool.Parse(message.GetData("canPass"));
                            var canAllIn = bool.Parse(message.GetData("canAllIn"));
                            var canSet = bool.Parse(message.GetData("canSet"));

                            gameForm.ShowAuction(minValue, maxValue, canPass, canAllIn, canSet);
                        }
                    }
                    break;

                case MessageTypes.MessageType.AddTextToMainScreen:
                    {
                        if (!isServer)
                        {
                            nowShowText = message.GetData("text");
                            bool show = bool.Parse(message.GetData("show"));
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

                case MessageTypes.MessageType.KickTheme:
                    {
                        if (!(messageToken.Equals(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            || messageToken.Equals(userTokenQueue.Peek())))
                        {
                            break;
                        }

                        int roundId = int.Parse(message.GetData("round"));
                        int themeId = int.Parse(message.GetData("theme"));

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

                                var messageSend = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.AddTextToMainScreen)
                                    .Add("text", nowShowText)
                                    .Add("show", true.ToString());
                                AddMessageForAll(messageSend);

                                messageSend = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.FinalRate);

                                foreach (var checkUser in users)
                                {
                                    if (checkUser.Money > 0)
                                    {
                                        checkUser.AddDataToSend(messageSend);
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
                                var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                   .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                   .SetCommand(MessageTypes.MessageType.KickTheme)
                                   .Add("round", roundId.ToString())
                                   .Add("theme", themeId.ToString());
                                AddMessageForAll(sendMessage);

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
                            var user = GetUser(messageToken);
                            if (user != null)
                            {
                                int rate = int.Parse(message.GetData("rate"));
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
                        var user = GetUser(messageToken);
                        user?.SetFinalAns(message.GetData("data"));
                    }
                    break;
            }
        }

        private void OnGetMessage(ClientServer.Message<MessageTypes.MessageType> message)
        {
            if (state == State.End)
            {
                return;
            }


            var messageToken = message.Token;

            lock (users)
            {
                if (isServer)
                {
                    users.ForEach((user) => { if (user.Token.Equals(messageToken)) { user.Update(); } });
                }

                switch (message.MessageType)
                {
                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.FileRecived:
                        {
                            string fileName = message.GetData("fileName");
                            string filePath = message.GetData("filePath");

                            if (fileName == DataStore.Utils.PackUtils.PackManager.BasePackName)
                            {
                                FileLoad(MessageTypes.FileType.Pack, filePath);
                            }
                            else if (fileName == User.AdminImgName)
                            {
                                FileLoad(MessageTypes.FileType.AdminImg, filePath);
                            }
                            else
                            {
                                FileLoad(MessageTypes.FileType.UserImg, filePath);
                            }
                        }
                        break;

                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.SendReg:
                        {
                            if (isServer == false)
                            {
                                var userDataMessage = new Message<MessageTypes.MessageType>(
                                    Message<MessageTypes.MessageType>.GeneralMessageType.User)
                                    .SetToken(Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.SendUserData)
                                    .Add("name", message.GetData("name"))
                                    .Add("token", message.GetData("token"));
                                OnGetMessage(userDataMessage);
                            }
                        }
                        break;

                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.GetReg:
                        {
                            if (isServer)
                            {
                                var userName = message.GetData("name");

                                var connectedUser = new Data.User(messageToken, userName, isServer);

                                users.Add(connectedUser);
                                gameForm.AddToChat(userName + " connect");

                                var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.AddToChat)
                                    .Add("data", userName + " connect");

                                AddMessageForAll(sendMessage);


                                var sendUserData = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.SendUserData)
                                    .Add("token", connectedUser.Token)
                                    .Add("name", connectedUser.Name)
                                    .Add("money", connectedUser.Money.ToString());
                                AddMessageForAll(sendUserData);

                                foreach (var userFrom in users)
                                {
                                    sendUserData = new ClientServer.Message<MessageTypes.MessageType>()
                                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                        .SetCommand(MessageTypes.MessageType.SendUserData)
                                        .Add("token", userFrom.Token)
                                        .Add("name", userFrom.Name)
                                        .Add("money", userFrom.Money.ToString());
                                    connectedUser.AddDataToSend(sendUserData);

                                    if (userFrom.IsImgAvailable)
                                    {
                                        var imgAvailableMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                            .SetCommand(MessageTypes.MessageType.AvailableImage)
                                            .Add("imageName", userFrom.Token);

                                        connectedUser.AddDataToSend(imgAvailableMessage);
                                    }

                                }

                                gameForm.AddUserData(userName, "0", messageToken);

                                var adminData = new ClientServer.Message<MessageTypes.MessageType>()
                                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                    .SetCommand(MessageTypes.MessageType.SendAdminData)
                                    .Add("name", adminName);

                                connectedUser.AddDataToSend(adminData);


                                if (isGameStarted)
                                {
                                    SendRoundData(connectedUser);

                                    var message1 = new ClientServer.Message<MessageTypes.MessageType>()
                                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                        .SetCommand(MessageTypes.MessageType.ForseShowMain)
                                        .Add("round", nowRound.ToString());

                                    connectedUser.AddDataToSend(message1);
                                }

                            }
                        }
                        break;

                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.SendFile:
                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.GetFile:
                        {
                            if (isServer)
                            {
                                users.Find((x) => x.Token.Equals(messageToken))?.StartLoadFile();
                            }
                        }
                        break;

                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.FileSended:
                        {
                            if (isServer)
                            {
                                users.Find((x) => x.Token.Equals(messageToken)).FileLoaded();
                            }
                        }
                        break;

                    case ClientServer.Message<MessageTypes.MessageType>.GeneralMessageType.User:
                        {
                            OnGetUserMassage(message);
                        }
                        break;

                }
            }
        }

        private ClientServer.Message<MessageTypes.MessageType> GetMessageToSend(string token)
        {
            lock (users)
            {
                foreach (var user in users)
                {
                    if (user.Token.Equals(token))
                    {
                        return user.GetMessage();
                    }
                }
            }

            return new ClientServer.Message<MessageTypes.MessageType>()
                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                .SetCommand(MessageTypes.MessageType.Kick)
                .Add("token", token);
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
                            nextState = State.ShowRoundName;
                            endQCount = 0;
                            nowRound++;

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

                                    var trueMessage = new ClientServer.Message<MessageTypes.MessageType>()
                                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                        .SetCommand(isFinal ?
                                                MessageTypes.MessageType.ChoiseTheme :
                                                MessageTypes.MessageType.ChoiseQuestion)
                                        .Add("token", userChoiseToken);

                                    AddMessageForAll(trueMessage);

                                    var findUser = GetUser(userChoiseToken);

                                    if (findUser != null)
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
                        var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(client.Token)
                            .SetCommand(MessageTypes.MessageType.Ready);
                        users[0].AddDataToSend(message);
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

                    var user = GetUser(userAnsToken);
                    nowShowText = user.Name;
                    nowShowText += "\nОтвет: " + user.FinalAns;
                    nowShowText += "\nСтавка: " + user.Rate;
                    gameForm.ShowImageBackground(GetCurrentImage(), -1);

                    var messageSend = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.AddTextToMainScreen)
                            .Add("text", nowShowText)
                            .Add("show", true.ToString());
                    AddMessageForAll(messageSend);

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

                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                        .SetCommand(MessageTypes.MessageType.StartCanAnswer)
                        .Add("timeSec", isFinal ? timeWaitAnswerFinal.ToString() : timeWaitAnswerNormal.ToString());

                    AddMessageForAll(message);

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
                var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                    .SetCommand(MessageTypes.MessageType.ShowAnswer);
                AddMessageForAll(message);
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
                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                        .SetCommand(MessageTypes.MessageType.ForseShowMain)
                        .Add("round", nowRound.ToString());

                    AddMessageForAll(message);

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

        private void OnRectClick(int x, int y)
        {
            if (state != State.ChoiseQuestion && state != State.ChoiseFinalTheme)
            {
                return;
            }

            var find = GetRectUnderMouse(x, y);

            if (find == null)
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

                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(client.Token)
                        .SetCommand(MessageTypes.MessageType.ChoiseQuestion)
                        .Add("round", nowRound.ToString())
                        .Add("theme", find.ThemeId.ToString())
                        .Add("question", find.QuestionId.ToString());

                    users[0].AddDataToSend(message);
                }
                else
                {
                    nextState = State.ShowFinalThemes;

                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(client.Token)
                        .SetCommand(MessageTypes.MessageType.KickTheme)
                        .Add("round", nowRound.ToString())
                        .Add("theme", find.ThemeId.ToString());

                    users[0].AddDataToSend(message);
                }
            }
            else
            {
                if (state == State.ChoiseQuestion)
                {
                    userChoiseToken = "";
                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                        .SetCommand(MessageTypes.MessageType.ShowQuestion)
                        .Add("round", nowRound.ToString())
                        .Add("theme", find.ThemeId.ToString())
                        .Add("question", find.QuestionId.ToString());
                    AddMessageForAll(message);

                    ShowQuestion(nowRound, find.ThemeId, find.QuestionId);
                }
                else
                {
                    userChoiseToken = "";
                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                        .SetCommand(MessageTypes.MessageType.KickTheme)
                        .Add("round", nowRound.ToString())
                        .Add("theme", find.ThemeId.ToString());

                    AddMessageForAll(message);
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
                    var user = GetUser(userChoiseToken);
                    AdminSay("Кому?");
                    if (user != null)
                    {
                        var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.CanChoiceUser);
                        user.AddDataToSend(message);
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
                        var user = GetUser(userChoiseToken) ?? users[new Random().Next(users.Count)];
                        var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.AuctionChoice)
                            .Add("minValue", nowQuestion.Cost.ToString())
                            .Add("maxValue", user.Money.ToString())
                            .Add("canPass", false.ToString())
                            .Add("canAllIn", (user.Money >= nowQuestion.Cost).ToString())
                            .Add("canSet", true.ToString());


                        foreach (var checkUser in users)
                        {
                            checkUser.SetRate(0);
                        }

                        CreateQueueByMoney(user.Token);

                        user.AddDataToSend(message);
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
                            var user = GetUser(userChoiseToken);
                            if (user != null)
                            {
                                adminText += "\nВопрос без риска.";
                            }
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

                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                        .SetCommand(MessageTypes.MessageType.StartGame)
                        .Add("round", nowRound.ToString());

                    AddMessageForAll(message);

                    isGameStarted = true;
                    isGamePaused = false;
                    nextState = State.Start;
                    OnEndAct();
                }
                else
                {
                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken);

                    if (!isGamePaused)
                    {
                        message.SetCommand(MessageTypes.MessageType.SetPause);
                        gameForm.Pause();
                        isGamePaused = true;
                    }
                    else
                    {
                        message.SetCommand(MessageTypes.MessageType.StartGame)
                            .Add("round", nowRound.ToString());
                        gameForm.Start();
                        isGamePaused = false;
                    }

                    AddMessageForAll(message);
                }
            }
            return isGamePaused;
        }

        private void AddMessageForAll(ClientServer.Message<MessageTypes.MessageType> message)
        {
            lock (users)
            {
                foreach (var user in users)
                {
                    user.AddDataToSend(message);
                }
            }
        }

        private void OnChoiseUser(string token)
        {
            lock (users)
            {
                var user = GetUser(token);

                if (user == null)
                {
                    return;
                }

                if (isServer)
                {
                    gameForm.SetCanChoise(false);

                    if (state != State.Bagcat)
                    {
                        var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.ChoiseQuestion)
                            .Add("token", token);

                        AddMessageForAll(message);
                        userChoiseToken = token;
                        AdminSay(user.Name + " выбирайте вопрос");
                    }
                    else
                    {
                        AdminSay(user.Name + ", вопрос для вас");
                        user.AddDataToSend(
                            new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.StartAutoAnswer));

                        AddMessageForAll(
                            new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.ForceShowQuestion));

                        userChoiseToken = user.Token;

                        nextState = State.ShowQuestion;
                        OnEndAct();
                    }
                }
                else
                {
                    if (!token.Equals(client.Token))
                    {
                        gameForm.SetCanChoise(false);

                        var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(client.Token)
                            .SetCommand(MessageTypes.MessageType.UserClickUser)
                            .Add("token", token);
                        AddMessageForAll(message);
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

                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                            .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                            .SetCommand(MessageTypes.MessageType.AdminSay)
                            .Add("text", data);

                    AddMessageForAll(message);
                }
            }
        }

        private void OnTryAnswerClick()
        {
            if (!isServer)
            {
                lock (users)
                {
                    var message = new ClientServer.Message<MessageTypes.MessageType>()
                        .SetToken(client.Token)
                        .SetCommand(MessageTypes.MessageType.TryAnswer);

                    users[0].AddDataToSend(message);
                }
            }
        }

        private void OnUserAnswer(AnswerType type)
        {
            lock (users)
            {
                if (userAnsToken.Equals("") || !isServer)
                    return;

                var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken);

                var findUser = GetUser(userAnsToken);


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
                    if (!isFinal)
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
                        message.SetCommand(MessageTypes.MessageType.StartGame)
                            .Add("round", nowRound.ToString());

                        if (!nowQuestion.IsNormal)
                        {
                            nextState = State.ShowAnswer;
                            OnEndAct();
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

                if (!isFinal)
                {
                    gameForm.ShowAnsMenu(false);
                    AddMessageForAll(message);
                    userAnsToken = "";
                }

            }
        }

        private void UpdateUser(Data.User user)
        {
            lock (users)
            {
                gameForm.UpdateMoney(user.Token, user.Money.ToString());

                var updateMessage = new ClientServer.Message<MessageTypes.MessageType>()
                       .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                       .SetCommand(MessageTypes.MessageType.UpdateMoney)
                       .Add("token", user.Token)
                       .Add("money", user.Money.ToString());

                AddMessageForAll(updateMessage);
            }
        }

        public void Kick(string token)
        {
            lock (users)
            {
                var user = GetUser(token);

                if (user == null)
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
                var adminSayMessage = new ClientServer.Message<MessageTypes.MessageType>()
                          .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                          .SetCommand(MessageTypes.MessageType.AddToChat)
                          .Add("data", user.Name + " leave");

                gameForm.AddToChat(user.Name + " leave");
                gameForm.AddToChat("last money: " + user.Money);

                AddMessageForAll(adminSayMessage);

                var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                    .SetCommand(MessageTypes.MessageType.Kick)
                    .Add("token", user.Token);

                gameForm.Kick(user.Token);

                users.Remove(user);
                AddMessageForAll(message);

                userTokenQueue.Remove(user.Token);
            }
        }

        private void ForseShowMain()
        {
            gameForm.SetCanChoise(false);

            var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                    .SetCommand(MessageTypes.MessageType.ForseShowMain)
                    .Add("round", nowRound.ToString());

            AddMessageForAll(message);
            gameForm.FinishMedia(false);

            nowQuestion = null;
            userAnsToken = "";
            autoAnswer = false;

            state = State.ShowAnswer;
            nextState = package.GetRound(nowRound).IsFinal ? State.ChoiseFinalTheme : State.ShowMain;
            OnEndAct();
        }

        private void SendRoundData(Data.User user)
        {
            var message = new ClientServer.Message<MessageTypes.MessageType>()
                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                .SetCommand(MessageTypes.MessageType.SendUsedQuestion)
                .Add("round", nowRound.ToString());

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
                            message.Add(count.ToString(), roundId + " " + themeId + " " + questionId);
                            count++;
                        }
                    }
                }
            }

            message.Add("count", count.ToString());

            lock (users)
            {
                user.AddDataToSend(message);
            }

        }

        private Data.User GetUser(string token)
        {
            lock (users)
            {
                foreach (var user in users)
                {
                    if (user.Token.Equals(token))
                    {
                        return user;
                    }
                }
                return null;
            }
        }

        private void StartConfigMoney(string token)
        {
            Data.User user = GetUser(token);
            if (user != null)
            {
                gameForm.StartConfigUser(user.Name, user.Money);
            }
        }

        private string GetFilePath(string name)
        {
            return loader.GetFilePath(name);
        }

        private void AcceptConfigUser(string token, int money)
        {
            var user = GetUser(token);
            if (user != null)
            {
                lock (users)
                {
                    user.Money = money;
                    UpdateUser(user);
                }
            }
        }

        private void SetChoiceUser(string token)
        {
            var trueMessage = new ClientServer.Message<MessageTypes.MessageType>()
                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                .SetCommand(MessageTypes.MessageType.ChoiseQuestion)
                .Add("token", token);

            userChoiseToken = token;

            AddMessageForAll(trueMessage);
            AdminSay(GetUser(token).Name + " выбирайте вопрос");
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

                var message = new ClientServer.Message<MessageTypes.MessageType>()
                     .SetToken(client.Token)
                     .SetCommand(state == State.FinalRate ?
                           MessageTypes.MessageType.FinalRate :
                           MessageTypes.MessageType.AuctionChoice)
                     .Add("rate", rate.ToString());

                users[0].AddDataToSend(message);
            }
        }

        private void NextUserQueue()
        {
            lock (users)
            {
                userTokenQueue.Enqueue(userTokenQueue.Dequeue());
                userTokenQueue.RemoveAll((x) => GetUser(x).IsPass);
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
            lock (users)
            {
                var message = new ClientServer.Message<MessageTypes.MessageType>()
                    .SetToken(client.Token)
                    .SetCommand(MessageTypes.MessageType.FinalAnswer)
                    .Add("data", data);

                users[0].AddDataToSend(message);
            }
        }

        private void EndGame(string data)
        {
            nowShowText = data;

            var messageEnd = new ClientServer.Message<MessageTypes.MessageType>()
                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                .SetCommand(MessageTypes.MessageType.AddTextToMainScreen)
                .Add("text", data)
                .Add("show", false.ToString()); ;
            AddMessageForAll(messageEnd);

            var message = new ClientServer.Message<MessageTypes.MessageType>()
                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                .SetCommand(MessageTypes.MessageType.EndGame);
            AddMessageForAll(message);
        }

        private void NextUserMove()
        {
            lock (users)
            {
                if (state == State.ChoiseFinalTheme)
                {
                    NextUserQueue();

                    var sendMessage = new ClientServer.Message<MessageTypes.MessageType>()
                      .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                      .SetCommand(MessageTypes.MessageType.ChoiseTheme)
                      .Add("token", userTokenQueue.Peek());
                    AddMessageForAll(sendMessage);

                    AdminSay(GetUser(userTokenQueue.Peek()).Name + " уберите 1 тему");
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

                            var messageSend = new ClientServer.Message<MessageTypes.MessageType>()
                                .SetToken(ClientServer.Server<MessageTypes.MessageType>.ServerToken)
                                .SetCommand(MessageTypes.MessageType.ShowQuestion)
                                .Add("round", nowRound.ToString())
                                .Add("theme", id.ToString())
                                .Add("question", "0");

                            AddMessageForAll(messageSend);

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

    }
}
