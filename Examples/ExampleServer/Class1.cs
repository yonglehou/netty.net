using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netty.Net;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ExampleServer
{

    public class myChannelHandler : ChannelHandler
    {
        public static int connectCount = 0;
        public static int readBytes = 0;
        public static int readCount = 0;
        public static int writeBytes = 0;

        BytesBuffer sendbuf = BytesBufferFactory.Instance.StringBuffer("hello");

        public override void ChannelConnect(ChannelContext context)
        {
            Interlocked.Increment(ref connectCount);
            Console.WriteLine(context.Channel.RemotePoint.ToString());
        }

        public override void MessageRecieve(ChannelContext context, byte[] bytes, int index, int length)
        {
            Interlocked.Increment(ref readCount);
            Interlocked.Add(ref readBytes, length+4);

            context.Send(sendbuf);

            
        }

        public override void WriteComplete(ChannelContext context, int length)
        {
            Interlocked.Add(ref writeBytes, length);
            //base.WriteComplete(context, length);
        }

        public override void ExceptionCaught(ChannelContext context, Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        
    }

    public class Class1
    {
        static ServerBootStrap server;

        private static int timer_second=5;

        public static void StatTimerFunc(object obj)
        {
            Console.WriteLine("连接数:{0},收到包数:{1} p/s,字节数:{2} kb/s,发送字节数 {3} kb/s", myChannelHandler.connectCount, myChannelHandler.readCount / timer_second, myChannelHandler.readBytes / timer_second / 1024,myChannelHandler.writeBytes/timer_second/1024);
            Interlocked.Exchange(ref myChannelHandler.readBytes, 0);
            Interlocked.Exchange(ref myChannelHandler.readCount, 0);
            Interlocked.Exchange(ref myChannelHandler.writeBytes, 0);
        }

        public static void Main()
        {

            log4net.Config.XmlConfigurator.Configure();
            System.IO.StreamReader sr = new System.IO.StreamReader("input.txt");
            string ip = sr.ReadLine();
            string port = sr.ReadLine();

            ChannelHandler chandler=new myChannelHandler();

            Timer stattimer = new Timer(new TimerCallback(StatTimerFunc), null, timer_second * 1000, timer_second * 1000);

            server = new ServerBootStrap(chandler, new TotalLengthEncoder(), new TotalLengthDecoder(), new ChannelConfig(),new ServerConfig());

            server.Bind(ip, int.Parse(port));
            
        }

    }
}
