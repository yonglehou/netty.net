using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netty.Net
{
    /// <summary>
    /// Channel事件的实现接口类
    /// </summary>
    public class ChannelHandler
    {

        public virtual void Bound(ChannelContext context)
        {
        }

        public virtual void ChannelOpen(ChannelContext context)
        {
        }
        public virtual void ChannelClose(ChannelContext context)
        {
        }

        public virtual void ChannelConnect(ChannelContext context)
        {
        }

        public virtual void MessageRecieve(ChannelContext context, byte[] bytes, int index, int length)
        {

        }

        public virtual void ExceptionCaught(ChannelContext context, Exception ex)
        {
        }

        public virtual void WriteRequest(ChannelContext context, BytesBuffer buffer)
        {
        }

        public virtual void WriteComplete(ChannelContext context, int length)
        {
        }


    }
}
