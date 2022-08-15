namespace StatusViewer
{
    public class StatusTextViewModel : StatusBaseViewModel
    {
        string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChangedEvent("StatusText");
            }
            }
        public StatusTextViewModel(ProjMessageModel messageModel) : base(messageModel)
        {
            StatusText = messageModel.StringValue;
        }
        public void Update(ProjMessageModel messageModel)
        {
            StatusText = messageModel.StringValue;
            LastUpdate = System.DateTime.Now.ToLongTimeString();
        }
    }
}
