using System.Threading.Tasks;

namespace training_diary_backend.Services.Polar
{
    public class AccessLink
    {
        private readonly Oauth _oauth;
        public AccessLink(Oauth oauth)
        {
            _oauth = oauth;

        }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string redirect_url { get; set; }

        public async Task<string> authorization_url()
        {
            return await _oauth.get_authorization_url();
        }
        
        public async Task<string> get_access_token(string code)
        {
            return await _oauth.get_access_token(code.ToString());
        }
    }
}