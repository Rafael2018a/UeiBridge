using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge.Library
{
    public class StatusEntryJson
    {
        string _fieldTitle;
        string [] _formattedStatus;
        StatusTrait _trait;

        public StatusEntryJson(string title, string [] formattedStatus, StatusTrait trait)
        {
            this._fieldTitle = title;
            this._formattedStatus = formattedStatus;
            this._trait = trait;
        }

        public string FieldTitle { get => _fieldTitle; set => _fieldTitle = value; }
        public string [] FormattedStatus { get => _formattedStatus; set => _formattedStatus = value; }
        public StatusTrait Trait { get => _trait; }
    }
}
