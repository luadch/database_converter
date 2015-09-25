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

using LsonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flex2Luadch
{
    public class ProfilesParser
    {
        private string FlexHubFile;
        private List<string> profiles;

        public List<string> Profiles
        {
            get { return profiles; }
            private set { profiles = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fromFile">FlexHub accounts file</param>
        public ProfilesParser(string fromFile)
        {
            FlexHubFile = fromFile;
            profiles = new List<string>();
        }

        /// <summary>
        /// Parses the FlexHub accounts file looking for profiles
        /// </summary>
        public void Parse()
        {
            var tAccounts = LsonVars.Parse(File.ReadAllText(FlexHubFile, Encoding.GetEncoding("windows-1252")));

            foreach (var accounts in tAccounts["tAccounts"].GetDict())
            {
                foreach (var account in accounts.Value.GetDict())
                {
                    if (account.Key.GetString() == "sProfile" && !profiles.Contains(account.Value.GetString()))
                    {
#if DEBUG
                        Console.WriteLine("Found profile: {0}", account.Value.GetString());
#endif
                        profiles.Add(account.Value.GetString());
                    }
                }
            }
        }

        /// <summary>
        /// Writes list of profiles to disk
        /// </summary>
        public void Write()
        {
            if (File.Exists("Profiles.lua"))
            {
                // Ask if we should overwrite?
                Console.WriteLine("The file 'Profiles.lua' already exists.");
                Console.Write("Overwrite? [Y/N]: ");

                ConsoleKeyInfo cki;

                while (true)
                {
                    cki = Console.ReadKey();
                    if (cki.Key.ToString().ToLower() != "y" && cki.Key.ToString().ToLower() != "n")
                    {
                        Console.WriteLine();
                        Console.Write("Overwrite? [Y/N]: ");
                    }
                    else
                    {
                        Console.WriteLine();
                        break;
                    }
                }

                if (cki.Key.ToString().ToLower() == "y")
                {
                    File.Delete("Profiles.lua");
                }
                else
                {
                    Program.PrintError("Nothing to do, exiting");
                }
            }

            FileStream fs = null;

            try
            {
                fs = new FileStream("Profiles.lua", FileMode.CreateNew);
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                {
#if DEBUG
                    writer.WriteLine("--[[ Encoding: {0} / Date: {1} ]]--{2}", writer.Encoding, DateTime.Now, Environment.NewLine);
#endif
                    writer.WriteLine("sProfiles = {");
                    foreach (string sProfile in profiles)
                    {
                        writer.WriteLine("    [\"{0}\"] = 20,", sProfile);
                    }

                    writer.WriteLine("}");
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }
        }
    }
}