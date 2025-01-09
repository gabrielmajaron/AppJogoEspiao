namespace AppEspiaoJogo.Models
{
    internal class PlacesViewModel
    {
        public List<string> Locations { get; set; }

        public PlacesViewModel()
        {
            Locations = Game.Locations.GetAll();
        }
    }
}
