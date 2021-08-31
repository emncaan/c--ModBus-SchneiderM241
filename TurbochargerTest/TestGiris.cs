using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyModbus;
using System.IO;
using System.Reflection;


namespace TurboCharger_Test
{
    public partial class Form1 : Form
    {
        public float havagiris_tp;
        public float havagiris_sp;
        public float havagiris_temp;
        public float kompresorgiris_tp;
        public float kompresorgiris_temp;
        public float turbincikis_tp;
        public float turbincikis_temp;
        public float kompresorcikis_tp;
        public float kompresorcikis_temp;
        public float debi;
        public float rpm;
        public string data;
        public int motor_start;
        public int motor_hz;
        public int motor_durum;
       

        //---- Baglanti  
        ModbusClient plc;
        //---- Log
        string fileName;
        FileStream stream;
        //--- Grafik
        public int i = 0;
     
   
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            plc = new ModbusClient();
            toolStripStatusLabel1.Text = ("PLC bağlantısı yok");
            toolStripStatusLabel2.Text = ("Kayıt alınmıyor");
            timer1.Interval = 500;
            timer2.Interval = 1000;
            timer3.Interval = 500;
            textBox12.Text = ("TestLog - " + DateTime.Now.ToShortDateString());

            // --------- Alınan değerleri yazdırma -------

           
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            
            try
            {
                if (plc.Connected)
                { 
                    plc.Disconnect(); 
                }

                
                plc.IPAddress = txtIpAddress.Text;
                plc.Port = 502;
                plc.SerialPort = null;


                plc.Connect();
                timer1.Start();
                tabControl1.SelectedIndex = 1;
                plc.LogFileFilename = "test.txt";

                if (plc.Connected)
                {
                    toolStripStatusLabel1.Text = ("PLC bağlantısı var.");

                }

            }

