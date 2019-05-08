using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace xrFPmodule
{
    /// <summary>
    /// Interaction logic for Verification.xaml
    /// </summary>
    public partial class Verification : Window
    {
        private Data mydata;
        private int counter = 5;

        public Verification(Data data)
        {
            InitializeComponent();
            mydata = data;
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

            if (counter==0)
            {
                this.Close();
            }

            mydata.Update();
        }
    }
}
