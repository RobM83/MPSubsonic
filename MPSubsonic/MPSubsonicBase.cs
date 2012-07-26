using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;

namespace MPSubsonic
{
    public class MPSubsonicBase : GUIWindow, ISetupForm
    {
        private enum view
        {
            servers,
            musicFolders,
            artists,
            items
        }

        //How should the "icons" look
        private enum mpView
        {
            List,
            Icons,
            BigIcons,
            Albums,
            Filmstrip,
            Coverflow
        }

        #region MapSettings class
        [Serializable]
        public class MapSettings
        {
            protected int _SortBy;
            protected int _ViewAs;
            protected bool _SortAscending;

            public MapSettings()
            {
                // Set default view
                _SortBy = 0;
                _ViewAs = (int)mpView.List;
                _SortAscending = true;
            }

            [XmlElement("SortBy")]
            public int SortBy
            {
                get { return _SortBy; }
                set { _SortBy = value; }
            }

            [XmlElement("ViewAs")]
            public int ViewAs
            {
                get { return _ViewAs; }
                set { _ViewAs = value; }
            }

            [XmlElement("SortAscending")]
            public bool SortAscending
            {
                get { return _SortAscending; }
                set { _SortAscending = value; }
            }
        }
        #endregion

        //Variables
        private DataWorker dbWorker = DataWorker.getDataWorker();
        private Worker worker = Worker.GetInstance();
        private List<SubSonicServer> servers;

        private SubSonicServer currServer;
        private view currView;
        private SubSonicItem currItem;

        private List<int> history;

        private List<Artist> artists;
        private List<SubSonicItem> items;
        private List<SubSonicItem> prevItems = new List<SubSonicItem>();
        private List<SubSonicItem> prevprevItems = new List<SubSonicItem>(); //Yikes!

        private MPSubSonicPlayList playList = MPSubSonicPlayList.GetInstance();
        //private PlayList currPlaylist = new PlayList();
        //private PlayListPlayer plPlayer = PlayListPlayer.SingletonPlayer;

        private MapSettings mapSettings = new MapSettings();

        //Controls
        //[SkinControlAttribute(100)] protected GUIListControl listControl = null;
        [SkinControlAttribute(100)]
        protected GUIFacadeControl listControl = null;
        [SkinControlAttribute(10)]
        protected GUIButtonControl btnSwitchView = null;
        [SkinControlAttribute(20)]
        protected GUIButtonControl btnSearch = null;
        [SkinControlAttribute(30)]
        protected GUIButtonControl btnPlayList = null;



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
            return 827966;
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
                return 827966;
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

            //plPlayer = PlayListPlayer.SingletonPlayer;
            //plPlayer.Reset();
            //currPlaylist = plPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            //plPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

            GetServers();
        }

