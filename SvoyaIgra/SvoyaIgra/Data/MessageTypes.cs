namespace SvoyaIgra.Data
{
    public class MessageTypes
    {
        public enum MessageType
        {
            None,
            AddToChat,
            SendUserData,
            StartGame,
            PackLoaded,
            Ready,
            ChoiseQuestion,
            ShowQuestion,
            SetPause,
            TryAnswer,
            ShowAnswer,
            AdminSay,
            UpdateMoney,
            SendAdminData,
            Kick,
            ForseShowMain,
            SendUsedQuestion,
            StartCanAnswer,
            CanChoiceUser,
            UserClickUser,
            StartAutoAnswer,
            ForceShowQuestion,
            AuctionChoice,
            AddTextToMainScreen,
            FinalKickTheme,
            ChoiseTheme,
            FinalRate,
            FinalAnswer,
            EndGame,
        }

        public enum FileType
        {
            Pack,
            AdminImg,
            UserImg
        }
    }
}
