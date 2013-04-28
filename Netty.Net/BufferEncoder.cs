using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netty.Net
{
    public class BufferEncoder
    {
        public virtual BytesBuffer Encode(ChannelContext context, object obj)
        {
            return null;
        }
    }


    public class TotalLengthEncoder : BufferEncoder
    {
        private int offset = 0;

        HeadLengthFieldType headsize;

        public TotalLengthEncoder(HeadLengthFieldType _headsize = HeadLengthFieldType.Int32, int _offset = 0)
        {
            offset = _offset;
            headsize = _headsize;
        }

        public override BytesBuffer Encode(ChannelContext context, object obj)
        {
            BytesBuffer mydata = (BytesBuffer)obj;
            BytesBuffer bb = new BytesBuffer(offset + (int)headsize + mydata.ReadableBytes());
            bb.WriteSlice(offset);
            switch (headsize)
            {
                case HeadLengthFieldType.Int32: bb.WriteInt(4+ mydata.ReadableBytes()); break;
                case HeadLengthFieldType.Int16: bb.WriteInt16((Int16)(2+mydata.ReadableBytes())); break;
            }
            bb.SetBytes(mydata.Bytes(), mydata.ReaderIndex(), mydata.ReadableBytes(), bb.WriterIndex());
            return bb;
        }
    }
}
