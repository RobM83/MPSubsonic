using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPSubsonic
{
    class Artist
    {
        private string _name;
        private string _id;

        public string Name {
            set {
                _name = value;
            }
            get {
                return _name;
            }
        }

        public string Id {
            set {
                _id = value;
            }
            get {
                return _id;
            }
        }
    }
}
