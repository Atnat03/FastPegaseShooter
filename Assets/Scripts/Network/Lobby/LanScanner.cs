using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class LanScanner
{
    private const int PORT = 7777;
    List<string> foundServers = new();

    public Action<List<string>> OnListChange;

    public async Task ScanLocalNetwork()
    {
        string localIP = GetLocalIPAddress();
        string subnet = localIP.Substring(0, localIP.LastIndexOf('.') + 1);

        foundServers.Clear();

        List<Task> tasks = new List<Task>();

        for (int i = 1; i < 255; i++)
        {
            string ip = subnet + i;
            tasks.Add(PingServer(ip));
        }

        await Task.WhenAll(tasks);

        OnListChange?.Invoke(foundServers);
        UnityEngine.Debug.Log($"Serveurs trouvés : {foundServers.Count}");
    }

    private async Task PingServer(string ip)
    {
        using (UdpClient client = new UdpClient())
        {
            client.Client.ReceiveTimeout = 200;
            try
            {
                byte[] ping = Encoding.UTF8.GetBytes("ping");
                await client.SendAsync(ping, ping.Length, ip, PORT);

                var result = await client.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer);
                if (msg == "pong")
                {
                    lock (foundServers)
                    {
                        if (!foundServers.Contains(ip))
                        {
                            foundServers.Add(ip);
                            OnListChange?.Invoke(foundServers);
                        }
                    }
                }
            }
            catch { /* Timeout ou pas de serveur */ }
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }
}