using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Vlix.HttpServer
{
    public class RequestMatch
    {        
        public bool CheckHostName { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public MatchType HostNameMatchType { get; set; } = MatchType.Exact;
        public string HostNameMatch { get; set; } = null;
        public bool CheckPort { get; set; } = false;
        public int? Port { get; set; } = null;
        public bool CheckPath { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public MatchType PathMatchType { get; set; } = MatchType.Wildcard;
        public string PathMatch { get; set; } = null;

        Regex hostRegex = null;
        public bool IsHostMatch(string host)
        {
            if (this.HostNameMatchType == MatchType.Exact) return this.HostNameMatch == host;
            else
            {
                if (hostRegex == null) { if (HostNameMatchType == MatchType.Wildcard) hostRegex = new Wildcard(HostNameMatch, RegexOptions.IgnoreCase); else hostRegex = new Regex(HostNameMatch, RegexOptions.IgnoreCase); }
                return hostRegex.IsMatch(host);
            }            
        }
        Regex pathRegex = null;
        public bool IsPathMatch(string path)
        {
            if (this.PathMatchType == MatchType.Exact) return this.PathMatch == path;
            else
            {
                if (pathRegex == null) { if (PathMatchType == MatchType.Wildcard) pathRegex = new Wildcard(PathMatch); else pathRegex = new Regex(PathMatch); }
                return pathRegex.IsMatch(path);
            }
        }
    }

    public enum MatchType { Exact, Wildcard, Regex }
}