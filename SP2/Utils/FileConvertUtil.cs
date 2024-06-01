using CsvHelper;
using CsvHelper.Configuration;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
namespace SP2.Utils
{
    public static class FileConvertUtil
    {
        private static readonly string path = "../../../CSVFiles/";
        private static readonly CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            DetectDelimiter = true
        };
        private static readonly FileStreamOptions opts = new()
        {
            Access = FileAccess.Read,
            Share = FileShare.Read
        };

        public static async Task<ConverterResult> ConvertCsv(string filename)
        {
            string[] line;
            int rows = 0;

            XSSFWorkbook wb = new();
            ISheet sheet = wb.CreateSheet(filename);

            try
            {
                using var reader = new StreamReader(path + filename, opts);
                using var csv = new CsvParser(reader, config);

                while (await csv.ReadAsync())
                {
                    line = csv.Record;
                    IRow row = sheet.CreateRow(rows++);
                    int cols = 0;

                    foreach (string s in line)
                    {
                        ICell cell = row.CreateCell(cols++);
                        SetCellUtil.SetCellValue(cell, s);
                    }
                }
            }
            catch (FileNotFoundException) //fajl ne postoji
            {
                return new ConverterResult
                {
                    ResultState = ResultState.NotFound,
                    Workbook = null
                };
            }
            catch (Exception ex) //greska pri pristupu fajlu
            {
                Console.WriteLine(ex.Message + "\n");
                return new ConverterResult
                {
                    ResultState = ResultState.ServerError,
                    Workbook = null
                };
            }

            return new ConverterResult
            {
                ResultState = ResultState.Success,
                Workbook = wb
            };
        }
    }
}
