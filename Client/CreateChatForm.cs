namespace Client
{
    public partial class CreateChatForm : Form
    {
        public List<string> list = new List<string>();
        public string users = "";
        public CreateChatForm()
        {
            InitializeComponent();
        }
        public void SetMode(bool single)
        {
            if (single)
            {
                label1.Text = "Выберите участника чата";
                listBox1.SelectionMode = SelectionMode.One;
            }
            else
            {
                label1.Text = "Выберите участников чата";
                listBox1.SelectionMode = SelectionMode.MultiSimple;
            }
        }
        private void ListForm_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = list;
            listBox1.SelectedIndex = -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                foreach (var item in listBox1.SelectedItems)
                {
                    users += item?.ToString()?.Trim() + ",";
                }
                users = users.Remove(users.Length - 1, 1);
                this.Close();
            }
            else
                MessageBox.Show("Выберите участников");
        }
    }
}
