using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace StatusViewer
{
    public class StatusEntryViewModel : ViewModelBase
    {
        string[] _statusText;
        long _updateCounter = 0;
        string _desc;
        SolidColorBrush _entryBorderBrush;
        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                RaisePropertyChanged();
            }
        }
        public string[] StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChanged();
            }
        }
        public long UpdateCounter
        {
            get => _updateCounter;
            set
            {
                _updateCounter = value;
                RaisePropertyChanged();
            }
        }
        //Color _borderBrushColor;
        //public Color BorderBrushColor
        //{
        //    get => _borderBrushColor;
        //    set
        //    {
        //        _borderBrushColor = value;
        //        RaisePropertyChangedEvent("BorderBrushColor");
        //    }
        //}
        
        public SolidColorBrush EntryBorderBrush
        {
            get
            {
                return _entryBorderBrush;
            }
            set
            {
                _entryBorderBrush = value;
                RaisePropertyChanged();
            }
        }
        public StatusEntryViewModel(StatusEntryModel messageModel)
        {
            StatusText = messageModel.StringValue;
            Desc = messageModel.Desc;
            Application.Current.Dispatcher.Invoke(() =>
            {
                EntryBorderBrush = (messageModel.Trait == UeiBridge.Library.StatusTrait.IsRegular) ? new SolidColorBrush(System.Windows.Media.Colors.RoyalBlue) : new SolidColorBrush(System.Windows.Media.Colors.Red);
            });
        }
        public void Update(StatusEntryModel messageModel)
        {
            StatusText = messageModel.StringValue;
            ++UpdateCounter;
        }
    }
}
