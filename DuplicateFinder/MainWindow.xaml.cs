using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DuplicateFinder
{
    public partial class MainWindow : Window
    {
        private readonly ExtensionHandler _extensionHandler;
        private readonly Dictionary<string, List<string>> _duplicates;

        public MainWindow()
        {
            _extensionHandler = new ExtensionHandler();
            _duplicates = new Dictionary<string, List<string>>();

            InitializeComponent();
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            try
            {
                var rootDirs = ConfigurationManager.AppSettings["rootPaths"].Split(';').Select(path => new DirectoryInfo(path));

                RecursiveLocateDuplicateVideoFiles(rootDirs);

                WriteResults();

                MessageBox.Show("Done. " + _duplicates.Count + " duplicates found");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, ex.StackTrace));
            }
        }

        private void WriteResults()
        {
            var result = new List<string>();

            foreach (var dupe in _duplicates)
            {
                result.Add(dupe.Key);

                result.AddRange(dupe.Value.Select(path => "\t" + path));
            }

            listBox.ItemsSource = result;
        }

        private void RecursiveLocateDuplicateVideoFiles(IEnumerable<DirectoryInfo> rootDirs)
        {
            foreach (var dir in rootDirs)
            {
                if (!dir.Exists)
                    continue;

                var extensions = _extensionHandler.FindDistinctFileExtensions(dir);

                var files = dir.GetFiles();

                LocateDuplicateVideoFiles(dir, files, extensions);

                var subDirs = dir.GetDirectories();

                if (subDirs.Any())
                {
                    RecursiveLocateDuplicateVideoFiles(subDirs);
                }
            }
        }

        private void LocateDuplicateVideoFiles(DirectoryInfo dir, IEnumerable<FileInfo> files, IEnumerable<string> extensions)
        {
            var duplicates =
                files.Where(f => extensions.Contains(f.Extension))
                    .GroupBy(f => f.Extension)
                    .Where(grp => grp.Count() > 1)
                    .ToArray();
            
            foreach (var entry in duplicates)
            {
                if (_duplicates.ContainsKey(entry.Key))
                {
                    _duplicates[entry.Key].AddRange(entry.Select(c => c.FullName));
                }
                else
                {
                    _duplicates.Add(entry.Key, entry.Select(c => c.FullName).ToList());
                }
            }
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;

            var path = btn.Tag as string;

            if (string.IsNullOrEmpty(path))
                return;

            path = SanitizePath(path);

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                var file = new FileInfo(path);

                if (file.Exists && !string.IsNullOrEmpty(file.DirectoryName))
                {
                    Process.Start(file.DirectoryName);
                }
                else
                {
                    MessageBox.Show("Could not open path " + path);
                }
            }
        }

        private static string SanitizePath(string path)
        {
            path = path.Trim();

            if (path.StartsWith(@"\t"))
            {
                path = path.Substring(path.IndexOf(@"\t", StringComparison.Ordinal), path.Length);
            }

            return path;
        }
    }
}
