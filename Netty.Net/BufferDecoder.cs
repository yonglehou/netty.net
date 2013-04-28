using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Netty.Net;

namespace Netty.Net
{
    public class BufferDecoder
    {
        /// <summary>
        /// return number of package decoded
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recieveBytes"></param>
        /// <returns></returns>
        public virtual int Decode(ChannelContext obj, int recieveBytes)
        {
            BytesBuffer bb = obj.RecieveBuffer;
            bb.WriteSlice(recieveBytes);
            ChannelHandler handler = obj.ChannelHandler;
            handler.MessageRecieve(obj, bb.Bytes(), bb.ReaderIndex(), bb.ReadableBytes());
            bb.Set(0, 0);

            return recieveBytes;
        }
    }

    public enum HeadLengthFieldType
    {
        Int32=4,
        Int16=2
    }

    public class TotalLengthDecoder:BufferDecoder
    {
        private int recieveOffset = 4;

        private HeadLengthFieldType headSize = HeadLengthFieldType.Int32;


        public TotalLengthDecoder(HeadLengthFieldType headsize=HeadLengthFieldType.Int32, int recieveoffset = 0)
        {
            headSize = headsize;
            recieveOffset = recieveoffset;
        }

        

        public override int Decode(ChannelContext obj, int recieveBytes)
        {
            BytesBuffer bb = obj.RecieveBuffer;
            bb.WriterIndex(bb.WriterIndex() + recieveBytes);
            //so.recvedSize += recievedBytes;

            int package_get = 0;
            ChannelHandler hanlder = obj.ChannelHandler;
            while (bb.ReadableBytes() >=recieveOffset +(int) headSize )
            {
                bb.ReadSlice(recieveOffset);
                int totalLength=0;
                int reader = bb.ReaderIndex() + recieveOffset;
                switch (headSize)
                {
                    case HeadLengthFieldType.Int32: totalLength = bb.GetInt(reader); break;
                    case HeadLengthFieldType.Int16: totalLength = bb.GetInt16(reader); break;
                }
                int bodyLength = totalLength - recieveOffset - (int)headSize;
                if (bb.ReadableBytes() >= totalLength)
                {
                    //能读出一个package
                    bb.ReadSlice(recieveOffset + (int)headSize);
                    hanlder.MessageRecieve(obj, bb.Bytes(), bb.ReaderIndex(), bodyLength);

                    bb.ReadSlice(bodyLength);
                    ++package_get;
                }
                else
                {
                    break;
                }
            }

            if (bb.ReadableBytes() > 0)
            {
                bb.SetBytes(bb.Bytes(), bb.ReaderIndex(), bb.ReadableBytes(), 0);
                bb.ReaderIndex(0);
            }
            else
            {
                bb.Set(0, 0);
            }
            return package_get;
        }




        
    }

   
}
