using DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace Leslie_s_Swim_Workout_Picker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICollection<string> selectedTags;
        private readonly ICollection<Workout> workouts;
        private readonly Random random;

        public MainWindow()
        {
            InitializeComponent();

            random = new Random();

            selectedTags = new List<string>();

            workouts = LoadWorkouts().OrderBy(w => w.Title).ToArray();

            var tags = workouts.SelectMany(w => w.Tags).Distinct().OrderBy(t => t);

            foreach (var tag in tags)
            {
                TagsListBox.Items.Add(new TextBlock(){Text = tag, HorizontalAlignment = HorizontalAlignment.Center});
            }
        }

        private static Collection<Workout> LoadWorkouts()
        {
            var serializer = new XmlSerializer(typeof(Collection<Workout>));

            using (var reader = XmlReader.Create(LoadOffline(), new XmlReaderSettings() { IgnoreWhitespace = true }))
            {
                return (Collection<Workout>)serializer.Deserialize(reader);
            }
        }

        private static Stream LoadOffline()
        {
            return typeof(MainWindow).Assembly.GetManifestResourceStream("LeslieSwim.Swim-Dojo-Archive.xml");
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            TagsListBox.SelectedItems.Clear();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                selectedTags.Add(((TextBlock)item).Text);
            }

            foreach (var item in e.RemovedItems)
            {
                selectedTags.Remove(((TextBlock)item).Text);
            }

            WorkoutsListBox.Items.Clear();

            if (selectedTags.Any())
            {
                foreach (var workout in workouts)
                {
                    if (!selectedTags.Except(workout.Tags).Any())
                    {
                        WorkoutsListBox.Items.Add(workout);
                    }
                }
            }
        }

        private void OnPickRandomClick(object sender, RoutedEventArgs e)
        {
            PickAndOpenRandomWorkout();
        }

        private void PickAndOpenRandomWorkout()
        {
            if (WorkoutsListBox.Items.Count > 0)
            {
                var i = random.Next(WorkoutsListBox.Items.Count);

                Process.Start(new ProcessStartInfo()
                {
                    FileName = @"https://www.swimdojo.com" + ((Workout)WorkoutsListBox.Items[i]).Link, UseShellExecute = true
                });
            }
        }

        private void OnTagsKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    PickAndOpenRandomWorkout();
                    break;
                case Key.Escape:
                    TagsListBox.SelectedItems.Clear();
                    selectedTags.Clear();
                    break;
            }
        }
    }
}
