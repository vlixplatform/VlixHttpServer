using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Vlix.HttpServer
{
    public class Rule
    {
        public bool Enable { get; set; } = true;
        public RequestMatch RequestMatch { get; set; } = new RequestMatch();
        public IReponseAction ResponseAction { get; set; } = new RedirectAction();

        public string Process(Scheme scheme, string host, int port, string path)
        {
            if (this.Enable)
            {
                bool portMatch = (this.RequestMatch.AnyPort || (!this.RequestMatch.AnyPort && (this.RequestMatch.Port == port)));
                bool hostMatch = (this.RequestMatch.AnyHostName || (!this.RequestMatch.AnyHostName && (this.RequestMatch.GetHostRegex().IsMatch(host))));
                bool uRLMatch = (this.RequestMatch.AnyPath || (!this.RequestMatch.AnyHostName && (this.RequestMatch.GetPathRegex().IsMatch(path))));
                if (this.ResponseAction is RedirectAction redirectAction)
                {
                    if (portMatch && hostMatch && uRLMatch)
                    {
                        string redirectScheme = (redirectAction.Scheme ?? scheme).ToString();
                        string redirectHost = redirectAction.HostName ?? host;
                        int redirectPort = redirectAction.Port ?? port;
                        string redirectPath = redirectAction.Path ?? path;
                        string redirectURL = redirectScheme + "://" + redirectHost + ":" + redirectPort + redirectPath;
                        return redirectURL;
                    }
                }
            }
            return null;
        }
        public bool IsMatch(Scheme scheme, string host, int port, string path)
        {            
            if (this.Enable)
            {
                bool portMatch = (this.RequestMatch.AnyPort || (!this.RequestMatch.AnyPort && (this.RequestMatch.Port == port)));
                bool hostMatch = (this.RequestMatch.AnyHostName || (!this.RequestMatch.AnyHostName && (this.RequestMatch.GetHostRegex().IsMatch(host))));
                bool uRLMatch = (this.RequestMatch.AnyPath || (!this.RequestMatch.AnyPath && (this.RequestMatch.GetPathRegex().IsMatch(path))));
                return (portMatch && hostMatch && uRLMatch);                    
            } 
            return false;
        }
    }

    public class HttpToHttpsRedirectRule : Rule
    {
        public HttpToHttpsRedirectRule()
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                AnyHostName = true,
                HostNameMatch = null,
                AnyPort = false,
                Port = 80,
                PathMatch = "*"
            };

            this.ResponseAction = new RedirectAction()
            {
                Scheme = Scheme.https,
                HostName = null,
                Port = 443,
                Path = null
            };
        }
    }

    
}