using System.ComponentModel;

namespace LudusaviPlaynite
{
    public class LibraryItem : INotifyPropertyChanged
    {
        private bool _isDisabled;

        public string Name { get; set; }

        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (_isDisabled != value)
                {
                    _isDisabled = value;
                    OnPropertyChanged(nameof(IsDisabled));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
