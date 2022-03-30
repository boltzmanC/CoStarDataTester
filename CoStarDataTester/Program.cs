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
    class Program
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

            //CoStarTester.CostarAreaFiles();
            //USAnnualDeliverableTester.CostarAreaFilesManualCheck();
            //USAnnualDeliverableTester.CostarAreaFilesSummaries(); //sums total variable values.. should be less than or eqaul to national totals.

            //USAnnualDeliverableTester.CoStarUSManualTestGapFile();

            //USAnnualDeliverableTester.CostarSumDataColumns();

            // CoStar Canada
            //CanadaAnnualDeliverableTester.CostarCanadaTestGapFile();
            //CanadaAnnualDeliverableTester.CostarCanadaManualTestGapfile();
            //CanadaAnnualDeliverableTester.CostarReformatNightlyFeedOutput();

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
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Open Confluence Page.");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Automated Check of CoStar US Point files.");
                Console.WriteLine("{0,5}{1,-10}", "3. ", "Manual Check of CoStar US Point files.");
                Console.WriteLine("{0,5}{1,-10}", "4. ", "Automated Check of CoStar AREA files.");
                Console.WriteLine("{0,5}{1,-10}", "5. ", "Manual Check of CoStar AREA files.");
                Console.WriteLine("{0,5}{1,-10}", "6. ", "Summarize AREA files.");
                Console.WriteLine("{0,5}{1,-10}", "7. ", "Manual Check of CoStar US GAP file.");
                Console.WriteLine("{0,5}{1,-10}", "8. ", "Sum data columns.");

                Console.WriteLine();

                Console.WriteLine("{0,5}{1,-10}", "9. ", "Automated Check of CoStar CANADA GAP file.");
                Console.WriteLine("{0,5}{1,-10}", "10. ", "Manual Check of CoStar CANADA GAP file.");
                Console.WriteLine("{0,5}{1,-10}", "11. ", "Reformat CoStar NIGHTLY Feed output.");

                Console.WriteLine();

                Console.WriteLine("{0,5}{1,-10}", "12. ", "Multi-File Reformat Nightly Feed.");
                //Console.WriteLine("{0,5}{1,-10}", "13. ", "");

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

                    case "2":
                        USAnnualDeliverableTester.CostarPointsFiles();
                        break;

                    case "3":
                        USAnnualDeliverableTester.CostarPointsFilesManualCheck();
                        break;

                    case "4":
                        USAnnualDeliverableTester.CostarAreaFiles();
                        break;

                    case "5":
                        USAnnualDeliverableTester.CostarAreaFilesManualCheck();
                        break;

                    case "6":
                        USAnnualDeliverableTester.CostarAreaFilesSummaries();
                        break;

                    case "7":
                        USAnnualDeliverableTester.CoStarUSManualTestGapFile();
                        break;

                    case "8":
                        USAnnualDeliverableTester.CostarSumDataColumns();
                        break;

                    //Canada
                    case "9":
                        CanadaAnnualDeliverableTester.CostarCanadaTestGapFile();
                        break;

                    case "10":
                        CanadaAnnualDeliverableTester.CostarCanadaManualTestGapfile();
                        break;

                    case "11":
                        CanadaAnnualDeliverableTester.CostarReformatNightlyFeedOutput();
                        break;

                    case "12":
                        ConvertNightlyFeed.CoStarMultiFileReformatNightlyFeed();
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
