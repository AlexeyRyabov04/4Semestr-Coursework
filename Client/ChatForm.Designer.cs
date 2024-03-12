namespace Client
{
    partial class ChatForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            chatsPanel = new Panel();
            chatPanel = new Panel();
            closeButton = new Button();
            messageTextBox = new TextBox();
            label1 = new Label();
            nameLabel = new Label();
            label2 = new Label();
            sendTextButton = new Button();
            sendFileButton = new Button();
            newChatButton = new Button();
            newSecretChatButton = new Button();
            label3 = new Label();
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            SuspendLayout();
            // 
            // chatsPanel
            // 
            chatsPanel.AutoScroll = true;
            chatsPanel.BackColor = SystemColors.ButtonHighlight;
            chatsPanel.Location = new Point(27, 61);
            chatsPanel.Name = "chatsPanel";
            chatsPanel.Size = new Size(219, 425);
            chatsPanel.TabIndex = 0;
            // 
            // chatPanel
            // 
            chatPanel.AutoScroll = true;
            chatPanel.BackColor = SystemColors.ButtonHighlight;
            chatPanel.Location = new Point(289, 61);
            chatPanel.Name = "chatPanel";
            chatPanel.Size = new Size(871, 425);
            chatPanel.TabIndex = 1;
            // 
            // closeButton
            // 
            closeButton.BackColor = Color.MediumPurple;
            closeButton.Cursor = Cursors.Hand;
            closeButton.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            closeButton.ForeColor = SystemColors.ButtonHighlight;
            closeButton.Location = new Point(1038, 12);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(122, 43);
            closeButton.TabIndex = 2;
            closeButton.Text = "Выход";
            closeButton.UseVisualStyleBackColor = false;
            closeButton.Click += closeButton_Click;
            // 
            // messageTextBox
            // 
            messageTextBox.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            messageTextBox.Location = new Point(289, 527);
            messageTextBox.Name = "messageTextBox";
            messageTextBox.Size = new Size(607, 33);
            messageTextBox.TabIndex = 3;
            messageTextBox.KeyDown += messageTextBox_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 38);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 4;
            label1.Text = "Ваши чаты:";
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            nameLabel.Location = new Point(27, 13);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(53, 25);
            nameLabel.TabIndex = 5;
            nameLabel.Text = "label";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(289, 38);
            label2.Name = "label2";
            label2.Size = new Size(29, 15);
            label2.TabIndex = 6;
            label2.Text = "Чат:";
            // 
            // sendTextButton
            // 
            sendTextButton.BackColor = Color.MediumPurple;
            sendTextButton.Cursor = Cursors.Hand;
            sendTextButton.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            sendTextButton.ForeColor = SystemColors.ControlLightLight;
            sendTextButton.Location = new Point(902, 527);
            sendTextButton.Name = "sendTextButton";
            sendTextButton.Size = new Size(126, 33);
            sendTextButton.TabIndex = 7;
            sendTextButton.Text = "Отправить текст";
            sendTextButton.UseVisualStyleBackColor = false;
            sendTextButton.Click += sendTextButton_Click;
            // 
            // sendFileButton
            // 
            sendFileButton.BackColor = Color.MediumPurple;
            sendFileButton.Cursor = Cursors.Hand;
            sendFileButton.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            sendFileButton.ForeColor = SystemColors.ControlLightLight;
            sendFileButton.Location = new Point(1034, 526);
            sendFileButton.Name = "sendFileButton";
            sendFileButton.Size = new Size(126, 33);
            sendFileButton.TabIndex = 8;
            sendFileButton.Text = "Отправить файл";
            sendFileButton.UseVisualStyleBackColor = false;
            sendFileButton.Click += sendFileButton_Click;
            // 
            // newChatButton
            // 
            newChatButton.BackColor = Color.MediumPurple;
            newChatButton.Cursor = Cursors.Hand;
            newChatButton.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            newChatButton.ForeColor = SystemColors.ButtonHighlight;
            newChatButton.Location = new Point(27, 492);
            newChatButton.Name = "newChatButton";
            newChatButton.Size = new Size(219, 30);
            newChatButton.TabIndex = 9;
            newChatButton.Text = "Новый чат";
            newChatButton.UseVisualStyleBackColor = false;
            newChatButton.Click += newChatButton_Click;
            // 
            // newSecretChatButton
            // 
            newSecretChatButton.BackColor = Color.MediumPurple;
            newSecretChatButton.Cursor = Cursors.Hand;
            newSecretChatButton.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            newSecretChatButton.ForeColor = SystemColors.ButtonHighlight;
            newSecretChatButton.Location = new Point(27, 530);
            newSecretChatButton.Name = "newSecretChatButton";
            newSecretChatButton.Size = new Size(219, 30);
            newSecretChatButton.TabIndex = 10;
            newSecretChatButton.Text = "Новый секретный чат";
            newSecretChatButton.UseVisualStyleBackColor = false;
            newSecretChatButton.Click += newSecretChatButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(289, 507);
            label3.Name = "label3";
            label3.Size = new Size(120, 15);
            label3.TabIndex = 11;
            label3.Text = "Введите сообщение:";
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog1";
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1197, 583);
            Controls.Add(label3);
            Controls.Add(newSecretChatButton);
            Controls.Add(newChatButton);
            Controls.Add(sendFileButton);
            Controls.Add(sendTextButton);
            Controls.Add(label2);
            Controls.Add(nameLabel);
            Controls.Add(label1);
            Controls.Add(messageTextBox);
            Controls.Add(closeButton);
            Controls.Add(chatPanel);
            Controls.Add(chatsPanel);
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Мессенджер с AES шифрованием";
            FormClosed += ChatForm_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel chatsPanel;
        private Panel chatPanel;
        private Button closeButton;
        private TextBox messageTextBox;
        private Label label1;
        private Label nameLabel;
        private Label label2;
        private Button sendTextButton;
        private Button sendFileButton;
        private Button newChatButton;
        private Button newSecretChatButton;
        private Label label3;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
    }
}