using System;

namespace SvoyaIgra.Data
{
    public class User
    {
        public bool CanAnswer { get; private set; }

        public string Name { get; private set; }

        public bool HasName { get; private set; }

        public int Money { get; set; }

        public readonly string Token;

        public string FinalAns { get; private set; }

        public bool IsImgAvailable { get; private set; }

        public int Rate { get; private set; }

        public bool IsPass { get { return Rate == -1; } }

        public bool IsAllIn { get { return Rate == Money && Rate > 0; } }

        public bool IsReady { get; private set; }

        public User(string Token, string name = "")
        {
            this.Token = Token;
            this.Money = 0;

            HasName = name != "";
            Name = name;

            IsImgAvailable = false;
            CanAnswer = true;

            IsReady = false;
            Rate = -1;
        }

        public void SetReady()
        {
            IsReady = true;
        }

        public void SetCanAnswer(bool can)
        {
            CanAnswer = can;
        }

        public void SetName(string name)
        {
            HasName = true;
            this.Name = name;
        }

        public void SetRate(int value)
        {
            if (value == -1)
            {
                Console.WriteLine(Token + " pass");
            }

            Rate = value;
        }

        public void SetFinalAns(string data)
        {
            FinalAns = data;
        }

    }
}
