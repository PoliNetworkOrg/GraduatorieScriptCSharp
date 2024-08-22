namespace PoliNetwork.Graduatorie.Scraper.Utils;

using System;
using System.Collections.Specialized;
using System.Web; // For this you need to reference System.Web assembly from the GAC

public static class UriExtensions
{
    public static Uri SetQueryVal(this Uri uri, string name, object value)
    {
        var nvc = HttpUtility.ParseQueryString(uri.Query);
        nvc[name] = value.ToString();
        return new UriBuilder(uri) {Query = nvc.ToString()}.Uri;
    }
    
    public static Uri RemoveQueryVal(this Uri uri, string name)
    {
        var nvc = HttpUtility.ParseQueryString(uri.Query);
        nvc.Remove(name);
        return new UriBuilder(uri) { Query = nvc.ToString() }.Uri;
    }
}