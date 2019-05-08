using System;
using System.Windows;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;

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
        public Data mydata;
        private Enrollment enroller;
        private Verification verify;
        // Tcp thread
        public Thread t1;
        public TcpListener listener = null;
        public TcpClient client;
        public NetworkStream stream;

        public void TcpThread()
        {
            try
            {
                // Set the TcpListener on port 13000
                Int32 port = 13000;
                // Set the server IP address
                IPAddress myServerIP = IPAddress.Parse("10.144.140.27");
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
                    // Start to listen for connections from a client.
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        tcpStatusTextBox.AppendText("Waiting for a connection...\r\n");
                    }));

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    client = listener.AcceptTcpClient();

                    // Process the connection here. (Add the client to a
                    // server table, read data, etc.)
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        tcpStatusTextBox.AppendText("Client connected!\r\n");
                    }));

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
                                    tcpStatusTextBox.AppendText("User: " + data.Remove(0, 1) + "\r\n");
                                    ConfirmButton_Click(null, null);
                                    EnrollButton_Click(null, null);
                                    tcpStatusTextBox.AppendText("User" + nameTextBox.Text + " disconnected!\r\n");
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
                            // Wrong Flag
                            default:
                                this.Dispatcher.Invoke(new Action(delegate
                                {
                                    tcpStatusTextBox.AppendText("Fail! " + data[0] + "\r\n");
                                }));
                                break;
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    tcpStatusTextBox.AppendText("SocketException: " + e + "\r\n");
                }));
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void dataInit()
        {
            mydata = new Data();
            mydata.OnChange += delegate { ExchangeData(false); };	// Track data changes to keep the form synchronized
            ExchangeData(false);								// fill data with default values from controls
            mydata.folderPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data";
            mydata.logPath = mydata.folderPath + "\\dataLog.txt";
            if (!System.IO.Directory.Exists(mydata.folderPath))
                System.IO.Directory.CreateDirectory(mydata.folderPath);
            if (!System.IO.File.Exists(mydata.logPath))
            {
                System.IO.File.Create(mydata.logPath);
            }
            else
            {
                mySR();
                mySW();
            }
        }

        // Read dataLog.txt
        private void mySR()
        {
            using (StreamReader sr = new StreamReader(mydata.logPath))
            {
                mydata.userNum = Convert.ToInt32(sr.ReadLine());
                string line;
                while ((line=sr.ReadLine())!=null)
                {
                    database user = new database();
                    user.username = line;
                    user.fpNum = Convert.ToInt32(sr.ReadLine());
                    mydata.userList.Add(user);
                }
            }
        }

        // Read .fpt files
        private void mySW()
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

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            // Enrollment Window
            enroller = new Enrollment(mydata, stream);
            enroller.Show();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            mydata.tempName = this.nameTextBox.Text;
            this.enrollButton.IsEnabled = true;
        }

        private void CloseAndSaveButton_Click(object sender, RoutedEventArgs e)
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
            this.Close();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            verify = new Verification(mydata, stream);
            verify.ShowDialog();
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            dataInit();
            this.confirmButton.IsEnabled = true;
            this.verifyButton.IsEnabled = true;
            this.closeAndSaveButton.IsEnabled = true;
            this.tcpStartButton.IsEnabled = true;
            TcpStartButton_Click(null, null);
        }

        private void TcpStartButton_Click(object sender, RoutedEventArgs e)
        {
            // create a new thread for tcp
            t1 = new Thread(new ThreadStart(TcpThread));
            // start the tcpThread
            t1.Start();
            tcpStartButton.IsEnabled = false;
            tcpCloseButton.IsEnabled = true;
        }

        private void TcpCloseButton_Click(object sender, RoutedEventArgs e)
        {
            t1.Abort();
            //tcpStartButton.IsEnabled = true;
            tcpCloseButton.IsEnabled = false;
        }
    }
}
