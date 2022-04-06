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
    public class AdditionalTestCases
    {
        public static void AdditionalTestCasesMenu()
        {
            bool done = false;

            while (done != true)
            {
                //what would you like to do.

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine();
                Console.WriteLine("Select a process:");
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Count total points in file.");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Remove duplicate points from target file.");
                Console.WriteLine("{0,5}{1,-10}", "3. ", "Remove target column from target file.");
                Console.WriteLine("{0,5}{1,-10}", "4. ", "Sum data columns.");
                Console.WriteLine("{0,5}{1,-10}", "back ", "back.");
                Console.WriteLine("{0,5}{1,-10}", "exit ", "exit.");
                //Console.WriteLine("{0,5}{1,-10}", "7. ", "");
                //Console.WriteLine("{0,5}{1,-10}", "8. ", "");
                Console.WriteLine();
                Console.ResetColor();

                //Get user input.
                Console.Write("Selection: ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        CostarTotalPointsCount();
                        break;

                    case "2":
                        CostarPointsDupeRemove();
                        break;

                    case "3":
                        CostarRemoveColumnFromFile();
                        break;

                    case "4":
                        CostarSumDataColumns();
                        break;

                    

                    // other options
                    case "back":
                        Program.ProcessorStartMenu();
                        break;

                    case "exit":
                        done = true; // we are done but manually exit app here.
                        FunctionTools.ExitApp();
                        break;

                    default:
                        Console.WriteLine("not a valid input");
                        AdditionalTestCasesMenu();
                        break;
                }
            }
        }

        // additional tests
        private static void CostarTotalPointsCount()
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

        private static void CostarPointsDupeRemove()
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

        private static void CostarRemoveColumnFromFile()
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
        private static void CostarSumDataColumns()
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



    }
}
