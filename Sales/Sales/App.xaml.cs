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
            var apiSecurity = Application.Current.Resources["APISecurity"].ToString();
            TokenResponse token = null;

            switch (socialNetwork)
            {
                case "Instagram":
                    var responseInstagram = profile as InstagramResponse;
                    token = await apiService.LoginInstagram(
                        apiSecurity,
                        "/api",
                        "/Users/LoginInstagram",
                        responseInstagram);
                    break;

                case "Facebook":
                    var responseFacebook = profile as FacebookResponse;
                    token = await apiService.LoginFacebook(
                        apiSecurity,
                        "/api",
                        "/Users/LoginFacebook",
                        responseFacebook);
                    break;

                case "Twitter":
                    var responseTwitter = profile as TwitterResponse;
                    token = await apiService.LoginTwitter(
                        apiSecurity,
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

            var user = await apiService.GetUserByEmail(
                apiSecurity,
                "/api",
                "/Users/GetUserByEmail",
                token.TokenType,
                token.AccessToken,
                token.UserName);

            UserLocal userLocal = null;
            if (user != null)
            {
                userLocal = Converter.ToUserLocal(user);
                dataService.DeleteAllAndInsert(userLocal);
                dataService.DeleteAllAndInsert(token);
            }

            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.Token = token;
            mainViewModel.User = userLocal;
            mainViewModel.RegisterDevice();
            mainViewModel.Matches = new MatchesViewModel();
            Application.Current.MainPage = new MasterPage();
            Settings.IsRemembered = "true";
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