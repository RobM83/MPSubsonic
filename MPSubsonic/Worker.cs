using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Xml;
using System.Diagnostics;

namespace MPSubsonic
{
    class Worker
    {
        private static Worker instance = null;
                
        private static string appName = "mpsubsonic";   //Application name used in requests
        private static string apiVersion = "1.5.0";     //Tested & used API version      

        //Singleton
        public static Worker GetInstance()
        {
            if (instance == null)
            {
                instance = new Worker();
            }
            return instance;
        }

        private Worker() { }

        public bool connect(SubSonicServer server) {           
            return true;
        }

        
        /// <summary>
        /// Check if the server is alive & credentials are correct
        /// </summary>
        /// <param name="server"></param>
        /// <returns>true if the login and the request was successful</returns>
        public bool ping(SubSonicServer server) {            
            String result;

            result = Request(server, "ping", null);
            XmlDocument xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(result);


            //TODO check if the server returns OK.
            if (xmlResponse.ChildNodes[1].Attributes["status"].Value == "ok")
            {
                return true;
            }
            else {
                return false;
            }            
        }

        public List<Artist> GetIndexes(SubSonicServer server, int musicFolderId) {
            //TODO add to request:  string musicFolderId, string modifiedSince
            List<Artist> artists = new List<Artist>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("musicFolderId", musicFolderId.ToString());            
            
            String result = Request(server, "getIndexes", parameters);
            
            XmlDocument xmlFolders = new XmlDocument();
            xmlFolders.LoadXml(result);

            if (xmlFolders.ChildNodes[1].Name == "subsonic-response")
            {
                //Walk through the indexes
                for (int index = 0; index < xmlFolders.ChildNodes[1].FirstChild.ChildNodes.Count; index++) {
                    for (int i = 0; i < xmlFolders.ChildNodes[1].FirstChild.ChildNodes[index].ChildNodes.Count; i++) {
                        Artist artist = new Artist();
                        artist.Name = xmlFolders.ChildNodes[1].FirstChild.ChildNodes[index].ChildNodes[i].Attributes["name"].Value;
                        artist.Id = xmlFolders.ChildNodes[1].FirstChild.ChildNodes[index].ChildNodes[i].Attributes["id"].Value;
                        artists.Add(artist);
                    }
                
                }
                
            }

            return artists;        
        }

        

        public List<SubSonicItem> GetMusicDirectory(SubSonicServer server, string folderId) {
            List<SubSonicItem> items = new List<SubSonicItem>();
            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("id", folderId);

            String result = Request(server, "getMusicDirectory", parameters);

            XmlDocument xmlFolders = new XmlDocument();
            xmlFolders.LoadXml(result);


            if (xmlFolders.ChildNodes[1].Name == "subsonic-response")
            {
                if (xmlFolders.ChildNodes[1].FirstChild.Name == "directory")
                {
                    int i = 0;
                    for (i = 0; i < xmlFolders.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
                    {
                        //<child id="11" parent="1" title="Arrival" artist="ABBA" isDir="true" coverArt="22"/>  <-- EXAMPLE!
                        SubSonicItem item = new SubSonicItem();
                        item.ChildId    = int.Parse(xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value);
                        item.ParentId   = int.Parse(xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["parent"].Value);
                        item.Title      = xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["title"].Value;
                        item.Artist     = xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["artist"].Value;
                        item.IsDir      = bool.Parse(xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["isDir"].Value);
                        item.CoverArtId = int.Parse(xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["coverArt"].Value);
                        items.Add(item);
                    }
                }
            }

            return items;

        }

        /// <summary>
        ///     Get all the music folders from the specific server
        /// </summary>
        /// <param name="server"></param>
        /// <returns>A list with all the folders on the specific server</returns>
        public Dictionary<int, string> GetMusicFolders(SubSonicServer server) { 
            Dictionary<int, string> folders = new Dictionary<int, string>();

            String result = Request(server, "getMusicFolders", null);
            
            //<subsonic-response xmlns="http://subsonic.org/restapi" status="ok" version="1.5.0">
            //  <musicFolders>
            //      <musicFolder id="0" name="Albums New"/>
            //      <musicFolder id="1" name="Albums Tagged"/>
            //      <musicFolder id="3" name="Albums Untagged"/>
            //      <musicFolder id="5" name="Films"/>
            //      <musicFolder id="6" name="Series"/>
            //  </musicFolders>
            //</subsonic-response>

            XmlDocument xmlFolders = new XmlDocument();
            xmlFolders.LoadXml(result);

            if (xmlFolders.ChildNodes[1].Name == "subsonic-response") {
                if (xmlFolders.ChildNodes[1].FirstChild.Name == "musicFolders") {
                    int i = 0;
                    for (i = 0; i < xmlFolders.ChildNodes[1].FirstChild.ChildNodes.Count; i++) {
                        int id = int.Parse(xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value);
                        string name = xmlFolders.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["name"].Value;

                        folders.Add(id, name);
                    }
                }
            }

            return folders;
        }

        /// <summary>
        ///     Does the specific request to the Subsonic server.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>The XML returned by Subsonic in a string</returns>
        private static string Request(SubSonicServer server, string method, Dictionary<string, string> parameters) {
            string authHeader = server.UserName + ":" + server.Password;
            authHeader = Convert.ToBase64String(Encoding.Default.GetBytes(authHeader));
            
            string requestURL = server.Address + "/rest/" + method + ".view?v=" + apiVersion + "&c=" + appName;
           
            if (parameters != null) {
                foreach (KeyValuePair<string, string> parameter in parameters) {
                    requestURL += "&" + parameter.Key + "=" + parameter.Value;
                }
            }

            Debug.WriteLine(requestURL);

            WebRequest request = WebRequest.Create(requestURL);
            request.Method = "GET";
            request.Headers["Authorization"] = "Basic " + authHeader;
            WebResponse response = request.GetResponse();
            
            //Long way to get the response as a string
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            Debug.Write(result);

            return result;
        }
    }
}
