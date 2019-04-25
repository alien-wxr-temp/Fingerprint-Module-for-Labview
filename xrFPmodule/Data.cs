using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xrFPmodule
{
    //
    //Refer to "UISupportSample CS\AppData.cs"
    //

    public delegate void OnChangeHandler();

    // Keeps application-wide data shared among forms and provides notifications about changes
    //
    // Everywhere in this application a "document-view" model is used, and this class provides
    // a "document" part, whereas forms implement a "view" parts.
    // Each form interested in this data keeps a reference to it and synchronizes it with own 
    // controls using the OnChange() event and the Update() notificator method.
    //

    public class Data
    {
        public const int MaxTemplates = 100;
        // shared data
        public int EnrolledFingersMask = 0;
        public int MaxEnrollFingerCount = MaxTemplates;
        public bool IsEventHandlerSucceeds = true;
        public bool IsFeatureSetMatched = false;
        public int FalseAcceptRate = 0;
        public DPFP.Template[] Templates = new DPFP.Template[MaxTemplates];

        // data change notification
        public void Update() { OnChange(); }        // just fires the OnChange() event
        public event OnChangeHandler OnChange;
    }
}
