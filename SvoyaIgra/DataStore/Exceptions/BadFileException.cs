using System;

namespace DataStore.Exceptions
{
    public class BadFileException : Exception
    {
        public BadFileException() : base("Bad pack")
        {

        }

        public BadFileException(string message) : base(message)
        {

        }
    }
}
