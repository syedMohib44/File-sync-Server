using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace OsProjectServer
{
    public partial class Form1 : Form
    {

        string rd;
        byte[] b1;
        string[] v;
        int[] m;
        //TcpListener list;

        public Form1()
        {
            InitializeComponent();
            if (!Directory.Exists(strDesPath))
            {
                Directory.CreateDirectory(strDesPath);
            }
        }

        string[] filenames;
        TcpListener server = new TcpListener(IPAddress.Loopback, 5055);
        TcpClient client;
        private void Form1_Load(object sender, EventArgs e)
        {
            server.Start(1);
            CheckForIllegalCrossThreadCalls = false;

            client = server.AcceptTcpClient();

            StreamReader sr = new StreamReader(client.GetStream());
            rd = sr.ReadLine();
            string lent = rd.Substring(rd.LastIndexOf('.') + 1);
            v = new string[Convert.ToUInt32(lent)];
            m = new int[Convert.ToUInt32(lent)];
            filenames = new string[Convert.ToUInt32(lent)];
            int i = 0;

            while (i < filenames.Length/*rd != null*/)
            {
                string[] arr = rd.Split('.');
                if (arr.Length == 1)
                {
                    button1.Enabled = true;
                    return;
                }
                filenames[i] = arr[0] + "." + arr[1];
                string strDesFilePath = string.Format(@"{0}\{1}", strDesPath, Path.GetFileName(filenames[i]));
                if (!File.Exists(strDesFilePath))
                    File.Copy(filenames[i], strDesFilePath, true);
                rd = sr.ReadLine();
                i++;
            }        
            Thread t = new Thread(writeDataToServer);
            t.Start();
        }
        string strDesPath = @"C:\backup";

        private void writeDataToServer()
        {
            while (true)
            {
                textBox1.Text = strDesPath;
                Stream s = client.GetStream();
                for (int i = 0; i < m.Length; i++)
                {
                    b1 = new byte[m[i]];
                    s.Read(b1, 0, b1.Length);
                    File.WriteAllBytes(/*textBox1.Text + */"C:\\backup\\" + Path.GetFileName(filenames[i]), b1);
                }
                Thread.Sleep(4000);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void button2_Click(object sender, EventArgs e)
        {
        }
    }
}
