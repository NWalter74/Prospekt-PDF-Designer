using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Prospekt_PDF_Designer.Helpers;
using Prospekt_PDF_Designer.Services;

using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Prospekt_PDF_Designer.Views
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings-codebehind.md
    //License for Windows template Studio https://marketplace.visualstudio.com/items/WASTeamAccount.WindowsTemplateStudio/license
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        public SettingsPage()
        {
            InitializeComponent();

            //take out data
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Windows.Storage.ApplicationDataCompositeValue composite = localSettings.Values["Connectionstring"] as Windows.Storage.ApplicationDataCompositeValue;
            if (null != composite)
            {
                Server.Text = composite["Server"].ToString();
                Port.Text = composite["Port"].ToString();
                Username.Text = composite["Username"].ToString();
                Password.Password = composite["Password"].ToString();
            }
            Windows.Storage.ApplicationDataCompositeValue compositeTimeSetter = localSettings.Values["Timings"] as Windows.Storage.ApplicationDataCompositeValue;
            if (null != compositeTimeSetter)
            {
                int timeSetter;
                int.TryParse(compositeTimeSetter["TimeSetter"].ToString(), out timeSetter);

                switch (timeSetter)
                {
                    default:
                        default1min.IsChecked = true;
                        break;
                    case 5:
                        time5min.IsChecked = true;
                        break;

                    case 10:
                        time10min.IsChecked = true;
                        break;
                    case 30:            //45
                        maxTime.IsChecked = true;
                        break;
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            int timeSetter = 1;

            if (default1min.IsChecked == true)
            {
                timeSetter = 1;
            }

            if(time5min.IsChecked == true)
            {
                timeSetter = 5;
            }

            if (time10min.IsChecked == true)
            {
                timeSetter = 10;
            }

            if (maxTime.IsChecked == true)
            {
                timeSetter = 30;        //45
            }
            //get data from settings page
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();

            composite["Server"] = Server.Text.Trim();
            composite["Port"] = Port.Text.Trim();
            composite["Username"] = Username.Text.Trim();
            composite["Password"] = Password.Password.Trim();

            //save
            localSettings.Values["Connectionstring"] = composite;

            Windows.Storage.ApplicationDataCompositeValue compositeTimeSetter = new Windows.Storage.ApplicationDataCompositeValue();
            compositeTimeSetter["TimeSetter"] = timeSetter;

            localSettings.Values["Timings"] = compositeTimeSetter;


            this.Frame.Navigate(typeof(WelcomePage));
        }

        private void CancelSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WelcomePage));
        }
    }
}
