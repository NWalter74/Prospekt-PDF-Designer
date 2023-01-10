using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Prospekt_PDF_Designer.Views
{
    public sealed partial class WelcomePage : Page//, INotifyPropertyChanged
    {
        public WelcomePage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (txboxusername.Text == "" || txboxpassword.Password == "")
                {
                    //throw new Exception("Please enter a valid username and password!");
                    throw new Exception("Snälla mata in giltig användarnamn och lösenord!");
                }

                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["Connectionstring"];
                if(null==composite)
                {
                    //throw new Exception("Please set database connection settings before login");
                    throw new Exception("Snälla mata in databas inställningar innan du loggar in");
                }
                Database.SetConnetion(composite["Server"].ToString(), composite["Port"].ToString(), composite["Username"].ToString(), composite["Password"].ToString());

                string mysql = "SELECT * FROM kundregister.användarinfo WHERE ANV_NAMN = '" + txboxusername.Text + "' AND ANV_LOSEN = '" + txboxpassword.Password + "'";
                bool result = await Database.Execute(mysql);

                if (Database.dtable.Rows.Count == 0)
                {
                    //throw (new Exception("Combination av username and password does not exist!"));
                    throw (new Exception("Dennna kombination av användarnamn och lösenord finns inte!"));
                }

                this.Frame.Navigate(typeof(MainPage), txboxusername.Text);

            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }

        private void Settings_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
