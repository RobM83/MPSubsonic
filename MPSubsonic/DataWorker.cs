using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MediaPortal.Configuration;
using SQLite.NET;


namespace MPSubsonic
{
    class DataWorker
    {
        private const string dbName = "MPSubsonic.db3";
        private string dbFile;
        private static DataWorker instance = null;
        private SQLiteClient db;

        public static DataWorker getDataWorker()
        {
            if (instance == null)
            {
                instance = new DataWorker();
                return instance;
            }
            else
            {
                return instance;
            }
        }

        private DataWorker() { 
            //Check if file exists otherwise create file & tables
            dbFile = Config.GetFile(Config.Dir.Database, dbName);

            if (!File.Exists(dbFile))
            {
                db = new SQLiteClient(dbFile);
                db.Execute("CREATE TABLE servers (name char(20), address char(250), username char(50), password char(50));");
            }
            else {
                db = new SQLiteClient(dbFile);
            }
        }

        /// <summary>
        /// Add a list of servers to the Database
        /// </summary>
        /// <param name="servers"></param>
        public void addServers(List<SubSonicServer> servers){
            db.Execute("DELETE FROM servers");
            foreach (SubSonicServer server in servers) {
                //string test = string.Format(server.Name + "," + server.Address + "," + server.UserName + "," + server.Password);
                string sql = string.Format("INSERT INTO servers VALUES ('{0}', '{1}', '{2}', '{3}');", server.Name, server.Address, server.UserName, server.Password);

                db.Execute(sql);
            }
        }

        /// <summary>
        /// Create a list of all servers
        /// </summary>
        /// <returns>A list with servers</returns>
        public List<SubSonicServer> getServers()
        {
            List<SubSonicServer> servers = new List<SubSonicServer>();

            SQLiteResultSet results = db.Execute("SELECT * FROM servers");
            for (int i = 0; i < results.Rows.Count; i++) {
                SubSonicServer server = new SubSonicServer();            
                //TODO make it more fault proof
                server.Name = results.Rows[i].fields[0];
                server.Address = results.Rows[i].fields[1];
                server.UserName = results.Rows[i].fields[2];
                server.Password = results.Rows[i].fields[3];
                servers.Add(server);
            }
                  

            return servers;
        }
    }
}
