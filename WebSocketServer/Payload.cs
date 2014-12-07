using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer
{
    static public class Payload
    {
        //Payload length:  7 bits, 7+16 bits, or 7+64 bits

        //The length of the "Payload data", in bytes: if 0-125, that is the
        //payload length.If 126, the following 2 bytes interpreted as a
        //16-bit unsigned integer are the payload length.If 127, the
        //following 8 bytes interpreted as a 64-bit unsigned integer(the
        //most significant bit MUST be 0) are the payload length.Multibyte
        //length quantities are expressed in network byte order.  Note that
        //in all cases, the minimal number of bytes MUST be used to encode
        //the length, for example, the length of a 124-byte-long string
        //can't be encoded as the sequence 126, 0, 124.  The payload length
        //is the length of the "Extension data" + the length of the
        //"Application data".  The length of the "Extension data" may be
        //zero, in which case the payload length is the length of the
        //"Application data".
        public static int GetPlayloadLength(ArraySegment<byte> data)
        {
            var byte0 = data.ElementAt(1) & 0x7f;
            if (0 <= byte0 && byte0 <= 125)
            {
                return byte0;
            }
            if (byte0 == 126)
            {
                return BitConverter.ToInt16(data.Array, 3);
            }
            if (byte0 == 127)
            {
                //though this is allowed, we don't allow our game payload is longer than int.MaxValue.
                //return BitConverter.ToInt64(data.Array, 3);
                throw new InvalidPayloadException();
            }

            throw new InvalidPayloadException();
        }

        public static int GetPlayloadExtraOffset(ArraySegment<byte> data)
        {
            var byte0 = data.ElementAt(1) & 0x7f;
            if (0 <= byte0 && byte0 <= 125)
            {
                return 0;
            }
            if (byte0 == 126)
            {
                return 2;
            }
            if (byte0 == 127)
            {
                return 8;
            }

            throw new InvalidPayloadException();
        }

        public static ArraySegment<byte> GetPayloadData(ArraySegment<byte> data)
        {
            Mask.CheckMask(data);

            return new ArraySegment<byte>(data.Array, 6 + GetPlayloadExtraOffset(data), GetPlayloadLength(data));
        }
    }
}
