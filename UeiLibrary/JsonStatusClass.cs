using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge.Library
{
    public class JsonStatusClass
    {
        string _fieldTitle;
        string _formattedStatus;

        public JsonStatusClass(string desc, string formattedStatus)
        {
            _fieldTitle = desc;
            this._formattedStatus = formattedStatus;
        }

        public string FieldTitle { get => _fieldTitle; set => _fieldTitle = value; }
        public string FormattedStatus { get => _formattedStatus; set => _formattedStatus = value; }
    }
}
