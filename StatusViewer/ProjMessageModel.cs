using System;
using System.Diagnostics;
using UeiBridge.Library;

namespace StatusViewer
{
    public class StatusEntryModel
    {
        string [] _stringValue;
        string _desc;
        StatusTrait _trait;

        public string [] StringValue { get => _stringValue; }
        public string Desc { get => _desc; }
        public StatusTrait Trait { get => _trait; }
        public StatusEntryModel(StatusEntryJson js)
        {
            _desc = js.FieldTitle;
            _stringValue = js.FormattedStatus;
            _trait = js.Trait;
        }
    }
}
