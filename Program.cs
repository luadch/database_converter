#region License Information (MIT)

/*
The MIT License (MIT)

Copyright (c) 2015

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

#endregion License Information (MIT)

using System;
using System.IO;
using System.Text;

namespace Flex2Luadch
{
    public class Program
    {
        private static string FlexHubFile = "HubAccounts.ini";
        private static string LuaDchFile = "user.tbl";
        private static string Profiles = null;

        public static void Main(string[] args)
        {
            CLI cmdLine = new CLI(args);

            // If called with help or usage, we should print info and exit directly
            if (cmdLine["help"] != null || cmdLine["usage"] != null || cmdLine["?"] != null)
            {
                PrintUsage();
            }

            if (cmdLine["license"] != null)
            {
                PrintLicense();
            }

            if (cmdLine["in"] != null)
            {
                FlexHubFile = cmdLine["in"];
            }

            if (cmdLine["out"] != null)
            {
                LuaDchFile = cmdLine["out"];
            }

            if (cmdLine["profiles"] != null)
            {
                if (File.Exists(cmdLine["profiles"]))
                {
                    Profiles = cmdLine["profiles"];
                }
                else
                {
                    Profiles = "Profiles.lua";
                }
            }

            if (cmdLine["genprofiles"] != null)
            {
                // Generate the Profiles.lua file
                if (File.Exists(FlexHubFile))
                {
                    ProfilesParser parser = new ProfilesParser(FlexHubFile);
                    parser.Parse();

                    if (parser.Profiles.Count > 0)
                    {
                        Console.WriteLine("{0} profiles found in '{1}'", parser.Profiles.Count, FlexHubFile);
                        parser.Write();

                        Console.WriteLine("{0} profiles written to 'Profiles.lua'", parser.Profiles.Count);
                    }
                    else
                    {
                        Console.WriteLine("0 profiles found in '{0}', exiting.", FlexHubFile);
                    }
                    Environment.Exit(0);
                }
                else
                {
                    PrintError("The file '{0}' does not exist.", FlexHubFile);
                }
            }

            if (File.Exists(FlexHubFile))
            {
                AccountsParser parser = new AccountsParser(FlexHubFile, LuaDchFile, Profiles);

                parser.Parse();

                if (parser.Accounts.Count > 0)
                {
                    Console.WriteLine("{0} accounts found in '{1}'", parser.Accounts.Count, FlexHubFile);
                }
                else
                {
                    Console.WriteLine("0 accounts found in '{0}', exiting.", FlexHubFile);
                    Environment.Exit(0);
                }

                // Writes to file
                parser.Convert();
            }
            else
            {
                Console.WriteLine("The file '{0}' does not exist", FlexHubFile);
            }
        }

        /// <summary>
        /// Prints out error message and exists.
        /// Works similar to Console.WriteLine(string, args)
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="args">Arguments</param>
        public static void PrintError(string text, params object[] args)
        {
            Console.WriteLine(text, args);
            Environment.Exit(0);
        }

        /// <summary>
        /// Prints usage/help text and exits
        /// </summary>
        public static void PrintUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage: Flex2Luadch [OPTIONS]");
            sb.AppendLine();
            sb.AppendLine("Option:          Description: ");
            sb.AppendLine("--in=FILE        - FlexHub accounts file (HubAccounts.ini)");
            sb.AppendLine("--out=FILE       - LuaDch accounts file (user.tbl)");
            sb.AppendLine("--profiles=FILE  - File with Flexhub Profiles with corresponding LuaDch levels");
            sb.AppendLine("--genprofiles    - Creates 'Profiles.lua' that can be used for manipulating above");
            sb.AppendLine("--help           - This text");
            sb.AppendLine("--usage          - This text");
            sb.AppendLine();
            sb.AppendLine("If --in is omitted, it will look in current directory for HubAccounts.ini");
            sb.AppendLine("If --out is omitted, it will write to user.tbl in the current directory");
            sb.AppendLine("If --profiles is omitted, it will use the default Flexhub to LuaDch profile conversion");
            sb.AppendLine();
            sb.AppendLine("The parameter --profiles is only really needed if you have created your own profiles in FlexHub,");
            sb.AppendLine("and/or want the default ones to have different levels in LuaDch");
            sb.AppendLine("The parameter --genprofiles can be used to generate a 'Profiles.lua' file that can be tweaked");
            sb.AppendLine("to achieve the above mentioned");
            Console.WriteLine(sb.ToString());
            Environment.Exit(0);
        }

        /// <summary>
        /// Prints license information and exits
        /// </summary>
        public static void PrintLicense()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Flex2Luadch is licensed under the MIT license");
            sb.AppendLine("LsonLib, that is included, is licensed under GPLv3");
            Console.WriteLine(sb.ToString());
            Environment.Exit(0);
        }
    }
}