using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageMatcher
{
    class ImageHelper
    {
        internal static int MatchThreshold = 20; //(80 + 160) / 2;
        internal static int NonMatchThreshold = 40; //(80 + 160) / 2;
        internal static int SampleSize = 16; //32;
        internal static bool IsOutput = false;

        internal static ImageFPrint GetSample(string url)
        {
            Stream imageData = new MemoryStream(NetworkManager.Host.GetData(url));
            Image image = Image.FromStream(imageData);
            Bitmap bitmap = new Bitmap(image, SampleSize, SampleSize);

            Stream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);

            byte[] bytes = GetImageBytes(GetGrayScale(stream));
            int threshold = GetThresHold(bytes);
            bytes = GetImageBytes(GetBlackWhite(stream, threshold));

            return new ImageFPrint() { URL = url, FingerPrint = bytes.Select(v => (byte)(v & 0x1)).ToList() };
        }

        internal static List<ImageFPrint> GetSample(List<string> urls)
        {
            List<ImageFPrint> list = new List<ImageFPrint>();

            foreach (string url in urls)
            {
                list.Add(GetSample(url));
            }

            return list;
        }

        private static byte[] GetImageBytes(Stream imageData)
        {
            Bitmap image = new Bitmap(imageData);
            Color c = Color.Transparent;
            byte[] bytes = new byte[image.Width * image.Height];
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    c = image.GetPixel(x, y);
                    bytes[x * image.Width + y] = (byte)((c.R + c.G + c.B) / 3);
                }
            }

            return bytes;
        }

        private static Stream GetGrayScale(Stream imageData)
        {
            Bitmap image = new Bitmap(imageData);
            Color c = Color.Transparent;
            byte cv = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    c = image.GetPixel(x, y);
                    cv = (byte)((c.R + c.G + c.B) / 3);
                    image.SetPixel(x, y, Color.FromArgb(0, cv, cv, cv));
                }
            }

            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);

            return ms;
        }

        private static Stream GetBlackWhite(Stream imageData, int thresHolder)
        {
            Bitmap image = new Bitmap(imageData);
            Color c = Color.Transparent;
            byte cv = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    c = image.GetPixel(x, y);
                    cv = (byte)((c.R + c.G + c.B) / 3);
                    cv = (byte)(cv >= thresHolder ? 255 : 0);
                    image.SetPixel(x, y, Color.FromArgb(0, cv, cv, cv));
                }
            }

            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);

            return ms;
        }

        private static int GetThresHold(byte[] srcData)
        {
            int[] histData = new int[256];
        	int maxLevelValue;
	        int threshold;

            int ptr;

            // Clear histogram data
            // Set all values to zero
            ptr = 0;
            while (ptr < histData.Length) histData[ptr++] = 0;

            // Calculate histogram and find the level with the max value
            // Note: the max level value isn't required by the Otsu method
            ptr = 0;
            maxLevelValue = 0;
            while (ptr < srcData.Length)
            {
                int h = 0xFF & srcData[ptr];
                histData[h]++;
                if (histData[h] > maxLevelValue) maxLevelValue = histData[h];
                ptr++;
            }

            // Total number of pixels
            int total = srcData.Length;

            float sum = 0;
            for (int t = 0; t < 256; t++) sum += t * histData[t];

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += histData[t];					// Weight Background
                if (wB == 0) continue;

                wF = total - wB;						// Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;				// Mean Background
                float mF = (sum - sumB) / wF;		// Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t;
                }
            }

            return threshold;
        }

        internal static bool Compare(Stream src, Stream dist, float passPct)
        {
            List<byte> result = new List<byte>();

            Bitmap srcImage = new Bitmap(src);
            Bitmap distImage = new Bitmap(dist);

            for (int x = 0; x < srcImage.Width; x++)
            {
                for (int y = 0; y < srcImage.Height; y++)
                {
                    result.Add((byte)(0xFF & (srcImage.GetPixel(x, y).R ^ distImage.GetPixel(x, y).R)));
                }
            }

            float currSamePixelPct = result.Where(p => p > 0).Count() / result.Count * 100;

            return currSamePixelPct >= passPct;
        }

        internal static int Compare(ImageFPrint src, ImageFPrint dist)
        {
            List<byte> result = new List<byte>();

            for (int i = 0; i < SampleSize * SampleSize; i++)
            {
                result.Add((byte)(src.FingerPrint[i] ^ dist.FingerPrint[i]));
            }

            return result.Where(p => p > 0).Count();
        }

        internal static void Compare(List<ImageFPrint> src, List<ImageFPrint> dist, string key)
        {
            List<byte> result = new List<byte>();
            int diff = 0;
            bool isMatch = false;
            ConsoleColor c = Console.ForegroundColor;

            WriteFile("Match,Evaluation,Matched Threshold,Non-Matched Threshold,Group,Link1,Link2", true);

            foreach (ImageFPrint s in src)
            {
                foreach (ImageFPrint d in dist)
                {
                    diff = Compare(s, d);
                    if (diff <= MatchThreshold)
                    {
                        Console.WriteLine(string.Format("{0}\n{1}", s.URL, d.URL));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format("Matched!\t\tDiff Point: {0} (Range: [0, {1}, {2}])\n", diff, MatchThreshold, NonMatchThreshold));
                        WriteFile(string.Format("Yes,{0},{1},{2},{3},{4},{5}", diff, MatchThreshold, NonMatchThreshold, key, s.URL, d.URL));
                        Console.ForegroundColor = c;
                        isMatch = true;
                        break;
                    }
                    else if (diff > MatchThreshold && diff <= NonMatchThreshold)
                    {
                        Console.WriteLine(string.Format("{0}\n{1}", s.URL, d.URL));
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(string.Format("Samilar!\t\tDiff Point: {0} (Range: [0, {1}, {2}])\n", diff, MatchThreshold, NonMatchThreshold));
                        WriteFile(string.Format("Maybe,{0},{1},{2},{3},{4},{5}", diff, MatchThreshold, NonMatchThreshold, key, s.URL, d.URL));
                        Console.ForegroundColor = c;
                        isMatch = true;
                    }
                }

                if (!isMatch)
                {
                    Console.WriteLine(string.Format("{0}", s.URL));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("Not Matched!\t\tDiff Point: {0} (Range: [0, {1}, {2}])\n", diff, MatchThreshold, NonMatchThreshold));
                    WriteFile(string.Format("No,{0},{1},{2},{3},{4},{5}", diff, MatchThreshold, NonMatchThreshold, key, s.URL, ""));
                    Console.ForegroundColor = c;
                    isMatch = false;
                }
            }
        }

        private static void WriteFile(string data, bool isHeader = false)
        {
            if (IsOutput)
            {
                if (isHeader)
                {
                    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "result.csv"), "");
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "result.csv"), true))
                {
                    sw.WriteLine(data);
                }
            }
        }

        private static Stream GetResource(string info)
        {
            var assembly = typeof(ImageHelper).Assembly;
            return assembly.GetManifestResourceStream(assembly.EntryPoint.DeclaringType.Namespace + info);
        }

        internal static void GetSampleFiles()
        {
            try
            {
                using (StreamReader sr = new StreamReader(GetResource(".Resx.major.txt")))
                {
                    using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "major.txt")))
                    {
                        sw.Write(sr.ReadToEnd());
                    }
                }
                using (StreamReader sr = new StreamReader(GetResource(".Resx.compare.txt")))
                {
                    using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "compare.txt")))
                    {
                        sw.Write(sr.ReadToEnd());
                    }
                }
            }
            catch { }
        }
    }
}
