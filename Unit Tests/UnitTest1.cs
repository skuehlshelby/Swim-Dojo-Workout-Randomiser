using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using DataObjects;
using HtmlAgilityPack;
using NUnit.Framework;

namespace Unit_Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            const string url = @"https://www.swimdojo.com/archive";

            var archiveGroupList = XPathExpression.Compile(@"//ul[@class='archive-group-list']");
            var archiveGroup = XPathExpression.Compile(@"//li[@class='archive-group']");
            var groupTitle = XPathExpression.Compile(@".//a[@class='archive-group-name-link']");
            var archiveItem = XPathExpression.Compile(@".//li[@class='archive-item ']/a[@class='archive-item-link ']");

            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Archive — Swim Dojo.htm")));

            var groups = doc.DocumentNode.SelectNodes(archiveGroup);

            var workouts = new Collection<Workout>();

            var serializer = new XmlSerializer(typeof(Collection<Workout>));
            
            foreach (var group in groups)
            {
                var category = group.SelectSingleNode(groupTitle).InnerText.Trim();

                foreach (var workoutNode in group.SelectNodes(archiveItem))
                {
                    var workout = new Workout(workoutNode.InnerText.Trim(), workoutNode.GetAttributeValue("href", string.Empty));

                    if (workouts.Contains(workout))
                    {
                        workout = workouts.First(w => w.Equals(workout));

                        if (!workout.Tags.Contains(category))
                        {
                            workout.Tags.Add(category);
                        }
                    }
                    else
                    {
                        workout.Tags.Add(category);
                        workouts.Add(workout);
                    }
                }
            }

            using (var writer = File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.xml")))
            {
                serializer.Serialize(writer, workouts);
            }
        }

        [Test]
        public void DeserializeWorkout()
        {
            using (var reader = new XmlTextReader(File.OpenText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Swim-Dojo-Archive.xml"))))
            {
                reader.ReadStartElement();

                var workout = (Workout)Activator.CreateInstance(typeof(Workout), true);
                ((IXmlSerializable)workout).ReadXml(reader);
            }
        }

        [Test]
        public void Serialize()
        {
            var workout = new Workout("Title", "Link", "Beginner", "Cool", "0-1000");

            var serializer = new XmlSerializer(typeof(Workout));

            using (var file = File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test2.xml")))
            {
                using (var writer = XmlWriter.Create(file))
                {
                    serializer.Serialize(writer, workout);
                }
            }

            using (var reader = new XmlTextReader(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test2.xml"))))
            {
                workout = (Workout)serializer.Deserialize(reader);
            }
        }
    }
}