using System.ComponentModel;

namespace AliasGame.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtNickname;
    private Button btnJoin;
    private Label lblCurrentWord;
    private Label lblTimer;
    private ListBox lstPlayers;
    private Button btnStart;
    private Button btnCorrect;
    private Button btnIncorrect;
    
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.txtNickname = new TextBox();
        this.btnJoin = new Button();
        this.lblCurrentWord = new Label();
        this.lblTimer = new Label();
        this.lstPlayers = new ListBox();
        this.btnStart = new Button();
        this.btnCorrect = new Button();
        this.btnIncorrect = new Button();
        this.AcceptButton = btnJoin;

        // Настройка элементов
        txtNickname.MaxLength = 20;
        txtNickname.PlaceholderText = "Введите ваше имя";
        
        txtNickname.Location = new System.Drawing.Point(20, 20);
        txtNickname.Size = new System.Drawing.Size(150, 20);

        btnJoin.Location = new System.Drawing.Point(180, 20);
        btnJoin.Text = "Присоединиться";

        lblCurrentWord.Location = new System.Drawing.Point(20, 60);
        lblCurrentWord.Size = new System.Drawing.Size(200, 30);
        lblCurrentWord.Text = "Ожидание слова...";

        lblTimer.Location = new System.Drawing.Point(230, 60);
        lblTimer.Text = "60";

        lstPlayers.Location = new System.Drawing.Point(20, 100);
        lstPlayers.Size = new System.Drawing.Size(200, 150);

        btnStart.Location = new System.Drawing.Point(20, 260);
        btnStart.Text = "Начать игру";

        btnCorrect.Location = new System.Drawing.Point(230, 100);
        btnCorrect.Text = "Угадал";

        btnIncorrect.Location = new System.Drawing.Point(230, 130);
        btnIncorrect.Text = "Не угадал";

        // Добавление элементов на форму
        Controls.Add(txtNickname);
        Controls.Add(btnJoin);
        Controls.Add(lblCurrentWord);
        Controls.Add(lblTimer);
        Controls.Add(lstPlayers);
        Controls.Add(btnStart);
        Controls.Add(btnCorrect);
        Controls.Add(btnIncorrect);

        Text = "Alias Game";
        ClientSize = new System.Drawing.Size(350, 300);
        
    }

    #endregion
}