using System.Windows;

namespace xrFPServer
{
    /// <summary>
    /// Interaction logic for Verification.xaml
    /// </summary>
    public partial class Verification : Window
    {
        private Data mydata;

        public Verification(Data data)
        {
            InitializeComponent();
            mydata = data;
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
                    break;
                }
            }

            if (!res.Verified)
                EventHandlerStatus = DPFP.Gui.EventHandlerStatus.Failure;

            mydata.Update();
        }
    }
}
