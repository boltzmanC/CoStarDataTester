using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Data;
using FluentFTP;

namespace CoStarDataTester
{
    public class USAnnualDeliverableTester
    {
        //COSTAR File Processes.
        // ADD CHECKS FOR UNIQUE COLUMNS IN DATA PULL.

        // Every year the Costar Deliverable consists of two main file groups. 
        //    Points Files - Site Data.
        //    Area Files - Standard Geos. 
        //
        // Then later in the year we process the Catchup file. Smaller set of Points.
        //    Catchup File - Site Data

        // All data that is delivered to Costar is Processed on the Backend and delivered to E1 Support via SDFILER.
        // To test. Compare values of files delivered to Platform values. Custom report were created. 


        // Annual Points Files Deliverable
        public static void CostarPointsFiles()
        {
            // Points are delievered in a .csv file on sdfiler.
            //    upload into the costar instance.
            //
            // 1. Build reports. Format:
            //    Only able to use 3 radii at a time. thats fine. but still need a way to test.
            //    s1 | r1 | data
            //    s1 | r2 | data
            //    s1 | r3 | data
            //
            // 2. Costar Data Format: 1,2,3,5,10 mile radii
            //    s1 | r1 | data
            //    s1 | r2 | data
            //    s1 | r3 | data
            //    s1 | r4 | data
            //    s1 | r5 | data

            // BEGIN
            //Console.SetWindowSize(150, 60);

            Console.Write("Drag and Drop Costar Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();

            Console.Write("Drag and Drop E1 Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string e1directory = Console.ReadLine();
            char e1deli = FunctionTools.GetDelimiter();
            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            string[] e1filepaths = Directory.GetFiles(@e1directory);

            // Use dicts. One for each Radii. 1-3 and 4-5.
            Dictionary<string, string[]> e1radii1info = new Dictionary<string, string[]>();
            Dictionary<string, string[]> e1radii2info = new Dictionary<string, string[]>();
            Dictionary<string, string[]> e1radii3info = new Dictionary<string, string[]>();

            Dictionary<string, string[]> e1radii4info = new Dictionary<string, string[]>();
            Dictionary<string, string[]> e1radii5info = new Dictionary<string, string[]>();

            List<Dictionary<string, string[]>> dictionariestotest = new List<Dictionary<string, string[]>>();
            List<int> radiinumbertotest = new List<int>();

            int radii123fileindex = 0;
            int radii45fileindex = 0;

            // Read e1 radii files. 
            if (e1filepaths.Length >= 2)
            {
                Console.WriteLine();
                Console.WriteLine("E1 Files: ");
                int filenumber = 1;
                foreach (var file in e1filepaths)
                {
                    Console.WriteLine("{0} - {1}", filenumber++, FunctionTools.GetFileNameWithoutExtension(file));
                }

                Console.Write("Enter number for radi 1-3 file: ");
                string answer = Console.ReadLine().Trim();
                Console.Write("Enter number for radi 4-5 file: ");
                string answer2 = Console.ReadLine().Trim();

                //convert endered numbers.
                bool parsedanswer = Int32.TryParse(answer, out radii123fileindex);
                bool parsedanswer2 = Int32.TryParse(answer2, out radii45fileindex);
                //correct numbers for index.
                radii123fileindex -= 1;
                radii45fileindex -= 1;

                // read the e1 files.
                for (int x = 0; x <= e1filepaths.Length - 1; x++)
                {
                    if (x == radii123fileindex)
                    {
                        CostarReadE1Radii123PointsFile(e1filepaths[radii123fileindex], e1deli, e1radii1info, e1radii2info, e1radii3info);

                        //Dictionaries to Test.
                        dictionariestotest.Add(e1radii1info);
                        dictionariestotest.Add(e1radii2info);
                        dictionariestotest.Add(e1radii3info);

                        //radii to test.
                        radiinumbertotest.Add(1);
                        radiinumbertotest.Add(2);
                        radiinumbertotest.Add(3);
                    }

                    if (x == radii45fileindex)
                    {
                        CostarReadE1Radii45PointsFile(e1filepaths[radii45fileindex], e1deli, e1radii4info, e1radii5info);
                        dictionariestotest.Add(e1radii4info);
                        dictionariestotest.Add(e1radii5info);
                        radiinumbertotest.Add(4);
                        radiinumbertotest.Add(5);
                    }
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("E1 File: ");
                Console.WriteLine(FunctionTools.GetFileNameWithoutExtension(e1filepaths[0]));
                Console.WriteLine();
                Console.Write("Radii123 File (1) or Radii45 File (2), Enter 1 or 2: ");
                string answer = Console.ReadLine().Trim();
                int filenumber = 0;
                bool parsedanswer = Int32.TryParse(answer, out filenumber);

                if (filenumber == 1)
                {
                    CostarReadE1Radii123PointsFile(e1filepaths[radii123fileindex], e1deli, e1radii1info, e1radii2info, e1radii3info);
                    dictionariestotest.Add(e1radii1info);
                    dictionariestotest.Add(e1radii2info);
                    dictionariestotest.Add(e1radii3info);
                    radiinumbertotest.Add(1);
                    radiinumbertotest.Add(2);
                    radiinumbertotest.Add(3);
                }
                else
                {
                    CostarReadE1Radii45PointsFile(e1filepaths[radii45fileindex], e1deli, e1radii4info, e1radii5info);
                    dictionariestotest.Add(e1radii4info);
                    dictionariestotest.Add(e1radii5info);
                    radiinumbertotest.Add(4);
                    radiinumbertotest.Add(5);
                }
            }

            // Done with E1 files. Everything stored in memory.
            //*****************************************************************

            // Read the Costar File.

            Console.WriteLine();
            Console.WriteLine("Reading Costar Files...");
            char costardeli = FunctionTools.GetDelimiter();

            int costarfilesprocesses = 0;
            List<string> failedsitesmasterlist = new List<string>();

            Dictionary<string, List<int>> failedsitesandvalues = new Dictionary<string, List<int>>(); // list SiteID, column index

            foreach (var file in costarfilepaths)
            {
                using (StreamReader costarfile = new StreamReader(file))
                {
                    // Example line from Costar Data File.
                    // AREA_ID,    ID,       RING, RING_DEFN, LAT,         LON,           ALCOHOLIC_BEVERAGES_CY
                    // 10000060_1, 10000060, 1,    1,         39.7577442,  -87.1055097,   18342
                    string readline = string.Empty;
                    string header = costarfile.ReadLine().Replace("\"", string.Empty);
                    string[] headervalues = header.Split(costardeli);
                    //List<string> failedsites = new List<string>();

                    while ((readline = costarfile.ReadLine()) != null)
                    {
                        // Find radii number and ID.
                        readline = readline.Replace("\"", string.Empty);

                        string[] splitreadline = readline.Split(costardeli);

                        string siteidwithring = splitreadline[0];
                        string siteid = splitreadline[1];   // consistent across deliverables. no need to change.
                        string ring = splitreadline[2]; //this is used to find the radii dictionaries.
                        int ringnumber = 0;
                        bool ringparse = Int32.TryParse(ring, out ringnumber); //get radii number 1-5.

                        // if fileformat changes.
                        //char radditotest = siteidradii[siteidradii.Length - 1]; //get the last character of the siteid. this will match the radii number.
                        //int radiinumber = 0;
                        //bool radiinumberparse = Int32.TryParse(radditotest.ToString(), out radiinumber);

                        if (radiinumbertotest.Contains(ringnumber))
                        {
                            int index = radiinumbertotest.IndexOf(ringnumber); //get index of ring value from the radiinumber list. index will match dictionarytotest list becuase they added in the same order.

                            string failedsiteinfo = CostarTestCostarRadiiXLine(dictionariestotest[index], siteid, ring, splitreadline, file);
                            if (!failedsitesmasterlist.Contains(failedsiteinfo))
                            {
                                failedsitesmasterlist.Add(failedsiteinfo);
                            }
                        }
                    }
                }

                costarfilesprocesses++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\rCostar Files Processed - {0}.", costarfilesprocesses);
                Console.ResetColor();
            }

            string failedlist = FunctionTools.GetDesktopDirectory() + "\\failedsites.txt";
            using (StreamWriter failures = new StreamWriter(failedlist))
            {
                foreach (var f in failedsitesmasterlist)
                {
                    failures.WriteLine(f);
                }
            }

            // again
            Console.WriteLine();
            Console.Write("Run again (y/n): ");
            string again = Console.ReadLine().Trim().ToUpper();

            if (again == "Y")
            {
                CostarPointsFiles();
            }


        }

        public static void CostarReadE1Radii123PointsFile(string filepath, char delimiter, Dictionary<string, string[]> radii1, Dictionary<string, string[]> radii2, Dictionary<string, string[]> radii3)
        {
            //read the one file into memory. same as old program. THIS WILL ONLY READ THE RADII 1-3 file
            using (StreamReader e1file = new StreamReader(filepath))
            {
                string line;
                // Read past header lines.
                e1file.ReadLine(); //reads report name line.
                e1file.ReadLine(); //reads area name line.
                e1file.ReadLine(); //reads the header line.

                while ((line = e1file.ReadLine()) != null)
                {
                    line = line.Replace("\"", string.Empty); //removes all " from the line if they are present in the file. no address just unformatted numbers so its fine.
                    string[] splitline = line.Split(delimiter);
                    int linelength = splitline.Length;
                    string key = splitline[0];

                    if (!radii1.ContainsKey(key)) //check for radii 1 info.
                    {
                        List<string> listvalues = new List<string>();
                        for (int v = 1; v < linelength; v++)
                        {
                            listvalues.Add(splitline[v]);
                        }
                        string[] values = listvalues.ToArray();
                        radii1.Add(key, values);

                        //*****************************************************************
                        string line2 = e1file.ReadLine();
                        string line3 = e1file.ReadLine();
                        line2 = line2.Replace("\"", string.Empty); //removes all " from the line if they are present in the file. no address just unformatted numbers so its fine.
                        line3 = line3.Replace("\"", string.Empty);
                        string[] splitline2 = line2.Split(delimiter);
                        string[] splitline3 = line3.Split(delimiter);
                        string key2 = splitline2[0];
                        string key3 = splitline3[0];

                        if (!radii2.ContainsKey(key2))
                        {
                            List<string> listvalues2 = new List<string>();
                            for (int v = 1; v < linelength; v++)
                            {
                                listvalues2.Add(splitline2[v]);
                            }
                            string[] values2 = listvalues2.ToArray();
                            radii2.Add(key2, values2);
                        }

                        if (!radii3.ContainsKey(key3))
                        {
                            List<string> list_values3 = new List<string>();
                            for (int v = 1; v < linelength; v++)
                            {
                                list_values3.Add(splitline3[v]);
                            }
                            string[] values3 = list_values3.ToArray();
                            radii3.Add(key3, values3);
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("E1 file - {0} - Processed.", filepath);
            Console.WriteLine("Radii 1 - {0}, unique values", radii1.Count());
            Console.WriteLine("Radii 2 - {0}, unique values", radii2.Count());
            Console.WriteLine("Radii 3 - {0}, unique values", radii3.Count());
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void CostarReadE1Radii45PointsFile(string filepath, char delimiter, Dictionary<string, string[]> radii1, Dictionary<string, string[]> radii2)
        {
            //read the one file into memory. same as old program. THIS WILL ONLY READ THE RADII 1-3 file
            using (StreamReader e1file = new StreamReader(filepath))
            {
                string line;
                // Read past header lines.
                e1file.ReadLine(); //reads report name line.
                e1file.ReadLine(); //reads area name line.
                e1file.ReadLine(); //reads the header line.

                while ((line = e1file.ReadLine()) != null)
                {
                    line = line.Replace("\"", string.Empty); //removes all " from the line if they are present in the file. no address just unformatted numbers so its fine.
                    string[] splitline = line.Split(delimiter);
                    int linelength = splitline.Length;
                    string key = splitline[0];

                    if (!radii1.ContainsKey(key)) //check for radii 1 info.
                    {
                        List<string> listvalues = new List<string>();
                        for (int v = 1; v < linelength; v++)
                        {
                            listvalues.Add(splitline[v]);
                        }
                        string[] values = listvalues.ToArray();
                        radii1.Add(key, values);

                        //*****************************************************************
                        string line2 = e1file.ReadLine();
                        line2 = line2.Replace("\"", string.Empty); //removes all " from the line if they are present in the file. no address just unformatted numbers so its fine.
                        string[] splitline2 = line2.Split(delimiter);
                        string key2 = splitline2[0];

                        if (!radii2.ContainsKey(key2))
                        {
                            List<string> listvalues2 = new List<string>();
                            for (int v = 1; v < linelength; v++)
                            {
                                listvalues2.Add(splitline2[v]);
                            }
                            string[] values2 = listvalues2.ToArray();
                            radii2.Add(key2, values2);
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("E1 file - {0} - Processed.", filepath);
            Console.WriteLine("Radii 4 - {0}, unique values", radii1.Count());
            Console.WriteLine("Radii 5 - {0}, unique values", radii2.Count());
            Console.WriteLine();
            Console.ResetColor();
        }

        public static string CostarTestCostarRadiiXLine(Dictionary<string, string[]> radiidictionary, string siteid, string radiinumber, string[] splitline, string currentfile)
        {
            //string failstate = "fail";

            if (radiidictionary.ContainsKey(siteid))
            {
                int splitlinelength = splitline.Length - 7; // data to test starts at splitline[6].
                for (int v = 0; v < splitlinelength; v++)
                {
                    int radiidictindex = v + 6;

                    if (radiidictionary[siteid][v] != splitline[radiidictindex] && (radiidictionary[siteid][v] != string.Empty || splitline[radiidictindex] != string.Empty))
                    {
                        //tests individual values that arent equal
                        long costarvalue = Convert.ToInt64(Convert.ToDouble(radiidictionary[siteid][v]));
                        long e1value = Convert.ToInt64(Convert.ToDouble(splitline[radiidictindex]));

                        if (costarvalue != e1value && (costarvalue + 1) != e1value && (costarvalue - 1) != e1value)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("File: {0}", FunctionTools.GetFileNameWithoutExtension(currentfile));
                            Console.Write(" SiteID: {0:10}, Radii: {1}, Column: {2} CValue: {3}, E1Value: {4}", siteid, radiinumber, radiidictindex, splitline[radiidictindex], radiidictionary[siteid][v]);
                            Console.WriteLine();
                            Console.ResetColor();

                            string badsiteinfo = siteid + "_" + radiinumber + "_" + radiidictindex + "_" + splitline[radiidictindex] + "_" + radiidictionary[siteid][v];

                            return badsiteinfo;
                        }
                    }
                }
            }

            return "siteid_radiinumber_columnnumber_costarvalue_e1value";
        }

        public static void CostarPointsFilesManualCheck() // add indexer for costarpointsfilesmanualcheck function
        {
            // Output all variables for user specified site, also output all radii values.

            // Costar Data Format: 1,2,3,5,10 mile radii
            //    s1 | r1 | data
            //    s1 | r2 | data
            //    s1 | r3 | data
            //    s1 | r4 | data
            //    s1 | r5 | data

            Console.WriteLine();
            Console.WriteLine("Drag and Drop Costar Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();
            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            char deli = FunctionTools.GetDelimiter();
            //char qualifier = GetTXTQualifier();

            Dictionary<string, List<string>> radiivalues = new Dictionary<string, List<string>>();
            List<string> columnnames = new List<string>();

            Console.Write("Enter Radii # to test: ");
            string ringtotest = Console.ReadLine().Trim(); //out of memory exception when testing all radii. change to test just 1.

            using (StreamReader columnnamereader = new StreamReader(costarfilepaths[0]))
            {
                string header = columnnamereader.ReadLine();
                //string[] headervalues = SplitLineWithTxtQualifier(header, deli, qualifier, false);
                string[] headervalues = header.Split(deli);

                for (int v = 6; v <= headervalues.Length - 1; v++)
                {
                    columnnames.Add(headervalues[v]);
                }
            }

            int pointsadded = 0;
            int filesread = 0;
            foreach (var f in costarfilepaths)
            {
                using (StreamReader file = new StreamReader(f))
                {
                    string line = string.Empty;
                    file.ReadLine();

                    while ((line = file.ReadLine()) != null)
                    {
                        // Example line from Costar Data File.
                        // AREA_ID,    ID,       RING, RING_DEFN, LAT,         LON,           ALCOHOLIC_BEVERAGES_CY
                        // 10000060_1, 10000060, 1,    1,         39.7577442,  -87.1055097,   18342
                        line = line.Replace("\"", string.Empty);
                        string[] splitline = line.Split(deli);

                        string sitekey = splitline[1];
                        string sitering = splitline[2];

                        if (sitering == ringtotest)
                        {
                            if (!radiivalues.ContainsKey(sitekey))
                            {
                                List<string> valuestoadd = new List<string>();
                                for (int i = 6; i <= splitline.Length - 1; i++)
                                {
                                    valuestoadd.Add(splitline[i]);
                                }
                                radiivalues.Add(sitekey, valuestoadd);
                                pointsadded++;
                            }
                        }
                    }
                }
                filesread++;
                Console.WriteLine("\rFiles Read - {0}", filesread);
            }

            Console.WriteLine("Points read - {0}", pointsadded);

            int bufferwidth = Console.BufferWidth;
            int bufferheight = 600;
            Console.SetBufferSize(bufferwidth, bufferheight);

            string answer = string.Empty;
            Console.Write("Enter Site ID to search or \"exit\" to close the program: ");
            answer = Console.ReadLine();

            while (answer != "exit")
            {
                if (radiivalues.ContainsKey(answer))
                {
                    string[] columnarray = columnnames.ToArray();
                    List<string> keyvalues = radiivalues[answer];
                    string[] valuearray = keyvalues.ToArray();

                    //line builder
                    for (int v = 0; v <= columnarray.Length - 1; v++)
                    {
                        Console.WriteLine("{0,10} | {1,-30} | {2,-10}", answer, columnarray[v], valuearray[v]);
                    }
                    Console.WriteLine();
                }
                Console.Write("Enter Site ID to search or \"exit\" to close the program: ");
                answer = Console.ReadLine();
            }

            //close.
            if (answer == "exit")
            {
                Environment.Exit(0);
            }
        }

    }
}
