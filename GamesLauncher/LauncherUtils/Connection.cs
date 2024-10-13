using ClientServer;
using System;
using System.Collections.Generic;
using static LauncherUtils.Messages;

namespace LauncherUtils
{
    public class Connection
    {

        public readonly string MyToken;
        public readonly string RemoteToken;

        private DateTime lastUpdateTime = DateTime.Now;

        private const int maxNotActiveTime = 5 * 60 * 1000;

        public Connection(string myToken, string remoteToken)
        {
            this.MyToken = myToken;
            this.RemoteToken = remoteToken;
        }

        public void Update()
        {
            lastUpdateTime = DateTime.Now;
        }

        public bool IsActive
        {
            get
            {
                return (DateTime.Now - lastUpdateTime).TotalMilliseconds < maxNotActiveTime;
            }
        }
    }
}
