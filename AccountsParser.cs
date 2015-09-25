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
    public class AccountsParser
    {
        private string FlexHub;
        private string LuaDch;
        private List<Account> accounts;
        private Dictionary<string, int> profiles;

        /// <summary>
        /// List used to store accounts found in the FlexHub accounts file
        /// </summary>
        public List<Account> Accounts
        {
            get { return accounts; }
            private set { accounts = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fromFile">FlexHub Accounts file</param>
        public AccountsParser(string fromFile)
            : this(fromFile, "user.tbl", null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fromFile">FlexHub Accounts file</param>
        /// <param name="toFile">LuaDch Accounts file</param>
        public AccountsParser(string fromFile, string toFile)
            : this(fromFile, toFile, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fromFile">FlexHub Accounts file</param>
        /// <param name="toFile">LuaDch Accounts file</param>
        /// <param name="profilesFile">File containing profile-level information</param>
        public AccountsParser(string fromFile, string toFile, string profilesFile)
        {
            FlexHub = fromFile;
            LuaDch = toFile;
            profiles = new Dictionary<string, int>();

            if (profilesFile != null && File.Exists(profilesFile))
            {
                LoadProfiles(profilesFile);
            }
            else
            {
                LoadProfiles();
            }
            accounts = new List<Account>();
        }

        /// <summary>
        /// Parses the Flexhub Accounts File and stores found accounts in the Accounts List
        /// </summary>
        public void Parse()
        {
            var tAccounts = LsonVars.Parse(File.ReadAllText(FlexHub, Encoding.GetEncoding("windows-1252")));

            foreach (var accounts in tAccounts["tAccounts"].GetDict())
            {
                //Console.WriteLine("Key: {0}, Value: {1}", accounts.Key.ToString(), accounts.Value.ToString());
                Account acct = new Account();

                foreach (var account in accounts.Value.GetDict())
                {
                    switch (account.Key.GetString())
                    {
                        case "sNick":
                            acct.Nick = account.Value.GetString();
                            break;
                        case "sPassword":
                            acct.Password = account.Value.GetString();
                            break;
                        case "sReggedBy":
                            acct.ReggedBy = account.Value.GetString();
                            break;
                        case "sProfile":
                            acct.Profile = account.Value.GetString();
                            break;
                        case "iRegDate":
                            acct.RegDate = (int)account.Value.GetIntLenientSafe();
                            break;
                    }
                }
                Accounts.Add(acct);
            }
        }

        /// <summary>
        /// Converts and saves the information to the LuaDch accounts file
        /// </summary>
        public void Convert()
        {
            if (File.Exists(LuaDch))
            {
                // Ask if we should overwrite?
                Console.WriteLine("The file '{0}' already exists.", LuaDch);
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
                    File.Delete(LuaDch);
                }
                else
                {
                    Program.PrintError("Nothing to do, exiting");
                }
            }

            FileStream fs = null;

            try
            {
                fs = new FileStream(LuaDch, FileMode.CreateNew);
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                {
#if DEBUG
                    writer.WriteLine("--[[ Encoding: {0} / Date: {1} ]]--{2}", writer.Encoding, DateTime.Now, Environment.NewLine);
#endif
                    writer.WriteLine("return {");
                    foreach (Account account in Accounts)
                    {
                        writer.WriteLine(
                            "    { " +
                            "by = \"" + account.ReggedBy.Replace(" ", "") + "_(F)\", " +
                            "date = \"" + UnixTimeToDate(account.RegDate) + "\", " +
                            "level = " + GetLevel(account.Profile) + ", " +
                            "nick = \"" + account.Nick + "\", " +
                            "password = \"" + account.Password + "\", " +
                            "},"
                            );
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
            Console.WriteLine("{0} accounts written to '{1}'", Accounts.Count, LuaDch);
        }

        /// <summary>
        /// Converts between FlexHub sProfile (string) and LuaDch Level (integer)
        /// </summary>
        /// <param name="profile">Profile name</param>
        /// <returns>level</returns>
        private int GetLevel(string profile)
        {
            if (profile == null)
            {
                return 0;
            }

            int level = 0;
            if (profiles.TryGetValue(profile.ToLower(), out level))
            {
                return level;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts UnixTime (used in FlexHub) to the DateTime-format used by LuaDch
        /// </summary>
        /// <param name="unixtime">Time in seconds since epoch</param>
        /// <returns>String with date</returns>
        private string UnixTimeToDate(int unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            return dtDateTime.ToString("yyyy-MM-dd") + " / " + dtDateTime.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Loads default profile to level values
        /// </summary>
        private void LoadProfiles()
        {
            profiles.Add("user", 10);
            profiles.Add("reg", 20);
            profiles.Add("vip", 30);
            profiles.Add("kvip", 40);
            profiles.Add("superuser", 50);
            profiles.Add("bots", 55);
            profiles.Add("operator", 60);
            profiles.Add("moderator", 70);
            profiles.Add("admin", 80);
            profiles.Add("hubowner", 100);
        }

        /// <summary>
        /// Loads profile to level values from file
        /// </summary>
        /// <param name="profilesFile">File to load profiles from</param>
        private void LoadProfiles(string profilesFile)
        {
            var tProfiles = LsonVars.Parse(File.ReadAllText(profilesFile));

            foreach (var profile in tProfiles["sProfiles"].GetDict())
            {
                // For some reason, the quotes stick around when the file is read
                // this is a "ugly" fix for it.
                string key = profile.Key.ToString().ToLower();
                key = key.Substring(1, key.Length - 2);
#if DEBUG
                Console.WriteLine("Key: {0}, Value: {1}", key, profile.Value.GetIntSafe());
#endif
                profiles.Add(key, (int)profile.Value.GetIntSafe());
            }
            Console.WriteLine("{0} profiles loaded", profiles.Count);
        }
    }
}