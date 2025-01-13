using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AppEspiaoJogo.Models
{
    internal class PlacesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _locations;
        public ObservableCollection<string> Locations
        {
            get => _locations;
            set
            {
                if (_locations != value)
                {
                    _locations = value;
                    OnPropertyChanged(nameof(Locations));
                }
            }
        }

        public PlacesViewModel()
        {
            Locations = new ObservableCollection<string>(Game.Locations.GetAll());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
