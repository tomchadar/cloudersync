using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace ClouderSync.Views
{
    public partial class ConnectionList : Form
    {
        protected ClouderSyncPackage package;

        public ConnectionList(ClouderSyncPackage _package)
        {
            package = _package;
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnNewConnection_Click(object sender, EventArgs e)
        {
            ConnectionEntryForm dlgSettings = new ConnectionEntryForm(package);

            DialogResult res=dlgSettings.ShowDialog();


        }

       
    }
}
