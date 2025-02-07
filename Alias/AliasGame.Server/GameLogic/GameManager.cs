using System.Timers;
using Alias.Models;

namespace Alias.GameLogic;

public class GameManager
{
    private GameState _gameState;
    private System.Timers.Timer _timer;
    private Random _random;
    private List<string> _words;

    public event Action<GameState> OnGameStateUpdated;

    public GameManager(List<string> words)
    {
        _gameState = new GameState();
        _random = new Random();
        _words = words;

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) => 
        {
            _gameState.TimeLeft--;
            if (_gameState.TimeLeft <= 0) SwitchToNextPlayer();
            OnGameStateUpdated?.Invoke(_gameState);
        };
    }

    public void StartGame()
    {
        if (_gameState.Players.Count < 2) return;

        _gameState.CurrentPlayer = _gameState.Players[_random.Next(_gameState.Players.Count)];
        _gameState.TimeLeft = 60;
        _gameState.CurrentWord = GetRandomWord();
        _timer.Start();
        OnGameStateUpdated?.Invoke(_gameState);
    }

    public void ProcessPlayerMove(string playerId, bool isCorrect)
    {
        if (_gameState.CurrentPlayer.Id != playerId) return;

        if (isCorrect) _gameState.CurrentPlayer.Score++;
        SwitchToNextPlayer();
        OnGameStateUpdated?.Invoke(_gameState);
    }

    private void SwitchToNextPlayer()
    {
        int index = _gameState.Players.IndexOf(_gameState.CurrentPlayer);
        _gameState.CurrentPlayer = _gameState.Players[(index + 1) % _gameState.Players.Count];
        _gameState.CurrentWord = GetRandomWord();
        _gameState.TimeLeft = 60;
    }

    public void AddPlayer(Player player)
    {
        _gameState.Players.Add(player);
        OnGameStateUpdated?.Invoke(_gameState);
    }

    private string GetRandomWord() => _words[_random.Next(_words.Count)];
}