using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            setTag(this);
        }

        public void LogOnTextbox(string s)
        {
            textBox1.Text += s + "\r\n";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Select(this.textBox1.TextLength, 0);//光标定位到文本最后
            textBox1.ScrollToCaret();//滚动到光标处
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
            foreach (var tdl in Program.toDoLists)
            {
                LogOnTextbox("Messages In List " + Program.toDoLists.IndexOf(tdl).ToString() + " Count: " + tdl.Count.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LogOnTextbox("Chunks Count:" + Program.chunks.Count.ToString());


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Program.LoadApp();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.StopServer();
        }



        private float x = 640f;//定义当前窗体的宽度
        private float y = 480f;//定义当前窗体的高度
        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0)
                {
                    setTag(con);
                }
            }
        }
        private void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    string[] mytag = con.Tag.ToString().Split(new char[] { ';' });
                    //根据窗体缩放的比例确定控件的值
                    //      Debug.WriteLine(mytag[0]);
                    //       Debug.WriteLine(newx);
                    con.Width = Convert.ToInt32(System.Convert.ToInt32(mytag[0]) * newx);//宽度
                    con.Height = Convert.ToInt32(System.Convert.ToInt32(mytag[1]) * newy);//高度
                    con.Left = Convert.ToInt32(System.Convert.ToInt32(mytag[2]) * newx);//左边距
                    con.Top = Convert.ToInt32(System.Convert.ToInt32(mytag[3]) * newy);//顶边距
                    Single currentSize = System.Convert.ToInt32(mytag[4]) * newy;//字体大小
                    con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    if (con.Controls.Count > 0)
                    {
                        setControls(newx, newy, con);
                    }
                }
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            float xMultiplier = (this.Width) / x;
            float yMultiplier = (this.Height) / y;
            setControls(xMultiplier, yMultiplier, this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EntityBeh.SpawnNewEntity(new System.Numerics.Vector3(0f, 100f, 0f),0f,0f,0f, 0);
        }
    }
}
