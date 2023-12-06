using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 申请
{


    public partial class Projects : Form
    {
        Form1 form1 = new Form1();
        public List<ProjectData> excelData;
        public SmsBaseClass fSmsBaseClass;
        private string excelFilePath;
        public Projects()
        {
            
            InitializeComponent();
           
            
            excelData = new List<ProjectData>();
            //ReadExcelData(excelFilePath);
           
        }

        public class ProjectData
        {
            public string Type { get; set; }
            public string MachineType { get; set; }
            public string ProjectName { get; set; }
            public string ProjectNumber { get; set; }
        }

        private void ReadExcelData(string filePath)
        {
            // 清空现有数据
           
            FileInfo fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text)) break;

                    var projectData = new ProjectData
                    {
                        //读取excel表中的Types[AI或者B]
                        Type = worksheet.Cells[row, 3].Text,

                        //读取excel表中的MachineType[翻盖机台/飞针机台/Box/上料机]
                        MachineType = worksheet.Cells[row, 8].Text,

                        //读取excel表中的ProjectName
                        ProjectName = worksheet.Cells[row, 7].Text,

                        //读取excel表中的ProjectNumber [立项时的编号]
                        ProjectNumber = worksheet.Cells[row, 5].Text
                    };

                    excelData.Add(projectData);
                }
            }
        }

        public class ExcelReader
        {

            public List<string> GetFilteredProjects(string filePath, string type, string machineType)
            {
                var items = new List<string>();
                FileInfo fileInfo = new FileInfo(filePath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // 假设数据在第一个工作表

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // 假设从第二行开始读取
                    {
                        if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text))
                        {
                            break; // 如果 E 列为空，则停止循环,即项目编号为空时
                        }

                        string typeValue = worksheet.Cells[row, 3].Text; // C列
                        string machineTypeValue = worksheet.Cells[row, 8].Text; // H列
                        if (typeValue == type && machineTypeValue == machineType)
                        {
                            string projectName = worksheet.Cells[row, 7].Text; // G列
                            if (!string.IsNullOrEmpty(projectName))
                            {
                                items.Add(projectName);
                            }
                        }
                    }

                }
                return items;

            }

        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboBox3();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboBox3();
        }

        private void comboBox3_DropDown(object sender, EventArgs e)
        {
            // 更新 comboBox3 的内容
            UpdateComboBox3();
        }

        private void UpdateComboBox3()
        {
            string type = comboBox1.SelectedItem?.ToString(); // 从 ComboBox1 获取选定的类型
            string machineType = comboBox2.SelectedItem?.ToString(); // 从 ComboBox2 获取选定的机器类型

            var filteredProjects = excelData
                .Where(p => (type == null || p.Type == type) && (machineType == null || p.MachineType == machineType))
                .Select(p => p.ProjectName)
                .Distinct()
                .ToList();

            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(filteredProjects.ToArray());
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Form1 form1 = new Form1();
           
            form1.SetProjectNumber(selectedProjectNumber);
            form1.Show();
            this.Hide();
        }

        private void Xuanze_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                excelFilePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(excelFilePath); // 获取文件名
                textBox1.Text = fileName;
                // 在这里重新调用 ReadExcelData 方法，使用新的文件路径
                ReadExcelData(excelFilePath);
            }
        }

        public void LoadData(string filePath)
        {
            excelFilePath = filePath;
            ReadExcelData(excelFilePath);
        }


        //返回按钮
        private void button2_Click(object sender, EventArgs e)
        {
            if (fSmsBaseClass != null && fSmsBaseClass.IsOpen)
            {
                fSmsBaseClass.ClosePort(); // 关闭串口
            }
            form1.Show();
            this.Hide();
        }
        private string selectedProjectNumber;
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //每次点击combobox3的下拉按钮都会执行Updatacombobox3
           
            string selectedProjectName = comboBox3.SelectedItem?.ToString();
            selectedProjectNumber = excelData.FirstOrDefault(p => p.ProjectName == selectedProjectName)?.ProjectNumber;
        }

        private void Projects_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
    
}
