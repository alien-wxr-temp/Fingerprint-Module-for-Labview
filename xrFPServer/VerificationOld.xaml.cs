using System.Windows;
using System.Net;
using System.Net.Sockets;
using System;

namespace xrFPServer
{
    /// <summary>
    /// Interaction logic for Verification.xaml
    /// </summary>
    public partial class Verification : Window
    {
        private Data mydata;
        private NetworkStream stream;

        public Verification(Data data,NetworkStream Stream)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mydata = data;
            stream = Stream;
        }

        private void VerifyControl_OnComplete(object Control, DPFP.FeatureSet FeatureSet, ref DPFP.Gui.EventHandlerStatus EventHandlerStatus)
        {
            DPFP.Verification.Verification ver = new DPFP.Verification.Verification();
            DPFP.Verification.Verification.Result res = new DPFP.Verification.Verification.Result();

            // Compare feature set with all stored templates.
            for (int i = 0; i < mydata.serialNum; i++)
            {
                ver.Verify(FeatureSet, mydata.Templates[i], ref res);
                mydata.IsFeatureSetMatched = res.Verified;
                mydata.FalseAcceptRate = res.FARAchieved;
                if (res.Verified)
                {
                    string msg = "Welcome! " + mydata.serialName[i];
                    System.Windows.MessageBox.Show(msg, "Welcome", MessageBoxButton.OK);
                    msg = "1" + string.Format("{0:00}", mydata.serialName[i].Length) + mydata.serialName[i];
                    byte[] msg2 = System.Text.Encoding.ASCII.GetBytes(msg);
                    stream.Write(msg2, 0, msg2.Length);
                    this.Close();
                    break;
                }
            }

            if (!res.Verified)
            {
                string msg = "Unknown access!";
                System.Windows.MessageBox.Show(msg, "Warning", MessageBoxButton.OK);
                byte[] msg2 = System.Text.Encoding.ASCII.GetBytes("0");
                stream.Write(msg2, 0, msg2.Length);
                EventHandlerStatus = DPFP.Gui.EventHandlerStatus.Failure;
            }

            mydata.Update();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes("0");
            stream.Write(msg, 0, msg.Length);
            this.Close();
        }
    }
}
