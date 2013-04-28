using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Netty.Net
{
    /// <summary>
    /// Channel上下文
    /// </summary>
    public class ChannelContext
    {

        private DateTime lastAliveTime;



        public Channel Channel;

        public ChannelConfig ChannelConfig { get; set; }

        public void Active()
        {
            lastAliveTime = DateTime.Now;
        }

        public ChannelContext(Socket _worksocket, ChannelConfig channel_config)
        {
            Channel = new Channel(_worksocket);
            ChannelConfig = channel_config;
            RecieveBuffer = new BytesBuffer(channel_config.readBufferSize);
            SendBuffer = new BytesBuffer(channel_config.writeBufferSize);
        }

        public Socket GetSocket()
        {
            return Channel.GetSocket();
        }

        /// <summary>
        /// 包的解析类
        /// </summary>
        public BufferDecoder Decoder = new BufferDecoder();
        /// <summary>
        /// 包的封装类
        /// </summary>
        public BufferEncoder Encoder = new BufferEncoder();


        public ChannelHandler ChannelHandler = new ChannelHandler();


        public BytesBuffer RecieveBuffer { get; set; }
        public BytesBuffer SendBuffer { get; set; }

        protected object sendLockObj = new object();


        public static void recieveCallback(IAsyncResult ar)
        {
            
            ChannelContext context = (ChannelContext)ar.AsyncState;
            ChannelHandler handler = context.ChannelHandler;
            Socket socket = context.GetSocket();
            try
            {

                int readBytes = socket.EndReceive(ar);
                context.Decoder.Decode(context, readBytes);

                SocketError error;
                socket.BeginReceive(context.RecieveBuffer.Bytes(), context.RecieveBuffer.WriterIndex(), context.RecieveBuffer.WriteableBytes(), SocketFlags.None, out error, new AsyncCallback(recieveCallback), context);
                if (error != SocketError.Success)
                {
                    handler.ExceptionCaught(context, new SocketException((int)error));
                }
            }
            catch (Exception ex)
            {
                context.Close();
                handler.ExceptionCaught(context, ex);
            }
            context.Active();
        }

        public static void sendCallback(IAsyncResult ar)
        {
            ChannelContext context = (ChannelContext)ar.AsyncState;
            ChannelHandler handler = context.ChannelHandler;
            Socket socket = context.GetSocket();
            bool istaken = false;
            try
            {
                int sentBytes = socket.EndSend(ar);
                handler.WriteComplete(context, sentBytes);
                Monitor.Enter(context.sendLockObj, ref istaken);

                BytesBuffer bb = context.SendBuffer;
                bb.ReadSlice(sentBytes);
                if (bb.ReadableBytes() > 0)
                {
                    bb.SetBytes(bb.Bytes(), bb.ReaderIndex(), bb.ReadableBytes(), 0);
                    bb.ReaderIndex(0);
                }
                else
                {
                    bb.Set(0, 0);
                }
                if (bb.ReadableBytes() > 0)
                {
                    SocketError error;
                    socket.BeginSend(bb.Bytes(), bb.ReaderIndex(), bb.ReadableBytes(), SocketFlags.None, out error, new AsyncCallback(sendCallback), context);
                    if (error != SocketError.Success)
                    {
                        handler.ExceptionCaught(context, new SocketException((int)error));
                    }
                }

            }
            catch (Exception ex)
            {
                context.Close();
                handler.ExceptionCaught(context, ex);
            }
            finally
            {
                if (istaken) Monitor.Exit(context.sendLockObj);
            }
        }

        public void Close()
        {
            try
            {
                Socket socket = GetSocket();
                socket.Close();
                socket.Shutdown(SocketShutdown.Both);
                ChannelHandler.ChannelClose(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("close exception:{0}",ex.Message);
            }
        }

        public bool IsDead(int deadtimeout)
        {
            if ((DateTime.Now - lastAliveTime).TotalMilliseconds > deadtimeout * 1000)
            {
                return true;
            }
            return false;
        }


        public bool Send(BytesBuffer mybuffer)
        {
            bool sendresult = true;
            bool istaken = false;
            BytesBuffer newbuffer = Encoder.Encode(this, mybuffer);
            try
            {
                Monitor.Enter(sendLockObj, ref istaken);
                bool isempty = SendBuffer.ReadableBytes() == 0;
                if (!SendBuffer.WriteBytes(newbuffer.Bytes(), newbuffer.ReaderIndex(), newbuffer.ReadableBytes()))
                {
                    //ChannelHandler.ExceptionCaught(this, new Exception("try write failed"));
                    sendresult = false;
                }
                else
                {
                    if (isempty)
                    {
                        SocketError error;
                        Socket workSocket = Channel.GetSocket();
                        workSocket.BeginSend(SendBuffer.Bytes(), SendBuffer.ReaderIndex(), SendBuffer.ReadableBytes(), SocketFlags.None, out error, new AsyncCallback(sendCallback), this);
                        if (error != SocketError.Success)
                        {
                            ChannelHandler.ExceptionCaught(this, new SocketException((int)error));
                        }
                    }
                }
            }
            finally
            {
                if (istaken) Monitor.Exit(sendLockObj);
            }
            return sendresult;
        }

    }
}
