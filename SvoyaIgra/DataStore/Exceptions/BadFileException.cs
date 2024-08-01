using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
