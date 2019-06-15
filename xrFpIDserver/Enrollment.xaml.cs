using System;
using System.Windows;
using System.IO;
using System.Net.Sockets;

namespace xrFpIDserver
{
    /// <summary>
    /// Interaction logic for Enrollment.xaml
    /// </summary>
    public partial class Enrollment : Window, DPFP.Capture.EventHandler
    {
        #region <Member>
        private DPFP.Processing.Enrollment Enroller;
        private DPFP.Capture.Capture Capturer;
        private Data mydata;
        private NetworkStream stream;
        #endregion </Member>

        public Enrollment(Data data, NetworkStream Stream)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mydata = data;
            stream = Stream;
        }

        protected void Init()
        {
            this.statusTextBox.Clear();

            try
            {
                Capturer = new DPFP.Capture.Capture();              // Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;                   // Subscribe for capturing events.
                else
                {
                    SetPrompt("Can't initiate capture operation!");
                }

            }
            catch
            {
                System.Windows.MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Enroller = new DPFP.Processing.Enrollment();            // Create an enrollment.
            UpdateStatus();
        }

        protected void capture()
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                try
                {
                    Capturer = new DPFP.Capture.Capture();              // Create a capture operation.

                    if (null != Capturer)
                        Capturer.EventHandler = this;                   // Subscribe for capturing events.
                    else
                    {
                        SetPrompt("Can't initiate capture operation!");
                    }

                }
                catch
                {
                    System.Windows.MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Enroller = new DPFP.Processing.Enrollment();            // Create an enrollment.
                UpdateStatus();
            }));
        }

        protected void Process(DPFP.Sample Sample)
        {
            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

            // Check quality of the sample and add to enroller if it's good
            if (features != null)
                try
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
                            SetPrompt("End");
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

        private void EnrollmentWindow_Closed(object sender, EventArgs e)
        {
            Stop();
            byte[] msg = System.Text.Encoding.ASCII.GetBytes("1");
            stream.Write(msg, 0, msg.Length);
        }

        private void EnrollmentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            Start();
        }

        protected void CloseAndSave()
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

            mydata.Templates[mydata.serialNum++] = Enroller.Template;
            mydata.serialName.Add(mydata.tempName);

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
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.Close();
            }));
        }

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
            if (prompt == "End")
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    promptTextBox.Text = "Succeed";
                }));
                this.Dispatcher.Invoke(new Action(delegate
                {
                    System.Windows.MessageBox.Show(this, "Succeed", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                }));
                CloseAndSave();
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    promptTextBox.Text = prompt;
                }));
            }
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
    }
}
