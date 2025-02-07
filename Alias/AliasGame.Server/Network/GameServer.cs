using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Alias.GameLogic;
using Alias.Models;
using System.Text.Json;

namespace Alias.Network;

public class GameServer
{
    private TcpListener _listener;
    private GameManager _gameManager;
    private List<TcpClient> _clients = new List<TcpClient>();

    public GameServer(string ip, int port, List<string> words)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _gameManager = new GameManager(words);
        _gameManager.OnGameStateUpdated += OnGameStateUpdated;
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Server started");

        new Thread(() =>
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                _clients.Add(client);
                new Thread(() => HandleClient(client)).Start();
            }
        }).Start();
    }

    private void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessClientMessage(message);
            }
        }
        catch
        {
        }
        finally
        {
            client.Close();
            _clients.Remove(client);
        }
    }

    private void ProcessClientMessage(string message)
    {
        var parts = message.Split(':');
        switch (parts[0])
        {
            case GameCommands.PlayerJoin:
                var player = new Player { Id = parts[1], Nickname = parts[2] };
                _gameManager.AddPlayer(player);
                break;
            case GameCommands.PlayerMove:
                _gameManager.ProcessPlayerMove(parts[1], bool.Parse(parts[2]));
                break;
            case GameCommands.StartGame:
                _gameManager.StartGame();
                break;
        }
    }

    private void OnGameStateUpdated(GameState gameState)
    {
        var data = Encoding.UTF8.GetBytes(
            $"{GameCommands.GameState}:{Newtonsoft.Json.JsonConvert.SerializeObject(gameState)}");
        foreach (var client in _clients)
        {
            if (client.Connected)
                client.GetStream().Write(data, 0, data.Length);
        }
    }
    /*private void OnGameStateUpdated(GameState gameState)
    {
        var stateForAll = new GameState
        {
            Players = gameState.Players,
            CurrentPlayer = gameState.CurrentPlayer,
            TimeLeft = gameState.TimeLeft,
            CurrentWord = "" // Скрываем слово для всех, кроме текущего игрока
        };

        var stateForCurrentPlayer = new GameState
        {
            Players = gameState.Players,
            CurrentPlayer = gameState.CurrentPlayer,
            TimeLeft = gameState.TimeLeft,
            CurrentWord = gameState.CurrentWord // Показываем слово текущему игроку
        };

        var dataForAll =
            Encoding.UTF8.GetBytes(
                $"{GameCommands.GameState}:{Newtonsoft.Json.JsonConvert.SerializeObject(stateForAll)}");
        var dataForCurrentPlayer =
            Encoding.UTF8.GetBytes(
                $"{GameCommands.GameState}:{Newtonsoft.Json.JsonConvert.SerializeObject(stateForCurrentPlayer)}");

        byte[] buffer = new byte[1024]; // Добавляем объявление переменной buffer

        foreach (var client in _clients)
        {
            if (!client.Connected) continue;

            try
            {
                // Читаем идентификатор игрока из потока
                int bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;

                string playerId = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Отправляем состояние игры
                if (playerId == gameState.CurrentPlayer.Id)
                {
                    client.GetStream().Write(dataForCurrentPlayer, 0, dataForCurrentPlayer.Length);
                }
                else
                {
                    client.GetStream().Write(dataForAll, 0, dataForAll.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке данных клиенту: {ex.Message}");
            }
        }
    }*/
}