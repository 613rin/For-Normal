using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using 申请;
using System.Text.RegularExpressions;


namespace 申请
{
    public partial class Form1 : Form
    {
      
        //刘洋手机号
        private string PhoneNumber = "17625754714";

        //定义一个方法来获取textBox2的text值
        public string MachineCode
        {
            get { return textBox2.Text; }
        }

        private int YanZhengMa;
        private Timer timer = new Timer();
        //为验证码生成时间做定义
        private Timer YanZhengMaTime = new Timer();
        //定义按钮间隔时间
        private Timer buttonTimer = new Timer();
        public SmsBaseClass fSmsBaseClass;
        private DateTime lastClickTime;
       
        DengLu denglu = new DengLu();
      

        //定义机台号
        public string machineCode = null;
        //定义申请人员的ID
        public string enteredEmployeeID = "";
        //定义保存log的位置
        private const string LogFolderPath = @"D:\PP\Secre\Secret Demo\旧版\申请\Logs";
        private const string LogFilePath = LogFolderPath + @"\Normal.txt";

       

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
            TimeSpan timeRemaining = nextHour - now;
            textBox4.Text = $"{timeRemaining.Minutes}min {timeRemaining.Seconds}s";
        }

        #region  4种权重密码的算法
        /// <summary>
        /// 生成权重一的密码，每年更新一次
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GenerateYearlyKey(string machineCode, DateTime dateTime)
        {
            return dateTime.ToString("yyyy") + machineCode;
        }

       /// <summary>
       ///  生成权重二的密码，每月更新一次
       /// </summary>
       /// <param name="machineCode"></param>
       /// <param name="dateTime"></param>
       /// <returns></returns>
        public static string GenerateMonthlyKey(string machineCode, DateTime dateTime)
        {
            return dateTime.ToString("yyyyMM") + machineCode;
        }

        /// <summary>
        /// 权重三的密码，每天更新一次
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GenerateDaylyKey(string machineCode, DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd") + machineCode;
        }

        /// <summary>
        /// 生成权重四的密码，每小时更新一次密码
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GenerateKey(string machineCode, DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHH") + machineCode;
        }


        #endregion
        public static int GeneratePassword(string key)
        {
            int password = 0;
            foreach (char c in key)
            {
                password ^= c;
                password = (password << 1) | (password >> 31); // 位旋转
                password ^= (int)c * 31; // 乘质数以增加密码的复杂性
            }
            return Math.Abs(password % 100000000); // 将计算后的数字取模，以保证是8位数字的正数
        }


        #region   Log日志储存
        /// <summary>
        /// 用来储存访问记录LOG
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="projectCode"></param>
        /// <param name="accessGranted"></param>
        public static string EmployeeID { get; set; }
        public static void LogAccess(string employeeID, string projectCode, bool accessGranted)
        {
            ;
            string status = accessGranted ? "申请通过" : "申请失败";
            string logEntry = $"{DateTime.Now}: 申请ID号: {EmployeeID},   申请状态: {status},    申请的机台号 : {projectCode}";

            // Ensure the directory exists
            if (!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }

            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
        }

        #endregion
        public Form1()
        {

            InitializeComponent();
          
            fSmsBaseClass = new SmsBaseClass(); // 初始化 fSmsBaseClass 对象
            timer.Interval = 1000; // 设置定时器的时间间隔为1秒
            timer.Tick += Timer_Tick; // 添加Tick事件处理器
            //验证码每一小时刷新一次
            YanZhengMaTime.Interval = 60 * 60 * 1000;
            YanZhengMaTime.Tick += YanZhengMaTime_Tick;
            GenerateYanZhengMa();
            //按钮间隔时间30s
            buttonTimer.Interval = 30000; // 设置定时器的时间间隔为30秒
            buttonTimer.Tick += ButtonTimer_Tick; // 添加Tick事件处理器
            #region  打开4G模块的串口
            fSmsBaseClass = new SmsBaseClass(); // 初始化对象
            this.Leave += new EventHandler(Form1_Leave);
            

            #endregion
        }

        

        public void SetProjectNumber(string projectNumber)
        {
            textBox2.Text = projectNumber;
        }



        /// <summary>
        /// 生成验证码的函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YanZhengMaTime_Tick(object sender, EventArgs e)
        {
            
            GenerateYanZhengMa();
        }
        /// <summary>
        /// 按钮间隔函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTimer_Tick(object sender, EventArgs e)
        {
            button2.Enabled = true; // 启用按钮
            buttonTimer.Stop(); // 停止定时器
        }

