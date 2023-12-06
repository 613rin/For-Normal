using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace 申请
{
    public partial class Emergency : Form
    {
        public static string EmployeeID { get; set; }
        public SmsBaseClass fSmsBaseClass;
        //刘洋手机号
        private string PhoneNumber = "17625754714";

        public Emergency()
        {
            InitializeComponent();

            fSmsBaseClass = new SmsBaseClass(); // 初始化 fSmsBaseClass 对象
           


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


        //显示密码按钮
        private void button2_Click(object sender, EventArgs e)
        {
            Form1 Form1instance = new Form1();
            DengLu denglu = new DengLu();
            try
            {
                textBox4.Text = EmployeeID;
               
                string machineCode = Form1instance.MachineCode;




                byte mode = 0x00;
                byte blk_add = 0x02;
                byte num_blk = 1;
                byte[] snr = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                byte[] buffer = new byte[16 * num_blk];


                int nRet = denglu.WriteCard(mode, blk_add, num_blk, snr, buffer);
                string first2Bytes = BitConverter.ToString(buffer, 1, 1).Replace("-", "");
                int password = Form1.GeneratePassword(Form1.GenerateDaylyKey(machineCode, DateTime.Now));
                textBox1.Text = password.ToString("D8");
                textBox2.Text= DateTime.Now.ToString("yyyy年MM月dd日");
                string sendemetgnecy= DateTime.Now.ToString("yyyy年MM月dd日HH时mm分钟");

                textBox3.Text = first2Bytes.ToString();
               

                int Cishu = Convert.ToInt32(first2Bytes, 16);
                string employeeName = employeeNames.ContainsKey(EmployeeID) ? employeeNames[EmployeeID] : "未知";

                string message1 = $"{employeeName}：在{sendemetgnecy}使用了一次紧急按钮，该员工共使用紧急功能{Cishu}次";

                if (fSmsBaseClass != null && !fSmsBaseClass.IsOpen)
                {
                    fSmsBaseClass.OpenPort("COM10");

                }
                fSmsBaseClass.SendDTU(PhoneNumber, 0, message1);

                // 写入日志文件
                WriteToLogFile(message1);
            }

            catch(NullReferenceException ex)
            {
                MessageBox.Show("NullReferenceException caught: " + ex.Message);

            }

        }


        //返回Form1窗口
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort(); // 关闭串口
            }
           

            form1.Show();
            this.Hide();

        }
        //记录本地的Log
        private void WriteToLogFile(string message)
        {
            string logFilePath = @"D:\PP\Secre\Secret Demo\旧版\申请\Logs\Emergency.txt";
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(message);
            }
        }

        private void Emergency_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Text = EmployeeID;
            byte mode = 0x00;
            byte blk_add = 0x02;
            byte num_blk = 1;
            byte[] snr = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] buffer = new byte[16 * num_blk];

            int Nret = Reader.MF_Read(mode, blk_add, num_blk, snr, buffer);

            Array.Copy(buffer, DengLu.SharedBuffer, 16);
        }
    }
}
