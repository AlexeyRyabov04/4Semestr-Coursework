using System.Security.Cryptography;
using System.Text;
using ChatLibrary;

namespace Client
{
    public partial class LoginForm : Form
    {
        public Client client = new();
        public LoginForm()
        {
            InitializeComponent();
        }
        private string HashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    builder.Append(hashBytes[i].ToString("X2"));
                return builder.ToString();
            }
        }

        private async void JoinToMessanger(ChatMessage.MessageType type)
        {
            try
            {
                if (nameTextBox.Text.Length < 4 || passwordTextBox.Text.Length < 8)
                    MessageBox.Show("Неверено заполнены поля. Логин должен содержать не менее 4 символов, " +
                        "а пароль - не менее 8");
                else
                {
                    bool isConnected = await client.Authorization(nameTextBox.Text
                        , HashString(passwordTextBox.Text), type);
                    if (isConnected)
                    {
                        ChatForm chat = new ChatForm(client, this, nameTextBox.Text);
                        chat.client = client;
                        chat.Show();
                        this.Hide();
                    }
                    else if (type == ChatMessage.MessageType.LOGIN)
                        MessageBox.Show("Не удалось зайти в мессенджер. Возможно," +
                            " пользователь с таким именем уже в сети, либо введены неверные данные");
                    else if (type == ChatMessage.MessageType.SIGNUP)
                        MessageBox.Show("Пользователь с таким именем уже зарегистрирован");
                }
            }
            catch (Exception) { TryConnectToServer(); }
        }
        private void logInButton_Click(object sender, EventArgs e)
        {
            JoinToMessanger(ChatMessage.MessageType.LOGIN);
        }

        private void signUpButton_Click(object sender, EventArgs e)
        {
            JoinToMessanger(ChatMessage.MessageType.SIGNUP);
        }

        public void TryConnectToServer()
        {
            Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    nameTextBox.Enabled = false;
                    passwordTextBox.Enabled = false;
                    logInButton.Enabled = false;
                    signUpButton.Enabled = false;
                    label2.Visible = true;
                    client = new Client();
                }));
                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        client.ConnectToServer("127.0.0.1", 5555);
                        client.GenerateSessionKey();
                        isConnected = true;
                        Invoke(new Action(() =>
                        {
                            nameTextBox.Enabled = true;
                            passwordTextBox.Enabled = true;
                            logInButton.Enabled = true;
                            signUpButton.Enabled = true;
                            label2.Visible = false;
                        }));
                    }
                    catch (Exception) { }
                }
            });
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        client.ConnectToServer("127.0.0.1", 5555);
                        client.GenerateSessionKey();
                        isConnected = true;
                        Invoke(new Action(() =>
                        {
                            nameTextBox.Enabled = true;
                            passwordTextBox.Enabled = true;
                            logInButton.Enabled = true;
                            signUpButton.Enabled = true;
                            label2.Visible = false;
                        }));
                    }
                    catch (Exception) { }
                }
            });
        }
        private void textBox_keyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
                e.Handled = true;
        }
    }

}