            catch (Exception exc)
            {
                
                timer1.Stop();
                MessageBox.Show("PLC'ye bağlantı yapılamadı!","Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = ("PLC bağlantısı yok.");

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer2.Stop();
            button2.Enabled = false;
            button1.Enabled = true;
            toolStripStatusLabel2.Text = ("Kayıt alınmıyor");
            plc.WriteSingleRegister(3, 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer3.Start();
            tabControl1.SelectedIndex = 2;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {   // -------- Alınan değerler -------
            if (plc.Connected)
            {
                try
                {
                    havagiris_tp = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(80, 2));
                    havagiris_sp = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(82, 2));
                    havagiris_temp = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(16, 2));
                    turbincikis_tp = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(84, 2));
                    turbincikis_temp = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(14, 2));
                    kompresorgiris_tp = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(86, 2));
                    kompresorgiris_temp = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(18, 2));
                    kompresorcikis_tp = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(88, 2));
                    kompresorcikis_temp = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(22, 2));
                    debi = ModbusClient.ConvertRegistersToFloat(plc.ReadHoldingRegisters(78, 2));
                    rpm = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(40, 2));
                    motor_durum = ModbusClient.ConvertRegistersToInt(plc.ReadHoldingRegisters(2, 2));

                    if (motor_durum == 1)
                    {
                        textBox13.Text = "Motor devrede";
                    }

                    else
                    {
                        textBox13.Text = "Motor devre dışı";
                    }


                }
                catch (Exception ex)
                {
                    plc.Disconnect();
                    timer1.Stop();
                    MessageBox.Show("Bağlantı koptu!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    tabControl1.SelectedIndex = 0;
                    toolStripStatusLabel1.Text = ("PLC bağlantısı yok.");
                }
                textBox1.Text = havagiris_tp.ToString() + " bar";
                textBox2.Text = havagiris_sp.ToString() + " bar";
                textBox3.Text = (havagiris_temp / 10).ToString() + " °C";
                textBox4.Text = turbincikis_tp.ToString() + " bar";
                textBox5.Text = (turbincikis_temp / 10).ToString() + " °C";
                textBox6.Text = kompresorgiris_tp.ToString() + " bar";
                textBox7.Text = (kompresorgiris_temp / 10).ToString() + " °C";
                textBox8.Text = kompresorcikis_tp.ToString() + " bar";
                textBox9.Text = (kompresorcikis_temp / 10).ToString() + " °C";
                textBox10.Text = debi.ToString() + " kg/m³";
                textBox11.Text = rpm.ToString() + " rpm";
            }

        }
        //---- Log işlemleri
        private void timer2_Tick(object sender, EventArgs e)
        {
            string data =   (DateTime.Now + " " + havagiris_tp.ToString() + " " +
                            havagiris_sp.ToString() + " " + 
                            (havagiris_temp / 10).ToString() + " " +
                            turbincikis_tp.ToString() + " " +
                            (turbincikis_temp / 10).ToString() + " " +
                            kompresorgiris_tp.ToString() + " " + 
                            (kompresorgiris_temp / 10).ToString() + " " +
                            kompresorcikis_tp.ToString() + " " +
                            (kompresorcikis_temp / 10).ToString() + " " +
                            debi.ToString() + " " +
                            rpm.ToString() + " ");

            try
            {
                fileName = @"D:\Test\" + textBox12.Text.ToString() +".txt";
                stream = new FileStream(fileName, FileMode.Append);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                
                        writer.WriteLine(data);
                        writer.Close();
                        stream.Close();
                        
            }
            catch (Exception ex)
            {
                timer2.Stop();
                MessageBox.Show("Kayıt başlatılamadı!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = ("Kayıt alınmıyor");
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();

            }
        }
        //---- Log işlemleri bitiş-
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                plc.WriteSingleRegister(3, 1);
                toolStripStatusLabel2.Text = ("Kayıt alınıyor");
                button1.Enabled = false;
                button2.Enabled = true;
                timer2.Start();
                
               
            }
            catch(Exception ex)
            {
                timer2.Stop();
                MessageBox.Show("Kayıt başlatılamadı!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = ("Kayıt alınmıyor");
                plc.WriteSingleRegister(3, 0);
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            i++;
            //------ Basınçlar
            if (havagiris_tp != 0)
            {
                this.chart2.Series[0].Points.AddXY(i, havagiris_tp);

            }

            if (havagiris_sp != 0)
            {
                this.chart2.Series[1].Points.AddXY(i, havagiris_sp);

            }
            if (turbincikis_tp != 0)
            {
                this.chart2.Series[2].Points.AddXY(i, turbincikis_tp);

            }

            if (kompresorgiris_tp != 0)
            {
                this.chart2.Series[3].Points.AddXY(i, kompresorgiris_tp);

            }

            if (kompresorcikis_tp != 0)
            {
                this.chart2.Series[4].Points.AddXY(i, kompresorcikis_tp);

            }
            //------ Basınçlar -

            //------ Sıcaklıklar

            if (havagiris_temp != 0)
            {
                this.chart3.Series[0].Points.AddXY(i, (havagiris_temp / 10));

            }

            if (turbincikis_temp != 0)
            {
                this.chart3.Series[1].Points.AddXY(i, (turbincikis_temp / 10));

            }

            if (kompresorgiris_temp != 0)
            {
                this.chart3.Series[2].Points.AddXY(i, (kompresorgiris_temp / 10));

            }
            if (kompresorcikis_temp != 0)
            {
                this.chart3.Series[3].Points.AddXY(i, (kompresorcikis_temp / 10));

            }
            //------ Sıcaklıklar-

            //------ Debi-Rpm

            if (rpm != 0)
            {
                this.chart1.Series[0].Points.AddXY(i, rpm);

            }

            if (debi != 0)
            {
                this.chart1.Series[1].Points.AddXY(i, debi);

            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            motor_start = 0;
            if (checkBox1.Checked)
            {
                motor_start = 1;
            }

            else
            { 
                motor_start = 0;
            }


            plc.WriteSingleRegister(2, motor_start);

            

        }

        private void button4_Click(object sender, EventArgs e)
        {
            motor_hz = Convert.ToInt32(textBox14.Text);
          
            
            if (plc.Connected)
            {
                try
                {
                    plc.WriteSingleRegister(30, motor_hz);
                }
                catch
                {
                    MessageBox.Show("Kayıt başlatılamadı!", "Hata!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
             }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer3.Stop();
            i = 0;
        }
    }
}
