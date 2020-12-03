using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Vlix.HttpServer
{
    public class Rule
    {
        public bool Enable { get; set; } = true;
        public string Name { get; set; } = "New Rule";
        public RequestMatch RequestMatch { get; set; } = new RequestMatch();
        public IReponseAction ResponseAction { get; set; } = new RedirectAction();

      
        public bool IsMatch(Scheme scheme, string host, int port, string path)
        {            
            if (this.Enable)
            {
                bool portMatch = (!this.RequestMatch.CheckPort || (this.RequestMatch.CheckPort && (this.RequestMatch.Port == port)));
                bool hostMatch = (!this.RequestMatch.CheckHostName || (this.RequestMatch.CheckHostName && (this.RequestMatch.GetHostRegex().IsMatch(host))));
                bool uRLMatch = (!this.RequestMatch.CheckPath || (this.RequestMatch.CheckPath && (this.RequestMatch.GetPathRegex().IsMatch(path))));
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
                CheckHostName = true,
                HostNameMatch = null,
                CheckPort = false,
                Port = 80,
                PathMatch = "*"
            };

            this.ResponseAction = new RedirectAction()
            {
                RedirectScheme = true,
                Scheme = Scheme.https,
                RedirectPort = true,
                Port = 443                
            };
        }
    }

    public class SimpleHostNameRedirectRule : Rule
    {
        public SimpleHostNameRedirectRule(string hostNameMatch, string hostNameRedirect)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckHostName = false,
                HostNameMatch = hostNameMatch,
                HostNameMatchType = MatchType.Wildcard,
                CheckPort = true,
                CheckPath = true
            };
            this.ResponseAction = new RedirectAction()
            {
                RedirectHostName = true,
                HostName = hostNameRedirect,
            };
        }
    }
}