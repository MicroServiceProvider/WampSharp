﻿using System;
using System.Text;
using WampSharp.Core.Message;
using WampSharp.V2.Binding;
using WampSharp.V2.Binding.Parsers;

namespace WampSharp.RawSocket
{
    // TODO: The binding isn't hosted on the WampBindingHost, which avoids sharing 
    // TODO: event serialization
    public class RawSocketBinding<TMessage> : WampTransportBinding<TMessage, byte[]>
    {
        public RawSocketBinding(IWampTextBinding<TMessage> textBinding) :
            base(textBinding.Formatter, new RawSocketTextMessageParser(textBinding), textBinding.Name)
        {
        }

        public RawSocketBinding(IWampBinaryBinding<TMessage> binaryBinding) :
            base(binaryBinding.Formatter, new RawSocketBinaryMessageParser(binaryBinding), binaryBinding.Name)
        {
        }

        private abstract class RawSocketMessageParser : IWampMessageParser<TMessage, byte[]>
        {
            private const int mHeaderSize = sizeof (int);
            private RawSocketFrameHeaderParser mParser = new RawSocketFrameHeaderParser();

            public int HeaderSize
            {
                get { return mHeaderSize; }
            }

            protected void WriteHeader(int byteCount, byte[] bytes)
            {
                mParser.WriteHeader(FrameType.WampMessage, byteCount, bytes);
            }

            public abstract WampMessage<TMessage> Parse(byte[] raw);
            public abstract byte[] Format(WampMessage<object> message);
            public abstract WampMessage<TMessage> Parse(byte[] bytes, int position, int length);
            public abstract int Format(WampMessage<object> message, byte[] bytes, int position);
        }

        private class RawSocketBinaryMessageParser : RawSocketMessageParser
        {
            private readonly IWampBinaryBinding<TMessage> mBinaryBinding;

            public RawSocketBinaryMessageParser(IWampBinaryBinding<TMessage> binaryBinding)
            {
                mBinaryBinding = binaryBinding;
            }

            public override WampMessage<TMessage> Parse(byte[] raw)
            {
                WampMessage<TMessage> parsed = mBinaryBinding.Parse(raw);
                return parsed;
            }

            public override byte[] Format(WampMessage<object> message)
            {
                byte[] bytes = mBinaryBinding.Format(message);

                int byteCount = bytes.Length;

                byte[] result = WriteContent(byteCount, bytes);

                WriteHeader(byteCount, result);

                return result;
            }

            public override WampMessage<TMessage> Parse(byte[] bytes, int position, int length)
            {
                WampMessage<TMessage> parsed = mBinaryBinding.Parse(bytes, position, length);
                return parsed;
            }

            public override int Format(WampMessage<object> message, byte[] bytes, int position)
            {
                return mBinaryBinding.Format(message, bytes, position);
            }

            private byte[] WriteContent(int byteCount, byte[] bytes)
            {
                byte[] result =
                    new byte[HeaderSize + byteCount];

                Array.Copy(bytes, 0, result, HeaderSize, bytes.Length);

                return result;
            }
        }
        
        private class RawSocketTextMessageParser : RawSocketMessageParser
        {
            private readonly IWampTextBinding<TMessage> mTextBinding;

            public RawSocketTextMessageParser(IWampTextBinding<TMessage> textBinding)
            {
                mTextBinding = textBinding;
            }

            public override WampMessage<TMessage> Parse(byte[] raw)
            {
                string text = Encoding.UTF8.GetString(raw);
                WampMessage<TMessage> parsed = mTextBinding.Parse(text);
                return parsed;
            }

            public override byte[] Format(WampMessage<object> message)
            {
                string text = mTextBinding.Format(message);

                int byteCount =
                    Encoding.UTF8.GetByteCount(text);

                byte[] result = WriteContent(byteCount, text);

                WriteHeader(byteCount, result);

                return result;
            }

            public override WampMessage<TMessage> Parse(byte[] bytes, int position, int length)
            {
                return mTextBinding.Parse(bytes, position, length);
            }

            public override int Format(WampMessage<object> message, byte[] bytes, int position)
            {
                return mTextBinding.Format(message, bytes, position);
            }

            private byte[] WriteContent(int byteCount, string text)
            {
                byte[] result =
                    new byte[HeaderSize + byteCount];

                Encoding.UTF8.GetBytes
                    (text, 0, text.Length, result, HeaderSize);

                return result;
            }
        }
    }
}