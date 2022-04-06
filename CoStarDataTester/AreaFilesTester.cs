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
    public class AreaFilesTester
    {

        //todo.
        // ADD unique FILE check for CostarAreaFiles(). this allows for MULTI file testing 

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
