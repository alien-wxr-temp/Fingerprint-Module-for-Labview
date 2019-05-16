using System;
using System.Windows;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace xrFPServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    // a simple delegate for marshalling calls from event handlers to the GUI thread
    delegate void Function();

    public partial class MainWindow : Window
    {
        #region <Windows API>
        // activate a window and set it focused
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion </Windows API>

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        #region <Member>
        // data
        private Data mydata;
        // tcp thread
        private Thread tcpThread;
        // tcp member
        private TcpListener listener = null;
        private TcpClient client;
        private NetworkStream stream;
        // enrollment member
        private Enrollment enroller;
        // verification member
        private Verification verify;
        #endregion </Member>

        #region <TCP>
        private void TcpStart()
        {
            // create a new thread for tcp
            tcpThread = new Thread(new ThreadStart(TcpThread));
            // start the tcpThread
            tcpThread.Start();
        }
        private void TcpClose()
        {
            tcpThread.Abort();
        }
        public void TcpThread()
        {
            try
            {
                // Set the TcpListener on port 13000
                Int32 port = 13000;
                // Set the server IP address
                IPAddress myServerIP = IPAddress.Parse("127.0.0.1");
                // Set the TcpListener with the port and IP above.
                listener = new TcpListener(myServerIP, port);
                // Start listening for client requests
                listener.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    client = listener.AcceptTcpClient();

                    // Process the connection here. (Add the client to a
                    // server table, read data, etc.)
                    data = null;

                    // Get a stream object for reading and writing
                    stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        switch (data[0])
                        {
                            // Start Enrollment Function
                            case '1':
                                this.Dispatcher.Invoke(new Action(delegate
                                {
                                    nameTextBox.Text = data.Remove(0, 1);
                                    EnrollButton_Click(null, null);
                                }));
                                break;
                            // Start Verification Function
                            case '2':
                                this.Dispatcher.Invoke(new Action(delegate
                                {
                                    nameTextBox.Text = data.Remove(0, 1);
                                    VerifyButton_Click(null, null);
                                }));
                                break;
                            case '3':
                                this.Dispatcher.Invoke(new Action(delegate
                                {
                                    CloseButton_Click(null, null);
                                }));
                                break;
                            // Wrong Flag
                            default:
                                System.Windows.MessageBox.Show("Fail! " + data[0] + "\r\n");
                                break;
                        }
                    }
                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                System.Windows.MessageBox.Show("SocketException: " + e + "\r\n");
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();
            }
            TcpClose();
        }
        #endregion </TCP>

        #region <Data>
        public void DataInit()
        {
            mydata = new Data();
            mydata.OnChange += delegate { ExchangeData(false); };	// Track data changes to keep the form synchronized
            ExchangeData(false);								// fill data with default values from controls
            mydata.folderPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data";
            mydata.logPath = mydata.folderPath + "\\dataLog.txt";
            if (!System.IO.Directory.Exists(mydata.folderPath))
                System.IO.Directory.CreateDirectory(mydata.folderPath);
            if (System.IO.File.Exists(mydata.logPath))
            {
                MySR();
                MySW();
            }
        }

        // Read dataLog.txt
        private void MySR()
        {
            using (StreamReader sr = new StreamReader(mydata.logPath))
            {
                mydata.userNum = Convert.ToInt32(sr.ReadLine());
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    database user = new database();
                    user.username = line;
                    user.fpNum = Convert.ToInt32(sr.ReadLine());
                    mydata.userList.Add(user);
                }
            }
        }

        // Read .fpt files
        private void MySW()
        {
            for (int i = 0; i < mydata.userNum; i++)
            {
                for (int j = 0; j < mydata.userList[i].fpNum; j++)
                {
                    string fName = mydata.folderPath + "\\";
                    fName += string.Format("{0:0000}", i);
                    fName += string.Format("{0:0000}", j + 1);
                    fName += ".fpt";
                    using (FileStream fs = File.OpenRead(fName))
                    {
                        DPFP.Template template = new DPFP.Template(fs);
                        mydata.Templates[mydata.serialNum++] = template;
                        string name = mydata.userList[i].username;
                        mydata.serialName.Add(name);
                    }
                }
            }
        }

        // Simple dialog data exchange (DDX) implementation.
        private void ExchangeData(bool read)
        {
            if (read)
            {   // read values from the form's controls to the data object
                mydata.Update();
            }
            else
            {   // read valuse from the data object to the form's controls
            }
        }
        #endregion </Data>

        #region <Enrollment>
        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            mydata.tempName = this.nameTextBox.Text;
            // Enrollment Window
            enroller = new Enrollment(mydata, stream);
            enroller.Show();
            var hwnd = new WindowInteropHelper(enroller).Handle;  //获取window的句柄
            SetForegroundWindow(hwnd);
        }
        #endregion </Enrollment>

        #region <Verification>
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            // verification window
            verify = new Verification(mydata, stream);
            verify.Show();
            var hwnd = new WindowInteropHelper(verify).Handle;  //获取window的句柄
            SetForegroundWindow(hwnd);
        }
        #endregion </Verification>

        #region <Initialization>
        public void Init()
        {
            DataInit();
            TcpStart();
        }
        #endregion </Initialization>

        #region <Close>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(mydata.logPath);
            using (FileStream fs = new FileStream(mydata.logPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(mydata.userNum);
                    foreach (database user in mydata.userList)
                    {
                        sw.WriteLine(user.username);
                        sw.WriteLine(user.fpNum);
                    }
                }
            }
            Environment.Exit(0);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(null, null);
        }
        #endregion </Close>
    }
}
