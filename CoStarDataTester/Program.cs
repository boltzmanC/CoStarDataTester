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
    public class Program
    {
        static void Main(string[] args)
        {
            // settings
            FunctionTools.ConsoleSize();
            FunctionTools.Introduction();

            // start
            ProcessorStartMenu();


            //Costar US
            //USAnnualDeliverableTester.CostarPointsFiles(); //add check for unique variable IDS first.
            //USAnnualDeliverableTester.CostarPointsFilesManualCheck();

            //AreaFilesTester.CostarAreaFiles();
            //AreaFilesTester.CostarAreaFilesManualCheck();
            //AreaFilesTester.CostarAreaFilesSummaries(); //sums total variable values.. should be less than or eqaul to national totals.


            // CoStar Canada
            //NightlyFeedTester.CostarTestGapFile();
            //NightlyFeedTester.CostarManualTestNightlyfile();
            //NightlyFeedTester.CostarReformatNightlyFeedOutput();

            //nightly feed reformat
            //ConvertNightlyFeed.CoStarMultiFileReformatNightlyFeed();
        }


        public static void ProcessorStartMenu()
        {
            // start menu
            bool done = false;

            while (done != true)
            {
                //what would you like to do.

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("Select a process:");
                Console.WriteLine();
                Console.WriteLine("US Tests:");
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Open Confluence Page.");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Automated Check of CoStar US Point files.");
                Console.WriteLine("{0,5}{1,-10}", "3. ", "Manual Check of CoStar US Point files.");

                Console.WriteLine();
                Console.WriteLine("Nightly Feed Tests:");
                Console.WriteLine("{0,5}{1,-10}", "4. ", "Automated Check of CoStar Nightly Feed.");
                Console.WriteLine("{0,5}{1,-10}", "5. ", "Manual Check of CoStar Nightly Feed.");
                Console.WriteLine("{0,5}{1,-10}", "6. ", "Reformat CoStar Nightly Feed file.");
                Console.WriteLine("{0,5}{1,-10}", "7. ", "Multi-File Reformat Nightly Feed file.");

                Console.WriteLine();
                Console.WriteLine("Area Files Tests");
                Console.WriteLine("{0,5}{1,-10}", "8. ", "Automated Check of CoStar Area File.");
                Console.WriteLine("{0,5}{1,-10}", "9. ", "Manual Check of CoStar Area File.");
                Console.WriteLine("{0,5}{1,-10}", "10. ", "Area File Summary.");

                Console.WriteLine();
                Console.WriteLine("{0,5}{1,-10}", "more. ", "Additional test cases");
                Console.WriteLine("{0,5}{1,-10}", "exit. ", "End Program.");
                Console.WriteLine();
                Console.ResetColor();

                //Get user input.
                Console.Write("Selection: ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        System.Diagnostics.Process.Start("https://confluence.nexgen.neustar.biz/display/E1/CoStar+Deliverables");
                        break;


                    // US
                    case "2":
                        USAnnualDeliverableTester.CostarPointsFiles();
                        break;

                    case "3":
                        USAnnualDeliverableTester.CostarPointsFilesManualCheck();
                        break;


                    // nightly feed
                    case "4":
                        NightlyFeedTester.CostarAutoTestNightlyFeedFile();
                        break;

                    case "5":
                        NightlyFeedTester.CostarManualTestNightlyfile();
                        break;

                    case "6":
                        NightlyFeedTester.CostarReformatNightlyFeedOutput();
                        break;

                    case "7":
                        NightlyFeedTester.CoStarMultiFileReformatNightlyFeed();
                        break;


                    // Area files
                    case "8":
                        AreaFilesTester.CostarAreaFiles();
                        break;

                    case "9":
                        AreaFilesTester.CostarAreaFilesManualCheck();
                        break;

                    case "10":
                        AreaFilesTester.CostarAreaFilesSummaries();
                        break;


                    // addtional test cases
                    case "more":
                        AdditionalTestCases.AdditionalTestCasesMenu();
                        break;

                    // exit
                    case "exit":
                        done = true; // we are done but manually exit app here.
                        FunctionTools.ExitApp();
                        break;

                    default:
                        Console.WriteLine("not a valid input");
                        ProcessorStartMenu();
                        break;

                }
            }
        }

    }
}
