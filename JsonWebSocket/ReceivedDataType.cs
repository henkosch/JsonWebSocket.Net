using System;
using System.Collections.Generic;
using System.Text;

namespace JsonWebSocket
{
    public enum ReceivedDataType
    {
        TextJson,
        TextString,
        BinaryBson,
        BinaryData,
        Close
    }
}
