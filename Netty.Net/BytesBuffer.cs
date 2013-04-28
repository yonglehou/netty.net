using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netty.Net
{
    

    public class BytesBufferFactory
    {

        private static readonly BytesBufferFactory factory = new BytesBufferFactory();
        public static BytesBufferFactory Instance
        {
            get { return factory; }
        }

        public BytesBuffer DefaultBuffer(int capacity)
        {
            BytesBuffer bb = new BytesBuffer(capacity);
            return bb;
        }

        public BytesBuffer StringBuffer(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            BytesBuffer bb = new BytesBuffer(bytes.Length);
            bb.WriteBytes(bytes, 0, bytes.Length);
            return bb;
        }
    }

    public class BytesBuffer
    {
        private byte[] bytes;
        private int offset=0;
        private int capacity;
        private int readerIndex=0;
        private int writerIndex = 0;

        public int Capacity()
        {
            return capacity;
        }

        public byte[] Bytes()
        {
            return bytes;
        }

        public int ReaderIndex()
        {
            return readerIndex;
        }

        public void ReaderIndex(int index)
        {
            readerIndex = index;
        }

        public int WriterIndex()
        {
            return writerIndex;
        }

        public void WriterIndex(int index)
        {
            writerIndex = index;
        }

        
        public BytesBuffer(int _capacity)
        {
            capacity = _capacity;
            bytes = new byte[capacity];
        }
        /// <summary>
        /// readerindex向前滑动slice
        /// </summary>
        /// <param name="slice"></param>
        public void ReadSlice(int slice)
        {
            readerIndex += slice;
        }

        public void WriteSlice(int slice)
        {
            writerIndex += slice;
        }

        public int ReadableBytes()
        {
            return writerIndex - readerIndex >= 0 ? writerIndex - readerIndex : 0;
        }

        public void Set(int _readerIndex, int _writerIndex)
        {
            readerIndex = _readerIndex;
            writerIndex = _writerIndex;
        }

        public int ReadInt()
        {
            if (ReadableBytes() < 4)
            {
                return 0;
            }
            int ret= BitConverter.ToInt32(bytes, readerIndex);
            readerIndex += 4 ;
            return ret;
        }

        public Int16 ReadInt16()
        {
            if (ReadableBytes() < 2)
            {
                return 0;
            }
            Int16 ret = BitConverter.ToInt16(bytes, readerIndex);
            readerIndex += 2;
            return ret;
        }

        public void ReadBytes(byte[] dst,int dstindex,int leng)
        {
            int canread = ReadableBytes();
            if (dst != null && dst.Length > 0)
            {
                int shouldread = canread > leng ? leng : canread;
                Array.Copy(bytes, readerIndex, dst, dstindex, shouldread);
                readerIndex += shouldread;
            }
        }


        public int WriteableBytes()
        {
            return capacity - writerIndex > 0 ? capacity - writerIndex : 0;
        }

        public void WriteInt(int val)
        {
            int canwrite = WriteableBytes();
            if (canwrite >= 4)
            {
                BitConverter.GetBytes(val).CopyTo(bytes, writerIndex);
                writerIndex += 4;
            }
        }
        public void WriteInt16(Int16 val)
        {
            int canwrite = WriteableBytes();
            if (canwrite >= 2)
            {
                BitConverter.GetBytes(val).CopyTo(bytes, writerIndex);
                writerIndex += 2;
            }
        }

        public bool WriteBytes(byte[] source, int index, int length)
        {
            if (WriteableBytes() < length)
            {
                return false;
            }
            Array.Copy(source, index, bytes, writerIndex, length);
            writerIndex += length;
            return true;
        }

        /// <summary>
        /// 在当前的bytes 的 dstindex后面添加source的数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceindex"></param>
        /// <param name="length"></param>
        /// <param name="dstindex"></param>
        public void SetBytes(byte[] source, int sourceindex, int length,int dstindex)
        {
            Array.Copy(source, sourceindex, bytes, dstindex, length);
            writerIndex = dstindex + length;
        }

    }
}
