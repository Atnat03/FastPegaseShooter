using UnityEngine;
using System.Net.Sockets;
using System.Text;

namespace Network.Lobby
{
    public class UDPServer : MonoBehaviour
    {
        private const int PORT = 7777;
        private UdpClient _udp;

        private void Start()
        {
            _udp = new UdpClient(PORT);
            Listen();
        }

        private async void Listen()
        {
            while (true)
            {
                var result = await _udp.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer);
                if (msg == "ping")
                {
                    byte[] pong = Encoding.UTF8.GetBytes("pong");
                    await _udp.SendAsync(pong, pong.Length, result.RemoteEndPoint);
                }
            }
        }

        private void OnDestroy()
        {
            _udp.Close();
        }
    }
}