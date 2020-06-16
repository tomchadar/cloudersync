﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;

namespace CloudSync.Data
{
    public class ConnectEntryData : INotifyPropertyChanged
    {
        static string fileName = @"k:\codercrest\testme.xml";
        public CONNECTENTRY ce= new CONNECTENTRY();

        public ConnectEntryData()
        {
            //ce = new CONNECTENTRY();
        }
        public static string GetSettingsFile()
        {
            string fileDir = CloudSyncPackage.GetVSAppFolder();
            fileName = Path.Combine(fileDir, "connectionentry.xml");
            return fileName;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(ce, new PropertyChangedEventArgs(name));
            }
        }
        public bool ReadEntry()
        {
            return ReadEntry(ref this.ce);
        }
        protected bool ReadEntry(ref CONNECTENTRY ceDest)
        {
            try
            {
                string xmlFile = GetSettingsFile();
                XmlSerializer serializer = new XmlSerializer(typeof(CONNECTENTRY));// typeof(CONNECTENTRY));
                FileStream textReader = new FileStream(xmlFile, FileMode.Open);
                ceDest = (CONNECTENTRY)serializer.Deserialize(textReader);
                textReader.Close();
            }
            catch (Exception eRead)
            {
                CloudSyncPackage.Log(eRead.Message);
                return false;
            }
            return true;
        }
        public bool SaveEntry()
        {
            try
            {
                string xmlFile = GetSettingsFile();
                XmlSerializer serializer = new XmlSerializer(typeof(CONNECTENTRY));// typeof(CONNECTENTRY));
                TextWriter textWriter = new StreamWriter(fileName);
                serializer.Serialize(textWriter, (CONNECTENTRY)this.ce);
                textWriter.Close();
            }
            catch (Exception eWrite)
            {
                CloudSyncPackage.Log(eWrite.Message);
                return false;
            }
            return true;
        }

    }
}
