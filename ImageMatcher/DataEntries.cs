using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageMatcher
{
    [Serializable]
    class ImageSet
    {
        public string Key = "";
        public List<string> URLs = new List<string>();
    }

    [Serializable]
    class ImageFPrint
    {
        public string URL = "";
        public List<byte> FingerPrint = new List<byte>();
    }
}
