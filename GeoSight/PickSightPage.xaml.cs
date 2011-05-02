using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace GeoSight
{
    public partial class PickSightPage : PhoneApplicationPage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PickSightPage()
        {
            // Initialize the page
            InitializeComponent();
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            // Check if a sight has been selected.
            if (App.SelectedSight != null)
            {
                // Navigate back to the main page.
                this.NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Called when the "From a list" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ShowPickSightFromListPage(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightListPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Called when the "From a map" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ShowPickSightFromMapPage(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightMapPage.xaml", UriKind.Relative));
        }
    }
}
