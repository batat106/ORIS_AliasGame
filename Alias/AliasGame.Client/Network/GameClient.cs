using System;
using System.Net.Sockets;
using System.Text;

namespace AliasGame.Client.Network;

public class GameClient
{
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _receiveThread;

    public bool IsConnected => _client?.Connected ?? false;
    
    public event Action<string> OnMessageReceived;
    
    public void Connect(string ip, int port)
    {
        _client = new TcpClient();
        _client.Connect(ip, port);
        _stream = _client.GetStream();
        
        _receiveThread = new Thread(() =>
        {
            byte[] buffer = new byte[1024];

            while (_client.Connected)
            {
                try
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    OnMessageReceived?.Invoke(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }
                catch { break; }
            }
        });
        _receiveThread.Start();
    }
    
    public void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }
    
    public void Disconnect()
    {
        try
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
            _receiveThread?.Join(100); // Ждем завершения потока
        }
        catch { }

    }

}