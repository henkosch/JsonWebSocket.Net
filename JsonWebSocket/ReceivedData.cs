using System;
using System.Collections.Generic;
using System.Text;

namespace JsonWebSocket
{
    public class ReceivedData
    {
        public ReceivedData(ReceivedDataType type, object data)
        {
            this.Type = type;
            this.Data = data;
        }

        public dynamic TextJson { get { return Data; } }
        public string TextString { get { return Data as string; } }
        public dynamic BinaryBson { get { return Data; } }
        public byte[] BinaryData { get { return Data as byte[]; } }

        public ReceivedDataType Type { get; set; }
        public object Data { get; set; }
    }
}
