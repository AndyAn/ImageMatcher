using System.Net;
using System.IO;
using System;

namespace ImageMatcher
{
    public class NetworkManager
    {
        static NetworkManager manager = null;
        static object locker = new object();
        static HttpWebRequest req = null;
        static WebResponse res = null;
        static WebClient wc = null;

        private NetworkManager()
        {
            wc = new WebClient();
            //wc.Proxy = new WebProxy("172.26.0.62");
        }

        public static NetworkManager Host
        {
            get
            {
                lock (locker)
                {
                    if (manager == null)
                    {
                        manager = new NetworkManager();
                    }

                    return manager;
                }
            }
        }

        public void SetProxy(string ip)
        {
            wc.Proxy = new WebProxy(ip);
        }

        public byte[] GetData(string url)
        {
            return wc.DownloadData(new Uri(url));
        }
    }
}