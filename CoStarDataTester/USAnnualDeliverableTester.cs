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

        public static void CostarPointsFilesManualCheck()
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

        public static void CoStarUSManualTestGapFile()
        {
            Console.Write("Drag and Drop the variable group folder here: ");
            string bussumdirectory = Console.ReadLine();
            //Console.Write("Drag and Drop the CEX folder here: ");
            //string cexdirectory = Console.ReadLine();
            //Console.Write("Drag and Drop the DMGRA folder here: ");
            //string dmgradirectory = Console.ReadLine();
            //Console.Write("Drag and Drop the DMGRB folder here: ");
            //string dmgrbdirectory = Console.ReadLine();

            //string[] bussumfilepaths = Directory.GetFiles(bussumdirectory);
            //string[] cexfilepaths = Directory.GetFiles(cexdirectory);
            //string[] dmgrafilepaths = Directory.GetFiles(dmgradirectory);
            //string[] dmgrbfilepaths = Directory.GetFiles(dmgrbdirectory);

            //List<string>allfilepaths = new List<string>();
            //allfilepaths.AddRange(Directory.GetFiles(bussumdirectory));
            //allfilepaths.AddRange(Directory.GetFiles(cexdirectory));
            //allfilepaths.AddRange(Directory.GetFiles(dmgradirectory));
            //allfilepaths.AddRange(Directory.GetFiles(dmgrbdirectory));

            string[] costarfilepaths = Directory.GetFiles(bussumdirectory);//allfilepaths.ToArray();

            char deli = FunctionTools.GetDelimiter();
            //string[] costarfilepaths = Directory.GetFiles(costardirectory);

            // gap file dictionary.
            Dictionary<string, Dictionary<string, List<string>>> gapsitesinfo = new Dictionary<string, Dictionary<string, List<string>>>();
            HashSet<string> variablelist = new HashSet<string>();

            //read each file....
            foreach (var file in costarfilepaths)
            {
                // save all site info to dictionary

                using (StreamReader readfile = new StreamReader(file))
                {
                    string header = readfile.ReadLine();
                    header = header.Replace("\"", string.Empty);
                    string[] headersplit = header.Split(deli);
                    headersplit = headersplit.Skip(6).ToArray(); //remove area_id, id, ring, ring_defn, lat, lon

                    List<string> variablecolumns = new List<string>();

                    foreach (var variable in headersplit)
                    {
                        variablelist.Add(variable); // only uniques can be added.
                        variablecolumns.Add(variable); // current file variables, can use index of these as they are in order of file.
                    }

                    string line = string.Empty;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        line = line.Replace("\"", string.Empty);
                        string[] splitline = line.Split(deli);

                        string propertykey = splitline[1];

                        if (!gapsitesinfo.ContainsKey(propertykey))
                        {
                            string line2 = readfile.ReadLine();
                            string line3 = readfile.ReadLine();
                            string line4 = readfile.ReadLine();
                            string line5 = readfile.ReadLine();
                            line2 = line2.Replace("\"", string.Empty);
                            line3 = line3.Replace("\"", string.Empty);
                            line4 = line4.Replace("\"", string.Empty);
                            line5 = line5.Replace("\"", string.Empty);

                            string[] splitline2 = line2.Split(deli);
                            string[] splitline3 = line3.Split(deli);
                            string[] splitline4 = line4.Split(deli);
                            string[] splitline5 = line5.Split(deli);

                            if ((splitline2[1] == propertykey) && (splitline3[1] == propertykey) && (splitline5[1] == propertykey) && (splitline4[1] == propertykey))
                            {
                                Dictionary<string, List<string>> tempdict = new Dictionary<string, List<string>>();
                                string[] variablearray = variablecolumns.ToArray();

                                foreach (var v in variablecolumns)
                                {
                                    List<string> templist = new List<string>();
                                    int index = Array.IndexOf(variablearray, v);

                                    int valueindex = index + 6;

                                    templist.Add(splitline[valueindex]);
                                    templist.Add(splitline2[valueindex]);
                                    templist.Add(splitline3[valueindex]);
                                    templist.Add(splitline4[valueindex]);
                                    templist.Add(splitline5[valueindex]);

                                    tempdict.Add(v, templist);
                                }

                                gapsitesinfo.Add(propertykey, tempdict);
                            }
                            else
                            {
                                Console.WriteLine("E1 keys do not match");
                            }
                        }
                        else
                        {
                            string line2 = readfile.ReadLine();
                            string line3 = readfile.ReadLine();
                            string line4 = readfile.ReadLine();
                            string line5 = readfile.ReadLine();
                            line2 = line2.Replace("\"", string.Empty);
                            line3 = line3.Replace("\"", string.Empty);
                            line4 = line4.Replace("\"", string.Empty);
                            line5 = line5.Replace("\"", string.Empty);

                            string[] splitline2 = line2.Split(deli);
                            string[] splitline3 = line3.Split(deli);
                            string[] splitline4 = line4.Split(deli);
                            string[] splitline5 = line5.Split(deli);

                            if ((splitline2[1] == propertykey) && (splitline3[1] == propertykey) && (splitline5[1] == propertykey) && (splitline4[1] == propertykey))
                            {
                                Dictionary<string, List<string>> tempdict = new Dictionary<string, List<string>>();
                                string[] variablearray = variablecolumns.ToArray();

                                foreach (var variablekey in variablecolumns)
                                {
                                    if (!gapsitesinfo[propertykey].ContainsKey(variablekey))
                                    {
                                        List<string> templist = new List<string>();
                                        int index = Array.IndexOf(variablearray, variablekey);

                                        int valueindex = index + 1;

                                        templist.Add(splitline[valueindex]);
                                        templist.Add(splitline2[valueindex]);
                                        templist.Add(splitline3[valueindex]);

                                        tempdict.Add(variablekey, templist);
                                    }
                                }

                                foreach (var variablekey in tempdict.Keys)
                                {
                                    gapsitesinfo[propertykey].Add(variablekey, tempdict[variablekey]);
                                }

                            }
                            else
                            {
                                Console.WriteLine("123 - E1 keys do not match");
                            }
                        }
                    }
                }
            }

            // user input
            string answer = string.Empty;
            Console.Write("Enter Site ID to search or \"exit\" to close the program: ");
            answer = Console.ReadLine().Trim().Replace("\"", string.Empty);

            while (answer != "exit")
            {
                // radii to check
                Console.Write("Enter Site Radii to test (1-5): ");
                string radiientry = Console.ReadLine().Trim();

                int radiitotest = 0;
                Int32.TryParse(radiientry, out radiitotest);
                radiitotest -= 1; // 0 - 4

                if (gapsitesinfo.ContainsKey(answer))
                {
                    foreach (var variable in variablelist)
                    {
                        // output id | variable | r
                        Console.WriteLine("{0,10} | {1,-35} | {2,-10}", answer, variable, gapsitesinfo[answer][variable][radiitotest]);
                    }
                }

                Console.WriteLine();
                Console.Write("Enter Site ID to search or \"exit\" to close the program: ");
                answer = Console.ReadLine().Trim().Replace("\"", string.Empty);
            }
        }

        public static void CostarTotalPointsCount()
        {
            Console.Write("Drag and Drop Costar Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();

            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            char costardeli = FunctionTools.GetDelimiter();

            Dictionary<string, string> totalsites = new Dictionary<string, string>();
            Dictionary<string, string> totalsiteradii = new Dictionary<string, string>();

            int costarfilesprocesses = 0;
            int totalnumsites = 0;
            int totalnumsiteradii = 0;
            int dupechecksitecount = 0;

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

                    while ((readline = costarfile.ReadLine()) != null)
                    {
                        // Find radii number and ID.
                        readline = readline.Replace("\"", string.Empty);
                        string[] splitreadline = readline.Split(costardeli);

                        string siteidwithring = splitreadline[0];
                        string siteid = splitreadline[1];   // consistent across deliverables. no need to change.
                                                            //string ring = splitreadline[2]; //this is used to find the radii dictionaries.
                                                            //int ringnumber = 0;
                                                            //bool ringparse = Int32.TryParse(ring, out ringnumber); //get radii number 1-5.

                        if (!totalsites.ContainsKey(siteid))
                        {
                            totalsites.Add(siteid, "1");
                            totalnumsites++;
                        }

                        if (!totalsiteradii.ContainsKey(siteidwithring))
                        {
                            totalsiteradii.Add(siteidwithring, "1");
                            totalnumsiteradii++;
                        }
                        else
                        {
                            dupechecksitecount++;
                        }
                    }
                }

                costarfilesprocesses++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\rCostar Files Processed - {0}.", costarfilesprocesses);
                Console.ResetColor();

            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Total Sites - {0}", totalnumsites);
            Console.WriteLine("Total Radii - {0}", totalnumsiteradii);
            Console.WriteLine("Total Dupes - {0}", dupechecksitecount);
            Console.WriteLine();
        }

        public static void CostarPointsDupeRemove()
        {
            Console.Write("Drag and Drop Costar Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();

            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            char costardeli = FunctionTools.GetDelimiter();

            Dictionary<string, string> records = new Dictionary<string, string>();

            int costarfilesprocesses = 0;
            int totalnumsites = 0;
            int totalnumsiteradii = 0;
            int dupechecksitecount = 0;

            foreach (var file in costarfilepaths)
            {
                string dedupefile = FunctionTools.GetDesktopDirectory() + "\\" + FunctionTools.GetFileNameWithoutExtension(file) + "_dedupe.txt";

                Dictionary<string, string> totalsiteradii = new Dictionary<string, string>();

                string header = string.Empty;

                using (StreamReader costarfile = new StreamReader(file))
                {
                    // Example line from Costar Data File.
                    // AREA_ID,    ID,       RING, RING_DEFN, LAT,         LON,           ALCOHOLIC_BEVERAGES_CY
                    // 10000060_1, 10000060, 1,    1,         39.7577442,  -87.1055097,   18342
                    string readline = string.Empty;
                    header = costarfile.ReadLine();

                    while ((readline = costarfile.ReadLine()) != null)
                    {
                        // Find radii number and ID.
                        readline = readline.Replace("\"", string.Empty);
                        string[] splitreadline = readline.Split(costardeli);

                        string siteidwithring = splitreadline[0];
                        //string siteid = splitreadline[1];

                        if (!records.ContainsKey(siteidwithring))
                        {
                            records.Add(siteidwithring, "1");
                            totalnumsites++;
                        }

                        if (!totalsiteradii.ContainsKey(siteidwithring))
                        {
                            totalsiteradii.Add(siteidwithring, readline); //save current file info in here.
                            totalnumsiteradii++;
                        }
                        else
                        {
                            dupechecksitecount++;
                        }
                    }
                }

                using (StreamWriter writefile = new StreamWriter(dedupefile))
                {
                    writefile.WriteLine(header);
                    foreach (KeyValuePair<string, string> record in totalsiteradii)
                    {
                        writefile.WriteLine(record.Value);
                    }
                }

                costarfilesprocesses++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\rCostar Files Processed - {0}.", costarfilesprocesses);
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Total Sites - {0}", totalnumsites / 5);
            Console.WriteLine("Total Radii - {0}", totalnumsiteradii);
            Console.WriteLine("Total Dupes - {0}", dupechecksitecount);
            Console.WriteLine();
        }

        public static void CostarRemoveColumnFromFile()
        {
            //only for costar files. no txt qualifier needed.

            Console.Write("Drag and Drop Costar Points Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();

            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            char costardeli = FunctionTools.GetDelimiter();

            Console.Write("Column Name: ");
            string columntoremove = Console.ReadLine().Trim().ToUpper();

            Console.WriteLine();
            int costarfilesprocessed = 0;

            foreach (var file in costarfilepaths)
            {
                string removedcolumnfile = FunctionTools.GetDesktopDirectory() + "\\" + FunctionTools.GetFileNameWithoutExtension(file) + "_" + columntoremove + "_removed.txt";

                using (StreamWriter writefile = new StreamWriter(removedcolumnfile))
                {
                    using (StreamReader readfile = new StreamReader(file))
                    {
                        string header = readfile.ReadLine();
                        string[] headersplit = header.Split(costardeli);
                        int columntoremoveindex = FunctionTools.ColumnIndex(header, costardeli, columntoremove);
                        List<string> newheaderbuilder = new List<string>();

                        for (int x = 0; x <= headersplit.Length - 1; x++)
                        {
                            if (x != columntoremoveindex)
                            {
                                newheaderbuilder.Add(headersplit[x]);
                            }
                        }
                        writefile.WriteLine(string.Join(costardeli.ToString(), newheaderbuilder.ToArray()));

                        string line = string.Empty;
                        while ((line = readfile.ReadLine()) != null)
                        {
                            string[] splitline = line.Split(costardeli);
                            List<string> linebuilder = new List<string>();

                            for (int x = 0; x <= splitline.Length - 1; x++)
                            {
                                if (x != columntoremoveindex)
                                {
                                    linebuilder.Add(splitline[x]);
                                }
                            }

                            string newline = string.Join(costardeli.ToString(), linebuilder.ToArray());

                            writefile.WriteLine(newline);
                        }
                    }
                }

                costarfilesprocessed++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\rCostar Files Processed - {0}.", costarfilesprocessed);
                Console.ResetColor();
            }
            Console.WriteLine();
        }


        
        // CoStar Sum Columns
        public static void CostarSumDataColumns()
        {

            string file = FunctionTools.GetAFile();
            char deli = FunctionTools.GetDelimiter();
            char txtq = FunctionTools.GetTXTQualifier();

            string countsfile = FunctionTools.GetDesktopDirectory() + "\\" + FunctionTools.GetFileNameWithoutExtension(file) + "-variablesums.csv";

            Console.WriteLine();
            Console.Write("Enter column index to start summing at: ");
            string column = Console.ReadLine().Trim();
            int columnindex = Int32.Parse(column);

            //List<int> columnsums = new List<int>();

            using (StreamReader readfile = new StreamReader(file))
            {
                string header = readfile.ReadLine();
                List<string> headerlinebuilder = new List<string>();
                if (header.Contains(txtq))
                {
                    headerlinebuilder.AddRange(FunctionTools.SplitLineWithTxtQualifier(header, deli, txtq, false));
                }
                else
                {
                    headerlinebuilder.AddRange(header.Split(deli));
                }

                string[] headersplitline = headerlinebuilder.ToArray();

                //value storage
                long[] columnsums = new long[headersplitline.Length - 1]; //account for geography ID column.

                string line = string.Empty;
                while ((line = readfile.ReadLine()) != null)
                {
                    List<string> splitlinebuilder = new List<string>();
                    if (line.Contains(txtq))
                    {
                        splitlinebuilder.AddRange(FunctionTools.SplitLineWithTxtQualifier(line, deli, txtq, false));
                    }
                    else
                    {
                        splitlinebuilder.AddRange(line.Split(deli));
                    }

                    string[] splitline = splitlinebuilder.ToArray();

                    for (int x = columnindex; x <= splitline.Length - 1; x++)
                    {
                        if (!string.IsNullOrWhiteSpace(splitline[x]))
                        {
                            long value = Int64.Parse(splitline[x]);

                            columnsums[x - 1] += value;
                        }
                    }
                }

                using (StreamWriter sumfile = new StreamWriter(countsfile))
                {
                    List<string> newheader = new List<string>();
                    for (int x = columnindex; x <= headersplitline.Length - 1; x++)
                    {
                        newheader.Add(headersplitline[x]);
                    }

                    sumfile.WriteLine(string.Join(deli.ToString(), newheader.ToArray()));
                    sumfile.WriteLine(string.Join(deli.ToString(), columnsums.ToArray()));
                }
            }
        }


        // Annual Area Files Deliverable.
        public static void CostarAreaFiles()
        {
            // Assumes variables are in the same order in both sets of files.
            Console.Write("Drag and Drop Costar AREA Desktop Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();
            Console.Write("Drag and Drop E1 AREA Desktop Folder Here (CEX, BUSSUM, or DMGRA): ");
            string e1directory = Console.ReadLine();
            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            string[] e1filepaths = Directory.GetFiles(@e1directory);

            char e1deli = FunctionTools.GetDelimiter();
            char txtq = FunctionTools.GetTXTQualifier();

            Dictionary<string, List<string>> e1areas = new Dictionary<string, List<string>>();
            int areasadded = 0;
            foreach (var file in e1filepaths)
            {
                using (StreamReader e1file = new StreamReader(file))
                {
                    string line = string.Empty;
                    e1file.ReadLine();
                    e1file.ReadLine();
                    string header = e1file.ReadLine().Replace("\"", string.Empty); //get rid of header rows.

                    while ((line = e1file.ReadLine()) != null)
                    {
                        string[] splitline = FunctionTools.SplitLineWithTxtQualifier(line, e1deli, txtq, false); //line.Split(e1deli);

                        string e1key = splitline[0];
                        List<string> valuestoadd = new List<string>();
                        for (int v = 1; v <= splitline.Length - 1; v++)
                        {
                            valuestoadd.Add(splitline[v]);
                        }

                        //data starts at splitline[1].
                        if (!e1areas.ContainsKey(e1key))
                        {
                            e1areas.Add(e1key, valuestoadd);
                            areasadded++;
                        }
                    }
                }
            }

            Console.WriteLine("E1Areas read - {0}.", areasadded);

            //Costar Files.
            int areaschecked = 0;
            int areasfailed = 0;

            foreach (var file in costarfilepaths)
            {
                using (StreamReader costarfile = new StreamReader(file))
                {
                    string line = string.Empty;
                    string header = costarfile.ReadLine().Replace("\"", string.Empty); //get rid of header rows.
                    string[] headervalues = header.Split(e1deli);

                    while ((line = costarfile.ReadLine()) != null)
                    {
                        string[] splitline = FunctionTools.SplitLineWithTxtQualifier(line, e1deli, txtq, false); //line.Split(e1deli);
                        string costarkey = splitline[0];

                        if (e1areas.ContainsKey(costarkey))
                        {
                            for (int v = 1; v <= splitline.Length - 1; v++)
                            {
                                if ((e1areas[costarkey][v - 1] != splitline[v]) && (e1areas[costarkey][v - 1] != string.Empty) || (splitline[v] != string.Empty))
                                {
                                    //tests individual values that arent equal

                                    long costarvalue = Convert.ToInt64(Convert.ToDouble(e1areas[costarkey][v - 1]));
                                    long e1value = Convert.ToInt64(Convert.ToDouble(splitline[v]));

                                    if (costarvalue != e1value && (costarvalue + 1) != e1value && (costarvalue - 1) != e1value)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write("File: {0}", FunctionTools.GetFileNameWithoutExtension(file));
                                        Console.Write(" AreaID - {0}, Column - {1} Costar Value - {2}, E1 Value - {3}", costarkey, headervalues[v], splitline[v], e1areas[costarkey][v - 1]);
                                        Console.WriteLine();
                                        Console.ResetColor();
                                        areasfailed++;
                                    }
                                }
                            }
                            if (areasfailed == 0)
                            {
                                areaschecked++;
                            }
                        }
                    }

                }
            }
            Console.WriteLine("Areas checked - {0}.", areaschecked);

            Console.WriteLine();
            Console.Write("Again? (y/n): ");
            string again = Console.ReadLine().ToLower();

            if (again == "y")
            {
                CostarAreaFiles();
            }
        }

        public static void CostarAreaFilesManualCheck()
        {
            Console.WriteLine("Drag and Drop Costar AREA File Here: ");
            string costarfile = FunctionTools.GetAFile();
            char delimiter = FunctionTools.GetDelimiter();
            char txtq = FunctionTools.GetTXTQualifier();

            Dictionary<string, List<string>> costarareas = new Dictionary<string, List<string>>();
            int areasadded = 0;
            List<string> headerlist = new List<string>();

            using (StreamReader costardatafile = new StreamReader(costarfile))
            {
                // test for nick file or e1 export file.
                bool idfound = false;
                string headerline = string.Empty;

                while (idfound == false)
                {
                    string linetest = costardatafile.ReadLine().ToUpper();

                    if (linetest.Contains("AREA_ID") || linetest.Contains("GEOGRAPHY ID"))
                    {
                        idfound = true;
                        headerline = linetest;
                    }
                }

                string[] headernames = FunctionTools.LineStringToArray(headerline, txtq, delimiter);
                headerlist = headernames.ToList();

                string line = string.Empty;
                while ((line = costardatafile.ReadLine()) != null)
                {
                    string[] splitline = FunctionTools.LineStringToArray(line, txtq, delimiter);

                    //data starts at splitline[1].
                    string e1key = splitline[0];
                    List<string> valuestoadd = new List<string>();
                    for (int v = 1; v <= splitline.Length - 1; v++)
                    {
                        valuestoadd.Add(splitline[v]);
                    }

                    if (!costarareas.ContainsKey(e1key))
                    {
                        costarareas.Add(e1key, valuestoadd);
                        areasadded++;
                    }
                }
            }

            Console.WriteLine("Areas read - {0}.", areasadded);
            Console.WriteLine();

            string userentry = string.Empty;
            while (userentry != "exit")
            {
                Console.Write("Enter Area ID to search, \"exit\" to close or \"new\" to test another file: ");
                userentry = Console.ReadLine();

                //close.
                if (userentry == "exit")
                {
                    Environment.Exit(0);
                }

                if (userentry == "new")
                {
                    CostarAreaFilesManualCheck();
                }

                if (costarareas.ContainsKey(userentry))
                {
                    Console.WriteLine("File: {0}", FunctionTools.GetFileNameWithoutExtension(costarfile));

                    string[] columnarray = headerlist.ToArray();
                    //string spacer = " | ";

                    List<string> keyvalues = costarareas[userentry];
                    string[] valuearray = keyvalues.ToArray();

                    //line builder
                    for (int v = 1; v <= columnarray.Length - 1; v++)
                    {
                        //Console.WriteLine(answer + spacer + columnarray[v] + spacer + valuearray[v]);
                        string output = string.Format("{0,10} | {1,-40} | {2,20}", userentry, columnarray[v], valuearray[v - 1]);
                        Console.WriteLine(output);
                    }

                    Console.WriteLine();
                }
            }
        }

        public static void CostarAreaFilesSummaries()
        {
            Console.Write("Drag and Drop Costar AREA Desktop Folder Here (CEX, BUSSUM, or DMGRA): ");
            string costardirectory = Console.ReadLine();
            string[] costarfilepaths = Directory.GetFiles(@costardirectory);
            char e1deli = FunctionTools.GetDelimiter();

            Dictionary<string, List<string>> costarareas = new Dictionary<string, List<string>>();
            int areasadded = 0;

            //getcolumns;
            List<string> headervalues = new List<string>();
            using (StreamReader columnreader = new StreamReader(costarfilepaths[0]))
            {
                string header = columnreader.ReadLine().Replace("\"", string.Empty);
                string[] headernames = header.Split(e1deli);
                for (int v = 1; v <= headernames.Length - 1; v++)
                {
                    headervalues.Add(headernames[v]);
                }
            }

            foreach (var file in costarfilepaths)
            {
                using (StreamReader costarfile = new StreamReader(file))
                {
                    string line = string.Empty;
                    costarfile.ReadLine(); //get rid of header rows.

                    while ((line = costarfile.ReadLine()) != null)
                    {
                        line = line.Replace("\"", string.Empty);
                        string[] splitline = line.Split(e1deli);

                        string e1key = splitline[0];
                        List<string> valuestoadd = new List<string>();
                        for (int v = 1; v <= splitline.Length - 1; v++)
                        {
                            valuestoadd.Add(splitline[v]);
                        }

                        //data starts at splitline[1].
                        if (!costarareas.ContainsKey(e1key))
                        {
                            costarareas.Add(e1key, valuestoadd);
                            areasadded++;
                        }
                    }
                }
            }

            Console.WriteLine("Areas read - {0}.", areasadded);

            List<double> summaries = new List<double>();
            //double[] summaries = new double[headervalues.Count - 1];

            foreach (KeyValuePair<string, List<string>> value in costarareas)
            {
                List<string> newlist = value.Value;

                for (int v = 0; v <= newlist.Count - 1; v++)
                {
                    string tempvalue = newlist[v];
                    double valuetoadd = 0.000;
                    double.TryParse(tempvalue, out valuetoadd);

                    if (summaries.Count < newlist.Count)
                    {
                        summaries.Add(valuetoadd);
                    }
                    else
                    {
                        summaries[v] += valuetoadd;
                    }
                }
            }

            List<double> summarizedvalues = summaries.ToList();

            string newfile = FunctionTools.GetDesktopDirectory() + "\\" + FunctionTools.GetFileNameWithoutExtension(costarfilepaths[0]) + "_summaries.txt";
            using (StreamWriter outfile = new StreamWriter(newfile))
            {
                for (int v = 0; v <= summarizedvalues.Count - 1; v++)
                {
                    //label.Text = String.Format("{0:F3}", dec); // Show 3 Decimel Points

                    outfile.WriteLine("{0,10} | {1,-10}", headervalues[v], summarizedvalues[v]);
                }
            }

        }



    }
}
