using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace WebSocketServer
{
    class Program
    {
        static async void Run()
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);

            while (true)
            {
                var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);

                Console.WriteLine("A client connected.");

                var stream = client.GetStream();

                //enter to an infinite cycle to be able to handle every change in stream
                while (true)
                {
                    while (!stream.DataAvailable)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var bytes = new Byte[client.Available];

                    await stream.ReadAsync(bytes, 0, bytes.Length).ContinueWith(async (count) =>
                    {
                        var data = Encoding.UTF8.GetString(bytes);
                        if (new Regex("^GET").IsMatch(data))
                        {
                            var sec_websocket_key = new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                            var response = Encoding.UTF8.GetBytes(
                                "HTTP/1.1 101 Switching Protocols" + Environment.NewLine +
                                "Connection: Upgrade" + Environment.NewLine +
                                "Upgrade: websocket" + Environment.NewLine +
                                "Sec-WebSocket-Accept: " +
                                Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(sec_websocket_key))) + Environment.NewLine + Environment.NewLine);

                            await stream.WriteAsync(response, 0, response.Length).ConfigureAwait(false);
                        }
                        else
                        {
                            // hardcode, first byte must be 129 to indicate this is a text message.
                            if (bytes[0] != 129)
                            {
                                return;
                            }
                            // If the second byte minus 128 is between 0 and 125, this is the length of message.
                            // If it is 126, the following 2 bytes (16-bit unsigned integer), 
                            // if 127, the following 8 bytes (64-bit unsigned integer) are the length.
                            // hard code, this must be a short text message, so must be between 0 and 125.
                            if (bytes[1] < 128)
                            {
                                return;
                            }
                            var length = bytes[1] - 128;
                            if (length > 125)
                            {
                                return;
                            }
                            var raw = bytes;
                            var key = new Byte[4] { bytes[2], bytes[3], bytes[4], bytes[5], };
                            var decoded = new Byte[length];

                            for (int i = 0; i < decoded.Length; ++i)
                            {
                                decoded[i] = (byte)(bytes[i + 2 + key.Length] ^ key[i % 4]);
                            }

                            var text1 = Encoding.UTF8.GetString(decoded);
                            Console.WriteLine("receive {0}", text1);
                            // always response "go!"
                            var encoded = Encoding.UTF8.GetBytes("go!");
                            var response = new byte[2 + key.Length + encoded.Length];
                            response[0] = 129;
                            response[1] = (byte)(encoded.Length + 128);
                            response[2] = key[0];
                            response[3] = key[1];
                            response[4] = key[2];
                            response[5] = key[3];
                            for (int i = 0; i < encoded.Length; ++i)
                            {
                                response[6 + i] = (byte)(encoded[i] ^ key[i % 4]);
                            }
                            await stream.WriteAsync(response, 0, response.Length).ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);
                }
            }
        }
        static void Main()
        {
            Run();
            Console.ReadKey();
        }
    }
}
