using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace WebSocketServer
{
    static public class Handshake
    {
        static public bool IsHandshake(byte[] data)
        {
            return new Regex("^GET").IsMatch(Encoding.UTF8.GetString(data));
        }

        static public byte[] FormatHandshakeResponse(byte[] data)
        {
            var sec_websocket_key = new Regex("Sec-WebSocket-Key: (.*)").Match(Encoding.UTF8.GetString(data)).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            return Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols" + Environment.NewLine +
                "Connection: Upgrade" + Environment.NewLine +
                "Upgrade: websocket" + Environment.NewLine +
                "Sec-WebSocket-Accept: " +
                Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(sec_websocket_key))) + Environment.NewLine + Environment.NewLine);
        }
    }
}
