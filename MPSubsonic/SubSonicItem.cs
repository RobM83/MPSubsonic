/**
 * SubSonic Item - Folder or Music item
 * 
 * Contains a folder or (playable) "file".
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPSubsonic
{
    class SubSonicItem
    {
        //child id="11" parent="1" title="Arrival" artist="ABBA" isDir="true" coverArt="22"/>
        
        //<child id="111" parent="11" title="Dancing Queen" isDir="false"
	    //           album="Arrival" artist="ABBA" track="7" year="1978" genre="Pop" coverArt="24"
	   //            size="8421341" contentType="audio/mpeg" suffix="mp3" duration="146" bitRate="128"
	   //            path="ABBA/Arrival/Dancing Queen.mp3"/>

        private string _childId;    //Folder - Item
        private string _parentId;   //Folder - Item
        private string _title;      //Folder - Item
        private string _artist;     //Folder - Item
        private bool _isDir;        //Folder - Item
        private string _coverArtId; //Folder - Item
        private string _album;      //Item
        private string _track;      //Item
        private string _year;       //Item
        private string _genre;      //Item
        private int _size;          //Item
        private string _contentType;//Item
        private string _suffix;     //Item
        private int _duration;      //Item
        private int _bitrate;       //Item
        private string _path;       //Item

        public string ChildId{
            set {
                _childId = value;
            }
            get {
                return _childId;
            }
        }

        public string ParentId {
            set {
                _parentId = value;
            }
            get {
                return _parentId;
            }
        }

        public string Title {
            set {
                _title = value;
            }
            get {
                return _title;
            }
        }

        public string Artist {
            set {
                _artist = value;
            }
            get {
                return _artist;
            }
        }

        public bool IsDir {
            set {
                _isDir = value;
            }
            get {
                return _isDir;
            }
        }

        public string CoverArtId {
            set {
                _coverArtId = value;
            }
            get {
                return _coverArtId;
            }
        }

        public string Album {
            set {
                _album = value;
            }
            get {
                return _album;
            }
        }

        public string Track {
            set {
                _track = value;
            }
            get {
                return _track;
            }
        }

        public string Year {
            set {
                _year = value;
            }
            get {
                return _year;
            }
        }

        public string Genre {
            set {
                _genre = value;
            }
            get {
                return _genre;
            }
        }

        public int Size {
            set {
                _size = value;
            }
            get {
                return _size;
            }
        }

        public string ContentType {
            set {
                _contentType = value;
            }
            get {
                return _contentType;
            }
        }

        public string Suffix {
            set {
                _suffix = value;
            }
            get {
                return _suffix;
            }
        }

        public int Duration {
            set {
                _duration = value;
            }
            get {
                return _duration;
            }
        }

        public int Bitrate {
            set {
                _bitrate = value;
            }
            get {
                return _bitrate;
            }
        }

        public string Path {
            set {
                _path = value;
            }
            get {
                return _path;
            }
        }
    }
}