        private void GenerateYanZhengMa()
        {
            Random random = new Random();
            YanZhengMa = random.Next(10000, 100000); // 生成一个5位的随机数
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
             machineCode = textBox2.Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

       

        #region   申请密码的按钮部分
        private void button1_Click(object sender, EventArgs e)
        {
            bool accessGranted = false; // 默认为不允许访问
            string userEnteredCode = textBox5.Text;
          

         
                //检查此时的机台栏是否为空
                if (!string.IsNullOrEmpty(machineCode))
                {
                    //检查验证码是否正确
                    if (!string.IsNullOrEmpty(userEnteredCode) && int.TryParse(userEnteredCode, out int parparsedCode) && parparsedCode == YanZhengMa)
                    {
                        GeneratePassword(machineCode);
                        label1.Text = "Pass";
                        label1.BackColor = Color.Lime;
                        // 使用当前时间和机台编号生成密码
                        //每小时生成的密码
                        int password = GeneratePassword(GenerateKey(machineCode, DateTime.Now));
                        textBox3.Text = password.ToString("D8");

                        accessGranted = true; // 允许访问

                        // 计算距离下一个整点的时间差
                        DateTime now = DateTime.Now;
                        DateTime nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
                        TimeSpan timeRemaining = nextHour - now;
                        timer.Start(); // 启动定时器
                    }
                    else
                    {
                        label1.Text = "Fail";
                        label1.BackColor = Color.Red;
                        textBox3.Text = "请输入正确的验证码";
                        return; // 如果验证码不正确，直接返回，不再执行后续代码


                    }
                }
                else
                {
                    label1.Text = "Fail";
                    label1.BackColor = Color.Red;
                    textBox2.Text = "请输入机台号";
                    timer.Stop(); // 停止定时器
                    textBox4.Text = "";
                }
           
            //记录申请访问记录
            LogAccess(enteredEmployeeID, machineCode, accessGranted);
        }

        #endregion

        /// <summary>
        /// 验证法生成算法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        #region 工号名字对应关系
        /// <summary>
        /// 用来管理工号和名称对应关系
        /// </summary>
        private Dictionary<string, string> employeeNames = new Dictionary<string, string>
           {
            { "622003", "李云" },
            { "621003", "丁煜哲" },
           };
        #endregion

        #region   申请验证码的按钮部分
        private void button2_Click(object sender, EventArgs e)
        {
            string employeeName = employeeNames.ContainsKey(EmployeeID) ? employeeNames[EmployeeID] : "未知";
            string message = $"{employeeName}：{YanZhengMa}";
            //检查此时的机台栏是否为空
            if (!string.IsNullOrEmpty(machineCode))
                {

                if (fSmsBaseClass != null && !fSmsBaseClass.IsOpen)
                {
                    fSmsBaseClass.OpenPort("COM10");
                }

                DateTime now = DateTime.Now;
                    if ((now - lastClickTime).TotalSeconds < 30)
                    {
                        MessageBox.Show("点击太快，请等待30秒再点击。");
                        return;
                    }
                    //发送验证码的函数
                    fSmsBaseClass.SendDTU(PhoneNumber, 0, message);

                   //调试用窗口显示验证码
                    textBox6.Text = Convert.ToString(YanZhengMa);
                   
                    label1.Text = "Waiting";
                    label1.BackColor = Color.Yellow;
                    button2.Enabled = false; // 禁用按钮
                    buttonTimer.Start(); // 启动定时器
                    lastClickTime = now; // 更新上次点击的时间
                }
                else
                {
                    label1.Text = "Fail";
                    label1.BackColor = Color.Red;
                    textBox2.Text = "请输入机台编号";
                    timer.Stop(); // 停止定时器
                    textBox4.Text = "";
                }


          
        }

        #endregion

       
        #region 定义关闭软件时候触发事件
        /// <summary>
        /// 软件关闭的时候顺便把4G模块的串口给关了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort();
            }
            Application.Exit();
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort(); // 关闭串口
            }
        }
        #endregion  


        //前往紧急窗口
        private void button3_Click(object sender, EventArgs e)
        {
            
            Emergency emergency = new Emergency();
            fSmsBaseClass.ClosePort();
            emergency.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort(); // 关闭串口
            }
            denglu.Show();
            this.Hide();
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort(); // 关闭串口
            }
            Projects projects = new Projects();

            //OpenFileDialog openFileDialog = new OpenFileDialog
            //{
            //    Filter = "Excel Files|*.xlsx;*.xls",
            //    Title = "Select an Excel File"
            //};

            //if (openFileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    string selectedFilePath = openFileDialog.FileName;
            //    projects.LoadData(selectedFilePath);
            //}
          
            projects.Show();
            this.Hide();
        }
        
    }
}
