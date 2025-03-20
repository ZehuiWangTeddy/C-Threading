using Microsoft.Maui.Controls;

namespace TaskManager
{
    public partial class Dashboard : ContentPage
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private async void OnViewGraphsClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new Graph());
        }
    }
}