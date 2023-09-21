using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace MyMinecraftServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        public void LogOnTextbox(string s)
        {
            textBox1.Text += s + "\r\n";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogOnTextbox("\n");
            try
            {
                foreach (UserData u in Program.allUserData)
                {
                    LogOnTextbox(JsonConvert.SerializeObject(u));
                }
            }
            catch
            {
                LogOnTextbox("User list is was modified");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            LogOnTextbox("\nMessage Count:" + Program.toDoList.Count.ToString());
            LogOnTextbox("\nMessage Count:" + Program.toDoList2.Count.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<Vector2Int, Chunk> c in Program.chunks)
            {
                LogOnTextbox(JsonConvert.SerializeObject(c.Key));
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Program.LoadApp();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.StopServer();
        }
    }
}
