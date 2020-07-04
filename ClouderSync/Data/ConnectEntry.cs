using System;
using System.IO;
using System.Xml.Serialization;

namespace ClouderSync.Data
{
    [Serializable]
    public class CONNECTENTRY 
    {
        //
        // Summary:
        //     identifier that represents the selected item. For valid itemid values, see VSITEMID.
        [XmlAttribute]
        public string hostname { get; set; }// = "localhost";
        [XmlAttribute]
        public int port { get; set; }// = "localhost";
        [XmlAttribute]
        public string username { get; set; }
        [XmlAttribute]
        public string plainpass { get; set; }
        [XmlAttribute]
        public bool usekeyfile {
            get;
            set;//{ usekeyfile = value; OnPropertyChanged("usekeyfile"); }
        }
        [XmlAttribute]
        public string keyfile { get; set; }
        [XmlAttribute]
        public string localsrcpath { get; set; }
        [XmlAttribute]
        public string remotesrcpath { get; set; }
        [XmlAttribute]
        public bool ignoreexcludedfiles
        {
            get;
            set;//{ usekeyfile = value; OnPropertyChanged("usekeyfile"); }
        }

        public CONNECTENTRY()
        {
            hostname = "";
            port = 22;
            username = "";
            plainpass = "";
            usekeyfile = false;
            keyfile = "";
            localsrcpath = "";
            remotesrcpath = "";
            ignoreexcludedfiles = false;
        }

    }
}