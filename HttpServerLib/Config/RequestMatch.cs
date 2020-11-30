using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Vlix.HttpServer
{
    public class RequestMatch
    {        
        public bool AnyHostName { get; set; } = true;
        [JsonConverter(typeof(StringEnumConverter))]
        public MatchType HostNameMatchType { get; set; } = MatchType.Wildcard;
        public string HostNameMatch { get; set; } = null;
        public bool AnyPort { get; set; } = true;
        public int? Port { get; set; } = null;
        public bool AnyPath { get; set; } = true;
        [JsonConverter(typeof(StringEnumConverter))]
        public MatchType PathMatchType { get; set; } = MatchType.Wildcard;
        public string PathMatch { get; set; } = null;

        Regex hostRegex = null;
        public Regex GetHostRegex()
        {
            if (hostRegex == null) { if (HostNameMatchType == MatchType.Wildcard) hostRegex = new Wildcard(HostNameMatch); else hostRegex = new Regex(HostNameMatch); }
                return hostRegex;
        }
        Regex pathRegex = null;
        public Regex GetPathRegex()
        {
            if (pathRegex == null) { if (PathMatchType == MatchType.Wildcard) pathRegex = new Wildcard(PathMatch); else pathRegex = new Regex(PathMatch); }
            return pathRegex;
        }
    }

    public enum MatchType { Wildcard, Regex }
}