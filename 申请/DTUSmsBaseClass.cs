using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.IO;
namespace 申请
{
    /// <summary>
    /// 短信基类
    /// </summary>
    public class SmsBaseClass
    {
        #region 变量声明
        /// <summary>
        /// SerialPort对象
        /// </summary>
        public TextBox m_Textbox;
        private SerialPort fSerialPort;        
        /// <summary>
        /// 是否发送成功
        /// </summary>
        private bool bHeadFinded;
        private bool fBool_Succeed;

        public bool FBool_Succeed
        {
            get { return fBool_Succeed; }
            set { fBool_Succeed = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。
        private int iSeachHead = 0;
        private int iSeachTail = 0;

        private int iSearchSucess = 0;
        private int iSearchFail = 0;
        byte[] bufSucess;
        byte[] bufFail;
        byte[] bufHead;
        byte[] bufTail;
        byte[] bufSms = new byte[500];
        int ibufSms;

        public SmsBaseClass()
        {
            int i;
            fSerialPort =new SerialPort();            
            fSerialPort.DataReceived += MyDataReceived;

            ibufSms = 0;
            bHeadFinded = false;
            string s  = "SMS_SEND_SUCESS";
            bufSucess = System.Text.Encoding.Default.GetBytes(s);
            s = "SMS_SEND_FAIL";
            bufFail = System.Text.Encoding.Default.GetBytes(s);
            s = "+CMS:";
            bufHead = System.Text.Encoding.Default.GetBytes(s);
            s = "\r\n\0\0";
            bufTail = System.Text.Encoding.Default.GetBytes(s);
            i = bufTail.Length;
        }
        #endregion
        private void WriteSmsData(string filename,string sms)
        {
            try
            {
                FileStream aFile = new FileStream(filename, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(aFile);
                
                // Write data to file.
                aFile.Seek(0, SeekOrigin.End);
                sw.WriteLine("TIME:{0};SMS:{1}",DateTime.Now.ToLocalTime(),sms);
                sw.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("An IOException has been thrown!");
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                return;
            }
        
        }

        #region 打开串口
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="串口号"></param>
        /// <returns>true：打开成功；false：打开失败</returns>
        public bool OpenPort(string PStr_PortName)
        {
            try
            {
                if (!fSerialPort.IsOpen)
                {
                    fSerialPort.BaudRate = 9600;              //设置波特率
                    fSerialPort.PortName = PStr_PortName;     //设置COM端口
                    fSerialPort.Encoding = Encoding.GetEncoding("GBK");  
                    fSerialPort.Open();                       //串行端口开启                    
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 发送DTU短信
        public void SendDTU(string PStr_Phone, int PInt_Model, string PStr_Content)
        {            
            string str_Content = PStr_Phone + ":" + PInt_Model.ToString() + ":" + PStr_Content;
            fSerialPort.Write(str_Content);            
        }
        #endregion

        #region 关闭串口
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>true：关闭成功；false：关闭失败</returns>
        public bool ClosePort()
        {
            try
            {
                if (fSerialPort.IsOpen)
                {
                    fSerialPort.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region fSerialPort接收事件

        int SearchStrInStream2(byte[] strToCmp, byte c, ref int i)
        {
            if (i > strToCmp.Length)
            {
                i = 0;
                return 0;
            }
            if (i != 0)
            {
                if (c != strToCmp[i])
                    i = 0;
            }
            if (c == strToCmp[i])
            {
                (i)++;
            }
            if ((int)(i) == strToCmp.Length)
            {

                i = 0;
                return 1;
            }
            return 0;
        }
        
        private void MyDataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            int i;
            int n = fSerialPort.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
            byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据
            

            fSerialPort.Read(buf, 0, n);//读取缓冲数据

            for (i = 0; i < n; i++)
            {
                if (1 == SearchStrInStream2(bufSucess, buf[i], ref iSearchSucess))
                {
                    fBool_Succeed = true;                  //发送成功
                    MessageBox.Show ("Sucess");
                }

                if (1 == SearchStrInStream2(bufFail, buf[i], ref iSearchFail))
                {
                    fBool_Succeed = false;                  //发送成功
                    MessageBox.Show ("Fail"); 
                }
               
                //处理接收短信的部分，通过包头包围将短信内容分析出来，如果用户不需接收，不用关注
                if (bHeadFinded == true)
                {
                    bufSms[ibufSms] = buf[i];
                    ibufSms++;
                    if (1 == SearchStrInStream2(bufTail, buf[i], ref iSeachTail))
                    {
                        bHeadFinded = false;                  
                        builder.Remove(0,builder.Length) ;
                        builder.Append(Encoding.GetEncoding("GBK").GetString(bufSms));
                        
                        m_Textbox.Invoke(new EventHandler(delegate
                        {
                            m_Textbox.Text = m_Textbox.Text+builder.ToString();
                        }));
                        WriteSmsData("RcvSms.txt", builder.ToString());
                        //将数据写入到文件
                    }
                }
                if (1 == SearchStrInStream2(bufHead, buf[i], ref iSeachHead))
                {
                    bHeadFinded = true;                  //发送成功
                    ibufSms = 0;
                    m_Textbox.Invoke(new EventHandler(delegate
                     {
                           m_Textbox.Text = "找到包头\r\n";
                     }));
                }
            }

        }
       
        #endregion

    
    }
}
