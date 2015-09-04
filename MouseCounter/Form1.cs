using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using TaskScheduler;

namespace MouseCounter
{
    public partial class Form1 : Form
    {
        [Serializable]
        class CurrentCounterData
        {
            public DateTime date;
            public int left;
            public int right;
            public int middle;
        }

        MouseHook mh;
        CurrentCounterData data;

        public Form1()
        {
            InitializeComponent();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "current.data");
            try
            {
                data = Utility.Serializer.Deserialize(path) as CurrentCounterData;
                if (CheckDate())
                    SaveData();
            }
            catch (Exception)
            {
                data = new CurrentCounterData();
                data.date = DateTime.Now;
            }

        }

        private bool CheckDate()
        {
            var d1 = data.date;
            var d2 = DateTime.Now;
            return d1.Year != d2.Year || d1.Month != d2.Month || d1.Day != d2.Day;
        }

        private void SaveData()
        {
            if (!CheckDate())
                return;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "history.csv");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "date,left,right,middle\r\n");
            }
            File.AppendAllText(path, string.Format("{0},{1},{2},{3}\r\n", data.date.ToString("yyyy-MM-dd"), data.left, data.right, data.middle));

            data = new CurrentCounterData();
            data.date = DateTime.Now;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            mh = new MouseHook();
            mh.MouseDownEvent += Mh_MouseDownEvent;
            mh.SetHook();

            d(
                data.date.ToString("yyyy-MM-dd"),
                string.Format("左键：{0}", data.left),
                string.Format("右键：{0}", data.right),
                string.Format("中键：{0}", data.middle)
            );

            {
                // 检查当前是否开启开机自启
                TaskScheduler.TaskScheduler scheduler = new TaskScheduler.TaskScheduler();
                scheduler.Connect();
                var folder = scheduler.GetFolder("\\");
                try
                {
                    var task = folder.GetTask("MouseCounter");
                    startupToolStripMenuItem.Checked = task != null;
                }
                catch (Exception)
                {
                    startupToolStripMenuItem.Checked = false;
                }
                
            }

        }

        delegate void D(string s1, string s2, string s3, string s4);
        private void d(string s1, string s2, string s3, string s4)
        {
            label1.Text = s1;
            label2.Text = s2;
            label3.Text = s3;
            label4.Text = s4;
            notifyIcon1.Text = string.Format("MouseCounter\r\n\r\n{0}\r\n{1}\r\n{2}", s2, s3, s4);
        }

        private void Mh_MouseDownEvent(int btnmsg)
        {
            bool b = false;

            switch(btnmsg)
            {
                // WM_LBUTTONDOWN
                case 513:
                    data.left++;
                    b = true;
                    break;
                // WM_RBUTTONDOWN
                case 516:
                    data.right++;
                    b = true;
                    break;
                // WM_MBUTTONDOWN
                case 519:
                    data.middle++;
                    b = true;
                    break;
            }

            if (b)
            {
                SaveData();
                this.Invoke(new D(d), new object[] {
                    data.date.ToString("yyyy-MM-dd"),
                    string.Format("左键：{0}", data.left),
                    string.Format("右键：{0}", data.right),
                    string.Format("中键：{0}", data.middle)
                });
                /*
                label1.Text = btnmsg.ToString();
                label2.Text = string.Format("左键：{0}", data.left);
                label3.Text = string.Format("右键：{0}", data.right);
                label4.Text = string.Format("中键：{0}", data.middle);
                */
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mh.UnHook();
            SaveData();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "current.data");
            Utility.Serializer.Serialize(path, data);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void startupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TaskScheduler.TaskScheduler scheduler = new TaskScheduler.TaskScheduler();
            scheduler.Connect();

            if (startupToolStripMenuItem.Checked)
            {
                // 取消开机启动
                var folder = scheduler.GetFolder("\\");
                folder.DeleteTask("MouseCounter", 0);
                startupToolStripMenuItem.Checked = false;
            }
            else
            {
                // 创建开机启动
                var task = scheduler.NewTask(0);
                task.RegistrationInfo.Description = "MouseCounter";
                task.Settings.Enabled = true;
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Principal.UserId = scheduler.ConnectedUser;
                task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
                task.Settings.ExecutionTimeLimit = "PT0S";

                // 触发器
                var trigger = task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON) as TaskScheduler.ILogonTrigger;
                trigger.Enabled = true;
                trigger.UserId = scheduler.ConnectedUser;

                // 动作
                var action = task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC) as TaskScheduler.IExecAction;
                action.Path = Application.ExecutablePath;
                action.Arguments = "/startup";
                action.WorkingDirectory = Directory.GetCurrentDirectory();

                var folder = scheduler.GetFolder("\\");
                var regTask = folder.RegisterTaskDefinition("MouseCounter", task, (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN);

                startupToolStripMenuItem.Checked = true;
            }




        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // 根据命令行参数检查是否是开机自启，如果是就自动最小化运行
            var cmdline = Environment.CommandLine;
            if (cmdline.ToLower().IndexOf("/startup") != -1)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Visible = false;
            }
        }
    }

    
}
