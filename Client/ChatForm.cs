using ChatLibrary;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.Json;

namespace Client
{
    public partial class ChatForm : Form
    {
        public Client client;
        public LoginForm LoginForm;
        readonly CreateChatForm listForm = new();
        private bool closeButtonClicked = false;
        private int selectedChat = 0;
        public ChatForm(Client client, LoginForm parentForm, string Name)
        {
            this.client = client;
            LoginForm = parentForm;
            client.MessageReceived += OnMessageReceived;
            client.ChatsListReceived += OnChatsListReceived;
            InitializeComponent();
            nameLabel.Text = Name;
        }
        private void OnChatsListReceived(List<ChatInfo> chatsInfo)
        {
            Invoke(new Action(() =>
            {
                foreach (ChatInfo chatInfo in chatsInfo)
                    AddChatToPanel(chatInfo.ChatID, chatInfo.ChatName, chatInfo.ChatType);
            }));
        }
        private void OnMessageReceived(ChatMessage message)
        {
            switch (message.Type)
            {
                case ChatMessage.MessageType.ALL_LIST:
                    Invoke(new Action(() =>
                    {
                        CreateChat(ChatMessage.MessageType.ALL_LIST);
                    }));
                    break;
                case ChatMessage.MessageType.CONNECTED_LIST:
                    Invoke(new Action(() =>
                    {
                        CreateChat(ChatMessage.MessageType.CONNECTED_LIST);
                    }));
                    break;
                case ChatMessage.MessageType.TEXT:
                    Invoke(new Action(() =>
                    {
                        if (selectedChat == message.ChatID)
                        {
                            AddMessageToPanel(Encoding.Default.GetString(message.Data ?? new byte[0])
                                , message.Sender, message.Time, message.ID, false);
                            chatPanel.VerticalScroll.Value = chatPanel.VerticalScroll.Maximum;
                        }
                    }));
                    break;
                case ChatMessage.MessageType.FILE_SEND:
                    Invoke(new Action(() =>
                    {
                        if (selectedChat == message.ChatID)
                        {
                            AddMessageToPanel(message.FileName, message.Sender, message.Time, message.ID, true);
                            chatPanel.VerticalScroll.Value = chatPanel.VerticalScroll.Maximum;
                        }
                    }));
                    break;
                case ChatMessage.MessageType.FILE_REQUEST:
                    Invoke(new Action(() =>
                    {
                        File.WriteAllBytes(message.FileName, message.Data ?? new byte[0]);
                    }));
                    break;
                case ChatMessage.MessageType.NEW_CHAT:
                    Invoke(new Action(() =>
                    {
                        ChatInfo info = JsonSerializer.Deserialize<ChatInfo>(message.Data) ?? new();
                        AddChatToPanel(info.ChatID, info.ChatName, info.ChatType);
                    }));
                    break;
                case ChatMessage.MessageType.NEW_SECRET_CHAT:
                    Invoke(new Action(() =>
                    {
                        ChatInfo info = JsonSerializer.Deserialize<ChatInfo>(message.Data) ?? new();
                        AddChatToPanel(info.ChatID, info.ChatName, info.ChatType);
                    }));
                    break;
                case ChatMessage.MessageType.CHAT_HISTORY:
                    Invoke(new Action(() =>
                    {
                        List<ChatMessage>? history = JsonSerializer.Deserialize<List<ChatMessage>>(message.Data);
                        if (history != null)
                        {
                            chatPanel.Controls.Clear();
                            foreach (ChatMessage chatMessage in history)
                            {
                                string text = Encoding.Default.GetString(chatMessage.Data ?? new byte[0]);
                                bool isFile = (chatMessage.Type == ChatMessage.MessageType.TEXT) ? false : true;
                                if (isFile)
                                    text = chatMessage.FileName;
                                AddMessageToPanel(text, chatMessage.Sender, chatMessage.Time, chatMessage.ID, isFile);
                            }
                            chatPanel.ScrollControlIntoView(chatPanel.Controls[chatPanel.Controls.Count - 1]);
                        }
                    }));
                    break;
            }
        }

