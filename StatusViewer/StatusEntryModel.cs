using UeiBridge.Library;

namespace StatusViewer
{
    /// <summary>
    /// Adapter of StatusEntryJson
    /// </summary>
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
