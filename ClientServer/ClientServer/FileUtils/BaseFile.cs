namespace ClientServer.FileUtils
{
    internal class BaseFile
    {
        public string Token { get; private set; }

        public ProgressFileData ProgressData { get; private set; }

        public BaseFile(string name) : this(TokenGenerator.Generate(), name) { }

        public BaseFile(string token, string fileName)
        {
            Token = token;

            ProgressData = new ProgressFileData()
            {
                Name = fileName,
                State = ProgressFileData.States.Start,
                Progress = 0
            };
        }
    }
}
