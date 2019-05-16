using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace xrFPConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();
            // simple ui --- main
            Report("Welcome to xrFPConsole!\r\n");
            Report("-----------------------\r\n");
            Report("| 1-Enroll\r\n");
            Report("| 2-Verify\r\n");
            Report("| 3-Save and Quit\r\n");
            Report("-----------------------\r\n");
            Report("Please input your command:");
            string cmd;
            while ((cmd = Console.ReadLine()) != "3")
            {
                switch (cmd)
                {
                    case "1":
                        Report("enroll\r\n");
                        // start enrollment
                        Enrollment();
                        Report("\r\nPlease input your command:");
                        break;
                    case "2":
                        Report("verify\r\n");
                        // start Verification
                        Verification();
                        Report("\r\nPlease input your command:");
                        break;
                    default:
                        Report("\r\nPlease input a correct command again:");
                        break;
                }
            }
            Report("Bye!\r\n");
            Quit();
            Console.ReadKey();

        }

        #region <Member>
        // tcp thread
        private static Thread tcpThread;
        // data
        private static Data mydata;
        // tcp member
        private static TcpListener listener = null;
        private static TcpClient client;
        private static NetworkStream stream;
        private static Int32 port;
        private const IPAddress localhost = IPAddress.Parse("127.0.0.1");

        // enrollment member
        private static DPFP.Capture.Capture capturer;
        private static DPFP.Processing.Enrollment enroller;
        // verification member
        private static int counter = 5;
        private static DPFP.Verification.Verification verifier;
        #endregion </Member>

        #region <TCP>
        static void TcpThread()
        {
            try
            {

            }
            catch (SocketException e)
            {
                string msg = "SocketException: " + e + "\r\n";
                Report(msg);
                throw;
            }
        }
        #endregion </TCP>

        #region <Data>
        public static void DataInit()
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
        private static void MySR()
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
        private static void MySW()
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
        private static void ExchangeData(bool read)
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
        static void Enrollment()
        {
            Report("Please input your name:");
            mydata.tempName = Console.ReadLine();
        }
        #endregion </Enrollment>

        #region <Verification>
        static void Verification()
        {

        }
        #endregion </Verification>

        static void Init()
        {
            // init data
            DataInit();
            // init tcp
            tcpThread = new Thread(new ThreadStart(TcpThread));
            tcpThread.Start();
        }

        static void Quit()
        {
            // save DataLog.txt
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
            // abort tcp thread
            tcpThread.Abort();
        }

        static void Report(string msg)
        {
            Console.Write(msg);
        }
    }
}