        private Label CreateLabel(string text, int emSize, FontStyle style, DockStyle dock, int maxWidth, string name)
        {
            Label label = new()
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Arial", emSize, style),
                AutoSize = true,
                Dock = dock,
                MaximumSize = new Size(maxWidth, 0),
                Name = name
            };
            return label;
        }

        private Panel CreatePanel(int width, int height, BorderStyle border, int id, int left
            , Cursor cursor, int padding, Panel parentPanel)
        {
            Panel panel = new()
            {
                AutoSize = true,
                Width = width,
                Height = height,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.MediumPurple,
                BorderStyle = border,
                Left = left,
                Top = padding,
                Cursor = cursor
            };
            if (parentPanel.Controls.Count > 0)
                panel.Top = parentPanel.Controls[parentPanel.Controls.Count - 1].Bottom + padding;
            panel.Name = id.ToString();
            return panel;
        }

        private void saveButton_Click(object? sender, EventArgs e)
        {
            var button = sender as Button;
            var panel = button?.Parent as Panel;
            int id = Convert.ToInt32(panel?.Name);
            Label? label = panel?.Controls.Find("text", true).FirstOrDefault() as Label;
            saveFileDialog.FileName = label?.Text;
            string ext = Path.GetExtension(saveFileDialog.FileName);
            if (ext == ".exe" || ext == ".bin" || ext == ".com")
            {
                DialogResult res = MessageBox.Show("Этот файл может причинить вред вашему устройству."
                    , "Предупреждение", MessageBoxButtons.OKCancel);
                if (res != DialogResult.OK)
                {
                    return;
                }
            }
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            try
            {
                client.SendFileRequest(selectedChat, id, saveFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Соединение с сервером прервано");
                this.Close();
            }
        }
        private Button CreateButton()
        {
            Button button = new()
            {
                Text = "Сохранить файл",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Dock = DockStyle.Top,
                BackColor = Color.White,
                ForeColor = Color.MediumPurple,
            };
            button.Click += saveButton_Click;
            return button;
        }

        private void AddRegion(Panel panel)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int borderRadius = 50;
                Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);
                path.AddArc(rect.X, rect.Y, borderRadius, borderRadius, 180, 90);
                path.AddArc(rect.X + rect.Width - borderRadius, rect.Y, borderRadius, borderRadius, 270, 90);
                path.AddArc(rect.X + rect.Width - borderRadius, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 90, 90);
                path.CloseFigure();
                panel.Region = new Region(path);
            }
        }

        private void AddMessageToPanel(string text, string sender, string time, int id, bool isFile)
        {
            int left = 17;
            if (nameLabel.Text == sender)
                left = chatPanel.Width - 400 - left;
            Panel messagePanel = CreatePanel(400, 80, BorderStyle.None, id, left, Cursors.Arrow, 20, chatPanel);
            Label senderLabel = CreateLabel(sender, 12, FontStyle.Bold, DockStyle.Top, messagePanel.Width - 10, "sender");
            Label messageLabel = CreateLabel(text, 12, FontStyle.Regular, DockStyle.Top, messagePanel.Width - 10, "text");
            Label timeLabel = CreateLabel(time, 8, FontStyle.Italic, DockStyle.Bottom, messagePanel.Width - 10, "time");
            messagePanel.Controls.Add(timeLabel);
            if (isFile)
                messagePanel.Controls.Add(CreateButton());
            messagePanel.Controls.Add(messageLabel);
            messagePanel.Controls.Add(senderLabel);
            chatPanel.Controls.Add(messagePanel);
            AddRegion(messagePanel);
        }
        private void AddChatToPanel(int id, string name, string type)
        {
            Panel chatPanel = CreatePanel(chatsPanel.Width - 34, 60, BorderStyle.FixedSingle, id, 17, Cursors.Hand, 10, chatsPanel);
            Label nameLabel = CreateLabel(name, 10, FontStyle.Bold, DockStyle.Top, chatPanel.Width - 10, "name");
            Label typeLabel = CreateLabel(type, 8, FontStyle.Italic, DockStyle.Bottom, chatPanel.Width - 20, "type");
            chatPanel.Controls.Add(nameLabel);
            chatPanel.Controls.Add(typeLabel);
            chatPanel.Click += chatPanel_Click;
            chatsPanel.Controls.Add(chatPanel);
            chatsPanel.VerticalScroll.Value = chatsPanel.VerticalScroll.Maximum;
        }

        private void chatPanel_Click(object? sender, EventArgs e)
        {
            var panel = sender as Panel;
            selectedChat = Convert.ToInt32(panel?.Name);
            try
            {
                client.RequestChatHistory(selectedChat);
            }
            catch (Exception)
            {
                closeButtonClicked = true;
                MessageBox.Show("Соединение с сервером прервано");
                this.Close();
            }
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.Disconnect();
            if (closeButtonClicked)
            {
                LoginForm.Show();
                LoginForm.TryConnectToServer();
            }
            else
                LoginForm.Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            closeButtonClicked = true;
            Close();
        }

        private void CreateChat(ChatMessage.MessageType type)
        {
            listForm.list = client.usersList;
            for (int i = 0; i < listForm.list.Count; i++)
                listForm.list[i] = listForm.list[i].Trim();
            listForm.SetMode(false);
            listForm.list.Remove(client.Name);
            listForm.users = "";
            listForm.ShowDialog();
            if (listForm.users != "")
            {
                ChatMessage.MessageType chatType = ChatMessage.MessageType.NEW_SECRET_CHAT;
                if (type == ChatMessage.MessageType.ALL_LIST)
                    chatType = ChatMessage.MessageType.NEW_CHAT;
                try
                {
                    client.NewChat(listForm.users + "," + client.Name, chatType, listForm.users);
                }
                catch (Exception)
                {
                    closeButtonClicked = true;
                    MessageBox.Show("Соединение с сервером прервано");
                    this.Close();
                }
            }
        }

        private void newChatButton_Click(object sender, EventArgs e)
        {
            try { client.GetUsersList(ChatMessage.MessageType.ALL_LIST); }
            catch (Exception)
            {
                closeButtonClicked = true;
                MessageBox.Show("Соединение с сервером прервано");
                this.Close();
            }
        }

        private void newSecretChatButton_Click(object sender, EventArgs e)
        {
            try { client.GetUsersList(ChatMessage.MessageType.CONNECTED_LIST); }
            catch (Exception)
            {
                closeButtonClicked = true;
                MessageBox.Show("Соединение с сервером прервано");
                this.Close();
            }
        }

        private bool isSecret()
        {
            bool isSecret = false;
            Panel? panel = chatsPanel.Controls.Find(selectedChat.ToString(), true).FirstOrDefault() as Panel;
            Label? label = panel?.Controls.Find("type", true).FirstOrDefault() as Label;
            if (label != null && label.Text == "Секретный")
                isSecret = true;
            return isSecret;
        }
        private void sendTextButton_Click(object sender, EventArgs e)
        {
            if (selectedChat != 0)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
                        client.SendMessageToUsers(Encoding.Default.GetBytes(messageTextBox.Text), DateTime.Now.ToString(), isSecret(), selectedChat);
                    messageTextBox.Text = "";    
                }
                catch (Exception)
                {
                    closeButtonClicked = true;
                    MessageBox.Show("Соединение с сервером прервано");
                    this.Close();
                }
            }
            else
                MessageBox.Show("Выберите чат");
        }

        private void sendFileButton_Click(object sender, EventArgs e)
        {
            if (selectedChat != 0)
            {
                if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                    return;
                string filePath = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSizeInBytes = fileInfo.Length;
                if (fileSizeInBytes > 1024 * 1024 * 10)
                {
                    MessageBox.Show("Файл слишком большой");
                    return;
                }
                Task.Run(() =>
                {
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                    try
                    {
                        client.SendMessageToUsers(fileBytes, DateTime.Now.ToString(), isSecret(), selectedChat, fileName);
                    }
                    catch (Exception)
                    {
                        closeButtonClicked = true;
                        MessageBox.Show("Соединение с сервером прервано");
                        this.Close();
                    }
                });
            }
            else
                MessageBox.Show("Выберите чат");
        }

        private void messageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                sendTextButton_Click(sender, e);
        }
    }
}