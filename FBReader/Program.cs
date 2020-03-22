using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FBReader
{
    class Program
    {
        static List<string> filer;
        static List<POU> Fb;
        static List<POU> funcs;
        static List<string> projektmappar;
        static string proj;
        static List<String> tmc;


        static void Main(string[] args)
        {
            string startfolder = "";
            Console.WriteLine("Ange mapp som skall genomsökas: (Om inget anges så söks \"C:\\Git\" igenom");
            startfolder = Console.ReadLine();

            if (startfolder == "")
            {
                startfolder = @"C:\git\";
            }

            filer = new List<string>();
            Fb = new List<POU>();
            funcs = new List<POU>();
            projektmappar = new List<string>();
            tmc = new List<string>();

            string[] temp = Directory.GetDirectories(startfolder);
            foreach (string sf in temp)
            {
                projektmappar.Add(sf);
            }

            foreach (string path in projektmappar)
            {
                proj = Path.GetFileName(path);

                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }

                foreach (string fpath in filer)
                {
                    Program.ReadXML(fpath);
                }

                foreach (string s in tmc)
                {
                    foreach (POU p in Fb)
                    {
                        if (ReadTMC(s, p.fbName))
                            p.uses++;
                    }
                    foreach (POU p in funcs)
                    {
                        if (ReadTMC(s, p.fbName))
                            p.uses++;
                    }
                }

                Console.WriteLine("#----------------------------------------------------------------");
                Console.WriteLine("Funktionsblock\t" + proj);
                Console.WriteLine("#----------------------------------------------------------------");
                foreach (POU fb in Fb)
                {
                    Console.WriteLine("{0,-35}{1,-35}{2,-1}", fb.fbName, fb.projName, fb.uses);
                }
                Console.WriteLine("#----------------------------------------------------------------");
                Console.WriteLine("Funktioner\t" + proj);
                Console.WriteLine("#----------------------------------------------------------------");
                foreach (POU f in funcs)
                {
                    Console.WriteLine("{0,-35}{1,-35}{2,-1}", f.fbName, f.projName, f.uses);
                }

                filer.Clear();
                tmc.Clear();
                Fb.Clear();
                funcs.Clear();
            }
            Console.WriteLine("Färdig!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.ReadKey();
        }

        public static void ReadXML(string path)
        {
            XDocument doc = XDocument.Load(path);
            foreach (XElement el in doc.Root.Elements())
            {

                //Console.WriteLine("{0}", el.Name);
                foreach (XElement elem in el.Elements())
                {
                    //Console.WriteLine("{0}  {1}", elem.Name, elem.Value);

                    if (elem.Value.Contains("FUNCTION_BLOCK"))
                    {
                        //Console.WriteLine("Denna var ett FB:{0}", el.Attribute("Name").Value);

                        bool found = false;

                        foreach (POU p in Fb)
                        {
                            if (p.fbName == el.Attribute("Name").Value)
                            {
                                found = true;
                                break;
                            }

                        }

                        if (!found)
                        {
                            POU newP = new POU();
                            newP.fbName = el.Attribute("Name").Value;
                            newP.projName = proj;
                            Fb.Add(newP);
                        }
              
                    }

                    if (elem.Value.Contains("FUNCTION "))
                    {
                        bool found = false;

                        foreach (POU p in funcs)
                        {
                            if (p.fbName == el.Attribute("Name").Value)
                            {
                                found = true;
                                break;
                            }

                        }

                        if (!found)
                        {
                            POU newP = new POU();
                            newP.fbName = el.Attribute("Name").Value;
                            newP.projName = proj;
                            funcs.Add(newP);
                        }
     
                    }
                }

            }

    
        }


        public static bool ReadTMC(string path, string POU)
        {
            XDocument doc = XDocument.Load(path);
            foreach (XElement el in doc.Root.Elements())
            {

                //Console.WriteLine("{0}", el.Name);
                foreach (XElement elem in el.Elements())
                {
                    //Console.WriteLine("{0}  {1}", elem.Name, elem.Value);

                    if (elem.Value.Contains(POU))
                    {
                        return true;
                    }

                }

            }

            return false;
        }



        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            

            string ext = Path.GetExtension(path);

            if (ext == ".TcPOU")
            {
                filer.Add(path);
            }

            if (ext == ".tmc")
            {
                tmc.Add(path);
            }
        }


    }
}
