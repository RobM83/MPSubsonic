using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPSubsonic
{
    class SubSonicServer
    {
        private string _name;           //Global Name
        private string _address;        //URL or IP of the server
        private string _username;       //Username to login
        private string _password;       //Password to login
       
        //Properties
        public string Name {
            set {
                _name = value;
            }
            get {
                return _name;
            }
        }

        public string Address {
            set {
                _address = value;
            }
            get {
                return _address;
            }
        }

        public string UserName {
            set {
                _username = value;
            }
            get {
                return _username;
            }
        }

        public string Password {
            set {
                _password = value;
            }
            get {
                return _password;
            }
        }

        //Constructor
        public SubSonicServer() { }

        public SubSonicServer(String name, String address, String username, String password) {
            _name = name;
            _address = address;
            _username = username;
            _password = password;
        }
    }
}
