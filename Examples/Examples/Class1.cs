using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netty.Net;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ExamplesClient
{

    public class myHandler : Netty.Net.ChannelHandler
    {
        public static int sentCount = 0;
        public static int sentBytes = 0;
        public static int recvCount = 0;
        public static int recvBytes = 0;

        public ChannelContext mycontext;

        public ManualResetEvent connectEvent = new ManualResetEvent(false);

        public override void ChannelConnect(ChannelContext context)
        {
            Console.WriteLine("connected to :{0}",context.Channel.RemotePoint.ToString());
            mycontext=context;
            connectEvent.Set();
        }

        public override void MessageRecieve(ChannelContext context, byte[] bytes, int index, int length)
        {
            Interlocked.Increment(ref  myHandler.recvCount);
            Interlocked.Add(ref recvBytes, length+4);

            string str = Encoding.ASCII.GetString(bytes, index, length);
            //Console.WriteLine(str);
        }

        public override void WriteComplete(ChannelContext context, int length)
        {
            Interlocked.Add(ref sentBytes, length);
        }

        public override void ExceptionCaught(ChannelContext context, Exception ex)
        {
            //Console.WriteLine(ex.ToString());
        }

        public override void ChannelClose(ChannelContext context)
        {
            Console.WriteLine(context.Channel.RemotePoint.ToString());
        }
    }
    

    public class Class1
    {



        private static myHandler handler =new  myHandler();

        public static void ClientThreadStart()
        {
            System.IO.StreamReader sr = new System.IO.StreamReader("input.txt");
            string ip = sr.ReadLine();
            string port = sr.ReadLine();
            ClientBootStrap client = new ClientBootStrap(handler,new TotalLengthDecoder(),new TotalLengthEncoder(),new ChannelConfig());
            client.Start(ip, int.Parse(port));
            
            byte[] bytes = Encoding.ASCII.GetBytes("aaaaaaaaaazzz");
            byte[] senddata = new byte[bytes.Length + 4];
            BitConverter.GetBytes(bytes.Length + 4).CopyTo(senddata, 0);
            bytes.CopyTo(senddata, 4);

            /*int pHeaderSize = 8;
            int pReqSize = 64;

            byte[] myBytes = new byte[1000];

            int index = 0;
            string data="1";
            int totalLength=pHeaderSize+pReqSize+data.Length;

            BitConverter.GetBytes(totalLength).CopyTo(myBytes, index);
            index += 4;
            BitConverter.GetBytes(102).CopyTo(myBytes, index);
            index += 4;
            BitConverter.GetBytes(pReqSize).CopyTo(myBytes, index);
            index += 4;
            BitConverter.GetBytes(2).CopyTo(myBytes, index);
            index += 4;
            BitConverter.GetBytes(0).CopyTo(myBytes,index);
            index += 4;
            index += 32;
            index += 4;
            index += 16;*/

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.Append("aaaaaaaaaa");
            }

            BytesBuffer bb = BytesBufferFactory.Instance.StringBuffer(sb.ToString());


            handler.connectEvent.WaitOne();
            while (true)
            {
                handler.mycontext.Send(bb);
                
                //session.Send(senddata, 0, senddata.Length);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static void ClientSentPrint(object obj)
        {
            long countsent = myHandler.sentCount;
            long bytessent = myHandler.sentBytes;
            long countread = myHandler.recvCount;
            long bytesread = myHandler.recvBytes;
            Console.WriteLine("sent count:{0}, avg:{1} p/s, bytes:{2}, avg:{3} kb/s", countsent, countsent / timer_seconds, bytessent, bytessent / 1024 / timer_seconds);
            Console.WriteLine("read count:{0},avg:{1} p/s, bytes:{2},avg:{3} kb/s", countread, countread / timer_seconds, bytesread, bytesread / 1024 / timer_seconds);
            log4net.ILog logger = log4net.LogManager.GetLogger("logger");
            logger.WarnFormat("sent count:{0}, avg:{1} p/s, bytes:{2}, avg:{3} kb/s", countsent, countsent / timer_seconds, bytessent, bytessent / 1024 / timer_seconds);
            Interlocked.Exchange(ref myHandler.sentCount, 0);
            Interlocked.Exchange(ref myHandler.sentBytes, 0);
            Interlocked.Exchange(ref myHandler.recvBytes, 0);
            Interlocked.Exchange(ref myHandler.recvCount, 0);
        }


        private static int timer_seconds = 5;

        public static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            /*byte[] data = BitConverter.GetBytes(12);

            Console.WriteLine(string.Format("{0},{1},{2},{3}",data[0],data[1],data[2],data[3]));
            //return 0;
            //for (int i = 0; i < 100; i++)
            System.Threading.Timer timer = new Timer(new TimerCallback(TimerFunc), null, 0, timer_seconds * 1000);
            {
                StartClient();
            }
             return 0;*/
            Console.WriteLine("input number of client(1):");
            string input = Console.ReadLine();
            int number = 1;
            if (!int.TryParse(input, out number))
            {
                number = 1;
            }

            for (int i = 0; i < number; i++)
            {
                new System.Threading.Thread(new ThreadStart(ClientThreadStart)).Start();
            }
            System.Threading.Timer timer1 = new Timer(new TimerCallback(ClientSentPrint), null, 0, timer_seconds * 1000);

            //ClientThreadStart();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }

            
        }
    }
}
