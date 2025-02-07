namespace Alias.Models;

public class GameState
{
    public List<Player> Players { get; set; } = new List<Player>();
    public string CurrentWord { get; set; }
    public Player CurrentPlayer { get; set; }
    public int TimeLeft { get; set; }
}