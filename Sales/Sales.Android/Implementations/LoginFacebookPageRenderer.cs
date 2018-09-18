[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Sales.Views.LoginFacebookPage),
    typeof(Sales.Droid.Implementations.LoginFacebookPageRenderer))]

namespace Sales.Droid.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Android.App;
    using Common.Models;
    using Services;
    using Xamarin.Auth;
    using Xamarin.Forms.Platform.Android;

    public class LoginFacebookPageRenderer : PageRenderer
    {
        public LoginFacebookPageRenderer()
        {
            var activity = this.Context as Activity;

            var facebookAppID = Xamarin.Forms.Application.Current.Resources["FacebookAppID"].ToString();
            var facebookAuthURL = Xamarin.Forms.Application.Current.Resources["FacebookAuthURL"].ToString();
            var facebookRedirectURL = Xamarin.Forms.Application.Current.Resources["FacebookRedirectURL"].ToString();
            var facebookScope = Xamarin.Forms.Application.Current.Resources["FacebookScope"].ToString();

            var auth = new OAuth2Authenticator(
                clientId: facebookAppID,
                scope: facebookScope,
                authorizeUrl: new Uri(facebookAuthURL),
                redirectUrl: new Uri(facebookRedirectURL));

            auth.Completed += async (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    var accessToken = eventArgs.Account.Properties["access_token"].ToString();
                    var profile = await GetFacebookProfileAsync(accessToken);
                    await App.NavigateToProfile(profile, "Facebook");
                }
                else
                {
                    App.HideLoginView();
                }
            };

            activity.StartActivity(auth.GetUI(activity));
        }

        private async Task<FacebookResponse> GetFacebookProfileAsync(string accessToken)
        {
            var apiService = new ApiService();
            return await apiService.GetFacebook(accessToken);
        }
    }
}