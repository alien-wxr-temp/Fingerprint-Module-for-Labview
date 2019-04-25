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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace xrFPmodule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    // a simple delegate for marshalling calls from event handlers to the GUI thread
    delegate void Function();

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            mydata = new Data();
            mydata.folderPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data";
            mydata.logPath = mydata.folderPath + "\\dataLog.txt";
            if (!System.IO.Directory.Exists(mydata.folderPath))
                System.IO.Directory.CreateDirectory(mydata.folderPath);
            if (!System.IO.File.Exists(mydata.logPath))
                System.IO.File.Create(mydata.logPath);
        }

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            //Enrollment
            Enrollment enroller = new Enrollment();
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Fingerprint Template File (*.fpt)|*.fpt";
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = File.Open(save.FileName, FileMode.Create, FileAccess.Write))
                {
                    Template.Serialize(fs);
                }
            }
        }

        private new DPFP.Template Template;
        private Data mydata;
    }
}
