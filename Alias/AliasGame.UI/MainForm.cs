using AliasGame.Client.Network;
using AliasGame.UI.Models;

namespace AliasGame.UI;

public partial class MainForm : Form
{
    private GameClient _gameClient;
    private string _playerId;
    private GameState _currentState;
    public MainForm()
    {
        InitializeComponent();
        _playerId = Guid.NewGuid().ToString();
        SetupEventHandlers();
    }
    
    private void SetupEventHandlers()
    {
        btnJoin.Click += (s, e) => ConnectToServer();
        btnStart.Click += (s, e) => _gameClient.SendMessage(GameCommands.StartGame);
        btnCorrect.Click += (s, e) => SendMove(true);
        btnIncorrect.Click += (s, e) => SendMove(false);
    }
    
    private void ConnectToServer()
    {
        _gameClient = new GameClient();
        _gameClient.OnMessageReceived += HandleServerMessage;
        _gameClient.Connect("127.0.0.1", 12345);
        _gameClient.SendMessage($"{GameCommands.PlayerJoin}:{_playerId}:{txtNickname.Text}");
        
        txtNickname.Enabled = false;
        btnJoin.Enabled = false;
    }

    private void SendMove(bool isCorrect)
    {
        _gameClient.SendMessage($"{GameCommands.PlayerMove}:{_playerId}:{isCorrect}");
    }

    private void HandleServerMessage(string message)
    {
        var parts = message.Split(new[] { ':' }, 2);
        if (parts[0] != GameCommands.GameState) return;

        _currentState = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(parts[1]);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateUI));
            return;
        }

        lblCurrentWord.Text = _currentState.CurrentWord;
        lblTimer.Text = _currentState.TimeLeft.ToString();
        lstPlayers.Items.Clear();
        foreach (var player in _currentState.Players)
            lstPlayers.Items.Add($"{player.Nickname}: {player.Score}");
    }
}