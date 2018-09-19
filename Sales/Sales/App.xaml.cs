using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Sales
{
    using Helpers;
    using Views;
    using ViewModels;
    using Newtonsoft.Json;
    using Sales.Common.Models;
    using System;
    using System.Threading.Tasks;
    using Sales.Services;

    public partial class App : Application
    {
        #region Properties
        public static NavigationPage Navigator { get; internal set; }
        #endregion

        #region Constructors
        public App()
        {
            InitializeComponent();

            var mainViewModel = MainViewModel.GetInstance();

            if (Settings.IsRemembered)
            {

                if (!string.IsNullOrEmpty(Settings.UserASP))
                {
                    mainViewModel.UserASP = JsonConvert.DeserializeObject<UserASP>(Settings.UserASP);
                }

                mainViewModel.Products = new ProductsViewModel();
                this.MainPage = new MasterPage();
            }
            else
            {
                mainViewModel.Login = new LoginViewModel();
                this.MainPage = new NavigationPage(new LoginPage());
            }
        }
        #endregion

        #region Methods
        public static Action HideLoginView
        {
            get
            {
                return new Action(() => Current.MainPage = new NavigationPage(new LoginPage()));
            }
        }

        public static async Task NavigateToProfile<T>(T profile, string socialNetwork)
        {
            if (profile == null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                return;
            }

            var apiService = new ApiService();
            var dataService = new DataService();
            var url = Current.Resources["UrlUsersController"].ToString();
            TokenResponse token = null;

            switch (socialNetwork)
            {
                case "Instagram":
                    var responseInstagram = profile as InstagramResponse;
                    token = await apiService.LoginInstagram(
                        url,
                        "/api",
                        "/Users/LoginInstagram",
                        responseInstagram);
                    break;

                case "Facebook":
                    var responseFacebook = profile as FacebookResponse;
                    token = await apiService.LoginFacebook(
                        url,
                        "/api",
                        "/Users/LoginFacebook",
                        responseFacebook);
                    break;

                case "Twitter":
                    var responseTwitter = profile as TwitterResponse;
                    token = await apiService.LoginTwitter(
                        url,
                        "/api",
                        "/Users/LoginTwitter",
                        responseTwitter);
                    break;
            }

            if (token == null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                return;
            }

            Settings.IsRemembered = true;
            Settings.AccessToken = token.AccessToken;
            Settings.TokenType = token.TokenType;

            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlUsersController"].ToString();
            var response = await apiService.GetUser(url, prefix, $"{controller}/GetUser", token.UserName, token.TokenType, token.AccessToken);
            if (response.IsSuccess)
            {
                var userASP = (UserASP)response.Result;
                MainViewModel.GetInstance().UserASP = userASP;
                Settings.UserASP = JsonConvert.SerializeObject(userASP);
            }

            MainViewModel.GetInstance().Products = new ProductsViewModel();
            Application.Current.MainPage = new MasterPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}