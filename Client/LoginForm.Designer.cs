namespace Client
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            nameTextBox = new TextBox();
            loginLabel = new Label();
            passwordTextBox = new TextBox();
            passwordLabel = new Label();
            logInButton = new Button();
            signUpButton = new Button();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // nameTextBox
            // 
            nameTextBox.Enabled = false;
            nameTextBox.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            nameTextBox.Location = new Point(166, 84);
            nameTextBox.MaxLength = 15;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(241, 43);
            nameTextBox.TabIndex = 0;
            nameTextBox.KeyPress += textBox_keyPress;
            // 
            // loginLabel
            // 
            loginLabel.AutoSize = true;
            loginLabel.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            loginLabel.Location = new Point(57, 87);
            loginLabel.Name = "loginLabel";
            loginLabel.Size = new Size(99, 37);
            loginLabel.TabIndex = 1;
            loginLabel.Text = "Логин:";
            // 
            // passwordTextBox
            // 
            passwordTextBox.Enabled = false;
            passwordTextBox.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            passwordTextBox.Location = new Point(166, 172);
            passwordTextBox.MaxLength = 20;
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(241, 43);
            passwordTextBox.TabIndex = 2;
            passwordTextBox.UseSystemPasswordChar = true;
            passwordTextBox.KeyPress += textBox_keyPress;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            passwordLabel.Location = new Point(40, 175);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(116, 37);
            passwordLabel.TabIndex = 3;
            passwordLabel.Text = "Пароль:";
            // 
            // logInButton
            // 
            logInButton.BackColor = Color.MediumPurple;
            logInButton.Enabled = false;
            logInButton.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            logInButton.ForeColor = SystemColors.ButtonHighlight;
            logInButton.Location = new Point(40, 234);
            logInButton.Name = "logInButton";
            logInButton.Size = new Size(367, 49);
            logInButton.TabIndex = 4;
            logInButton.Text = "Войти";
            logInButton.UseVisualStyleBackColor = false;
            logInButton.Click += logInButton_Click;
            // 
            // signUpButton
            // 
            signUpButton.BackColor = Color.MediumPurple;
            signUpButton.Enabled = false;
            signUpButton.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            signUpButton.ForeColor = SystemColors.ButtonHighlight;
            signUpButton.Location = new Point(40, 303);
            signUpButton.Name = "signUpButton";
            signUpButton.Size = new Size(367, 49);
            signUpButton.TabIndex = 5;
            signUpButton.Text = "Зарегистрироваться";
            signUpButton.UseVisualStyleBackColor = false;
            signUpButton.Click += signUpButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(434, 37);
            label1.TabIndex = 6;
            label1.Text = "Мессенджер с AES шифрованием";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.Red;
            label2.Location = new Point(129, 57);
            label2.Name = "label2";
            label2.Size = new Size(185, 15);
            label2.TabIndex = 7;
            label2.Text = "Ожидание включение сервера...";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(449, 381);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(signUpButton);
            Controls.Add(logInButton);
            Controls.Add(passwordLabel);
            Controls.Add(passwordTextBox);
            Controls.Add(loginLabel);
            Controls.Add(nameTextBox);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Вход или Регистрация";
            Shown += LoginForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox nameTextBox;
        private Label loginLabel;
        private TextBox passwordTextBox;
        private Label passwordLabel;
        private Button logInButton;
        private Button signUpButton;
        private Label label1;
        private Label label2;
    }
}