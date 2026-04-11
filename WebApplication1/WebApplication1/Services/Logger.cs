using System.IO;
using System.Web.Hosting;

public class Logger
{
    private string path = HostingEnvironment.MapPath("~/App_Data/log.txt");

    public void Log(string message)
    {
        File.AppendAllText(path, message + "\n");
    }
}