using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace WebSocketServer
{
    public class Server
    {
        private readonly TcpListener _listener;

        public Server(IPAddress ip, int port)
        {
            _listener = new TcpListener(ip, port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server has started on {0}.{1}Waiting for a connection...", _listener.LocalEndpoint, Environment.NewLine);
        }

        public async void Run()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                Console.WriteLine("A client connected.");

                var stream = client.GetStream();

                //enter to an infinite cycle to be able to handle every change in stream
                while (true)
                {
                    while (!stream.DataAvailable)
                    {
                        await Task.Yield();
                        continue;
                    }

                    var data = new byte[client.Available];

                    await stream.ReadAsync(data, 0, data.Length).ContinueWith(async (count) =>
                    {
                        if (Handshake.IsHandshake(data))
                        {
                            var response = Handshake.FormatHandshakeResponse(data);
                            await stream.WriteAsync(response, 0, response.Length).ConfigureAwait(false);
                        }
                        else
                        {
                            var rawData = new ArraySegment<byte>(data);
                            var opcode = Opcode.GetOpCode(rawData);
                            // hardcode, first byte must be 129 to indicate this is a text message.
                            if (opcode != Opcode.Code.text)
                            {
                                return;
                            }
                            var length = Payload.GetPlayloadLength(rawData);
                            var key = Mask.GetMaskingKey(rawData, Payload.GetPlayloadExtraOffset(rawData));
                            var decoded = new byte[length];

                            for (int i = 0; i < decoded.Length; ++i)
                            {
                                decoded[i] = (byte)(data[i + 2 + key.Count] ^ key.ElementAt(i % 4));
                            }

                            var text1 = Encoding.UTF8.GetString(decoded);
                            Console.WriteLine("receive {0}", text1);
                            // always response "go!"
                            var encoded = Encoding.UTF8.GetBytes("go!");
                            var response = new byte[2 + encoded.Length];
                            response[0] = 129;
                            response[1] = (byte)(encoded.Length);
                            for (int i = 0; i < encoded.Length; ++i)
                            {
                                response[2 + i] = (byte)(encoded[i]);
                            }
                            //var binary = response.Select(x => Convert.ToString(x, 2)).ToArray();
                            //System.Diagnostics.Debug.WriteLine(string.Join(" ", binary));
                            await stream.WriteAsync(response, 0, response.Length).ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);
                }
            }
        }
    }
}
