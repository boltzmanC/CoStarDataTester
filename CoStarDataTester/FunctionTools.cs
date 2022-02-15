using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoStarDataTester
{
    public class FunctionTools
    {
        // Function Tools
        public static string GetAFile()
        {
            Console.Write("File:");

            string path = Console.ReadLine().Replace("\"", "");

            Console.WriteLine();

            return path;
        }

        public static char GetDelimiter()
        {
            Console.Write("Enter Delimeter: ");
            var delimeter = Console.ReadLine();

            char splitchar;

            while (!char.TryParse(delimeter, out splitchar))
            {
                Console.Write("Invalid Enter Delimiter");
                delimeter = Console.ReadLine();
            }

            //Console.WriteLine();

            return splitchar;
        }

        public static char GetTXTQualifier()
        {
            Console.Write("Enter Txt Qualifier: ");
            var delimeter = Console.ReadLine();

            char splitchar;

            while (!char.TryParse(delimeter, out splitchar))
            {
                Console.Write("Entry Invalid");
                delimeter = Console.ReadLine();
            }

            Console.WriteLine();

            return splitchar;
        }

        public static string GetColumn() //removes ", trims, toupper 
        {
            Console.Write("Enter Column Name: ");
            string columnname = Console.ReadLine();

            columnname = columnname.Replace("\"", string.Empty);
            columnname = columnname.Trim();
            columnname = columnname.ToUpper();

            Console.WriteLine();

            return columnname;
        }

        public static int ColumnIndexWithQualifier(string line, char del, char qualifier, string columnname)
        {
            columnname = columnname.ToUpper().Trim();

            string[] linesplit = SplitLineWithTxtQualifier(line, del, qualifier, false);

            int columnindex = 0;
            for (int x = 0; x <= linesplit.Length - 1; x++)
            {
                if (columnname == linesplit[x].ToUpper().Trim().Replace(qualifier.ToString(), string.Empty))
                {
                    columnindex = x;
                }

            }
            return columnindex;
        }

        public static int ColumnIndex(string line, char del, string columnname)
        {
            columnname = columnname.ToUpper().Trim();

            string[] linesplit = line.Split(del);

            int length = linesplit.Length;

            int col = 0;
            for (int x = 0; x <= length - 1; x++)
            {
                if (columnname == linesplit[x].ToUpper().Replace("\"", string.Empty).Trim())
                {
                    col = x;
                }
            }
            return col;
        }

        public static int ColumnIndexNew(string line, char del, string columnname, char txtq)
        {
            List<string> headerlinebuilder = new List<string>();
            if (line.Contains(txtq))
            {
                headerlinebuilder.AddRange(SplitLineWithTxtQualifier(line, del, txtq, false));
            }
            else
            {
                headerlinebuilder.AddRange(line.Split(del));
            }

            string[] splitline = headerlinebuilder.ToArray();

            int length = splitline.Length;

            int columnindex = 0;
            for (int x = 0; x <= length - 1; x++)
            {
                if (columnname.ToUpper() == splitline[x].ToUpper().Replace("\"", string.Empty).Trim())
                {
                    columnindex = x;
                }
            }
            return columnindex;
        }

        public static string[] LineStringToArray(string readline, char textqualifier, char delimiter)
        {
            List<string> splitlinebuilder = new List<string>();
            if (readline.Contains(textqualifier))
            {
                splitlinebuilder.AddRange(SplitLineWithTxtQualifier(readline, delimiter, textqualifier, false));
            }
            else
            {
                splitlinebuilder.AddRange(readline.Split(delimiter));
            }

            string[] splitline = splitlinebuilder.ToArray();

            return splitline;
        }

        public static string GetDesktopDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        public static string GetFileNameWithoutExtension(string filepath)
        {
            string filename = Path.GetFileNameWithoutExtension(@filepath);

            return filename;
        }

        public static int[] StringToIntArray(string value, char delimiter)
        {
            string[] arrayinfo = value.Split(delimiter);
            int[] newarray = new int[arrayinfo.Length];

            for (int i = 0; i < newarray.Length; ++i)
            {
                int number;
                string var = arrayinfo[i];

                if (int.TryParse(var, out number))
                {
                    newarray[i] = number;
                }
            }

            return newarray;
        }

        public static string CombineTwoValuesInArray(string line, char delimiter, int column1, int column2)
        {
            string[] array = line.Split(delimiter);

            array[column1] = array[column1] + "_" + array[column2];

            List<string> values = new List<string>();

            for (int y = 0; y <= array.Length - 1; y++)
            {
                if (y != column2)
                {
                    values.Add(array[y]);
                }
            }

            string[] newarray = values.ToArray();
            string separator = delimiter.ToString();

            return string.Join(separator, newarray);
        }

        public static int SumOfIntArray(params int[] intarray)
        {
            int result = 0;

            for (int i = 0; i < intarray.Length; i++)
            {
                result += intarray[i];
            }

            return result;
        }

        public static int randomnumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static string[] SplitLineWithTxtQualifier(string expression, char delimiter, char qualifier, bool ignoreCase) //true -> sets everything to lower.
        {
            if (ignoreCase)
            {
                expression = expression.ToLower();
                delimiter = char.ToLower(delimiter);
                qualifier = char.ToLower(qualifier);
            }

            int len = expression.Length;
            char symbol;
            List<string> list = new List<string>();
            string newField = null;

            for (int begin = 0; begin < len; ++begin)
            {
                symbol = expression[begin];

                if (symbol == delimiter || symbol == '\n')
                {
                    list.Add(string.Empty);
                }
                else
                {
                    newField = null;
                    int end = begin;

                    for (end = begin; end < len; ++end)
                    {
                        symbol = expression[end];
                        if (symbol == qualifier)
                        {
                            // bypass the unsplitable block of text
                            bool foundClosingSymbol = false;
                            for (end = end + 1; end < len; ++end)
                            {
                                symbol = expression[end];
                                if (symbol == qualifier)
                                {
                                    foundClosingSymbol = true;
                                    break;
                                }
                            }

                            if (false == foundClosingSymbol)
                            {
                                throw new ArgumentException("expression contains an unclosed qualifier symbol");
                            }

                            continue;
                        }

                        if (symbol == delimiter || symbol == '\n')
                        {
                            newField = expression.Substring(begin, end - begin);
                            begin = end;
                            break;
                        }
                    }

                    if (newField == null)
                    {
                        newField = expression.Substring(begin);
                        begin = end;
                    }

                    list.Add(newField.Replace("\"", string.Empty)); //added to remove " for simplification.
                }
            }
            return list.ToArray();
        }

        public static string[] ArrayWithNoTxtQualifier(string[] array, char qualifier)
        {
            List<string> values = new List<string>();

            foreach (string v in array)
            {
                string newvalue = v.Replace(qualifier.ToString(), string.Empty);

                values.Add(newvalue);
            }

            return values.ToArray();
        }



        // Settings
        public static void ConsoleSettings()
        {
            Console.SetWindowSize(150, 45);
            int bufferwidth = Console.BufferWidth;
            int bufferheight = 600;
            Console.SetBufferSize(bufferwidth, bufferheight);
        }




    }
}
