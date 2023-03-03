using System.Windows.Media;

namespace StatusViewer
{
    public class StatusEntryViewModel : StatusBaseViewModel
    {
        string [] _statusText;
        long _updateCounter = 0;
        public string [] StatusText
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

        Color _borderBrushColor;
        public Color BorderBrushColor
        {
            get => _borderBrushColor;
            set
            {
                _borderBrushColor = value;
                RaisePropertyChangedEvent("BorderBrushColor");
            }
        }

        public StatusEntryViewModel(StatusEntryModel messageModel) : base(messageModel)
        {
            StatusText = messageModel.StringValue;
            BorderBrushColor = (messageModel.Trait == UeiBridge.Library.StatusTrait.IsRegular)? System.Windows.Media.Colors.RoyalBlue : System.Windows.Media.Colors.Red;
        }
        public void Update(StatusEntryModel messageModel)
        {
            StatusText = messageModel.StringValue;
            ++UpdateCounter;
        }
    }
}
