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
        private int counter = 5;

        public Verification(Data data,NetworkStream Stream)
        {
            InitializeComponent();
            mydata = data;
            stream = Stream;
        }

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
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("1"+mydata.serialName[i]);
                    stream.Write(msg, 0, msg.Length);
                    msg = System.Text.Encoding.ASCII.GetBytes("21");
                    stream.Write(msg, 0, msg.Length);
                    this.Close();
                    break;
                }
            }

            if (!res.Verified)
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("0"+Convert.ToString(counter));
                stream.Write(msg, 0, msg.Length);
                EventHandlerStatus = DPFP.Gui.EventHandlerStatus.Failure;
            }

            if (counter == 0)
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("20");
                stream.Write(msg, 0, msg.Length);
                this.Close();
            }

            mydata.Update();
        }
    }
}
