using System.Timers;
using Alias.Models;

namespace Alias.GameLogic;

public class GameManager
{
    public GameState _gameState;
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

    public void RemovePlayer(string playerId)
    {
        var player = _gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return;

        _timer.Stop();
        _gameState.Players.Remove(player);

        if (_gameState.Players.Count < 2)
        {
            _gameState.CurrentPlayer = null;
            _gameState.CurrentWord = null;
            _gameState.TimeLeft = 0;
        }
        else if (_gameState.CurrentPlayer?.Id == playerId)
        {
            _gameState.CurrentPlayer = GetNextPlayer();
            _gameState.CurrentWord = GetRandomWord();
            _gameState.TimeLeft = 60;
            _timer.Start();
        }
        else
        {
            _timer.Start();
        }

        OnGameStateUpdated?.Invoke(_gameState);
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    public void ProcessPlayerMove(string playerId, bool isCorrect)
    {
        if (_gameState.CurrentPlayer?.Id != playerId) return;
        var player = _gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return;

        if (isCorrect) player.Score++;
        SwitchToNextPlayer();
        OnGameStateUpdated?.Invoke(_gameState);
    }

    private void SwitchToNextPlayer()
    {
        if (_gameState.Players.Count == 0) return;

        _gameState.CurrentPlayer = GetNextPlayer();
        if (_gameState.CurrentPlayer == null) return;

        _gameState.CurrentWord = GetRandomWord();
        _gameState.TimeLeft = 60;
    }

    public bool IsNicknameTaken(string nickname)
    {
        return _gameState.Players.Any(p => p.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));
    }

    public bool AddPlayer(Player player)
    {
        if (IsNicknameTaken(player.Nickname))
        {
            return false;
        }
    
        _gameState.Players.Add(player);
        OnGameStateUpdated?.Invoke(_gameState);
        return true;
    }

    private string GetRandomWord() => _words[_random.Next(_words.Count)];

    private Player GetNextPlayer()
    {
        if (_gameState.Players.Count == 0) return null;

        if (_gameState.CurrentPlayer == null)
        {
            return _gameState.Players[0];
        }

        int currentIndex = _gameState.Players.IndexOf(_gameState.CurrentPlayer);
        if (currentIndex == -1)
        {
            return _gameState.Players[0];
        }

        int nextIndex = (currentIndex + 1) % _gameState.Players.Count;
        return _gameState.Players[nextIndex];
    }
}