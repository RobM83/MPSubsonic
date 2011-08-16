using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace MPSubsonic
{
    public partial class SetupForm : Form
    {
        private List<SubSonicServer> servers = new List<SubSonicServer>();
        private int selectedServer;
        private DataWorker dbWorker = DataWorker.getDataWorker();

        public SetupForm()
        {
            InitializeComponent();
            servers = dbWorker.getServers();
            RefreshList();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            dbWorker.addServers(servers);
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            SubSonicServer server = new SubSonicServer();
            int uid = lstServers.Items.Count + 1;
            server.Name = "DummyServer" + uid.ToString();
            servers.Add(server);           
            RefreshList();
        }


        private void RefreshList(){
            //Refresh the listbox
            lstServers.DataSource = null;
            lstServers.Refresh();
            lstServers.DataSource = servers;
            lstServers.DisplayMember = "Name";
            if (lstServers.Items.Count < 1)
            {
                txtName.Enabled = false;
                txtAddress.Enabled = false;
                txtUserName.Enabled = false;
                txtPassword.Enabled = false;
                btnOk.Enabled = false;
                btnDelete.Enabled = false;
                btnCheck.Enabled = false;
                selectedServer = 0;
            }
            else {
                txtName.Enabled = true;
                txtAddress.Enabled = true;
                txtUserName.Enabled = true;
                txtPassword.Enabled = true;
                btnOk.Enabled = true;
                btnDelete.Enabled = true;
                btnCheck.Enabled = true;
                lstServers.SelectedIndex = 0; //TODO select the last selected if possible
            }

        }

        private void lstServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Show the right server in de details            
            if (lstServers.SelectedValue != null) {
                selectedServer = lstServers.SelectedIndex;
                txtName.Text = servers[selectedServer].Name;
                txtAddress.Text = servers[selectedServer].Address;
                txtUserName.Text = servers[selectedServer].UserName;
                txtPassword.Text = servers[selectedServer].Password;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            servers.RemoveAt(selectedServer);
            RefreshList();            
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            SubSonicServer server = updateServer(servers[selectedServer]);
            Worker wrk = Worker.GetInstance();
            if (wrk.ping(server)) {
                MessageBox.Show("Yeah! Let's rock.");
            }else{
                MessageBox.Show("Bummer");
            }
        }

        private SubSonicServer updateServer(SubSonicServer server){          
            //TODO Add checks on input
            server.Name = txtName.Text;
            if ((txtAddress.Text.ToLower()).Substring(0, 4) != "http") {
                txtAddress.Text = "http://" + txtAddress.Text;
            }
            server.Address = txtAddress.Text;
            server.UserName = txtUserName.Text;
            server.Password = txtPassword.Text;
            return server;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //Save the new data
            updateServer(servers[selectedServer]);
            RefreshList();
        }
    }
}
