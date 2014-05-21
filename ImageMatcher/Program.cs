using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ImageMatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            InitShell();

            string cmdLet = "";
            Console.Write("$ ");
            cmdLet = Console.ReadLine().ToLower();
            while (cmdLet != "exit")
            {
                Dictionary<string, string> pList = CmdletHelper.GetArguments(cmdLet.Split('='));

                if (pList.Count == 0)
                {
                    CmdletHelper.ShowHelp();
                    return;
                }

                foreach (string key in pList.Keys)
                {
                    switch (key)
                    {
                        case "set output":
                            ImageHelper.IsOutput = pList[key] == "file";
                            break;
                        case "get sample":
                            ImageHelper.GetSampleFiles();
                            Console.WriteLine(string.Format("Sample files generated at {0}.", Environment.CurrentDirectory));
                            break;
                        case "go match":
                            MatchImages();
                            Console.WriteLine("Match process complated.");
                            break;
                        case "set proxy":
                            NetworkManager.Host.SetProxy(pList[key]);
                            break;
                        case "set accuracy":
                            switch (pList[key])
                            {
                                case "low":
                                    ImageHelper.SampleSize = 8;
                                    ImageHelper.MatchThreshold = 5;
                                    ImageHelper.NonMatchThreshold = 10;
                                    break;
                                case "high":
                                    ImageHelper.SampleSize = 32;
                                    ImageHelper.MatchThreshold = 80;
                                    ImageHelper.NonMatchThreshold = 160;
                                    break;
                                case "normal":
                                default:
                                    ImageHelper.SampleSize = 16;
                                    ImageHelper.MatchThreshold = 20;
                                    ImageHelper.NonMatchThreshold = 40;
                                    break;
                            }
                            break;
                        case "get help":
                            CmdletHelper.ShowHelp();
                            break;
                        case "":
                            break;
                        default:
                            CmdletHelper.ShowError();
                            break;
                    }

                    Console.WriteLine();
                    Console.Write("$ ");
                    cmdLet = Console.ReadLine().ToLower();
                }

            }
        }

        private static void MatchImages()
        {
            string src = GetFileFromCmdLine("Please input the major file name: (press ENTER for finding {0})", "major.txt");
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(src + "\n");
            Console.ForegroundColor = c;

            string dist = GetFileFromCmdLine("Please input the compared file name: (press ENTER for finding {0})", "compare.txt");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(dist + "\n");
            Console.ForegroundColor = c;

            List<ImageSet> srcSet = LoadData(src);
            List<ImageSet> distSet = LoadData(dist);
            ImageSet iset;
            List<ImageFPrint> srcFP, distFP;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format("Sample size: {0}", ImageHelper.SampleSize));
            Console.WriteLine(string.Format("Matched Threshold: {0}", ImageHelper.MatchThreshold));
            Console.WriteLine(string.Format("Non-Matched Threshold: {0}", ImageHelper.NonMatchThreshold));
            Console.WriteLine(string.Format("Output to: {0}\n", ImageHelper.IsOutput ? "FILE" : "SCREEN"));
            Console.ForegroundColor = c;

            foreach (ImageSet item in srcSet)
            {
                iset = distSet.Where(p => p.Key == item.Key).FirstOrDefault();
                if (iset.Key != "")
                {
                    srcFP = ImageHelper.GetSample(item.URLs);
                    distFP = ImageHelper.GetSample(iset.URLs);

                    ImageHelper.Compare(srcFP, distFP, item.Key);
                }
            }
        }

        private static void InitShell()
        {
            Console.Title = AssemblyInfo.Title;
            //Console.CursorSize = 90;
            //Console.TreatControlCAsInput = true;

            Console.WriteLine(string.Format("{0} [Version {1}]", AssemblyInfo.Title, AssemblyInfo.Version));
            Console.WriteLine(string.Format("{0}  {1} {2}.", AssemblyInfo.Copyright, AssemblyInfo.Company, AssemblyInfo.Trademark));

            Console.WriteLine();
        }

        private static List<ImageSet> LoadData(string file)
        {
            List<ImageSet> results = new List<ImageSet>();

            using (StreamReader sr = new StreamReader(file))
            {
                ImageSet imgset = null;
                string line = "";
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("\t"))
                    {
                        if (imgset != null)
                        {
                            results.Add(imgset);
                        }
                        imgset = new ImageSet();
                        imgset.Key = line.Trim();
                    }
                    else
                    {
                        if (imgset == null)
                        {
                            imgset = new ImageSet();
                            imgset.URLs.Add(line);
                        }
                    }
                }

                results.Add(imgset);
            }

            List<ImageSet> list = new List<ImageSet>();

            list.AddRange(results.Where(p => p.URLs.Count > 0));
            list.Add(new ImageSet() { Key = "__default__", URLs = results.Where(p => p.URLs.Count == 0).Select(p => p.Key).ToList() });

            return list;
        }

        private static string GetFileFromCmdLine(string msg, string defaultFile)
        {
            string file = "";
            Console.WriteLine(string.Format(msg, defaultFile));
            file = Console.ReadLine();
            file = string.IsNullOrEmpty(file) ? defaultFile : file;

            file = Path.IsPathRooted(file) ? file : Path.Combine(Environment.CurrentDirectory, file);

            return file;
        }
    }
}
