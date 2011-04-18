using Microsoft.Phone.Controls;
using System;
using System.Windows.Navigation;

namespace GeoSight
{
    public partial class PickSightPage : PhoneApplicationPage
    {
        public PickSightPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check if a sight has been selected.
            if (App.SelectedSight != null)
            {
                // Navigate back to the main page.
                this.NavigationService.GoBack();
            }
        }

        private void btn_FromList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightListPage.xaml", UriKind.Relative));
        }

        private void btn_FromMap_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightMapPage.xaml", UriKind.Relative));
        }
    }
}
