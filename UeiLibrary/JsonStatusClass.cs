using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiLibrary
{
    public class JsonStatusClass
    {
        string _description;

        public JsonStatusClass(string desc)
        {
            _description = desc;
        }

        public string Description { get => _description; set => _description = value; }
    }
}
