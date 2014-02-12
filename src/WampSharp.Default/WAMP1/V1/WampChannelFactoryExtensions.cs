﻿using Newtonsoft.Json.Linq;
using WampSharp.Core.Serialization;
using WampSharp.Newtonsoft;
using WampSharp.V1.Api.Client;
using WampSharp.WebSocket4Net;

namespace WampSharp.V1
{
    public static class WampChannelFactoryExtensions
    {
        public static IWampChannel<JToken> CreateChannel(this IWampChannelFactory<JToken> factory, string address)
        {
            return factory.CreateChannel(address, new JTokenMessageParser());
        }

        public static IWampChannel<TMessage> CreateChannel<TMessage>(this IWampChannelFactory<TMessage> factory, string address, IWampMessageParser<TMessage> parser)
        {
            return factory.CreateChannel(new WebSocket4NetConnection<TMessage>(address, parser));
        }
    }
}