        private void GetServers()
        {
            //Get the available servers
            //TODO try / catch etc.
            servers = dbWorker.getServers();

            //Turn the servers in to menu-items
            //TODO add check if the server is available?
            //TODO bij 1 server, meteen naar folder lijst? ?
            if (servers.Count > 0)
            {
                //listControl.ListItems.Clear();
                listControl.Clear();
                for (int i = 0; i < servers.Count(); i++)
                {
                    currItem = null;
                    history = null;
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
            if (control == listControl)
            {
                if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
                    UpdateListControl();
                //play();
            }
            else if (control == btnSwitchView)
            {
                //Change view
                OnShowLayouts();
                ShowPanel();
                GUIControl.FocusControl(GetID, control.GetID);                  
                
            }
            else if (control == btnPlayList)
            {
                GUIWindowManager.ActivateWindow(827967);
            }
            
            base.OnClicked(controlId, control, actionType);
        }

        private void UpdateListControl()
        {
            //This method takes care of returning the right items to the browse list
            //The current code is a mess, but couldn't figure out quick enough a nice and clean way
            //so it definately needs some revision.
            //TODO rebuild UpdateListControl()           


            GUIListItem selectedItem = listControl.SelectedListItem;

            //View states
            //servers -> musicFolders -> artists -> items -> items -> items ...

            if (selectedItem.Label == "..")
            {
                if (history.Count < 5)
                {
                    //Go back to Artist/Music/Server
                    switch (history.Count)
                    {
                        case 1:
                            GetServers();
                            break;
                        case 2:
                            currView = view.servers;
                            break;
                        case 3:
                            currView = view.musicFolders;
                            break;
                        case 4:
                            currView = view.artists;
                            break;
                    }
                    history.RemoveAt(history.Count - 1);
                    selectedItem.ItemId = history[history.Count - 1];
                }
                else
                {
                    //Go back to previous item.                                        
                    history.RemoveAt(history.Count - 1);
                    selectedItem.ItemId = history[history.Count - 1];
                    currItem = prevprevItems[selectedItem.ItemId];
                }
            }
            else
            {
                if (currView == view.items)
                {
                    currItem = items[selectedItem.ItemId];

                    if (!currItem.IsDir)
                    {
                        //TODO clean this mess up
                        currItem.Server = currServer;
                        AddToPlaylist(currItem);
                        return;
                    }

                }


                if (history != null)
                {
                    history.Add(selectedItem.ItemId);
                }

            }

            //listControl.ListItems.Clear();            
            listControl.Clear();
            GUIListItem item;
            item = new GUIListItem();
            item.Label = "..";
            item.ItemId = -1;
            item.IsFolder = true;
            listControl.Add(item);

            switch (currView)
            {
                case view.servers:
                    //Which server?
                    if (selectedItem.ItemId != -1)
                    {
                        history = new List<int>();
                        history.Add(selectedItem.ItemId);
                        currServer = servers[selectedItem.ItemId];
                    }

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

                    currView = view.musicFolders;
                    break;
                case view.musicFolders:
                    //get folder (artists)                                                               
                    artists = worker.GetIndexes(currServer, selectedItem.ItemId);
                    for (int i = 0; i < artists.Count; i++)
                    {
                        item = new GUIListItem();
                        item.Label = artists[i].Name;
                        item.ItemId = i;
                        item.IsFolder = true;
                        listControl.Add(item);
                    }
                    currView = view.artists;
                    break;
                case view.artists:
                    //get items                        
                    items = worker.GetMusicDirectory(currServer, artists[selectedItem.ItemId].Id);
                    for (int i = 0; i < items.Count; i++)
                    {
                        item = new GUIListItem();
                        item.Label = items[i].Title;
                        item.ItemId = i;
                        item.IsFolder = items[i].IsDir;
                        listControl.Add(item);
                    }
                    currView = view.items;
                    break;
                case view.items:
                    //get (sub) items                                       
                    prevprevItems.Clear();
                    for (int i = 0; i < prevItems.Count; i++)
                    {
                        prevprevItems.Add(prevItems[i]);
                    }
                    prevItems.Clear();
                    for (int i = 0; i < items.Count; i++)
                    {
                        prevItems.Add(items[i]);
                    }
                    items = worker.GetMusicDirectory(currServer, currItem.ChildId);
                    for (int i = 0; i < items.Count; i++)
                    {
                        item = new GUIListItem();
                        item.Label = items[i].Title;
                        item.ItemId = i;
                        item.IsFolder = items[i].IsDir;
                        listControl.Add(item);
                    }
                    currView = view.items;

                    break;
            }

            listControl.SelectedListItemIndex = 0;
            GUIControl.FocusControl(GetID, listControl.GetID);


        }

       
        private void AddToPlaylist(SubSonicItem item)
        {
            //TODO remove server and add it to item
            playList.AddItem(item);
            Play();            
        }

        private void Play() {            
            if (!g_Player.Playing)
            {
                //Select the "right" file
                SubSonicItem item = playList.CurrentItem;

                //Start playing
                g_Player.PlayAudioStream(worker.GetStreamString(item.Server, item.ChildId));

                //File details
                g_Player.currentTitle = item.Title;
                g_Player.currentDescription = item.Title;
                GUIPropertyManager.SetProperty("#Play.Current.Artist", item.Artist);
                GUIPropertyManager.SetProperty("#Play.Current.Title", item.Title);
                GUIPropertyManager.SetProperty("#Play.Current.Album", item.Album);
                GUIPropertyManager.SetProperty("#Play.Current.Year", item.Year.ToString());
                GUIPropertyManager.SetProperty("#Play.Current.Track", item.Track.ToString());
                GUIPropertyManager.SetProperty("#Play.Current.Thumb", worker.GetCoverArt(item.Server, item.CoverArtId));
            }
        
        }

        protected virtual void OnShowLayouts()
        {
            GUIDialogMenu guiDialogMenu1 = (GUIDialogMenu)GUIWindowManager.GetWindow(2012);
            if (guiDialogMenu1 == null)
                return;
            guiDialogMenu1.Reset();
            guiDialogMenu1.SetHeading(792);
            guiDialogMenu1.Add(GUILocalizeStrings.Get(101));
            guiDialogMenu1.Add(GUILocalizeStrings.Get(100));
            guiDialogMenu1.Add(GUILocalizeStrings.Get(417));
            guiDialogMenu1.Add(GUILocalizeStrings.Get(529));
            guiDialogMenu1.Add(GUILocalizeStrings.Get(733));
            guiDialogMenu1.Add(GUILocalizeStrings.Get(791));

            guiDialogMenu1.SelectedLabel = mapSettings.ViewAs;
            guiDialogMenu1.DoModal(this.GetID);
            if (guiDialogMenu1.SelectedId == -1)
                return;
            mapSettings.ViewAs = guiDialogMenu1.SelectedId - 1;
        }

        void ShowPanel()
        {
            int itemIndex = listControl.SelectedListItemIndex;
            if (mapSettings.ViewAs == (int)mpView.BigIcons)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
            }
            else if (mapSettings.ViewAs == (int)mpView.Albums)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.AlbumView;
            }
            else if (mapSettings.ViewAs == (int)mpView.Icons)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.SmallIcons;
            }
            else if (mapSettings.ViewAs == (int)mpView.List)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.List;
            }
            else if (mapSettings.ViewAs == (int)mpView.Filmstrip)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
            }
            else if (mapSettings.ViewAs == (int)mpView.Coverflow)
            {
                listControl.CurrentLayout = GUIFacadeControl.Layout.CoverFlow;
            }
            if (itemIndex > -1)
            {
                GUIControl.SelectItemControl(GetID, listControl.GetID, itemIndex);
            }
        }
    }
}
    