using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace WampSharp.Owin
{
    public class WebSocketData
    {
        internal WebSocketData(IDictionary<string, object> webSocketContext, IOwinContext owinContext, string subProtocol)
        {
            WebSocketContext = webSocketContext;
            OwinContext = owinContext;
            SubProtocol = subProtocol;
        }

        public IDictionary<string, object> WebSocketContext { get; private set; }

        public IOwinContext OwinContext { get; private set; }

        public Task ReadTask { get; internal set; }

        public string SubProtocol { get; private set; }
    }
}