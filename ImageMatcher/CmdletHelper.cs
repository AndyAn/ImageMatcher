using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace ImageMatcher
{
    internal class CmdletHelper
    {
        internal static Dictionary<string, string> GetArguments(string[] args)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();

            List<string> argList = args.ToList();
            argList.Add("");

            list.Add(argList[0].Trim(), argList[1].Trim());

            return list;
        }

        internal static void ProgressStatus(int current, int maxCount)
        {
            int scrWidth = Console.WindowWidth - 17;
            string progressBar = "Progress: {0}% [{1}]{2}";

            int top = Console.CursorTop;
            int left = Console.CursorLeft;
            int cursor = current * scrWidth / maxCount;
            string pct = (current * 100 / maxCount).ToString().PadLeft(3);
            Console.SetCursorPosition(0, top);
            Console.Write(current < maxCount ? "Task Started..." : "Task Completed! ");
            Console.SetCursorPosition(0, top + 1);
            Console.Write(string.Format(progressBar, pct, (cursor > 0 ? new string('=', cursor - 1) + (current == maxCount ? "=" : ">") : "") + new string(' ', scrWidth - cursor), (current == maxCount ? "\n" : "")));
            if (current < maxCount)
            {
                Console.SetCursorPosition(left, top);
            }
        }

        internal static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        internal static void ShowError()
        {
            ShowMessage("Invalid command. Please use [Get Help] command to see the usage.\n");
        }

        private static string helpText = null;
        internal static void ShowHelp()
        {
            if (helpText == null)
            {
                //helpText = string.Format("\n{0} [Version {1}]\n", AssemblyInfo.Title, AssemblyInfo.Version);
                //helpText += string.Format("{0}  {1} {2}.\n\n", AssemblyInfo.Copyright, AssemblyInfo.Company, AssemblyInfo.Trademark);
                helpText = "";

                try
                {
                    using (StreamReader sr = new StreamReader(GetResource(".Resx.help.txt")))
                    {
                        helpText += sr.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    helpText += string.Format("Help on Usage({0})...", Path.GetFileNameWithoutExtension(typeof(CmdletHelper).Assembly.Location));
                }
            }

            ShowMessage(helpText + "\n");
        }

        private static Stream GetResource(string info)
        {
            var assembly = typeof(CmdletHelper).Assembly;
            return assembly.GetManifestResourceStream(assembly.EntryPoint.DeclaringType.Namespace + info);
        }
    }

    internal static class AssemblyInfo
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        private static string fileName = System.IO.Path.GetFileNameWithoutExtension(assembly.CodeBase);
        private static string title = null;
        private static string company = null;
        private static string product = null;
        private static string copyright = null;
        private static string trademark = null;
        private static string version = null;

        public static string Title
        {
            get
            {
                if (string.IsNullOrEmpty(title))
                {
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

                    if (attributes.Length > 0)
                    {
                        AssemblyTitleAttribute attribute = (AssemblyTitleAttribute)attributes[0];
                        if (attribute.Title.Length > 0)
                        {
                            title = attribute.Title;
                        }
                        else
                        {
                            title = fileName;
                        }
                    }
                    else
                    {
                        title = fileName;
                    }
                }

                return title;
            }
        }

        public static string Company
        {
            get
            {
                if (string.IsNullOrEmpty(company))
                {
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                    if (attributes.Length > 0)
                    {
                        AssemblyCompanyAttribute attribute = (AssemblyCompanyAttribute)attributes[0];
                        if (attribute.Company.Length > 0)
                        {
                            company = attribute.Company;
                        }
                        else
                        {
                            company = fileName;
                        }
                    }
                    else
                    {
                        company = fileName;
                    }
                }

                return company;
            }
        }

        public static string Product
        {
            get
            {
                if (string.IsNullOrEmpty(product))
                {
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                    if (attributes.Length > 0)
                    {
                        AssemblyProductAttribute attribute = (AssemblyProductAttribute)attributes[0];
                        if (attribute.Product.Length > 0)
                        {
                            product = attribute.Product;
                        }
                        else
                        {
                            product = fileName;
                        }
                    }
                    else
                    {
                        product = fileName;
                    }
                }

                return product;
            }
        }

        public static string Copyright
        {
            get
            {
                if (string.IsNullOrEmpty(copyright))
                {
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                    if (attributes.Length > 0)
                    {
                        AssemblyCopyrightAttribute attribute = (AssemblyCopyrightAttribute)attributes[0];
                        if (attribute.Copyright.Length > 0)
                        {
                            copyright = attribute.Copyright;
                        }
                        else
                        {
                            copyright = fileName;
                        }
                    }
                    else
                    {
                        copyright = fileName;
                    }
                }

                return copyright;
            }
        }

        public static string Trademark
        {
            get
            {
                if (string.IsNullOrEmpty(trademark))
                {
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);

                    if (attributes.Length > 0)
                    {
                        AssemblyTrademarkAttribute attribute = (AssemblyTrademarkAttribute)attributes[0];
                        if (attribute.Trademark.Length > 0)
                        {
                            trademark = attribute.Trademark;
                        }
                        else
                        {
                            trademark = fileName;
                        }
                    }
                    else
                    {
                        trademark = fileName;
                    }
                }

                return trademark;
            }
        }

        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    version = assembly.GetName().Version.ToString();
                }

                return version;
            }
        }
    }
}
