using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netty.Net
{
    public class ChannelConfig
    {
        public int connectTimeOutMilli = 10;
        public int readBufferSize = 1024;
        public int writeBufferSize = 1024;

        public ChannelConfig(int _readbytessize = 1024, int _writebytessize = 1024, int _connecttimeout = 3000)
        {
            readBufferSize = _readbytessize;
            writeBufferSize = _writebytessize;
            connectTimeOutMilli = _connecttimeout;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ServerConfig
    {
        public int heartBeatTimeOut = -1;

        public ServerConfig(int _heartbeat=-1)
        {
            heartBeatTimeOut = _heartbeat;
        }
    }
}
