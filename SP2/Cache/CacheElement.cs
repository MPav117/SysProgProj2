using NPOI.SS.UserModel;

namespace SP2.Cache
{
    public class CacheElement(IWorkbook workbook)
    {
        public IWorkbook Workbook { get; set; } = workbook;

        public DateTime LastUsed { get; set; } = DateTime.Now;
    }
}