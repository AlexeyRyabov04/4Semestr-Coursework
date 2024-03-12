using ChatLibrary;
using System.Text;

namespace Server
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
            Server server = new Server();
            Task.Run(() => server.StartServer());
            server.TextMessageReceived += OnTextMessageReceived;
        }
        private void OnTextMessageReceived(ChatMessage message)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.Text += message.Sender + ": " + Encoding.Default
                .GetString(message.Data ?? new byte[0]) + Environment.NewLine;
            }));
        }
    }
}