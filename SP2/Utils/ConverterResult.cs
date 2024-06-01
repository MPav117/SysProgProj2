using NPOI.SS.UserModel;

namespace SP2.Utils
{
    public class ConverterResult
    {
        public IWorkbook? Workbook { get; set; }
        public ResultState ResultState { get; set; }
    }

    public enum ResultState
    {
        NotFound,
        ServerError,
        Success
    }
}
