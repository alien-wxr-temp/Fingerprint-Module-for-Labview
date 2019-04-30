using System;
using System.Windows;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;

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
        private new DPFP.Template Template;
        public Data mydata;
        private Enrollment enroller;
        private Verification verify;

        // Set the TcpListener on port 13000.
        Int32 port = 13000;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        // TcpListener server
        private TcpListener server;

        public MainWindow()
        {
            InitializeComponent();
            this.confirmButton.IsEnabled = false;
            this.enrollButton.IsEnabled = false;
            this.verifyButton.IsEnabled = false;
            this.closeAndSaveButton.IsEnabled = false;
        }

        // TCP Start
        private void tcpStart()
        {
            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            // Enter the listening loop.
            while (true)
            {
                tcpStatusTextBox.AppendText("Waiting for a connection... " + "\r\n");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                tcpStatusTextBox.AppendText("Connected!" + "\r\n");

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    tcpStatusTextBox.AppendText("Received: " + data + "\r\n");

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    //stream.Write(msg, 0, msg.Length);
                }

                // Shutdown and end connection
                client.Close();
            }
        }

        // TCP Close
        private void tcpClose()
        {
            server.Stop();
        }

        protected void MakeReport(string message)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                tcpStatusTextBox.AppendText(message + "\r\n");
                tcpStatusTextBox.ScrollToEnd();
            }));
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
            enroller = new Enrollment(mydata);
            enroller.OnTemplate += this.OnTemplate;
            enroller.ShowDialog();
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                Template = template;
                if (Template != null)
                    System.Windows.MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
                else
                    System.Windows.MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
            }));
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
            tcpClose();
            this.Close();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            verify = new Verification(mydata);
            verify.ShowDialog();
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            //dataInit();
            this.confirmButton.IsEnabled = true;
            this.verifyButton.IsEnabled = true;
            this.closeAndSaveButton.IsEnabled = true;
            server = new TcpListener(localAddr, port);
            tcpStart();
        }
    }
}
