using AppEspiaoJogo.Game;

namespace AppEspiaoJogo.Models
{
    internal class PlacesViewModel
    {
        public List<Place> Locations { get; set; }

        public PlacesViewModel()
        {
            //Locations = Game.Locations.GetAll().Select(l => new Place { Name = l }).ToList();

            Locations = Game.Locations.GetPlaces();

            var locationsText = Game.Locations.GetAll().Select(l => new Place { Name = l }).ToList();

            locationsText.RemoveAll(l => Locations.FirstOrDefault(l2 => l2.Name == l.Name) != null);

            Locations.AddRange(locationsText);
        }

    }
}
