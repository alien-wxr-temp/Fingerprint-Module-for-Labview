using System;
using System.Windows;
using System.IO;
using System.Threading;

namespace xrFPmodule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window, DPFP.Capture.EventHandler
    {
        public Data mydata;
        public Thread enroller;
        private DPFP.Processing.Enrollment Enroller;
        private DPFP.Capture.Capture Capturer;
        public int counter;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region data:
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
        #endregion

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = true;
            statusTextBox.Clear();
            // Enrollment Group Work
            enroller = new Thread(new ThreadStart(Enrollment));
            enroller.Start();
        }

        #region Enrollment:
        private void Enrollment()
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();				// Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    SetPrompt("Can't initiate capture operation!");
            }
            catch
            {
                System.Windows.MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Enroller = new DPFP.Processing.Enrollment();            // Create an enrollment.
            UpdateStatus();
            Start();
        }

        protected void Process(DPFP.Sample Sample)
        {
            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

            // Check quality of the sample and add to enroller if it's good
            if (features != null) try
                {
                    MakeReport("The fingerprint feature set was created.");
                    Enroller.AddFeatures(features);     // Add feature set to template.
                }
                finally
                {
                    UpdateStatus();

                    // Check if template has been created.
                    switch (Enroller.TemplateStatus)
                    {
                        case DPFP.Processing.Enrollment.Status.Ready:   // report success and stop capturing
                            SetPrompt("Click Close, and then click Fingerprint Verification.");
                            Stop();
                            break;

                        case DPFP.Processing.Enrollment.Status.Failed:  // report failure and restart capturing
                            Enroller.Clear();
                            Stop();
                            UpdateStatus();
                            Start();
                            break;
                    }
                }
        }

        protected void Start()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    SetPrompt("Can't initiate capture!");
                }
            }
        }

        protected void Stop()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StopCapture();
                }
                catch
                {
                    SetPrompt("Can't terminate capture!");
                }
            }
        }

        #region Window Event Handlers:

        private int matching()
        {
            for (int i = 0; i < mydata.userNum; i++)
            {
                if (mydata.tempName == mydata.userList[i].username)
                    return i;
            }
            return -1;
        }

        #endregion

        #region EventHandler Members:

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            MakeReport("The fingerprint sample was captured.");
            SetPrompt("Scan the same fingerprint again.");
            Process(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was touched.");
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was disconnected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                MakeReport("The quality of the fingerprint sample is good.");
            else
                MakeReport("The quality of the fingerprint sample is poor.");
        }
        #endregion

        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();  // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        protected void SetStatus(string status)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                statusLabel.Content = status;
            }));
        }

        protected void SetPrompt(string prompt)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                promptTextBox.Text = prompt;
                if (prompt == "Click Close, and then click Fingerprint Verification.")
                    this.SaveButton.IsEnabled = true;
            }));
        }
        protected void MakeReport(string message)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                statusTextBox.AppendText(message + "\r\n");
                statusTextBox.ScrollToEnd();
            }));
        }

        private void UpdateStatus()
        {
            // Show number of samples needed.
            SetStatus(String.Format("Fingerprint samples needed: {0}", Enroller.FeaturesNeeded));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int userSeiral = matching();
            //save template
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            if (userSeiral != -1)
            {
                save.FileName = mydata.folderPath + "\\" + string.Format("{0:0000}", userSeiral);
                save.FileName += string.Format("{0:0000}", ++mydata.userList[userSeiral].fpNum);
                save.FileName += ".fpt";
            }
            else
            {
                save.FileName = mydata.folderPath + "\\" + string.Format("{0:0000}", mydata.userNum++);
                save.FileName += "0001.fpt";
                database user = new database();
                user.username = mydata.tempName;
                user.fpNum = 1;
                mydata.userList.Add(user);
            }
            using (FileStream fs = File.Open(save.FileName, FileMode.Create, FileAccess.Write))
            {
                Enroller.Template.Serialize(fs);
            }
            EnrollEnd();
        }

        private void EnrollEnd()
        {
            this.SaveButton.IsEnabled = false;
            enroller.Abort();
        }
        #endregion

        #region Verification:
        private void VerifyControl_OnComplete(object Control, DPFP.FeatureSet FeatureSet, ref DPFP.Gui.EventHandlerStatus EventHandlerStatus)
        {
            DPFP.Verification.Verification ver = new DPFP.Verification.Verification();
            DPFP.Verification.Verification.Result res = new DPFP.Verification.Verification.Result();
            counter--;

            // Compare feature set with all stored templates.
            for (int i = 0; i < mydata.serialNum; i++)
            {
                ver.Verify(FeatureSet, mydata.Templates[i], ref res);
                mydata.IsFeatureSetMatched = res.Verified;
                mydata.FalseAcceptRate = res.FARAchieved;
                if (res.Verified)
                {
                    System.Windows.MessageBox.Show("Welcome! " + mydata.serialName[i]);
                    counter = 0;
                    break;
                }
            }

            if (!res.Verified)
            {
                System.Windows.MessageBox.Show("Unkown fingerprint!!!");
                EventHandlerStatus = DPFP.Gui.EventHandlerStatus.Failure;
            }

            /*
            if (counter == 0)
            {
                this.Close();
            }
            */
            mydata.Update();
        }
        #endregion

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            mydata.tempName = this.nameTextBox.Text;
            this.enrollButton.IsEnabled = true;
        }

        private void QuitAndSaveButton_Click(object sender, RoutedEventArgs e)
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
            Stop();
            this.Close();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            //
            counter = 5;
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            dataInit();
            this.confirmButton.IsEnabled = true;
            this.verifyButton.IsEnabled = true;
            this.quitAndSaveButton.IsEnabled = true;
        }
    }
}
