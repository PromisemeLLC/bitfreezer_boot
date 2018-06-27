using System;
using System.Net;
using System.Xml;

namespace TailsBoot
{
    public class BtfUpdateXml
    {
        internal Version Version { get; }

        internal Uri Uri { get; }

        internal string FileName { get; }

        internal string Md5 { get; }

        internal string LaunchArgs { get; }

        private BtfUpdateXml(Version version, Uri uri, string fileName, string md5, string launchArgs)
        {
            Version = version;
            Uri = uri;
            FileName = fileName;
            Md5 = md5;
            LaunchArgs = launchArgs;
        }

        internal bool IsNewerThen(Version version)
        {
            return Version > version;
        }

        internal static bool ExistsOnServer(Uri location)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(location.AbsoluteUri);
                var resp = (HttpWebResponse)req.GetResponse();
                resp.Close();

                return resp.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                // ignored
                return false;
            }
        }

        internal static BtfUpdateXml Parse(Uri location, string appId)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(location.AbsoluteUri);

                var node = doc.DocumentElement?.SelectSingleNode("//update[@appId='" + appId + "']");

                if (node == null)
                    return null;
                // ReSharper disable once AssignNullToNotNullAttribute
                var version = Version.Parse(node["version"]?.InnerText);

                var url = node["url"]?.InnerText;
                var fileName = node["fileName"]?.InnerText;
                var md5 = node["md5"]?.InnerText;
                var launchArgs = node["launchArgs"]?.InnerText;

                return url != null ? new BtfUpdateXml(version, new Uri(url), fileName, md5, launchArgs) : null;
            }
            catch { return null; }
        }
    }
}
