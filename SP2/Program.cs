using System.Net;

namespace SP2
{
    internal class XLServer
    {
        static async Task Main()
        {
            RequestHandler handler = new();
            HttpListener listener = new();
            try
            {
                listener.Prefixes.Add("http://localhost:7777/");
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine(ex.ErrorCode + "\n");
            }

            if (listener.IsListening)
            {
                Console.WriteLine("Server podignut. Pocinje slusanje zahteva");

                while (true)
                {
                    try
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        Console.WriteLine("Primljen zahtev");

                        _ = Task.Run(() => handler.HandleRequest(context));
                    }
                    catch (HttpListenerException ex)
                    {
                        Console.WriteLine(ex.ErrorCode + "\n");
                    }
                }
            }
        }
    }
}
