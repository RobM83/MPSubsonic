/**
 * Maps Subsonic Items to an MediaPortal Playlist Item.
 * 
 * The identifier is the childID.
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Playlists;

namespace MPSubsonic
{
    class MPSubSonicPlayList
    {
        
        private static MPSubSonicPlayList instance = null;
        private List<SubSonicItem> playList;
        private bool playing;
        private Int16 currentItem;


        public static MPSubSonicPlayList GetInstance()
        {
            if (instance == null)
            {
                instance = new MPSubSonicPlayList();
            }
            return instance;
        }

        private MPSubSonicPlayList() { 
            playList = new List<SubSonicItem>();
            playing = false;
            currentItem = -1;
        }

        public void AddItem(SubSonicItem item) {
            playList.Add(item);
            //PlayListItem mpItem = new PlayListItem(item.Artist + " - " + item.Title, item.ChildId, item.Duration);
            //mpItem.Type = PlayListItem.PlayListItemType.AudioStream;   
        }

        public SubSonicItem CurrentItem {
            get {
                if (playList.Count > -1)
                {
                    if (currentItem == -1)
                    {
                        currentItem = 0;
                    }
                    return playList[currentItem];
                }
                else 
                {
                    return null;    
                }
            }
        }

        public int Count{
            get{
                return playList.Count();
               }
        }

        public bool Playing {
            get {
                return playing;
            }
        }

    }
}
