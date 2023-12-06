using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace 申请
{
    public partial class DengLu : Form
    {
        //定义可以访问人员的工号
        public string[] EmployeeID = new string[] { "622003", "621003" };
        public string enteredEmployeeID = "";
        public static byte[] SharedBuffer = new byte[16];  // 公共静态变量
        private System.Windows.Forms.Timer readCardTimer;
        public static string EnteredEmployeeID { get;  set; }
        private DateTime startTime;
        public DengLu()
        {
            InitializeComponent();
            readCardTimer = new Timer();
            readCardTimer.Interval = 1500; // 每1.5秒尝试读取一次
            readCardTimer.Tick += ReadCardTimer_Tick;
            startTime = DateTime.Now; // 设置startTime为当前时间
            readCardTimer.Start(); // 启动定时器

        }

        public void ResetReadCardTimer()
        {
            readCardTimer.Stop();
            readCardTimer.Start();

        }

        #region 读工号的程序
        private void ReadCardTimer_Tick(object sender, EventArgs e)
        {
            byte mode = 0x00;
            byte blk_add = 0x01;
            byte num_blk = 1;
            byte[] snr = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            byte blk_add2 = 0x02;

            byte[] buffer = new byte[16 * num_blk];


            int nRet = Reader.MF_Read(mode, blk_add, num_blk, snr, buffer);



            Console.WriteLine(nRet);

            if (nRet == 0)
            {
                string first3Bytes = BitConverter.ToString(buffer, 0, 3).Replace("-", "");
                Console.WriteLine(first3Bytes);

                // 在调用Invoke之前检查句柄是否已创建
                if (this.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        textBox1.Text = first3Bytes;

                        enteredEmployeeID = textBox1.Text;

                    }));
                }
                readCardTimer.Stop();
            }

        }
        private void convertStr(byte[] after, string before, int length)
        {
            for (int i = 0; i < length; i++)
            {
                after[i] = Convert.ToByte(before.Substring(2 * i, 2), 16);
            }
        }


        private string formatStr(string str, int num_blk)
        {

            string tmp = Regex.Replace(str, "[^a-fA-F0-9]", "");
            //长度不对直接报错
            //num_blk==-1指示不用检查位数
            if (num_blk == -1) return tmp;
            //num_blk==其它负数，为-1/num_blk
            if (num_blk < -1)
            {
                if (tmp.Length != -16 / num_blk * 2) return null;
                else return tmp;
            }
            if (tmp.Length != 16 * num_blk * 2) return null;
            else return tmp;
        }
        #endregion

        //往ID卡中写数据
        public int WriteCard(byte mode, byte blk_add, byte num_blk, byte[] snr, byte[] buffer1)
        {
            byte[] StayData = new byte[16];
            Array.Copy(SharedBuffer, buffer1, 16);
            Array.Copy(buffer1, StayData, 16);

            if (StayData[1] > 15)
            {
                MessageBox.Show("此卡已达到最大紧急次数，请返回集创重置");

            }

            string writeData = ConstructWriteData(StayData[1]);

          

            string bufferStr = formatStr(writeData, num_blk);

            convertStr(buffer1, bufferStr, 16 * num_blk);

            int nRet = Reader.MF_Write(mode, blk_add, num_blk, snr, buffer1);


            return nRet;

        }

        private string ConstructWriteData(byte emergencyCount)
        {

            switch (emergencyCount)
            {
                #region 写紧急次数部分

                case 0: return "FF 01 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 1: return "FF 02 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 2: return "FF 03 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 3: return "FF 04 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 4: return "FF 05 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 5: return "FF 06 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 6: return "FF 07 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 7: return "FF 08 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 8: return "FF 09 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 9: return "FF 0A FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 10: return "FF 0B FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 11: return "FF 0C FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 12: return "FF 0D FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 13: return "FF 0E FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 14: return "FF 0F FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 15: return "FF 10 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                case 16: return "FF 11 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
                default: return "FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            }
            #endregion
        }



        //    Array.Copy(buffer1, StayData, 16);

        //    if (StayData[1] > 15)
        //    {
        //        MessageBox.Show("此卡已达到最大紧急次数，请返回集创重置");
        //        return nRet;
        //    }

        //    string writeData = ConstructWriteData(StayData[1]);


        //    // 准备数据进行写入
        //    string bufferStr = formatStr(writeData, num_blk);
        //    convertStr(buffer1, bufferStr, 16 * num_blk);

        //    nRet = Reader.MF_Write(mode, blk_add, num_blk, snr, buffer1);

        //    return nRet;
        //}


        //private string ConstructWriteData(byte emergencyCount)
        //{

        //    switch (emergencyCount)
        //    {
        //        #region 写紧急次数部分

        //        case 0: return "FF 01 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 1: return  "FF 02 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 2: return  "FF 03 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
        //        case 3: return  "FF 04 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 4: return  "FF 05 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 5: return  "FF 06 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
        //        case 6: return  "FF 07 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 7: return  "FF 08 FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
        //        case 8: return  "FF 09 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 9: return  "FF 0A FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 10: return  "FF 0B FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 11: return  "FF 0C FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 12: return  "FF 0D FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 13: return " FF 0E FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 14: return  "FF 0F FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 15: return  "FF 10 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        case 16: return  "FF 11 FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //        default: return  "FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF"; 
        //    }
        //    #endregion
        //}







        private void button1_Click(object sender, EventArgs e)
        {
           

            if (EmployeeID.Contains(enteredEmployeeID))
            {

                Form1.EmployeeID = enteredEmployeeID;
                Emergency.EmployeeID = enteredEmployeeID;
                Form1 form1 = new Form1();

                // 显示Form1窗体
                form1.Show();

             

                // 隐藏当前窗体 (DengLu)
                this.Hide();

            }

            else
            {
                ResetReadCardTimer();  // 重新启动计时器
                MessageBox.Show("请刷正确的卡,打开超过30s刷卡器休眠，重新刷卡");

            }

        }

        private void DengLu_Load(object sender, EventArgs e)
        {

        }

        private void DengLu_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
