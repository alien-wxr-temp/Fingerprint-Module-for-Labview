using System.Collections.Generic;

namespace xrFpIDserver
{
    public delegate void OnChangeHandler();
    public class Data
    {
        public const int MaxTemplates = 100;
        // shared data
        public string folderPath;
        public string logPath;
        public string tempName;

        public int EnrolledFingersMask = 0;
        public int MaxEnrollFingerCount = MaxTemplates;
        public bool IsEventHandlerSucceeds = true;
        public bool IsFeatureSetMatched = false;
        public int FalseAcceptRate = 0;

        public DPFP.Template[] Templates = new DPFP.Template[MaxTemplates];
        public int serialNum = 0;
        public List<string> serialName = new List<string>();

        public int userNum = 0;
        public List<database> userList = new List<database>();
        // data change notification
        public void Update() { OnChange(); }        // just fires the OnChange() event
        public event OnChangeHandler OnChange;
    }

    public class database
    {
        public string username;
        public int fpNum;
    }
}
