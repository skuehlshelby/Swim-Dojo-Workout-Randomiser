using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using System.Xml.XPath;
using DataObjects;

namespace WorkoutScraper
{
    public class Program
    {
        private const string Url = @"https://www.swimdojo.com/archive";

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var html = LoadSwimDojoArchive();

            var workouts = ParseWorkouts(html);

            WriteToFile(workouts);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("An unhandled exception has occurred with the following message:");
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        private static HtmlDocument LoadSwimDojoArchive()
        {
            var html = new HtmlDocument();

            html.LoadHtml(UrlRequest(Url));

            return html;
        }

        private static string UrlRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Timeout = 600000; //60 second timeout
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";

            string responseContent;

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream ?? LoadOffline()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }

            return responseContent;
        }

        private static Stream LoadOffline()
        {
            return typeof(Program).Assembly.GetManifestResourceStream("WorkoutScraper.Archive — Swim Dojo.htm");
        }

        private static Collection<Workout> ParseWorkouts(HtmlDocument html)
        {
            var workouts = new Collection<Workout>();

            var categoryXPath = XPathExpression.Compile(@"//li[@class='archive-group']");
            var tagXPath = XPathExpression.Compile(@".//a[@class='archive-group-name-link']");
            var workoutXPath = XPathExpression.Compile(@".//li[@class='archive-item ']/a[@class='archive-item-link ']");

            var categories = html.DocumentNode.SelectNodes(categoryXPath);

            if (categories != null)
            {
                foreach (var category in categories)
                {
                    var tag = category.SelectSingleNode(tagXPath).InnerText.Trim();

                    foreach (var workoutNode in category.SelectNodes(workoutXPath))
                    {
                        var link = workoutNode.GetAttributeValue("href", string.Empty);

                        if (workouts.Any(w => w.Link == link))
                        {
                            var workout = workouts.First(w => w.Link == link);
                            workout.Tags.Add(tag);
                        }
                        else
                        {
                            var title = workoutNode.InnerText.Trim();
                            workouts.Add(new Workout(title, link, tag));
                        }
                    }
                }
            }

            return workouts;
        }

        private static void WriteToFile(Collection<Workout> workouts)
        {
            var writeLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Swim-Dojo-Archive.xml");

            if (File.Exists(writeLocation))
            {
                Console.WriteLine($"The save location {writeLocation} already exists. Overwrite? [Y/N]");

                if (char.ToUpper(Console.ReadKey(intercept: true).KeyChar) == 'N')
                {
                    Console.WriteLine($"The save operation has been aborted...");
                    return;
                }
            }

            if (!workouts.Any())
            {
                Console.WriteLine($"The site failed to load...");
                return;
            }

            var serializer = new XmlSerializer(typeof(Collection<Workout>));

            using (var writer = File.CreateText(writeLocation))
            {
                serializer.Serialize(writer, workouts);
            }

            Console.WriteLine($"An updated copy of Swim Dojo's workout archive has been saved to {writeLocation}.");
        }
    }
}
