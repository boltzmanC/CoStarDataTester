using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoStarDataTester
{
    public class CanadaAnnualDeliverableTester
    {
        //CoStar Canada GAP file
        public static void CostarCanadaTestGapFile()
        {
            // test CoStar Canada Gap File.
            // Steps
            // 1. Get file paths.
            // 2. Read E1 Files (going to be smaller than the costar/deliverable file). 
            // 3. Store E1 File info in memory.
            // 4. Read CoStar File, as reading check E1 Records. 
            // 5. While PropertyId = "____" save in temp list. compare whole list values to E1 File list values.


            // file format
            //PropertyID|Latitude|Longitude|VariableID|Radius1Amount|Radius2Amount|Radius3Amount|Radius4Amount|Radius5Amount

            //CoStar Points Variable List
            HashSet<string> e1variablelist = new HashSet<string>();

            // e1 files dictionary
            Dictionary<string, Dictionary<string, List<string>>> e1gapsitesinfo = new Dictionary<string, Dictionary<string, List<string>>>();

            //Read the E1 Files.
            CostarCanadaReadE1GapFiles(e1gapsitesinfo, e1variablelist);

            // data purge
            List<string> sitestoremove = new List<string>();
            foreach (var site in e1gapsitesinfo.Keys)
            {
                foreach (var variable in e1gapsitesinfo[site].Keys)
                {
                    if (e1gapsitesinfo[site][variable].Count != 5)
                    {
                        //e1gapsitesinfo.Remove(site); // if the site does not have 5 radii for any variable. remove it from the data. this is only targeting data from e1 which pulls sites at random.
                        //break;

                        //edit 12/7/20. error occurs 'Collection was modified; enumeration operation may not execute.' removing can not occur during itterations.
                        sitestoremove.Add(site);
                    }
                }
            }

            // data purge continued.
            if (sitestoremove.Count > 0)
            {
                foreach (string site in sitestoremove)
                {
                    e1gapsitesinfo.Remove(site);
                }
            }

            // status
            Console.WriteLine("E1 files read, beginning data tests.");

            List<string> failedgapdata = new List<string>();
            int failedradii = 0;
            int sitestested = 0;

            Console.WriteLine("-Enter CoStar Gap File-");
            string gapfile = FunctionTools.GetAFile();
            char gapdeli = FunctionTools.GetDelimiter();
            char txtq = '"';

            using (StreamReader gapdata = new StreamReader(gapfile))
            {
                string header = gapdata.ReadLine();

                int radii1 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius1Amount", txtq); //returns 0 based index.
                int radii2 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius2Amount", txtq);
                int radii3 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius3Amount", txtq);
                int radii4 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius4Amount", txtq);
                int radii5 = FunctionTools.ColumnIndexNew(header, gapdeli, "Radius5Amount", txtq);

                List<int> radiivalues = new List<int>();
                radiivalues.Add(radii1);
                radiivalues.Add(radii2);
                radiivalues.Add(radii3);
                radiivalues.Add(radii4);
                radiivalues.Add(radii5);

                // settings
                Dictionary<string, Dictionary<string, List<string>>> tempsitevaluesdict = new Dictionary<string, Dictionary<string, List<string>>>();
                string storedsiteid = string.Empty;
                string notfoundsiteid = string.Empty;

                string line = string.Empty;
                while ((line = gapdata.ReadLine()) != null)
                {
                    string[] splitline = line.Split(gapdeli);

                    string linesiteid = splitline[0];
                    string linevariableid = splitline[3];

                    if (linesiteid == storedsiteid)
                    {
                        // test the row with the temp dictionary
                        if (tempsitevaluesdict[storedsiteid].ContainsKey(linevariableid))
                        {
                            // test the data.
                            // get the line values for the variable. 
                            for (int x = 0; x < tempsitevaluesdict[storedsiteid][linevariableid].Count - 1; x++)
                            {
                                string e1filenumber = tempsitevaluesdict[storedsiteid][linevariableid][x];
                                string gapfilenumber = splitline[radiivalues[x]];

                                if (gapfilenumber != e1filenumber)
                                {
                                    long costarvalue = Convert.ToInt64(Convert.ToDouble(gapfilenumber));
                                    long e1value = Convert.ToInt64(Convert.ToDouble(e1filenumber));

                                    if (costarvalue != e1value && (costarvalue + 1) != e1value && (costarvalue - 1) != e1value)
                                    {
                                        // add gap file info to list.
                                        List<string> linebuilder = new List<string>();
                                        linebuilder.Add(storedsiteid);
                                        linebuilder.Add("radii-" + (x + 1).ToString());
                                        linebuilder.Add(linevariableid);
                                        linebuilder.Add(gapfilenumber);

                                        failedgapdata.Add(string.Join(gapdeli.ToString(), linebuilder.ToArray()));
                                        failedradii++;
                                    }
                                    else
                                    {
                                        sitestested++;
                                    }
                                }
                                else
                                {
                                    sitestested++;
                                }
                            }
                        }
                    }
                    else if (linesiteid != storedsiteid && e1gapsitesinfo.ContainsKey(linesiteid))
                    {
                        // new site id encountered. 
                        // add to temp dictionary and then test.
                        storedsiteid = linesiteid;

                        if (tempsitevaluesdict.Count != 0)
                        {
                            tempsitevaluesdict.Clear();
                        }

                        List<string> listbuilder = new List<string>();
                        Dictionary<string, List<string>> dictbuilder = new Dictionary<string, List<string>>();

                        foreach (var variable in e1gapsitesinfo[storedsiteid].Keys)
                        {
                            dictbuilder.Add(variable, e1gapsitesinfo[storedsiteid][variable]);
                        }

                        tempsitevaluesdict.Add(storedsiteid, dictbuilder);

                        if (tempsitevaluesdict.ContainsKey(linevariableid))
                        {
                            // test the data.

                            // get the line values for the variable. 
                            for (int x = 0; x < tempsitevaluesdict[storedsiteid][linevariableid].Count - 1; x++)
                            {
                                string e1filenumber = tempsitevaluesdict[storedsiteid][linevariableid][x];
                                string gapfilenumber = splitline[radiivalues[x]];

                                if (gapfilenumber != e1filenumber)
                                {
                                    long costarvalue = Convert.ToInt64(Convert.ToDouble(gapfilenumber));
                                    long e1value = Convert.ToInt64(Convert.ToDouble(e1filenumber));

                                    if (costarvalue != e1value && (costarvalue + 1) != e1value && (costarvalue - 1) != e1value)
                                    {
                                        // add gap file info to list.
                                        List<string> linebuilder = new List<string>();
                                        linebuilder.Add(storedsiteid);
                                        linebuilder.Add("radii-" + (x + 1).ToString());
                                        linebuilder.Add(linevariableid);
                                        linebuilder.Add(gapfilenumber);

                                        failedgapdata.Add(string.Join(gapdeli.ToString(), linebuilder.ToArray()));
                                        failedradii++;
                                    }
                                }
                            }
                        }

                        sitestested++;

                        //memory management.
                        e1gapsitesinfo.Remove(linesiteid);
                    }
                    else
                    {
                        // did not find the current id on the line.
                        // clear all data.
                        tempsitevaluesdict.Clear();
                        storedsiteid = string.Empty;
                    }
                }
            }

            if (failedradii > 0)
            {
                string failedlist = FunctionTools.GetDesktopDirectory() + "\\failedsites.txt";
                using (StreamWriter failures = new StreamWriter(failedlist))
                {
                    //header
                    //char newdeli = '|';
                    failures.WriteLine("PropertyID|Radii|VariableID|RadiusAmount");

                    foreach (var p in failedgapdata)
                    {
                        failures.WriteLine(p);
                    }
                }
            }

            Console.WriteLine("Failed Radii Values - {0}", failedradii);
            Console.WriteLine("Number of sites tested - {0}", sitestested / 507); // sites tested is variables * number of sites. 507 variables are tested for canada.
            Console.Write("Press any key to continue...");
            Console.ReadLine();
        }

        public static void CostarCanadaReadE1GapFiles(Dictionary<string, Dictionary<string, List<string>>> e1gapsitesinfo, HashSet<string> e1variablelist)
        {
            //bool allmatch = true;
            //HashSet<string> sitesnotfullyexported = new HashSet<string>();

            Console.Write("Drag and Drop E1 123radii report outputs Folder Here: ");
            string directory123 = Console.ReadLine();
            string[] e1filepaths123 = Directory.GetFiles(@directory123);

            Console.Write("Drag and Drop E1 510radii report outputs Folder Here: ");
            string directory510 = Console.ReadLine();
            string[] e1filepaths510 = Directory.GetFiles(@directory510);

            char e1deli = FunctionTools.GetDelimiter();

            foreach (var file in e1filepaths123)
            {
                using (StreamReader readfile = new StreamReader(file))
                {
                    // skip standard report output header lines.
                    readfile.ReadLine();
                    readfile.ReadLine();

                    string header = readfile.ReadLine();
                    header = header.Replace("\"", string.Empty);
                    string[] headersplit = header.Split(e1deli);

                    foreach (var variable in headersplit.Skip(1)) //skip property ID.
                    {
                        e1variablelist.Add(variable); // only uniques can be added.
                    }

                    string line = string.Empty;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        string[] splitline = line.Replace("\"", string.Empty).Split(e1deli);
                        string siteid = splitline[0];

                        string line2 = readfile.ReadLine();
                        string line3 = readfile.ReadLine();

                        string[] splitline2 = line2.Replace("\"", string.Empty).Split(e1deli);
                        string[] splitline3 = line3.Replace("\"", string.Empty).Split(e1deli);

                        if (!e1gapsitesinfo.ContainsKey(siteid))
                        {
                            if ((splitline2[0] == siteid) && (splitline3[0] == siteid))
                            {
                                Dictionary<string, List<string>> tempdict = new Dictionary<string, List<string>>();

                                foreach (var variable in headersplit.Skip(1))
                                {
                                    List<string> templist = new List<string>();
                                    int index = Array.IndexOf(headersplit, variable);

                                    templist.Add(splitline[index]);
                                    templist.Add(splitline2[index]);
                                    templist.Add(splitline3[index]);

                                    tempdict.Add(variable, templist);
                                }

                                e1gapsitesinfo.Add(siteid, tempdict);
                            }
                        }
                        else
                        {
                            foreach (var variable in headersplit.Skip(1))
                            {
                                if (!e1gapsitesinfo[siteid].ContainsKey(variable)) //siteid already in file. add additional values
                                {
                                    List<string> templist = new List<string>();
                                    int index = Array.IndexOf(headersplit, variable);

                                    templist.Add(splitline[index]);
                                    templist.Add(splitline2[index]);
                                    templist.Add(splitline3[index]);

                                    e1gapsitesinfo[siteid].Add(variable, templist);
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("{0} - file read", FunctionTools.GetFileNameWithoutExtension(file));
            }

            foreach (var file in e1filepaths510)
            {
                using (StreamReader readfile = new StreamReader(file))
                {
                    // skip standard report output header lines.
                    readfile.ReadLine();
                    readfile.ReadLine();

                    string header = readfile.ReadLine();
                    header = header.Replace("\"", string.Empty);
                    string[] headersplit = header.Split(e1deli);

                    foreach (var variable in headersplit.Skip(1))
                    {
                        e1variablelist.Add(variable); // only uniques can be added.
                    }

                    string line = string.Empty;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        string[] splitline = line.Replace("\"", string.Empty).Split(e1deli);

                        string siteid = splitline[0];

                        if (e1gapsitesinfo.ContainsKey(siteid))
                        {
                            string line2 = readfile.ReadLine();
                            string[] splitline2 = line2.Replace("\"", string.Empty).Split(e1deli);

                            if ((splitline2[0] == siteid))
                            {
                                foreach (var variable in headersplit)
                                {
                                    int index = Array.IndexOf(headersplit.ToArray(), variable);

                                    if (e1gapsitesinfo[siteid].ContainsKey(variable) && e1gapsitesinfo[siteid][variable].Count == 3)
                                    {
                                        e1gapsitesinfo[siteid][variable].Add(splitline[index]);
                                        e1gapsitesinfo[siteid][variable].Add(splitline2[index]);
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("{0} - file read", FunctionTools.GetFileNameWithoutExtension(file));
            }
        }

        public static void CostarCanadaManualTestGapfile()
        {

            // add variable lists so things are in order.

            // gap file dictionary.
            Dictionary<string, Dictionary<string, List<string>>> gapsitesinfo = new Dictionary<string, Dictionary<string, List<string>>>();
            HashSet<string> costarvariablelist = new HashSet<string>();

            Console.WriteLine("-Enter CoStar Gap File-");
            string gapfile = FunctionTools.GetAFile();
            char gapdeli = FunctionTools.GetDelimiter();

            Console.WriteLine("-Enter CoStar Variable File-");
            string variablefile = FunctionTools.GetAFile();
            List<string> tofindvariablelist = new List<string>();

            using (StreamReader consumer = new StreamReader(variablefile))
            {
                string line = string.Empty;

                while ((line = consumer.ReadLine()) != null)
                {
                    tofindvariablelist.Add(line.Trim());
                }
            }

            using (StreamReader gapdata = new StreamReader(gapfile))
            {
                string header = gapdata.ReadLine();

                string line = string.Empty;
                while ((line = gapdata.ReadLine()) != null)
                {
                    string[] splitline = line.Split(gapdeli);

                    // propertykey = property id splitline[0];
                    // variablekey = splitfline[3];

                    string propertykey = splitline[0];
                    string variablekey = splitline[3];

                    // if site not found, add it and clear variable list dict
                    if (!gapsitesinfo.ContainsKey(propertykey))
                    {
                        costarvariablelist.Clear();

                        Dictionary<string, List<string>> tempdict = new Dictionary<string, List<string>>();
                        List<string> templist = new List<string>();

                        if (!costarvariablelist.Contains(variablekey) && tofindvariablelist.Contains(variablekey))
                        {
                            costarvariablelist.Add(variablekey);

                            for (int x = 4; x < splitline.Length; x++)
                            {
                                templist.Add(splitline[x]);
                            }

                            tempdict.Add(variablekey, templist);
                            gapsitesinfo.Add(propertykey, tempdict);
                        }
                    }
                    else if (gapsitesinfo.ContainsKey(propertykey) && tofindvariablelist.Contains(variablekey))
                    {
                        List<string> templist = new List<string>();

                        if (!costarvariablelist.Contains(variablekey))
                        {
                            costarvariablelist.Add(variablekey);

                            for (int x = 4; x < splitline.Length; x++)
                            {
                                templist.Add(splitline[x]);
                            }

                            gapsitesinfo[propertykey].Add(variablekey, templist);
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
                    foreach (var variable in tofindvariablelist)
                    {
                        // output id | variable | r
                        Console.WriteLine("{0,10} | {1,-35} | {2,-10}", answer, variable, gapsitesinfo[answer][variable][radiitotest]);
                    }
                }
                else
                {
                    Console.WriteLine("Site ID not found.");
                }

                Console.WriteLine();
                Console.Write("Enter Site ID to search or \"exit\" to close the program: ");
                answer = Console.ReadLine().Trim().Replace("\"", string.Empty);
            }
        }

        public static void CostarCanadaReformatNightlyFeedOutput()
        {
            // Reformat the output of the CoStar Nightly feed files
            // Will work for either nightly files or "gap deliverable files". 
            // Output will be Separate files based on Variable Lists.
            // TO CHANGE FOR US FILES just change variable lists.

            // file format
            //PropertyID|Latitude|Longitude|VariableID|Radius1Amount|Radius2Amount|Radius3Amount|Radius4Amount|Radius5Amount

            Console.WriteLine("-Enter CoStar Gap File-");
            string gapfile = FunctionTools.GetAFile();
            char gapdeli = FunctionTools.GetDelimiter();
            char txtq = '"';

            Console.WriteLine("-Enter File Path to Variable List files-");
            string variablepaths = Console.ReadLine();
            string[] variablefilepaths = Directory.GetFiles(@variablepaths);


            foreach (var file in variablefilepaths)
            {
                Console.WriteLine();
                string outputname = FunctionTools.GetFileNameWithoutExtension(file);
                Console.WriteLine("Reading {0}.", outputname);

                List<string> variables = new List<string>();

                using (StreamReader consumer = new StreamReader(file))
                {
                    string line = string.Empty;

                    while ((line = consumer.ReadLine()) != null)
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
                    }
                }

                // output DICT values to new file.
                string newfile = FunctionTools.GetDesktopDirectory() + "\\" + outputname + "_output.txt";

                using (StreamWriter outfile = new StreamWriter(newfile))
                {
                    Console.WriteLine("Generating output for - {0}.", outputname);
                    char e1deli = ',';

                    // Costar Data Format: 1,2,3,5,10 mile radii
                    //    s1 | r1 | data
                    //    s1 | r2 | data
                    //    s1 | r3 | data
                    //    s1 | r4 | data
                    //    s1 | r5 | data

                    // read and reformat gap file.
                    string[] headertofill = { "AREA_ID", "ID", "RING", "RING_DEFN", "LAT", "LON" };

                    //create file header.
                    List<string> headerbuilder = new List<string>();
                    foreach (var item in headertofill)
                    {
                        headerbuilder.Add(item);
                    }

                    foreach (var item in variables) //target deliverable variables.
                    {
                        headerbuilder.Add(item);
                    }

                    outfile.WriteLine(string.Join(e1deli.ToString(), headerbuilder.ToArray()));

                    foreach (var lookupsiteid in sitedata.Keys) // itterate over the site IDs. 
                    {
                        for (int x = 0; x <= 4; x++) // for each radii 0-4
                        {
                            // build the line.
                            List<string> linebuilder = new List<string>();


                            linebuilder.Add(lookupsiteid + "_" + (x + 1)); //areaID

                            linebuilder.Add(lookupsiteid); //siteID

                            linebuilder.Add((x + 1).ToString()); //ring

                            //ring definition
                            string ringdefn = string.Empty;
                            if (x == 3)
                            {
                                ringdefn = "5";
                            }
                            else if (x == 4)
                            {
                                ringdefn = "10";
                            }
                            else
                            {
                                ringdefn = (x + 1).ToString();
                            }
                            linebuilder.Add(ringdefn);

                            linebuilder.Add(sitelatlondata[lookupsiteid][0]); // lat

                            linebuilder.Add(sitelatlondata[lookupsiteid][1]); // lon

                            foreach (var variable in variables) // for each variable using the list to make the header to verify their order.
                            {
                                if (sitedata[lookupsiteid].ContainsKey(variable))
                                {
                                    linebuilder.Add(sitedata[lookupsiteid][variable][x]);
                                }
                                else
                                {
                                    Console.WriteLine("{0} - siteid. {1} - variable missing...", lookupsiteid, variable);
                                }
                            }

                            // output the line to the file.
                            outfile.WriteLine(string.Join(e1deli.ToString(), linebuilder.ToArray()));
                        }
                    }
                }

                //clear the dicts.
                sitedata.Clear();
                sitelatlondata.Clear();


            }


        }



    }
}
