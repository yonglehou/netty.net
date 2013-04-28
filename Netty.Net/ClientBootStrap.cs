using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Netty.Net
{
    


    public class ClientBootStrap
    {
        

        public static long sentCount = 0;
        public static long sentBytes = 0;

        
        private Socket client_socket;


        private int recievedBufferSize = 1024;
        private int sendBufferSize = 1024;


        /// <summary>
        /// server_endpoint,channelHandler，bufferDecoder，bufferEncoder这三个必须传入
        /// </summary>
        private IPEndPoint server_endpoint;
        private ProtocolType protocol_type;
        private ChannelHandler channel_handler;
        private BufferDecoder buffer_decoder;
        private BufferEncoder buffer_encoder;
        private ChannelConfig channel_config;



        private ChannelContext client_channel_context;


        public ClientBootStrap(ChannelHandler chandler,BufferDecoder decoder,BufferEncoder encoder,  ChannelConfig ccconfig,  ProtocolType protocol=ProtocolType.Tcp)
        {
            channel_handler = chandler;
            buffer_decoder = decoder;
            buffer_encoder = encoder;
            channel_config = ccconfig;
            protocol_type = protocol;
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocol_type);
        }

        


        private void connectCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            client_channel_context = new ChannelContext(client, channel_config) { ChannelHandler = channel_handler, Decoder = buffer_decoder, Encoder = buffer_encoder };
            ChannelHandler handler = client_channel_context.ChannelHandler;
            try
            {
                // Retrieve the socket from the state object.
                
                // Complete the connection.
                client.EndConnect(ar);


                
                handler.ChannelConnect(client_channel_context);
                // Signal that the connection has been made.

                SocketError error;
                client.BeginReceive(client_channel_context.RecieveBuffer.Bytes(), client_channel_context.RecieveBuffer.WriterIndex(), client_channel_context.RecieveBuffer.WriteableBytes(), SocketFlags.None, out error,
                    new AsyncCallback(ChannelContext.recieveCallback), client_channel_context);

                if (error != SocketError.Success)
                {
                    handler.ExceptionCaught(client_channel_context, new SocketException((int)error));
                }
                
            }
            catch (Exception e)
            {

                handler.ExceptionCaught(client_channel_context, e);
                Console.WriteLine(e.ToString());
            }
        }



        public void Start(string ip, int port)
        {
            IPAddress ipa;
            if (!IPAddress.TryParse(ip, out ipa))
            {
                //on write
                return;
            }
            server_endpoint=new IPEndPoint(ipa,port);
            
            client_socket.BeginConnect(server_endpoint, new AsyncCallback(connectCallback), client_socket);
        }

       


    }
}
