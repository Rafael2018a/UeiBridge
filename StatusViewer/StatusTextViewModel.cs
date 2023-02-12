namespace StatusViewer
{
    public class StatusEntryViewModel : StatusBaseViewModel
    {
        string _statusText;
        long _updateCounter = 0;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChangedEvent("StatusText");
            }
        }
        public long UpdateCounter 
        { 
            get => _updateCounter; 
            set  
                { 
                _updateCounter = value;
                RaisePropertyChangedEvent("UpdateCounter");
            }
        }

        public StatusEntryViewModel(StatusEntryModel messageModel) : base(messageModel)
        {
            StatusText = messageModel.StringValue[0];
            BorderBrushColor = (messageModel.Trait == UeiBridge.Library.StatusTrait.IsRegular)? System.Windows.Media.Colors.RoyalBlue : System.Windows.Media.Colors.Red;
        }
        public void Update(StatusEntryModel messageModel)
        {
            StatusText = messageModel.StringValue[0];
            ++UpdateCounter;
        }
    }
}
