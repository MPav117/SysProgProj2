using NPOI.SS.UserModel;
using SP2.Cache;
using SP2.Utils;
using System.Net;
using System.Text;

namespace SP2
{
    public class RequestHandler
    {
        private readonly WorkbookCache cache = new(20, TimeSpan.FromHours(3));

        public async Task HandleRequest(HttpListenerContext context)
        {
            Console.WriteLine("Pocela obrada zahteva");

            HttpListenerResponse response = context.Response;

            //greska u request-u
            if (context == null
                || context.Request == null
                || context.Request.RawUrl == null
                || context.Request.RawUrl.Length < 6)
            {
                RespondError(response, "Greska u request-u", 400, "Bad Request");
                return;
            }

            //nije .csv fajl
            if (!context.Request.RawUrl.EndsWith(".csv"))
            {
                RespondError(response, "Trazeni fajl nije .csv", 415, "Unsupported Media Type");
                return;
            }

            string reqFile = context.Request.RawUrl[1..];

            IWorkbook? wb = cache.TryGet(reqFile);

            //nije u kesu
            if (wb == null)
            {
                ConverterResult res = await FileConvertUtil.ConvertCsv(reqFile);

                //fajl ne postoji
                if (res.ResultState == ResultState.NotFound)
                {
                    RespondError(response, "Trazeni fajl ne postoji", 404, "Not Found");
                    return;
                }

                //greska pri citanju
                if (res.ResultState == ResultState.ServerError)
                {
                    RespondError(response, "", 500, "Internal Server Error");
                    return;
                }

                wb = res.Workbook;
            }

            //azuriranje kesa
            cache.AddOrUse(wb!);

            response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            wb!.Write(response.OutputStream);
            response.Close();
            Console.WriteLine("Uspesna obrada zahteva");
        }

        private static void RespondError(HttpListenerResponse resp, string toPrint, int code, string desc)
        {
            Console.WriteLine(toPrint + "\n");
            resp.StatusCode = code;
            resp.StatusDescription = desc;
            string responseString = $"<HTML><BODY>{code} {desc}</BODY></HTML>";
            resp.Close(Encoding.UTF8.GetBytes(responseString), false);
        }
    }
}
