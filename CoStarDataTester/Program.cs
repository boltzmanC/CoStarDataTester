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

            //Costar ---------------------------------
            //USAnnualDeliverableTester.CostarPointsFiles(); //add check for unique variable IDS first.
            //USAnnualDeliverableTester.CostarPointsFilesManualCheck();

            //CoStarTester.CostarAreaFiles();
            //USAnnualDeliverableTester.CostarAreaFilesManualCheck();
            //USAnnualDeliverableTester.CostarAreaFilesSummaries(); //sums total variable values.. should be less than or eqaul to national totals.

            //USAnnualDeliverableTester.CoStarUSManualTestGapFile();

            //USAnnualDeliverableTester.CostarSumDataColumns();

            // CoStar Canada --------------------------------
            //CanadaAnnualDeliverableTester.CostarCanadaTestGapFile();
            //CanadaAnnualDeliverableTester.CostarCanadaManualTestGapfile();
            //CanadaAnnualDeliverableTester.CostarReformatNightlyFeedOutput();

            //nightly feed reformat
            ConvertNightlyFeed.CoStarMultiFileReformatNightlyFeed();
        }
    }
}
