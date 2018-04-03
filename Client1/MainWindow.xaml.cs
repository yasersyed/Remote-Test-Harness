/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client1 - WPF Application                  //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 *  MainWindow.xaml.cs - contains buttons and their functions
 *
 * Required files:
 * ---------------
 * - MainWindow.xaml.cs, CommChannel.cs, Repository.cs, Server.cs, msTimer.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 20 Nov 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using TestHarnessCS;

namespace TestHarnessCS
{
    public partial class MainWindow : Window
    {
        byte[] block;
        Sender send;
        IRepo channel;
        string ToSendPath = Environment.CurrentDirectory + "\\TestHarnessCS\\Client1_ToSend";
        string SavePath = Environment.CurrentDirectory + "\\TestHarnessCS\\Client1_SavedFiles";
        int BlockSize = 1024;

        [DllImport("Kernel32")]
        public static extern bool AllocConsole();
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            block = new byte[BlockSize];
            time = new msTimer();
        }
        //For executing button_click events on window load
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Show();
            browsebtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            reposendbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            serversendbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            getlogbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        static IRepo CreateServiceChannel(string url)
        {
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IRepo> factory
              = new ChannelFactory<IRepo>(binding, address);
            return factory.CreateChannel();
        }
        public Comm<MainWindow> comm { get; set; } = new Comm<MainWindow>();
        public string endpoint { get; } = Comm<MainWindow>.makeEndPoint("http://localhost", 8081);
        
        msTimer time = null;
        List<string> TDfilepaths = new List<string>();
        List<string> TLfilepaths = new List<string>();
        
        //createMessage for creating messages with author, from point and to point
        public TestHarnessCS.Message createMessage(string author, string fromEndPoint, string toEndPoint)
        {
            TestHarnessCS.Message msg = new TestHarnessCS.Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }
        //Creates XML file which is then sent to the server
        public string createXML()
        {
            foreach (string Files in Directory.GetFiles(ToSendPath+"//TD//", "*.dll", SearchOption.AllDirectories))
            {
                TDfilepaths.Add(Files.Replace(ToSendPath+ "//TD//", ""));
            }
            foreach (string Files in Directory.GetFiles(ToSendPath + "//TL//", "*.dll", SearchOption.AllDirectories))
            {
                TLfilepaths.Add(Files.Replace(ToSendPath+"//TL//", ""));
            }
            string path = Environment.CurrentDirectory + "\\TestHarnessCS\\TestRequest1.xml";
            XDocument xdoc = new XDocument();
            XElement root = new XElement("testRequest");
            xdoc.Add(root);
            XElement test = new XElement("Test", new XAttribute("Name", "First Test"));
            XElement testDriver = new XElement("testDriver", TDfilepaths[0].ToString());
            XElement testLibrary = new XElement("Library", TLfilepaths[0].ToString());
            test.Add(testDriver);
            test.Add(testLibrary);
            root.Add(test);
            xdoc.Save(path);
            return path;
        }
        void rcvThreadProc()
        {
            while (true)
            {
                TestHarnessCS.Message msg = comm.rcv.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();
                ResultWindow.Text = msg.body;
                if (msg.body == "quit")
                    break;
            }
        }
        //For receiving files from the repository
        public Stream receiveFile(string filename)
        {
            time.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            Console.WriteLine(sfilename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
                outStream = new FileStream(sfilename, FileMode.Open);
            else
                Console.WriteLine("Can't open File: " + Path.GetFullPath(sfilename));
            time.Stop();
            Console.Write("Received file : {0} in {1} ms", sfilename, time.ElapsedMicroseconds);
            return outStream;
        }
        //For downloading files from the repository
        void download(string filename)
        {
            msTimer hrt = new msTimer();
            int totalBytes = 0;
            try
            {
                time.Start();
                Stream strm = channel.downLoadFile(filename);
                string rfilename = Path.Combine(SavePath, filename);
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                time.Stop();
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} ms", filename, totalBytes, time.ElapsedMicroseconds);
            }
            catch
            {
                Console.WriteLine("\n");
            }
        }
        //For creating xml test request and sending it to server
        private void browsebtn_Click(object sender, RoutedEventArgs e)
        {
            List<String> files = new List<String>();
            label1.Content = "Test Request";
            textBox.Text = "";
            string inputFile = createXML();
            TestHarnessCS.Message msg = createMessage("Client1", endpoint, endpoint);
            var reader = new System.IO.StreamReader(inputFile, System.Text.Encoding.UTF8);
            var text = reader.ReadToEnd();
            reader.Close();
            textBox.Text = text;
            browsebtn.IsEnabled = false;
            serversendbtn.IsEnabled = true;            
        }
        //Sending Test Request in the form of XML
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text != "")
            {
                Console.WriteLine("\n");
                Console.WriteLine("Requirement #2 : Sending Test Request in the form of XML to Server and send dll file to Repository \n");
                MainWindow client = new MainWindow();
                Message msg = client.createMessage("Client1", client.endpoint, client.endpoint);
                string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
                msg.author = "Client";
                msg.from = endpoint;
                msg.to = remoteEndPoint;
                msg.body += textBox.Text;
                try
                {
                    send = new TestHarnessCS.Sender();
                    send.CreateSendChannel(remoteEndPoint);
                    send.PostMessage(msg);
                    Console.WriteLine(msg.body);
                }
                catch (Exception ex)
                {
                    MainWindow temp = new MainWindow();
                    StringBuilder msg1 = new StringBuilder(ex.Message);
                    temp.Content = msg1.ToString();
                    temp.Show();
                }
                serversendbtn.IsEnabled = false;
            }
            else
                MessageBox.Show("XML not loaded");
    }
        //Repo Send button for sending dll files to the repository
        private void reposendbtn_Click(object sender, RoutedEventArgs e)
        {
            channel = CreateServiceChannel("http://localhost:8000/IRepo");
            string fqname = ToSendPath + "//TD//" + TDfilepaths[0];
            using (var inputStream = new FileStream(fqname, FileMode.Open))
            {
                FileTransferMessage msg2 = new FileTransferMessage();
                msg2.filename = TDfilepaths[0];
                msg2.transferStream = inputStream;
                channel.upLoadFile(msg2);
            }
            string fqname1 = ToSendPath + "//TL//" + TLfilepaths[0];
            using (var inputStream = new FileStream(fqname, FileMode.Open))
            {
                FileTransferMessage msg2 = new FileTransferMessage();
                msg2.filename = TLfilepaths[0];
                msg2.transferStream = inputStream;
                channel.upLoadFile(msg2);
            }
            ((System.ServiceModel.Channels.IChannel)channel).Close();
        }
        //getlog button - used for getting logs from repository
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("\nRequirement 7 - Getting Log file from the Repository using FileStreaming\n");
            channel = CreateServiceChannel("http://localhost:8000/IRepo");
            download(TDfilepaths[0] + ".log");
            ((System.ServiceModel.Channels.IChannel)channel).Close();
            Console.Write("\n");
            string text = System.IO.File.ReadAllText(SavePath + "\\" + TDfilepaths[0] + ".log");
            ResultWindow.Text = text;
        }
    }
}