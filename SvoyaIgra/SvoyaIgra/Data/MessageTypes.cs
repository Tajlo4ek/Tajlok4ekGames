using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            AvailableImage,
            CanChoiceUser,
            UserClickUser,
            StartAutoAnswer,
            ForceShowQuestion,
            AuctionChoice,
            AddTextToMainScreen,
            KickTheme,
            ChoiseTheme,
            FinalRate,
            FinalAnswer,
            EndGame,
        }

        public enum FileType {
            Pack,
            AdminImg,
            UserImg
        }
    }
}
