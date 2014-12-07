using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer
{
    static public class Opcode
    {
        public enum Code
        {
            continuation,
            text,
            binary,
            close,
            ping,
            pong,
            error,
        };

        static public Code GetOpCode(ArraySegment<byte> data)
        {
            switch (data.ElementAt(0) & 0x0f)
            {
                //*  % x0 denotes a continuation frame
                case 0x00:
                    return Code.continuation;
                //*  % x1 denotes a text frame
                case 0x01:
                    return Code.text;
                //*  % x2 denotes a binary frame
                case 0x02:
                    return Code.binary;
                //*  % x3 - 7 are reserved for further non-control frames
                //*  % x8 denotes a connection close
                case 0x08:
                    return Code.close;
                //*  % x9 denotes a ping
                case 0x09:
                    return Code.ping;
                //*  % xA denotes a pong
                case 0x0A:
                    return Code.pong;
                //*  % xB - F are reserved for further control frames
                default:
                    return Code.error;
            }
        }

        internal static Code GetOpCode(byte v)
        {
            throw new NotImplementedException();
        }
    }
}
