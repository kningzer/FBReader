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
        static List<POU> allFb;
        static List<POU> allFuncs;
        static List<string> projektmappar;
        static string proj;
        static List<String> tmc;


        static void Main(string[] args)
        {
            bool bShowIndividual = false;
            bool bShowAll = false;

            string startfolder = "";
            Console.WriteLine("Ange mapp som skall genomsökas: (Om inget anges så söks \"C:\\Git\" igenom");
            startfolder = Console.ReadLine();

            if (startfolder == "")
            {
                startfolder = @"C:\git\";
            }

            Console.WriteLine("{0} kommer att sökas igenom.\n", startfolder);
            Console.WriteLine("Hur ska sökningen utföras\n" +
                              "1. Rapportera FB/Funk5 per projekt.\n" +
                              "2. Rapportera totala antalet hittade.\n" +
                              "3. Utför både 1 & 2\n");
            int val = GetIntFromUser(1, 3);

            if (val == 1)
            {
                bShowIndividual = true;
            }
            else if (val ==2)
            {
                bShowAll = true;
            }else if (val == 3)
            {
                bShowAll = true;
                bShowIndividual = true;
            }

            filer = new List<string>();
            Fb = new List<POU>();
            funcs = new List<POU>();
            projektmappar = new List<string>();
            tmc = new List<string>();
            allFb = new List<POU>();
            allFuncs = new List<POU>();

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
                if (bShowIndividual) {
                    Console.WriteLine(  ConsoleWindowFullLine());
                    Console.WriteLine("Funktionsblock\t" + proj);
                    Console.WriteLine(  ConsoleWindowFullLine());
                    foreach (POU fb in Fb)
                    {
                        Console.WriteLine("{0,-35}{1,-35}{2,-1}", fb.fbName, fb.projName, fb.uses);
                    }
                    Console.WriteLine(  ConsoleWindowFullLine());
                    Console.WriteLine("Funktioner\t" + proj);
                    Console.WriteLine(  ConsoleWindowFullLine());
                    foreach (POU f in funcs)
                    {
                        Console.WriteLine("{0,-35}{1,-35}{2,-1}", f.fbName, f.projName, f.uses);
                    }
                }
                filer.Clear();
                tmc.Clear();
                Fb.Clear();
                funcs.Clear();
            }

            if (bShowAll)
            {

                Console.WriteLine(ConsoleWindowFullLine());
                Console.WriteLine("Summering av FB/Funktioner i vald mapp");
                Console.WriteLine(  ConsoleWindowFullLine());
                foreach (POU fb in allFb)
                {
                    Console.WriteLine("{0,-35}{1,-35}", fb.fbName, fb.projName);
                }
                Console.WriteLine(  ConsoleWindowFullLine());
                Console.WriteLine("Funktioner. Sammanställning");
                Console.WriteLine(   ConsoleWindowFullLine());
                foreach (POU f in allFuncs)
                {
                    Console.WriteLine("{0,-35}{1,-35}", f.fbName, f.projName);
                }

            }
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Färdig med uppgift");
            Console.WriteLine( ConsoleWindowFullLine());
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
                            allFb.Add(newP);
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
                            allFuncs.Add(newP);
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


        /// <summary>
        /// Hjälpfunktion för att få in och behandla ett heltal från användaren
        /// Min och max för talet kan sättas och funktionen loopar till ett giltigt tal anges
        /// </summary>
        /// <param name="_min">Minsta heltal som användaren kan ange</param>
        /// <param name="_max">Största heltal som användaren kan ange</param>
        /// <returns>Tal som användaren angav mellan satta gränser</returns>
        public static int GetIntFromUser(int _min, int _max)
        {
            bool inputOK = false;
            int input = 0;

            //Loopa tills input ärok
            while (!inputOK)
            {
                try
                {
                    //Läs input från användare och försök att konvertera till int32
                    input = Int32.Parse(Console.ReadLine());
                    //Kontrollera om heltalet ör inom satta gränser
                    //Om inte så kasta ett nytt fel
                    if (input < _min || input > _max)
                        throw new Exception();
                    //Allt var ok, flagga för att bryta loopen
                    inputOK = true;
                }
                catch (Exception ex)
                {
                    //Inputen var felaktig, meddela användaren och loopa om 
                    Console.WriteLine("Du måste ange ett heltal mellan {0} och {1}", _min, _max);
                }
            }
            //Returnera användarens svar
            return input;
        }


        /// <summary>
        /// Skriver ut en hel rad av ett tecken baserad på konsollens nuvarande fönsterbredd
        /// </summary>
        /// <param name="filler">Valfri! Tecken att fylla med. Standard satt till *</param>
        /// <returns>En hel rad med angivet tecken för nuvarande fönsterbredd</returns>
        public static string ConsoleWindowFullLine(string filler = "#")
        {
            string output = "";
            //Fyll output med så många tecken som fönstret är brett
            for (int i = 0; i < Console.WindowWidth - 1; i++)
            {
                output += filler;
            }

            return output;

        }






    }


}
