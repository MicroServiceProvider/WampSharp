﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WampSharp.Core.Dispatch.Handler;
using WampSharp.Core.Message;
using WampSharp.Newtonsoft;
using WampSharp.Tests.TestHelpers;
using WampSharp.V2.Core.Contracts;

namespace WampSharp.Tests.Wampv2.IntegrationTests.MockBuilder
{
    public class MessageMapper : IMessageMapper
    {
        private readonly IWampRequestMapper<MockRaw> mMapper =
            new RequestMapper();

        private readonly JTokenEqualityComparer mComparer = new JTokenEqualityComparer();
        private static JsonSerializer mSerializer;

        public MessageMapper()
        {
            mSerializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            mSerializer.ContractResolver = new JsonPropertyNameContractResolver();
            mSerializer.Converters.Add( new MockRawConverter());
        }

        public WampMessage<MockRaw> MapRequest(WampMessage<MockRaw> message, IEnumerable<WampMessage<MockRaw>> messages, bool ignoreRequestId)
        {
            WampMethodInfo map = mMapper.Map(message);

            int[] indexes;
            
            if (!ignoreRequestId)
            {
                indexes = Enumerable.Range(0, map.Method.GetParameters().Length)
                                    .ToArray();
            }
            else
            {
                indexes =
                    map.Method.GetParameters().Select((x, i) => new {parameter = x, index = i})
                       .Where(x => x.parameter.Name != "requestId" &&
                           x.parameter.Name != "publicationId")
                       .Select(x => x.index)
                       .ToArray();
            }

            JArray formattedIncoming = Format(message, indexes);

            WampMessage<MockRaw> request =
                messages.Where(x => x.MessageType == message.MessageType)
                        .FirstOrDefault(x => AreEquivalent(formattedIncoming, indexes, x, message));

            return request;
        }

        private bool AreEquivalent(JArray formattedIncoming,
                                   int[] indexes,
                                   WampMessage<MockRaw> record,
                                   WampMessage<MockRaw> incoming)
        {
            if (record.Arguments.Length != incoming.Arguments.Length)
            {
                return false;
            }
            else
            {
                JArray formattedRecord = Format(record, indexes);
                return mComparer.Equals(formattedRecord, formattedIncoming);
            }
        }

        private JArray Format(WampMessage<MockRaw> message, int[] indexes)
        {
            JArray array =
                new JArray(indexes.Select(x => message.Arguments[x])
                                  .Select(x => Convert(x)));

            return array;
        }

        private static JToken Convert(MockRaw raw)
        {
            MockRaw[] array = raw.Value as MockRaw[];

            if (array != null)
            {
                return new JArray(array.Select(x => Convert(x)));
            }
            else if (raw.Value == null)
            {
                return new JValue((object)null);
            }
            else
            {
                return JToken.FromObject(raw.Value, mSerializer);
            }
        }
    }
}