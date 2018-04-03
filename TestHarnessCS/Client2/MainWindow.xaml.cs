/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client2 - WPF Application                  //
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
        [DllImport("Kernel32")]
        public static extern bool AllocConsole();
        byte[] block;
        //Opens Console with the WPF Application to show the process
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            block = new byte[BlockSize];
            time = new msTimer();
        }
        //Raising all button_click events on loading window
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Show();
            browsebtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            reposendbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            serversendbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            getlogbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        Sender send;
        IRepo channel;
        string ToSendPath = Environment.CurrentDirectory + "\\TestHarnessCS\\Client2_ToSend";
        string SavePath = Environment.CurrentDirectory + "\\TestHarnessCS\\Client2_SavedFiles";
        int BlockSize = 1024;
        List<string> TDfilepaths = new List<string>();
        List<string> TLfilepaths = new List<string>();
        //Creating channel for communcation
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
        public string endpoint { get; } = Comm<MainWindow>.makeEndPoint("http://localhost", 8082);
        msTimer time = new msTimer();
        //Create Message to use for sending requests to the server
        public TestHarnessCS.Message createMessage(string author, string fromEndPoint, string toEndPoint)
        {
            TestHarnessCS.Message msg = new TestHarnessCS.Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }
        //CreateXML() to create XML document by taking the file names from TD and TL directories respectively
        public string createXML()
        {

            foreach (string Files in Directory.GetFiles(ToSendPath + "//TD//", "*.dll", SearchOption.AllDirectories))
            {
                TDfilepaths.Add(Files.Replace(ToSendPath+ "//TD//", ""));
            }
            foreach (string Files in Directory.GetFiles(ToSendPath + "//TL//", "*.dll", SearchOption.AllDirectories))
            {
                TLfilepaths.Add(Files.Replace(ToSendPath + "//TL//", ""));
            }
            string path = Environment.CurrentDirectory+ "\\TestHarnessCS\\TestRequest2.xml";
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
        //rcvThreadProc - for getting messages and can use it to utilize the messages received
        void rcvThreadProc()
        {
            while (true)
            {
                TestHarnessCS.Message msg = comm.rcv.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n{0} received message:", comm.name);
                msg.showMsg();
                ResultWindow.Text = msg.body;
                if (msg.body == "quit")
                    break;
            }
        }
        //Downloading files from the repository
        void download(string filename)
        {
            int totalBytes = 0;
            msTimer hrt = new msTimer();
            try
            {
                hrt.Start();
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
                hrt.Stop();
                ulong time = hrt.ElapsedMicroseconds;
                Console.Write("\nReceived file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
            }
            catch
            {
                Console.Write("\n");
            }
        }
        //Uploading files to the Repository
        void uploadFile(string filename)
        {
            string fqname = Path.Combine(ToSendPath, filename);
            try
            {
                time.Start();
                using (var inputStream = new FileStream(fqname, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    channel.upLoadFile(msg);
                }
                time.Stop();
                Console.Write("\n  Uploaded file \"{0}\" in {1} microsec.", filename, time.ElapsedMicroseconds);
            }
            catch (Exception ex)
            {
                Console.Write("\n  can't find \"{0}\"", fqname);
                Console.Write("\n" + ex.Message);
            }
        }

        //For creating xml test request and sending it to server
        private void browsebtn_Click(object sender, RoutedEventArgs e)
        {
            List<String> files = new List<String>();
            label1.Content = "Test Request";
            textBox.Text = "";
            string inputFile = createXML();
            TestHarnessCS.Message msg = createMessage("Client2", endpoint, endpoint);
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
                TestHarnessCS.Message msg = client.createMessage("Client2", client.endpoint, client.endpoint);
                string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
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
            } ((IChannel)channel).Close();
        }

        //getlog button - used for getting logs from repository
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("\nRequirement 7 - Getting Log file from the Repository using FileStreaming\n");
            channel = CreateServiceChannel("http://localhost:8000/IRepo");
            download(TDfilepaths[0]+".log");
            ((System.ServiceModel.Channels.IChannel)channel).Close();
            Console.Write("\n");
            string text = System.IO.File.ReadAllText(SavePath + "\\"+ TDfilepaths[0]+ ".log");
            ResultWindow.Text = text;
        }
    }
}
