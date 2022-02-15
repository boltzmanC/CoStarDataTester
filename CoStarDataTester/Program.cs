using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoStarDataTester
{
    class Program
    {
        static void Main(string[] args)
        {

            //Costar ---------------------------------
            //CoStarTester.CostarPointsFiles(); //add check for unique variable IDS first.
            //CoStarTester.CostarPointsFilesManualCheck();

            //CoStarTester.CostarAreaFiles();
            CoStarTester.CostarAreaFilesManualCheck();
            //CoStarTester.CostarAreaFilesSummaries(); //sums total variable values.. should be less than or eqaul to national totals.

            //CoStarTester.CoStarUSManualTestGapFile();

            //CoStarTester.CostarSumDataColumns();

            // CoStar Canada --------------------------------
            //CoStarTester.CostarCanadaTestGapFile();
            //CoStarTester.CostarCanadaManualTestGapfile();
            //CoStarTester.CostarCanadaReformatNightlyFeedOutput();
        }
    }
}
