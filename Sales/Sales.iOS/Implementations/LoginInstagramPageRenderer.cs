[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Sales.Views.LoginInstagramPage),
    typeof(Sales.iOS.Implementations.LoginInstagramPageRenderer))]

namespace Sales.iOS.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Common.Models;
    using Services;
    using Xamarin.Auth;
    using Xamarin.Forms.Platform.iOS;

    public class LoginInstagramPageRenderer : PageRenderer
    {
        bool done = false;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (done)
            {
                return;
            }

            var instagramAppID = Xamarin.Forms.Application.Current.Resources["InstagramAppID"].ToString();
            var instagramAuthURL = Xamarin.Forms.Application.Current.Resources["InstagramAuthURL"].ToString();
            var instagramRedirectURL = Xamarin.Forms.Application.Current.Resources["InstagramRedirectURL"].ToString();
            var instagramScope = Xamarin.Forms.Application.Current.Resources["InstagramScope"].ToString();

            var auth = new OAuth2Authenticator(
                clientId: instagramAppID,
                scope: instagramScope,
                authorizeUrl: new Uri(instagramAuthURL),
                redirectUrl: new Uri(instagramRedirectURL));

            auth.Completed += async (sender, eventArgs) =>
            {
                DismissViewController(true, null);
                App.HideLoginView();

                if (eventArgs.IsAuthenticated)
                {
                    var accessToken = eventArgs.Account.Properties["access_token"].ToString();
                    var profile = await GetInstagramProfileAsync(accessToken);
                    App.NavigateToProfile(profile, "Instagram");
                }
                else
                {
                    App.HideLoginView();
                }
            };
            done = true;
            PresentViewController(auth.GetUI(), true, null);
        }

        public async Task<InstagramResponse> GetInstagramProfileAsync(string accessToken)
        {
            var InstagramProfileInfoURL = Xamarin.Forms.Application.Current.Resources["InstagramProfileInfoURL"].ToString();
            var requestUrl = string.Format("{0}={1}",
                InstagramProfileInfoURL,
                accessToken);

            var apiService = new ApiService();
            return await apiService.GetInstagram(requestUrl);
        }
    }
}