using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRZLibrary.Classes
{
    public class LookupEntry
    {
        private string _qRZ = string.Empty;
        private string _country = string.Empty;
        private string _dXCC = string.Empty;
        private string _name = string.Empty;
        private string _address1 = string.Empty;
        private string _address2 = string.Empty;
        private string _address3 = string.Empty;
        private string _email = string.Empty;

        public string QRZ { get => _qRZ; set => _qRZ = value; }
        public string Country { get => _country; set => _country = value; }
        public string DXCC { get => _dXCC; set => _dXCC = value; }
        public string Name { get => _name; set => _name = value; }
        public string Address1 { get => _address1; set => _address1 = value; }
        public string Address2 { get => _address2; set => _address2 = value; }
        public string Address3 { get => _address3; set => _address3 = value; }
        public string Email { get => _email; set => _email = value; }
    }
}
