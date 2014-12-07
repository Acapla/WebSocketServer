using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer
{
    public class DataFraming
    {
        private byte[] _fragmented_message = new byte[65536];
        private ArraySegment<byte> _unfragmented_message;
        private bool _fragmented = false;
        public void Add(byte[] data)
        {
            if (!_fragmented)
            {
                if (_IsFIN(data))
                {
                }
                else
                {

                }
            }
        }

        private bool _IsFIN(byte[] data)
        {
            //FIN: 1 bit

            //Indicates that this is the final fragment in a message.  The first
            //fragment MAY also be the final fragment.
            return (data[0] & 0x10) != 0x10;
        }

        private void _CheckRSV(byte[] data)
        {
            //RSV1, RSV2, RSV3: 1 bit each

            //MUST be 0 unless an extension is negotiated that defines meanings
            //for non - zero values.If a nonzero value is received and none of
            //the negotiated extensions defines the meaning of such a nonzero
            //value, the receiving endpoint MUST _Fail the WebSocket
            //Connection_.
        }

        private Opcode.Code _GetOpcode(ArraySegment<byte> data)
        {
            return Opcode.GetOpCode(data.ElementAt(1));
        }
    }
}
