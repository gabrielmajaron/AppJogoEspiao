using AppEspiaoJogo.Game;
using AppEspiaoJogo.Models;

namespace AppEspiaoJogo;

public partial class LocationsPage : ContentPage
{
	public LocationsPage()
	{
		InitializeComponent();
        BindingContext = new PlacesViewModel();
    }

    private async void OnPageDoubleTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}