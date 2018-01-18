using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

using Microsoft.Extensions.Configuration;

namespace EnvironmentTestTool
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static Settings GlobalSettings { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            Configuration = builder.Build();
            GlobalSettings = Configuration.GetSection("Settings").Get<Settings>();

            //start all tests
            Task<List<LogOutput>> pingTest = TestPing();
            Task<List<LogOutput>> webTest = TestWebResponse();

            string filePath = WriteLogHeader();

            //write all results when completed
            LogToTextFile("Ping Tests", pingTest.GetAwaiter().GetResult());
            LogToTextFile("Web Tests", webTest.GetAwaiter().GetResult());

            WriteLogFooter();

            Console.WriteLine($"Test results have been written {filePath}.");
            Console.ReadKey();
        }

        #region Test cases

        /// <summary>
        /// Pings list of websites for a response
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LogOutput>> TestPing()
        {
            List<Task<PingReply>> requests = new List<Task<PingReply>>();

            //Queue links for pinging task
            foreach (PingTarget link in GlobalSettings.PingTests)
            {
                requests.Add(Task.Run(() =>
                {
                    var ping = new Ping();
                    return ping.Send(link.Address);
                }));
            }

            List<LogOutput> resultsLog = new List<LogOutput>();
            LogOutput output = null;
            for (int i = 0; i < requests.Count; i++)
            {
                try
                {
                    PingTarget link = GlobalSettings.PingTests[i];
                    output = new LogOutput();
                    output.Title = link.Title;
                    output.Address = link.Address;

                    PingReply pingReply = await requests[i];

                    if (pingReply.Status == IPStatus.Success)
                    {
                        output.TestStatus = LogOutput.Status.Pass;
                        output.Details = $"{Convert.ToString(pingReply.RoundtripTime)}ms";
                    }
                    else
                    {
                        output.TestStatus = LogOutput.Status.Fail;
                        output.Details = pingReply.Status.ToString();
                    }


                    resultsLog.Add(output);
                }
                catch (Exception e)
                {
                    output.TestStatus = LogOutput.Status.Fail;
                    output.Details = e.InnerException.Message.ToString();
                    resultsLog.Add(output);
                }
            }

            return resultsLog;
        }

        /// <summary>
        /// Checks to see if web sites will give a valid response
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LogOutput>> TestWebResponse()
        {
            List<Task<string>> webrequests = new List<Task<string>>();

            foreach (WebTarget link in GlobalSettings.WebTests)
            {
                webrequests.Add(Task.Run(() =>
                    {
                        WebClient wc = new WebClient();
                        return wc.DownloadString(link.URL);
                    }));
            }

            List<LogOutput> resultsLog = new List<LogOutput>();
            LogOutput output = null;

            for (int i = 0; i < webrequests.Count; i++)
            {
                try
                {
                    WebTarget link = GlobalSettings.WebTests[i];
                    output = new LogOutput();
                    output.Title = link.Title;
                    output.Address = link.URL;

                    string response = await webrequests[i];

                    output.TestStatus = LogOutput.Status.Pass;
                    output.Details = "200";

                    resultsLog.Add(output);
                }
                catch (WebException we)
                {
                    output.Details = we.Message;

                    HttpWebResponse response = (HttpWebResponse)we.Response;
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                            output.TestStatus = LogOutput.Status.Pass;
                        else
                            output.TestStatus = LogOutput.Status.Fail;
                    }
                    else
                        output.TestStatus = LogOutput.Status.Fail;

                    resultsLog.Add(output);
                }
                catch (Exception e)
                {
                    output.TestStatus = LogOutput.Status.Fail;
                    output.Details = Convert.ToString(e.Message);
                    resultsLog.Add(output);
                }
            }
            return resultsLog;
        }

        #endregion

        #region log output  

        /// <summary>
        /// Writes all testing information to a log file
        /// </summary>
        /// <param name="title">type of test</param>
        /// <param name="log">list of test results</param>
        public static void LogToTextFile(string title, List<LogOutput> log)
        {
            try
            {
                using (FileStream fs = new FileStream($"{Environment.CurrentDirectory}/Logs/LogFile-{DateTime.Now.ToShortDateString()}.txt", FileMode.Append, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine();
                    writer.WriteLine($"---------------- {title} ----------------");
                    writer.WriteLine();

                    //write logged results of ping tests
                    foreach (LogOutput item in log)
                        writer.WriteLine($"{item.TestStatus}, {item.Title}, {item.Details}");

                    writer.Flush();
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot create or open LogFile.txt for writing.");
                Console.WriteLine(e.Message);
                return;
            }
        }

        public static string WriteLogHeader()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "/Logs"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "/Logs");

            string filePath;
            using (FileStream fs = new FileStream($"{Environment.CurrentDirectory}/Logs/LogFile-{DateTime.Now.ToShortDateString()}.txt", FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                filePath = fs.Name;

                writer.WriteLine("--------------------------------------------");
                writer.WriteLine($"-- Environment Testing Tool {GlobalSettings.Version}");
                writer.WriteLine($"-- Test Date: {DateTime.Now}");
                writer.WriteLine("--------------------------------------------");
                writer.WriteLine();

                writer.Flush();
                fs.Flush();
            }

            return filePath;
        }

        public static void WriteLogFooter()
        {
            using (FileStream fs = new FileStream($"{Environment.CurrentDirectory}/Logs/LogFile-{DateTime.Now.ToShortDateString()}.txt", FileMode.Append, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("----------------- Test Complete -----------------");
                writer.Flush();
                fs.Flush();
            }
        }

        #endregion
    }
}
