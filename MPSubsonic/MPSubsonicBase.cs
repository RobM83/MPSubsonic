using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private enum view { 
            servers,
            musicFolders,
            artists,
            items
        }

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

        private PlayList currPlaylist = new PlayList();
        private PlayListPlayer plPlayer = PlayListPlayer.SingletonPlayer;

        //Controls
        [SkinControlAttribute(100)] protected GUIListControl listControl = null;
        [SkinControlAttribute(2)] protected GUIButtonControl btnSearch = null;
        [SkinControlAttribute(3)] protected GUIButtonControl btnPlayList = null;


        
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

            plPlayer = PlayListPlayer.SingletonPlayer;
            plPlayer.Reset();
            currPlaylist = plPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);            
            plPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

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
                listControl.ListItems.Clear();
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
            if (control == listControl){
                if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
                    UpdateListControl();            
                    //play();
                }
            else if (control == btnPlayList){
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
                        
                        //A File
                        //TODO Make sure it is a audio file (for now!)
                      //g_Player.PlayAudioStream(worker.GetStreamString(currServer, currItem.ChildId));
                        //g_Player.currentTitle = currItem.Title;
                        //GUIPropertyManager.SetProperty("#Play.Current.Title", Util.Utils.GetFilename(fileName));
                        //g_Player.currentDescription = currItem.Title;
                      //GUIPropertyManager.SetProperty("#Play.Current.Artist", currItem.Artist);
                      //GUIPropertyManager.SetProperty("#Play.Current.Title", currItem.Title);
                      //GUIPropertyManager.SetProperty("#Play.Current.Album", currItem.Album);
                      //GUIPropertyManager.SetProperty("#Play.Current.Year", currItem.Year.ToString());
                      //GUIPropertyManager.SetProperty("#Play.Current.Track", currItem.Track.ToString());
                      //GUIPropertyManager.SetProperty("#Play.Current.Thumb", worker.GetCoverArt(currServer, currItem.CoverArtId));

                        AddToPlaylist(currServer, currItem);
                        return;
                    }

                }


                if (history != null)
                {
                    history.Add(selectedItem.ItemId);
                }

            }

            listControl.ListItems.Clear();
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


        private void AddToPlaylist(SubSonicServer server, SubSonicItem item){
            PlayListItem mpItem = new PlayListItem(item.Artist + " - " + item.Title, worker.GetStreamString(server, item.ChildId), item.Duration);
            mpItem.Type = PlayListItem.PlayListItemType.AudioStream;
            
            MusicTag tag = new MusicTag();
            tag.Album = item.Album;
            tag.Artist = item.Artist;
            //tag.CoverArtFile = worker.GetCoverArt(server, item.CoverArtId);
            tag.Year = int.Parse(item.Year);
            tag.Track = int.Parse(item.Track);            
            mpItem.MusicTag = tag;

            //playlistPlayer = PlayListPlayer.SingletonPlayer;
            //PlayList pl = playlistPlayer.GetPlaylist(PlayListType.Video);
            //playlistPlayer.CurrentPlaylistType = PlayListType.Video;

            currPlaylist.Add(mpItem);

            if (!g_Player.Playing)
            {
                plPlayer.Play(0);
            }
        }

    }
    
}

    
