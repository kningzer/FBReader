using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;




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
        static bool bShowIndividual = false;
        static bool bShowAll = false;
        static bool bCopy = false;

        [STAThread]
        static void Main(string[] args)
        {
            //Variabababababler
            string startfolder = "";
            bool pathOK = false;
            filer = new List<string>();
            Fb = new List<POU>();
            funcs = new List<POU>();
            projektmappar = new List<string>();
            tmc = new List<string>();
            allFb = new List<POU>();
            allFuncs = new List<POU>();

            //Fual sig lite
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("FB Detektiven");
            Console.WriteLine(ConsoleWindowFullLine());

            //Visa menyn
            Console.WriteLine("Hur ska sökningen utföras\n" +
                              "1. Rapportera vilka FB\\Funktioner var projekt innehåller från angiven plats.\n" +
                              "2. Rapportera totala antalet hittade FB\\Funktioner från angiven plats.\n" +
                              "3. Utför både 1 & 2\n" +
                              "4. Extrahera FB\\Funktioner och kopiera till separat mapp\n" +
                              "5. Avsluta\n");

            //Läs & hantera val från användaren
            int val = GetIntFromUser(1, 5);

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
            }else if(val == 4)
            {
                bCopy = true;
            }else if(val == 5)
            {
                Environment.Exit(1);
            }

            while (!pathOK)
            {
                //Be om sökväg
                Console.WriteLine("Ange mapp som skall genomsökas: (Tryck Enter för att öppna sökfönster)");
                startfolder = Console.ReadLine();
                //Öppna sökdialog om det behövs
                if (startfolder == "")
                {
                    startfolder = BrowseFolder();
                }

                if (!(startfolder == "") && Directory.Exists(startfolder))
                {
                    pathOK = true;
                }
                else
                {
                    Console.WriteLine("Ange en giltlig sökväg! Försök igen");
                }
            }
            //Informera
            Console.WriteLine("Sökning kommer utföras på: {0}", startfolder);
            Console.WriteLine("Programmet startar nu, vänta tills texten \"Färdig med uppgift\" visas.\n" +
                              "Konsollen uppdateras även om det inte är helt klart");


            //Kör program
            runProgram(startfolder);
 
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Färdig med uppgift");
            Console.WriteLine( ConsoleWindowFullLine());

            Console.ReadKey();
        }


        public static void runProgram(string startfolder)
        {
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
                    //KOntrollera filsökväg
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    //Kontrollera mapp
                    ProcessDirectory(path);
                }
                else
                {
                    //Om det siter sig, backa ur
                    Console.WriteLine("{0} är inte en korrekt sökväg eller mapp.", path);
                }

                //Kontrollera Twincat filer
                foreach (string fpath in filer)
                {
                    Program.ReadXML(fpath);
                }
                //Kontrollera tmc filer efter FB referenser
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
                //Skall inviduella projekt skrivas ut
                if (bShowIndividual)
                {
                    printProjectData();
                }
                //Rensa variabler
                filer.Clear();
                tmc.Clear();
                Fb.Clear();
                funcs.Clear();
            }
            //Skriv ut resultat om det önskas
            if (bShowAll)
            {
                printAllData();
            }
            //Kopiera filer om det önskas
            if (bCopy)
            {
                copyAllData(startfolder);
            }
        }

        //Visa sökruta
        public static string BrowseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            string path = "";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
            }

            return path;
        }

        //Sök igenom Twincat filer
        public static void ReadXML(string path)
        {
            //Ladda fil som xml
            XDocument doc = XDocument.Load(path);
            foreach (XElement el in doc.Root.Elements())
            {
                //gå igenom delar
                foreach (XElement elem in el.Elements())
                {
                 
                    //KOntrollera om FB text förekommer
                    if (elem.Value.Contains("FUNCTION_BLOCK"))
                    {

                        bool found = false;
                        //Kolla emot dubletter
                        foreach (POU p in Fb)
                        {
                            if (p.fbName == el.Attribute("Name").Value)
                            {
                                found = true;
                                break;

                            }

                        }
                        //Lägg till om unik
                        if (!found)
                        {
                            POU newP = new POU();
                            newP.fbName = el.Attribute("Name").Value;
                            newP.projName = proj;
                            newP.fullpath = path;
                            Fb.Add(newP);
                            allFb.Add(newP);
                        }
                    }
                    //Utför samma för funktioner
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
                            newP.fullpath = path;
                            funcs.Add(newP);
                            allFuncs.Add(newP);
                        }

                    }
                    

                }

            }
        }

        //Läs Twincat tmc filer som xml format och leta efter FB referenser i dem.
        public static bool ReadTMC(string path, string POU)
        {
            XDocument doc = XDocument.Load(path);
            foreach (XElement el in doc.Root.Elements())
            {
                foreach (XElement elem in el.Elements())
                {

                    if (elem.Value.Contains(POU))
                    {
                        return true;
                    }

                }

            }

            return false;
        }

        //Skriv data per projekt i konsollen
        public static void printProjectData()
        {
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Funktionsblock\t" + proj);
            Console.WriteLine(ConsoleWindowFullLine());
            foreach (POU fb in Fb)
            {
                string use = "";
                if (fb.uses > 0)
                {
                    use = "Används";
                }
                else
                {
                    use = "EJ använd";
                }

                
                Console.WriteLine("{0,-35}{1,-35}{2,-10}", fb.fbName, fb.projName, use);
            }
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Funktioner\t" + proj);
            Console.WriteLine(ConsoleWindowFullLine());
            foreach (POU f in funcs)
            {
                string use = "";
                if (f.uses > 0)
                {
                    use = "Används";
                }
                else
                {
                    use = "EJ använd";
                }
                Console.WriteLine("{0,-35}{1,-35}{2,-10}", f.fbName, f.projName, use);
            }
        }
        //Skriv ut summerad data i konsollen
        public static void printAllData()
        {
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Summering av FB/Funktioner i vald mapp");
            Console.WriteLine(ConsoleWindowFullLine());
            foreach (POU fb in allFb)
            {
                Console.WriteLine("{0,-35}{1,-35}", fb.fbName, fb.projName);
            }
            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("Funktioner. Sammanställning");
            Console.WriteLine(ConsoleWindowFullLine());
            foreach (POU f in allFuncs)
            {
                Console.WriteLine("{0,-35}{1,-35}", f.fbName, f.projName);
            }
        }
        //Kopiera alla summerade filer till en egen mapp
        public static void copyAllData(string startfolder)
        {
            string newPath = startfolder + "\\FB-Funks-" + DateTime.Now.ToString("yyyyMMdd");
            Directory.CreateDirectory(newPath);

            foreach (POU fb in allFb)
            {
                System.IO.File.Copy(fb.fullpath, newPath +"\\"+ Path.GetFileName(fb.fullpath), true);
            }

            foreach (POU f in allFuncs)
            {
                System.IO.File.Copy(f.fullpath, newPath +"\\"+ Path.GetFileName(f.fullpath), true);
            }

            Console.WriteLine(ConsoleWindowFullLine());
            Console.WriteLine("{0} antal FB & {1} antal funktioner kopierade till {2}", allFb.Count(), allFuncs.Count(), newPath);
            Console.WriteLine(ConsoleWindowFullLine());

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
