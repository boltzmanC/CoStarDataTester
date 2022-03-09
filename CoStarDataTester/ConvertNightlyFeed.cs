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
    public class ConvertNightlyFeed
    {
        // GAP file format
        //PropertyID|Latitude|Longitude|VariableID|Radius1Amount|Radius2Amount|Radius3Amount|Radius4Amount|Radius5Amount


        public static void CoStarMultiFileReformatNightlyFeed() // need to read 2k sites then write and clear cache.
        {
            Console.WriteLine("Enter CoStar Nightly Feed File: ");
            string gapfile = FunctionTools.GetAFile();
            char gapdeli = FunctionTools.GetDelimiter();
            char txtq = '"';

            Console.WriteLine("Enter File Path to Variable files: ");
            string variablefile = FunctionTools.GetAFile();

            //read variable file
            List<string> variables = new List<string>();

            Console.WriteLine();
            string outputname = FunctionTools.GetFileNameWithoutExtension(variablefile);
            Console.WriteLine("Reading {0}.", outputname);

            using (StreamReader readvariables = new StreamReader(variablefile))
            {
                string line = string.Empty;

                while ((line = readvariables.ReadLine()) != null)
                {
                    variables.Add(line.Trim());
                }
            }


            // site deliverable data
            Dictionary<string, Dictionary<string, List<string>>> sitedata = new Dictionary<string, Dictionary<string, List<string>>>();

            // site lat lon
            Dictionary<string, List<string>> sitelatlondata = new Dictionary<string, List<string>>();

            using (StreamReader gapdata = new StreamReader(gapfile))
            {
                // output to multifiles
                const int sitesplitamount = 2000;
                int filecount = 0;

                //begin read
                string header = gapdata.ReadLine();

                int radii1 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius1Amount", txtq); //returns 0 based index.
                int radii2 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius2Amount", txtq);
                int radii3 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius3Amount", txtq);
                int radii4 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius4Amount", txtq);
                int radii5 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius5Amount", txtq);
                int propertyid = FunctionTools.ColumnIndexNew(header, gapdeli, "PropertyID", txtq);
                int variableid = FunctionTools.ColumnIndexNew(header, gapdeli, "VariableID", txtq);
                int latindex = FunctionTools.ColumnIndexNew(header, gapdeli, "Latitude", txtq);
                int lonindex = FunctionTools.ColumnIndexNew(header, gapdeli, "Longitude", txtq);

                List<int> radiivalues = new List<int>();
                radiivalues.Add(radii1);
                radiivalues.Add(radii2);
                radiivalues.Add(radii3);
                radiivalues.Add(radii4);
                radiivalues.Add(radii5);

                string line = string.Empty;
                while ((line = gapdata.ReadLine()) != null)
                {
                    string[] splitline = line.Split(gapdeli);
                    string siteid = splitline[propertyid];
                    string linevariable = splitline[variableid];

                    if (variables.Contains(linevariable) && !sitedata.ContainsKey(siteid)) // doesnt contain siteid or variableid
                    {
                        Dictionary<string, List<string>> tempvariablevalues = new Dictionary<string, List<string>>();
                        List<string> tempradiivalues = new List<string>();

                        foreach (int radii in radiivalues)
                        {
                            tempradiivalues.Add(splitline[radii]);
                        }

                        // add variable and radii values to temp dict.
                        tempvariablevalues.Add(linevariable, tempradiivalues);

                        // add NEW site and line variable to parent dict.
                        sitedata.Add(siteid, tempvariablevalues);
                    }
                    else if (variables.Contains(linevariable) && sitedata.ContainsKey(siteid) && !sitedata[siteid].ContainsKey(linevariable)) // contains siteid, doesnt contain variableid
                    {
                        List<string> tempradiivalues = new List<string>();

                        foreach (int radii in radiivalues)
                        {
                            tempradiivalues.Add(splitline[radii]);
                        }

                        // add NEW line variable to parent dict.
                        sitedata[siteid].Add(linevariable, tempradiivalues);
                    }

                    if (!sitelatlondata.ContainsKey(siteid))
                    {
                        List<string> latlons = new List<string>();
                        latlons.Add(splitline[latindex]);
                        latlons.Add(splitline[latindex]);

                        // save the site id and lat lon
                        sitelatlondata.Add(siteid, latlons);
                    }

                    // write
                    if (sitedata.Count == sitesplitamount + 1)
                    {
                        //temp dict
                        Dictionary<string, Dictionary<string, List<string>>> tempsitedata = new Dictionary<string, Dictionary<string, List<string>>>();

                        foreach (var s in sitedata.Keys)
                        {
                            if (sitedata[s].Count == variables.Count)
                            {
                                tempsitedata.Add(s, sitedata[s]);
                            }
                        }

                        //new file.
                        filecount++;

                        // output file definitions
                        string outputfile = Path.GetDirectoryName(gapfile) + "\\" + outputname + $"_output_{filecount}.txt";
                        char outputfiledelimiter = ',';

                        //write out 2k sites to new file.
                        WriteOutReadSiteData(tempsitedata, sitelatlondata, variables, outputfile, outputfiledelimiter);

                        //clear dictionaries
                        foreach (var s in tempsitedata.Keys)
                        {
                            sitedata.Remove(s);
                            sitelatlondata.Remove(s);
                        }
                    }
                }

                // if end of file is reached.
                if (line == null)
                {
                    Console.WriteLine();
                    Console.WriteLine("EOF reached.");
                    Console.WriteLine($"Sites left to write: {sitedata.Count}");

                    //temp dict
                    Dictionary<string, Dictionary<string, List<string>>> tempsitedata = new Dictionary<string, Dictionary<string, List<string>>>();

                    foreach (var s in sitedata.Keys)
                    {
                        if (sitedata[s].Count == variables.Count)
                        {
                            tempsitedata.Add(s, sitedata[s]);
                        }
                    }

                    //new file.
                    filecount++;

                    // output file definitions
                    string outputfile = Path.GetDirectoryName(gapfile) + "\\" + outputname + $"_output_{filecount}.txt";
                    char outputfiledelimiter = ',';

                    //write out 2k sites to new file.
                    WriteOutReadSiteData(tempsitedata, sitelatlondata, variables, outputfile, outputfiledelimiter);

                    //clear dictionaries
                    //foreach (var s in tempsitedata.Keys)
                    //{
                    //    sitedata.Remove(s);
                    //    sitelatlondata.Remove(s);
                    //}
                }
            }
        }


        private static void WriteOutReadSiteData(Dictionary<string, Dictionary<string, List<string>>> sitedata, Dictionary<string, List<string>> sitelatlondata, List<string> variables, string outputfile, char outputfiledelimiter)
        {
            // file header.
            List<string> headerbuilder = new List<string>();

            string[] headertofill = { "AREA_ID", "ID", "RING", "RING_DEFN", "LAT", "LON" };

            foreach (var item in headertofill)
            {
                headerbuilder.Add(item);
            }

            foreach (var item in variables) //target deliverable variables.
            {
                headerbuilder.Add(item);
            }

            //write header
            File.AppendAllText(outputfile, string.Join(outputfiledelimiter.ToString(), headerbuilder.ToArray()) + Environment.NewLine); 

            foreach (var s in sitedata.Keys) //loop through keys
            {
                //latlon
                string lat = sitelatlondata[s][0]; //lat
                string lon = sitelatlondata[s][1]; // lon

                // get current siteidinfo
                Dictionary<string, List<string>> tempsitedict = sitedata[s]; //all site info for target site

                for (int x = 0; x <= 4; x++) // for each radii 0-4. these will be the lines that we output.
                {
                    BuildSiteOutputLine(s, outputfile, outputfiledelimiter, tempsitedict, variables, lat, lon, x);
                }
            }

            // show counts.
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"File created: {outputfile}");
            Console.WriteLine($"Site written: {sitedata.Count}");
            Console.WriteLine($"Variables: {variables.Count}");
            Console.WriteLine();
            Console.ResetColor();
        }


        private static void BuildSiteOutputLine(string siteid, string filepath, char delimiter, Dictionary<string, List<string>> siteinfo, List<string> variables, string lat, string lon, int radii)
        {
            // build the line.
            List<string> linebuilder = new List<string>();

            linebuilder.Add(siteid + "_" + (radii + 1)); //areaID

            linebuilder.Add(siteid); //siteID

            linebuilder.Add((radii + 1).ToString()); //ring

            //ring definition
            string ringdefn = string.Empty;
            if (radii == 3)
            {
                ringdefn = "5";
            }
            else if (radii == 4)
            {
                ringdefn = "10";
            }
            else
            {
                ringdefn = (radii + 1).ToString();
            }
            linebuilder.Add(ringdefn);

            linebuilder.Add(lat); // lat

            linebuilder.Add(lon); // lon

            foreach (var variable in variables) // for each variable using the list to make the header to verify their order.
            {
                if (siteinfo.ContainsKey(variable))
                {
                    linebuilder.Add(siteinfo[variable][radii]);
                }
                else
                {
                    Console.WriteLine("{0} - siteid. {1} - variable missing...", siteid, variable);
                }
            }

            // output the line to the file.
            File.AppendAllText(filepath, string.Join(delimiter.ToString(), linebuilder.ToArray()) + Environment.NewLine);
        }



    }
}
