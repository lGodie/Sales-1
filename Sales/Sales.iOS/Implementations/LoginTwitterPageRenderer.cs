[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Sales.Views.LoginTwitterPage),
    typeof(Sales.iOS.Implementations.LoginTwitterPageRenderer))]

namespace Sales.iOS.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Common.Models;
    using Newtonsoft.Json;
    using Xamarin.Auth;
    using Xamarin.Forms.Platform.iOS;

    public class LoginTwitterPageRenderer : PageRenderer
    {
        bool done = false;
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (done)
            {
                return;
            }

            var twitterConsumerKey = Xamarin.Forms.Application.Current.Resources["TwitterConsumerKey"].ToString();
            var twitterConsumerSecret = Xamarin.Forms.Application.Current.Resources["TwitterConsumerSecret"].ToString();
            var twitterRequestTokenURL = Xamarin.Forms.Application.Current.Resources["TwitterRequestTokenURL"].ToString();
            var twitterAuthorizeURL = Xamarin.Forms.Application.Current.Resources["TwitterAuthorizeURL"].ToString();
            var url = Xamarin.Forms.Application.Current.Resources["Url"].ToString();
            var twitterAccessTokenURL = Xamarin.Forms.Application.Current.Resources["TwitterAccessTokenURL"].ToString();

            var auth = new OAuth1Authenticator(
                consumerKey: twitterConsumerKey,
                consumerSecret: twitterConsumerSecret,
                requestTokenUrl: new Uri(twitterRequestTokenURL),
                authorizeUrl: new Uri(twitterAuthorizeURL),
                callbackUrl: new Uri(url),
                accessTokenUrl: new Uri(twitterAccessTokenURL));

            auth.Completed += async (sender, eventArgs) =>
            {
                DismissViewController(true, null); App.HideLoginView();
                if (eventArgs.IsAuthenticated)
                {
                    var profile = await GetTwitterProfileAsync(eventArgs.Account);
                    App.NavigateToProfile(profile, "Twitter");
                }
                else
                {
                    App.HideLoginView();
                }
            };

            done = true;
            PresentViewController(auth.GetUI(), true, null);
        }

        public async Task<TwitterResponse> GetTwitterProfileAsync(Account account)
        {
            var TwitterProfileInfoURL = Xamarin.Forms.Application.Current.Resources["TwitterProfileInfoURL"].ToString(); var requestUrl = new OAuth1Request(
                "GET",
                new Uri(TwitterProfileInfoURL), null,
                account);

            var response = await requestUrl.GetResponseAsync();
            return JsonConvert.DeserializeObject<TwitterResponse>(response.GetResponseText());
        }
    }
}