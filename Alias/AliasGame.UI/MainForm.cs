using AliasGame.Client.Network;
using AliasGame.UI.Models;

namespace AliasGame.UI;

public partial class MainForm : Form
{
    private GameClient _gameClient;
    private string _playerId;
    private GameState _currentState;
    private bool _isClosing = false;

    public MainForm()
    {
        InitializeComponent();
        _playerId = Guid.NewGuid().ToString();
        SetupEventHandlers();
        
        // Добавляем обработчик закрытия формы
        this.FormClosing += MainForm_FormClosing;
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
        if (string.IsNullOrWhiteSpace(txtNickname.Text))
        {
            MessageBox.Show("Пожалуйста, введите имя игрока.", 
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _gameClient = new GameClient();
            _gameClient.OnMessageReceived += HandleServerMessage;
            _gameClient.Connect("127.0.0.1", 12345);
            _gameClient.SendMessage($"{GameCommands.PlayerJoin}:{_playerId}:{txtNickname.Text.Trim()}");

            // Временно отключаем элементы управления до получения ответа от сервера
            EnableJoinControls(false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения к серверу: {ex.Message}", 
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            EnableJoinControls(true);
        }
    }

    private void SendMove(bool isCorrect)
    {
        _gameClient.SendMessage($"{GameCommands.PlayerMove}:{_playerId}:{isCorrect}");
    }

    private void HandleServerMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(HandleServerMessage), message);
            return;
        }

        var parts = message.Split(new[] { ':' }, 2);
    
        if (parts[0] == GameCommands.JoinResponse)
        {
            if (parts[1] == "nickname_taken")
            {
                MessageBox.Show("Игрок с таким именем уже присутствует в игре. Пожалуйста, выберите другое имя.", 
                    "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
                // Разблокируем элементы управления для повторного ввода
                EnableJoinControls(true);
                txtNickname.SelectAll(); // Выделяем текст для удобства
                txtNickname.Focus();
                return;
            }
        }
        else if (parts[0] == GameCommands.GameState)
        {
            _currentState = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(parts[1]);
            UpdateUI();
        }
    }
    private void EnableJoinControls(bool enabled)
    {
        txtNickname.Enabled = enabled;
        btnJoin.Enabled = enabled;
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_isClosing && _gameClient?.IsConnected == true)
        {
            _isClosing = true;
            e.Cancel = true; // Временно отменяем закрытие

            try
            {
                // Отправляем команду выхода на сервер
                _gameClient.SendMessage($"{GameCommands.PlayerLeave}:{_playerId}");
                
                // Используем асинхронное ожидание
                Task.Run(async () =>
                {
                    await Task.Delay(300); // Даем время на обработку сообщения
                    
                    this.Invoke(() =>
                    {
                        _gameClient?.Disconnect();
                        this.FormClosing -= MainForm_FormClosing; // Убираем обработчик
                        this.Close(); // Закрываем форму
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения о выходе: {ex.Message}");
                this.Close();
            }
        }
    }

    private void UpdateUI()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateUI));
            return;
        }
        // Показываем слово и кнопки управления только текущему игроку
        bool isCurrentPlayer = _currentState.CurrentPlayer?.Id == _playerId;

        lblCurrentWord.Text = isCurrentPlayer 
            ? _currentState.CurrentWord 
            : $"Объясняет: {_currentState.CurrentPlayer?.Nickname ?? ""}";
        
        btnCorrect.Enabled = isCurrentPlayer;
        btnIncorrect.Enabled = isCurrentPlayer;
        
        lblTimer.Text = _currentState.TimeLeft.ToString();
        lstPlayers.Items.Clear();

        foreach (var player in _currentState.Players)
            lstPlayers.Items.Add($"{player.Nickname}: {player.Score}");
    }
}