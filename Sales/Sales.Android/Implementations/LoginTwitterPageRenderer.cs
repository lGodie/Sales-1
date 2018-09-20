[assembly: Xamarin.Forms.ExportRenderer(
    typeof(Sales.Views.LoginTwitterPage),
    typeof(Sales.Droid.Implementations.LoginTwitterPageRenderer))]

namespace Sales.Droid.Implementations
{
    using System;
    using System.Threading.Tasks;
    using Android.App;
    using Common.Models;
    using Newtonsoft.Json;
    using Xamarin.Auth;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;

    public class LoginTwitterPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var activity = this.Context as Activity;

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
                authorizeUrl: new Uri(twitterAuthorizeURL), callbackUrl: new Uri(url),
                accessTokenUrl: new Uri(twitterAccessTokenURL));

            auth.Completed += async (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    TwitterResponse profile = await GetTwitterProfileAsync(eventArgs.Account);
                    App.NavigateToProfile(null);
                }
                else
                {
                    App.HideLoginView();
                }
            };

            activity.StartActivity(auth.GetUI(activity));
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