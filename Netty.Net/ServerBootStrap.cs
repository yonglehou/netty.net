using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Concurrent;
using Netty.Net;

namespace Netty.Net
{



    public class ServerBootStrap
    {


        private Socket listenSocket;

        private ManualResetEvent acceptResetEvent = new ManualResetEvent(false);
        private CancellationTokenSource stopToken = new CancellationTokenSource();

        private log4net.ILog errorlog = log4net.LogManager.GetLogger(LogAppender.netlib_error.ToString());
        private log4net.ILog warnlog = log4net.LogManager.GetLogger(LogAppender.netlib_warn.ToString());

        private ConcurrentDictionary<long, ChannelContext> contextObjectsDict = new ConcurrentDictionary<long, ChannelContext>();

        private int newsocketIDCounter = 0;

       

        
        private ChannelHandler channel_handler;
        private BufferDecoder buffer_decoder;
        private BufferEncoder buffer_encoder;
        private ChannelConfig channel_config;


        private ServerConfig server_config;

        private ChannelContext bind_context;


        private Timer checkChannelIsAliveTimer;
        private int checkingChannelIsChecking=0;
        

        //public ServerBootStrap(int recievebuffersize = 1024, int sendbuffersize = 1024,int protocolHeadSize=4,int recieveoffset=0,int sendoffset=0)
        //{
        //    recieveBufferSize = recievebuffersize;
        //    sendBufferSize = sendbuffersize;
        //    packageHeaderSize = protocolHeadSize;
        //    recieveOffset = recieveoffset;
        //    sendOffset = sendoffset;
        //}

        public ServerBootStrap(ChannelHandler handler, BufferEncoder encoder, BufferDecoder decoder, ChannelConfig ccconfig,ServerConfig _serverconfig)
        {
            channel_handler = handler;
            buffer_encoder = encoder;
            buffer_decoder = decoder;
            channel_config = ccconfig;
            server_config = _serverconfig;
        }
        

        private void bindAndListen(string ip,int port)
        {
            try
            {
                IPEndPoint localendpoint = new IPEndPoint(IPAddress.Parse(ip), port);
                listenSocket.Bind(localendpoint);
                listenSocket.Listen(10000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private long newSocketID()
        {
            return Interlocked.Increment(ref newsocketIDCounter);

            //lock (newsocketidLockObj)
            //{
            //    Interlocked.Increment(ref newsocketIDCounter);
            //    long result = newsocketIDCounter;
            //    int looptime = 0;
            //    while (sessionObjectsDict.ContainsKey(newsocketIDCounter)&& looptime<1000)
            //    {
            //        Interlocked.Increment(ref newsocketIDCounter);
            //        ++looptime;
            //    }
            //    if (looptime >= 1000)
            //    {
            //        throw new Exception("cannot generate newsocketid");
            //    }
            //    if (!sessionObjectsDict.TryAdd(newsocketIDCounter, null))
            //    {
            //        new Exception("add session object failed");
            //    }
            //    result = newsocketIDCounter;
            //    return result;
            //}
        }

        private void accepedCallback(IAsyncResult ar)
        {
            acceptResetEvent.Set();
            long socketid = newSocketID();
            Socket currentSocket = (Socket)ar.AsyncState;
            
            try
            {
                
                Socket socket = currentSocket.EndAccept(ar);
                ChannelContext context = new ChannelContext(socket, channel_config) { ChannelHandler = channel_handler, Decoder = buffer_decoder, Encoder = buffer_encoder };
                ChannelHandler handler = context.ChannelHandler;

                if (!contextObjectsDict.TryAdd(newsocketIDCounter, null))
                {
                    new Exception("add session object failed");
                }
                //currentSocket.LocalEndPoint.
                handler.ChannelConnect(context);

                contextObjectsDict[socketid] = context;
                SocketError error;
                socket.BeginReceive(context.RecieveBuffer.Bytes(),context.RecieveBuffer.WriterIndex(),context.RecieveBuffer.WriteableBytes() , SocketFlags.None, out error, new AsyncCallback(ChannelContext.recieveCallback), context);
                if (error != SocketError.Success)
                {
                    handler.ExceptionCaught(context, new SocketException((int)error));
                }

            }
            catch (SocketException ex)
            {
                errorlog.ErrorFormat("accept error {0}", ex.ToString());
                channel_handler.ExceptionCaught(bind_context, ex);
            }
            catch (Exception ex)
            {
                errorlog.ErrorFormat("acceptcallback:{0}", ex.Message);
                channel_handler.ExceptionCaught(bind_context, ex);
            }
            
        }


        private void CheckChannelIsAliveFunc(object obj)
        {
            if (checkingChannelIsChecking == 0)
            {
                Interlocked.Exchange(ref checkingChannelIsChecking, 1);
            }
            else
            {
                return;
            }
            foreach(long key in contextObjectsDict.Keys)
            {
                ChannelContext context = contextObjectsDict[key];
                if (context.IsDead(server_config.heartBeatTimeOut))
                {
                    context.Close();
                }
                
            }

            Interlocked.Exchange(ref checkingChannelIsChecking, 0);
        }


        public void Bind(string ip, int port)
        {

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bindAndListen(ip,port);

            bind_context = new ChannelContext(listenSocket, channel_config);

            if (server_config.heartBeatTimeOut != -1)
            {
                checkChannelIsAliveTimer = new Timer(new TimerCallback(CheckChannelIsAliveFunc), null, 0, server_config.heartBeatTimeOut * 1000);
            }

            while (!stopToken.IsCancellationRequested)
            {
                acceptResetEvent.Reset();
                try
                {
                    listenSocket.BeginAccept(new AsyncCallback(accepedCallback), listenSocket);
                }
                catch (Exception ex)
                {
                    //
                    errorlog.ErrorFormat("accept failed:{0}", ex.Message);

                    channel_handler.ExceptionCaught(bind_context, ex);
                }
                acceptResetEvent.WaitOne();
            }
        }



        public void Stop()
        {
            stopToken.Dispose();
        }

    }
}
