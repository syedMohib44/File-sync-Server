using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OsProjectClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        string n;
        byte[] b1;
        string[] fileNames;

        TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Any, 5050));
        private void SendToServer()
        {
            //if(!client.Connected)
            //    client.Connect(IPAddress.Loopback, 5055);

            // while (true)
            //{

            Stream[] s = new Stream[fileNames.Length];
            for (int i = 0; i < fileNames.Length; i++)
            {
                s[i] = client.GetStream();
                b1 = File.ReadAllBytes(fileNames[i]);
                string[] fil = fileNames[i].Split('\\');
                 s[i].Write(b1, 0, b1.Length);
                s[i].Flush();
            }
            //client.Close();
            //Thread.Sleep(5000);
            //}
        }
        private void button2_Click(object sender, EventArgs e)
        {

            //label1.Text = "File Transferred....";
        }
        string strSourcePath, strDesPath = @"C:\backup";
        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                strSourcePath = folderBrowserDialog1.SelectedPath;
                string t = folderBrowserDialog1.SelectedPath;
                string[] fileArr = Directory.GetFiles(t);
                fileNames = new string[fileArr.Length];
                //TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Loopback, 5055));
                client.Connect(IPAddress.Loopback, 5055);
                StreamWriter sw = new StreamWriter(client.GetStream());
                for (int i = 0; i < fileArr.Length; i++)
                {
                    string[] fileName = fileArr[i].Split('\\');
                    fileNames[i] = fileArr[i];
                    t = fileName[fileName.Length - 1];

                    FileInfo fi = new FileInfo(fileArr[i]);
                    n = fi.FullName + "." + fi.Length + "." + fileArr.Length;
                    sw.WriteLine(n);
                    button1.Enabled = false;
                }
                sw.Flush();
                //client.Close();
                //label1.Text = "File Transferred....";
                SendToServer();
            }
            //Thread th = new Thread(SendToServer);
            //th.Start();

            Thread watcherTh = new Thread(watch);
            watcherTh.Start();
        }



        string tempFileSource, tempFileDest;
        WatcherChangeTypes TYPES;

        private void watch()
        {
            FileSystemEventArgs argu = null;
            FileSystemEventHandler e = null;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            while (true)
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = strSourcePath;

                if (!FolderTheSame())
                {
                    argu = new FileSystemEventArgs(TYPES, strDesPath, tempFileSource);
                    e = new FileSystemEventHandler(OnChanged);
                    OnChanged(this, argu);
                    //string strSourcePath = @"D:\Temp";

                    //watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                }
                //watcher.Created += new FileSystemEventHandler(OnChanged);
                Thread.Sleep(5000);
            }
        }

        int fileC = 0;
        private void OnApplicationExit(object source, EventArgs e)
        {
            string path = @"" + DateTime.UtcNow.Day + "." + DateTime.UtcNow.Month + "." + DateTime.UtcNow.Year + ".txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                //path += fileC.ToString();
                //fileC++;
            }

            //File.ope
            TextWriter tw = new StreamWriter(path, true);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                tw.WriteLine(listBox1.Items[i]);
            }
            tw.Close();
            client.Close();
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            listBox1.Items.Add("File: " + e.FullPath + " " + e.ChangeType);
            syncUpdatedFiles(e.FullPath);
            //Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void syncUpdatedFiles(string strSourceFile)
        {
            //string strDesPath = @"C:\backup";
            //get filename
            string strFileName = Path.GetFileName(strSourceFile);
            string strDesFilePath = string.Format(@"{0}\{1}", strDesPath, strFileName);
            var val = File.Exists(strDesFilePath);
            //check whether the destination path contatins the same file
            if (!File.Exists(strDesFilePath))
            {
                for (; ; )
                {
                    if (IsFileLocked(strSourceFile))
                    {
                        File.Copy(strSourceFile, strDesFilePath, true);
                        break;
                    }
                }
            }

            else
            {
                UpdateFile(tempFilePathS, tempFilePathD /*strSourceFile, strDesFilePath*/);
            }

        }

        long s;
        private void UpdateFile(string strSourceFile, string strDesFilePath)
        {
            byte[] bd, bs;
            FileStream fd = new FileStream(strDesFilePath, FileMode.Open);
            FileStream fs = new FileStream(strSourceFile, FileMode.Open);
            bs = new byte[fs.Length];
            bd = new byte[fd.Length];

            fs.Read(bs, 0, (int)fs.Length);
            fd.SetLength(fs.Length);
            fd.Write(bs, 0, bs.Length);

            fd.Close();
            fs.Close();
        }

        string tempFilePathS, tempFilePathD;
        private bool FolderTheSame()
        {
            int fileCounter = 0;
            string[] arrFilesSource = Directory.GetFiles(strSourcePath);
            string[] arrFilesDest = Directory.GetFiles(strDesPath);

            for (int i = 0; i < arrFilesDest.Length; i++)
            {
                for (int j = 0; j < arrFilesSource.Length; j++)
                {
                    string[] getsrcFilename = arrFilesSource[j].Split('\\');
                    string[] getdestFilename = arrFilesDest[i].Split('\\');
                    if (getsrcFilename[getsrcFilename.Length - 1] == getdestFilename[getdestFilename.Length - 1])
                    {
                        byte[] bd, bs;
                        FileStream fd = new FileStream(arrFilesDest[i], FileMode.Open);
                        FileStream fs = new FileStream(arrFilesSource[j], FileMode.Open);
                        bs = new byte[fs.Length];
                        bd = new byte[fd.Length];

                        fs.Read(bs, 0, bs.Length);
                        fd.Read(bd, 0, bd.Length);
                        if (FilesTheSame(bd, bs))
                        {
                            fileCounter++;
                        }
                        if (!FilesTheSame(bd, bs))
                        {
                            tempFileDest = getdestFilename[getdestFilename.Length - 1];
                            tempFileSource = getsrcFilename[getsrcFilename.Length - 1];

                            tempFilePathS = arrFilesSource[j];
                            tempFilePathD = arrFilesDest[i];
                            TYPES = WatcherChangeTypes.Changed;
                            fs.Close();
                            fd.Close();
                            return false;
                        }
                        //else
                        //{
                        //    return false;
                        //}
                        fs.Close();
                        fd.Close();
                    }
                    string strFileName = Path.GetFileName(arrFilesSource[j]);
                    string strDesFilePath = string.Format(@"{0}\{1}", strDesPath, strFileName);
                    if (!File.Exists(strDesFilePath))
                    {
                        if (IsFileLocked(arrFilesSource[j]))
                        {
                            File.Copy(arrFilesSource[j], strDesFilePath, true);
                            return false;
                        }
                    }
                }
            }

            if (arrFilesSource.Length == fileCounter)
            {
                return true;
            }
            return false;
        }

        private bool FilesTheSame(byte[] b1, byte[] b2)
        {
            if (b1.LongLength == b2.LongLength)
            {
                for (long l = 0; l < b1.LongLength; l++)
                {
                    if (b1[l] != b2[l])
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsFileLocked(string strSourcePath)
        {
            try
            {
                using (Stream stream = new FileStream(strSourcePath, FileMode.Open))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
