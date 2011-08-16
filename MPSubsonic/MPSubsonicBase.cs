using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

namespace MPSubsonic
{
    public class MPSubsonicBase : GUIWindow, ISetupForm
    {
        private enum view { 
            servers,
            folders,
            subFolder,
            albums
        }

        //Variables
        private DataWorker dbWorker = DataWorker.getDataWorker();
        private Worker worker = Worker.GetInstance();
        private List<SubSonicServer> servers;              
        private SubSonicServer currServer;
        private view currView;

        //Controls
        [SkinControlAttribute(100)] protected GUIListControl listControl = null;
        
        
        #region ISetupForm Members

        public string PluginName()
        {
            return "Subsonic";
        }

        public string Description()
        {
            return "MediaPortal Subsonic Plugin";
        }

        public string Author()
        {
            return "Rob Maas";
        }

        public void ShowPlugin()
        {
            SetupForm setup = new SetupForm();
            setup.ShowDialog();
            //MessageBox.Show("Nothing to configure, this is just an example");
        }

        public bool CanEnable()
        {
            return true;
        }

        public int GetWindowId()
        {
            // WindowID of windowplugin belonging to this setup
            // enter your own unique code
            return 1;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has it's own setup screen
        public bool HasSetup()
        {
            return true;
        }


        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = PluginName();
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }

        // With GetID it will be an window-plugin / otherwise a process-plugin
        // Enter the id number here again
        public override int GetID
        {
            get
            {
                return 1;
            }
        }

        #endregion

        //Load skin
        public override bool Init()
        {           
            return Load(GUIGraphicsContext.Skin + @"\mpsubsonic.xml");
        }


        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            GetServers();
        }

        private void GetServers(){
                        //Get the available servers
            //TODO try / catch etc.
            servers = dbWorker.getServers();

            //Turn the servers in to menu-items
            //TODO add check if the server is available?
            //TODO bij 1 server, meteen naar folder lijst?
            if (servers.Count > 0)
            {
                for (int i = 0; i < servers.Count(); i++)
                {
                    currView = view.servers;
                    GUIListItem item = new GUIListItem();
                    item.Label = servers[i].Name;
                    item.ItemId = i;
                    item.IsFolder = true;                   
                    listControl.Add(item);
                }
                listControl.SelectedListItemIndex = 0;
                GUIControl.FocusControl(GetID, listControl.GetID);
            }
            else
            {
                GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow(
                  (int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dlg.SetHeading("No servers found!");
                dlg.SetLine(1, "Please add you server(s) in the");
                dlg.SetLine(2, "Mediaportal configuration - plugins.");
                dlg.SetLine(3, String.Empty);
                dlg.DoModal(GUIWindowManager.ActiveWindow);
            }
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (control == listControl){
                if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
                    UpdateListControl();
                }           
            base.OnClicked(controlId, control, actionType);
        }


        //private SubSonicServer getServer(string name) {
        //    for (int i = 0; i < servers.Count; i++) {
        //        if (servers[i].Name == name) {
        //            return servers[i];
        //        }
        //    }
        //    return null;
        //}

        private void UpdateListControl()
        {
            GUIListItem item;
            //Dictionary<int, string> folders;
            GUIListItem selectedItem = listControl.SelectedListItem;
            listControl.ListItems.Clear();

            if (selectedItem.Label == "..") {
                switch (currView) { 
                    case view.folders:
                        GetServers();    
                        break;
                    case view.subFolder:
                        currView = view.servers;
                        break;    
                    
                }
            }

            item = new GUIListItem();
            item.Label = "..";
            item.IsFolder = true;
            listControl.Add(item);                       

            if (selectedItem != null) {
                switch (currView){
                    case view.servers:
                        //Which server?
                        currServer = servers[selectedItem.ItemId];
                        //Get Folder
                        Dictionary<int, string> folders = worker.GetMusicFolders(currServer);

                        foreach (KeyValuePair<int, string> folder in folders)
                        {
                            item = new GUIListItem();
                            item.ItemId = folder.Key;
                            item.Label = folder.Value;
                            item.IsFolder = true;
                            listControl.Add(item);
                        }

                        currView = view.folders;
                        break;
                    case view.folders:
                        //get folder (artists)
                        List<Artist> artists = worker.GetIndexes(currServer, selectedItem.ItemId);
                        for (int i = 0; i < artists.Count; i++)
                        {
                            item = new GUIListItem();
                            item.Label = artists[i].Name;
                            //item.Label2 = artists[i].Id;
                            item.IsFolder = true;
                            listControl.Add(item);
                        }
                        currView = view.subFolder;                        
                        break;
                    case view.subFolder:
                        //get subfolders
                        List<SubSonicItem> items = worker.GetMusicDirectory(currServer, selectedItem.Label2);
                        for (int i = 0; i < items.Count; i++)
                        {
                            item = new GUIListItem();
                            item.Label = items[i].Title;
                            //.selectedItem.item.Label2 = subfolders[i].Id;
                            item.IsFolder = items[i].IsDir;
                            listControl.Add(item);
                        }
                        currView = view.subFolder;
                        break;
                    case view.albums:
                        break;
                }

            }

            listControl.SelectedListItemIndex = 0;
            GUIControl.FocusControl(GetID, listControl.GetID);


        }

    }
}

    
