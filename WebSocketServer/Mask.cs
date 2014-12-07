using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer
{
    public class Mask
    {
        //Mask:  1 bit

        //Defines whether the "Payload data" is masked.If set to 1, a
        //masking key is present in masking-key, and this is used to unmask
        //the "Payload data" as per Section 5.3.  All frames sent from
        //client to server have this bit set to 1.
        static public void CheckMask(ArraySegment<byte> data)
        {
            if ((data.ElementAt(1) & 0x80) != 0x80)
            {
                throw new InvalidFormatException();
            }
        }

        static public ArraySegment<byte> GetMaskingKey(ArraySegment<byte> data, int payloadExtraOffset)
        {
            return new ArraySegment<byte>(data.Array, 2 + payloadExtraOffset, 4);
        }
    }
}
