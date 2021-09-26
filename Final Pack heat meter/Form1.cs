using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace Final_Pack_heat_meter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int lastMin=0;
        public object[,] ObTab = null;
        public bool[] warning = { false, false, false, false, false, false, false, false };
        public int minTemp, maxTemp, minHum, maxHum = 0;
        public bool freshData = false;

        public void readConfig()
        {
            string[] lines = File.ReadAllLines("config.config");
            foreach(string line in lines)
            {
                string sub = line.Substring(0, line.IndexOf(">")+1);
                switch (sub)
                {
                    case "<minTemperature>" : minTemp = Convert.ToInt32(line.Substring(line.IndexOf(">") + 1, line.LastIndexOf("<") - line.IndexOf(">") - 1)); break;
                    case "<maxTemperature>": maxTemp = Convert.ToInt32(line.Substring(line.IndexOf(">") + 1, line.LastIndexOf("<") - line.IndexOf(">") - 1)); break;
                    case "<minHumidity>": minHum = Convert.ToInt32(line.Substring(line.IndexOf(">") + 1, line.LastIndexOf("<") - line.IndexOf(">") - 1)); break;
                    case "<maxHumidity>": maxHum = Convert.ToInt32(line.Substring(line.IndexOf(">") + 1, line.LastIndexOf("<") - line.IndexOf(">") - 1)); break;
                }

                //line = line.Substring(line.IndexOf(">") + 1);

            }
        }

        public string[,] getData()
        {
            freshData = false;
            string[,] currentDownload = new string[8, 2]; //0. to store current temperature value read from dbView (0-7: oil cooling watertemp., 8-15: mould cooling watertemp) 
                                                          //1. to store current location read from dbView
            try
            {
                SqlConnection con = new SqlConnection("Data Source=HUAHONENER;User ID=PointViewer;Password=Lego2017");
                SqlCommand cmd = new SqlCommand(@"SELECT [Timestamp], [PointName], [Value] FROM [hwreportsview].[dbo].[MLD_Points_View]");
                con.Open();
                cmd.Connection = con;
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int j = 0;
                    for (int i = 0; i <= 70; i++)
                    {
                        reader.Read();
                        if (Convert.ToString(reader["PointName"]).StartsWith("HU1101AHU"))
                        {
                            string current = reader["PointName"].ToString();
                            currentDownload[j, 1] = Convert.ToString(reader["Value"]);
                            currentDownload[j, 0] = Convert.ToString(reader["PointName"]);
                            j++;
                        }
                    }
                }
                con.Close();
                freshData = true;
            }catch(SqlException e)
            {
                //MessageBox.Show(e.Message);
            }catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }


            return currentDownload;
        }

        public void refreshData()
        {
            string[,] data = null;
            data = getData();
            label2.Text = data[0, 1];
            //label2.Text = "0";//for test
            label4.Text = data[1, 1];
            label6.Text = data[2, 1];
            label8.Text = data[3, 1];
            label1.Text = data[4, 1];
            label3.Text = data[5, 1];
            label5.Text = data[6, 1];
            label7.Text = data[7, 1];


            ObTab = new object[,] { { panel1, label1 }, { panel2, label2 }, { panel3, label3 }, { panel4, label4 }, { panel5, label5 }, { panel6, label6 }, { panel7, label7 }, { panel8, label8 } };

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    Label tempLabel = (Label)ObTab[i, 1];
                    Panel tempPanel = (Panel)ObTab[i, 0];
                    if (i % 2 == 0)
                    {
                        if (Convert.ToDouble(tempLabel.Text) > minHum & Convert.ToDouble(tempLabel.Text) < maxHum)
                        {
                            tempPanel.BackColor = Color.Green;
                            tempLabel.ForeColor = Color.White;
                            warning[i] = false;
                        }
                        else
                        {
                            tempPanel.BackColor = Color.Red;
                            tempLabel.ForeColor = Color.Black;
                            warning[i] = true;
                        }
                    }
                    else
                    {
                        if (Convert.ToDouble(tempLabel.Text) > minTemp & Convert.ToDouble(tempLabel.Text) < maxTemp)
                        {
                            tempPanel.BackColor = Color.Green;
                            tempLabel.ForeColor = Color.White;
                            warning[i] = false;
                        }
                        else
                        {
                            tempPanel.BackColor = Color.Red;
                            tempLabel.ForeColor = Color.Black;
                            warning[i] = true;
                        }
                    }
                }
                label2.Text += " °C";
                label4.Text += " °C";
                label6.Text += " °C";
                label8.Text += " °C";
                label1.Text += " %";
                label3.Text += " %";
                label5.Text += " %";
                label7.Text += " %";
            }catch(Exception e)
            {
                //magic
            }
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            readConfig();
            refreshData();
            if (DateTime.Now.Minute < 10) label9.Text = DateTime.Now.Hour + ":0" + DateTime.Now.Minute;
            else label9.Text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;      
            timer1.Start();
            timer2.Start();
            changeSize();
        }

        public void changeSize()
        {
            try
            {
                tableLayoutPanel1.Size = new Size(Form1.ActiveForm.Size.Width - 15, Form1.ActiveForm.Size.Height - 115);
                label9.Location = new Point(Form1.ActiveForm.Size.Width / 2 - label9.Size.Width / 2, label9.Location.Y);

                panel5.Size = new Size((tableLayoutPanel1.Size.Width - 24) / 2, tableLayoutPanel1.Size.Height / 2);
                panel6.Size = new Size((tableLayoutPanel1.Size.Width - 24) / 2, tableLayoutPanel1.Size.Height / 2);
                panel7.Size = new Size((tableLayoutPanel1.Size.Width - 24) / 2, tableLayoutPanel1.Size.Height / 2);
                panel8.Size = new Size((tableLayoutPanel1.Size.Width - 24) / 2, tableLayoutPanel1.Size.Height / 2);
                panel1.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
                panel2.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
                panel3.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
                panel4.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);

                label1.Location = new Point(panel1.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label2.Location = new Point(panel2.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label3.Location = new Point(panel3.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label4.Location = new Point(panel4.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label5.Location = new Point(panel5.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label6.Location = new Point(panel6.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label7.Location = new Point(panel7.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
                label8.Location = new Point(panel8.Size.Width / 2 - label1.Size.Width / 2, panel1.Size.Height / 2 - label1.Size.Height / 2);
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                //magic
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            changeSize();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Minute != lastMin & DateTime.Now.Second == 10)
            {
                if (DateTime.Now.Minute < 10) label9.Text = DateTime.Now.Hour + ":0" + DateTime.Now.Minute;
                else label9.Text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
                if (freshData)
                {
                    refreshData();
                }
                lastMin = DateTime.Now.Minute;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            for (int i=0;i<8;i++)
            {
                if(warning[i])
                {
                    Panel tempPanel = (Panel)ObTab[i, 0];
                    if (DateTime.Now.Second % 2 == 0) tempPanel.BackColor = Color.Red;
                    else tempPanel.BackColor = Color.Yellow;
                }
            }
        }
    }
}
