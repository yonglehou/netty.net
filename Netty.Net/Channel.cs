using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Netty.Net
{
    public class Channel
    {
        private Socket workSocket;
        public Channel(Socket _socket)
        {
            workSocket = _socket;
        }

        public Socket GetSocket()
        {
            return workSocket;
        }

        public EndPoint LocalPoint
        {
            get
            {
                return workSocket.LocalEndPoint;
            }
        }

        public EndPoint RemotePoint
        {
            get
            {
                return workSocket.RemoteEndPoint;
            }
        }
    }
}
