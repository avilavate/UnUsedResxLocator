using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Collections;
using System.Configuration;

namespace ResxCleanUp
{
    class Program
    {
        private static bool IsConfigPresent()
        {
            return File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
        static void Main(string[] args)
        {
            if (!IsConfigPresent())
            {
                Console.WriteLine("Could not find App Config in setup!");
                Console.Read();
                return;
            }
            string rootPath = string.Empty, logEntriesPath = string.Empty, existingResourceFilePath = string.Empty;


            rootPath = ConfigurationManager.AppSettings["PATHTOPLUGIN"];
            logEntriesPath = ConfigurationManager.AppSettings["OUTDIR"] + "entries.txt";
            existingResourceFilePath = ConfigurationManager.AppSettings["ResxFilePath"];

            // Define a new Resource file which will be generated out of only used entries
            var newResourceFilePath = rootPath + "testNew.resx";

            // Define the project folder which will be scanned for each and every RESX file entry.
            // NOTE: We should not include the DLLs (in bin/obj) of the same project or existing RESX files because
            // if they are scanned against the RESX entries, then we cannot find orphan entries
            var projectPath = rootPath;

            // Iterate all the keys of existing resource file and put them in Hashtable for later processing
            var resourceEntries = new Hashtable();
            var reader = new ResXResourceReader(existingResourceFilePath);
            foreach (DictionaryEntry d in reader)
            {
                resourceEntries.Add(d.Key.ToString(), d.Value == null ? "" : d.Value.ToString());
            }
            reader.Close();

            // Output the total count of entries in RESX file and number of files to be scanned in a given project
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Total Entries in RESX File : " + resourceEntries.Keys.Count);
            var files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories);
            Console.WriteLine("Total Files to be scanned in project : " + files.Count());
            Console.ForegroundColor = ConsoleColor.Green;
            // Iterate all the keys from hashtable and check if the key exists in any of the file present in the project
            var orphanEntries = new List<string>();
            foreach (var key in resourceEntries.Keys)
            {
                var found = 0;
                foreach (var file in files)
                {
                    if (file.Split(new char[] { '.' })[file.Split(new char[] { '.' }).Length - 1].Contains("cshtml"))
                    {
                        // If RESX Entry found in a given file
                        if (File.ReadAllText(file).Contains(key.ToString()))
                        {
                            // Log the entry
                            File.AppendAllText(logEntriesPath, key + "---" + Path.GetFileName(file));
                            File.AppendAllText(logEntriesPath, "\r\n");
                            Console.WriteLine(key + "---" + Path.GetFileName(file));
                            found = found + 1;
                            break;
                        }
                    }
                }

                // If RESX entry not found, then save it in orphans list
                if (found == 0)
                    orphanEntries.Add(key.ToString());
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Resource used and not initiallized in resx:");
            File.AppendAllText(logEntriesPath, "Resource used and not initiallized in resx: ");
            foreach (var file in files)
            {
                if (file.Split(new char[] { '.' })[file.Split(new char[] { '.' }).Length - 1].Contains("cshtml"))
                {
                    var lines = File.ReadLines(file);
                    foreach (var line in lines)
                    {
                        if (line.Contains("@Resources."))
                        {
                            var index = line.IndexOf("@Resources."); int end = 0;
                            for (int i = index; i < line.Length; i++)
                            {
                                if (line[i] == ' ' || line[i] == '<')
                                {
                                    end = i;
                                    break;
                                }
                            }
                            var notInresx = line.Substring(index, end - index).Replace("@Resources.", "");
                            //var allResxkeys= resourceEntries.Keys.Cast<string>().Aggregate((a, b) => { return a.ToString() + "|" + b.ToString(); });
                            if (end > 0 && !resourceEntries.ContainsKey(notInresx))
                            {
                                var u = notInresx + "--- " + Path.GetFileName(file);
                                Console.WriteLine(u);
                                File.AppendAllText(logEntriesPath, u);
                                end = 0;
                                break;
                            }
                        }
                    }
                }
            }
            // Write orphan Entries to Console and log file
            File.AppendAllText(logEntriesPath, "Unused Entries -- ");
            File.AppendAllText(logEntriesPath, "\r\n");

            Console.ForegroundColor = ConsoleColor.White;
            // CleanUp Completed and wait for further instructions
            Console.WriteLine("Scan completed ");
            Console.ReadLine();
        }
    }
}
