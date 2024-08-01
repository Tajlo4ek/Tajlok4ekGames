using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Utils
{
    public static class IdUtil
    {
        private static long id = 0;

        public static long GetId()
        {
            return id++;
        }

    }
}
