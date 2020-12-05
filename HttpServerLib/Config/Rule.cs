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
                bool hostMatch = (!this.RequestMatch.CheckHostName || (this.RequestMatch.CheckHostName && (this.RequestMatch.IsHostMatch(host))));
                bool pathMatch = (!this.RequestMatch.CheckPath || (this.RequestMatch.CheckPath && (this.RequestMatch.IsPathMatch(path))));
                return (portMatch && hostMatch && pathMatch);                    
            } 
            return false;
        }
    }

    public class HttpToHttpsRedirectRule : Rule
    {
        public HttpToHttpsRedirectRule(string hostName)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckHostName = true,
                HostNameMatch = hostName,
                CheckPort = true,
                Port = 80,
            };

            this.ResponseAction = new RedirectAction()
            {
                SetScheme = true,
                Scheme = Scheme.https,
                SetPort = true,
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
                CheckHostName = true,
                HostNameMatch = hostNameMatch,
            };
            this.ResponseAction = new RedirectAction()
            {
                SetHostName = true,
                HostName = hostNameRedirect,
            };
        }
    }

    public class SimplePathRedirectRule : Rule
    {
        public SimplePathRedirectRule(string pathMatch, string pathRedirect)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckPath = true,
                PathMatch = pathMatch,
                PathMatchType = MatchType.Wildcard
            };
            this.ResponseAction = new RedirectAction()
            {
                SetPath = true,
                Path = pathRedirect,
            };
        }
    }

    public class SimplePathDenyRule : Rule
    {
        public SimplePathDenyRule(string pathMatch)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckPath = true,
                PathMatch = pathMatch,
                PathMatchType = MatchType.Wildcard
            };
            this.ResponseAction = new DenyAction();
        }
    }

    public class HostAlternativeWWWDirectoryRule : Rule
    {
        public HostAlternativeWWWDirectoryRule(string hostMatch, string alternativeWWWDirectory)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckHostName = true,
                HostNameMatch =hostMatch,
            };
            this.ResponseAction = new AlternativeWWWDirectoryAction()
            {
                AlternativeWWWDirectory = alternativeWWWDirectory
            };
        }
    }

    public class SimpleReverseProxyRule : Rule
    {
        public SimpleReverseProxyRule(string host, string pathWildCard, int reverseProxyPort = 80)
        {
            this.Enable = true;
            this.RequestMatch = new RequestMatch()
            {
                CheckHostName = true,
                HostNameMatch = host,
                CheckPath = true,
                PathMatch = pathWildCard
            };
            this.ResponseAction = new ReverseProxyAction()
            {
                SetScheme = true,
                Scheme = Scheme.http,
                SetHostName = true,
                HostName = "localhost",
                SetPort =  true,
                Port = reverseProxyPort
            };
        }
    }
}