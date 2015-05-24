using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuplicateFinder
{
    class ExtensionHandler
    {
        private readonly List<string> _extensions;

        public ExtensionHandler()
        {
            _extensions = new List<string>();
        }

        public IEnumerable<string> FindDistinctFileExtensions(DirectoryInfo dir)
        {
             ExtractFileExtensionsFromDir(dir);

             RecursiveFindDistinctFileExtensions(dir.GetDirectories());

            return _extensions.Distinct();
        }

        private void RecursiveFindDistinctFileExtensions(IEnumerable<DirectoryInfo> dirs)
        {
            foreach (var dir in dirs)
            {
                 ExtractFileExtensionsFromDir(dir);

                var subDirs = dir.GetDirectories();

                if (subDirs.Any())
                {
                     RecursiveFindDistinctFileExtensions(subDirs);
                }
            }
        }

        private void ExtractFileExtensionsFromDir(DirectoryInfo dir)
        {
            var files = dir.GetFiles();

            if (files.Any())
            {
                var extensions = files.Select(f => f.Extension).Distinct();
                _extensions.AddRange(extensions);
            }
        }
    }
}
