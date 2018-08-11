using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CppIncludeChecker
{
    class FileContent
    {
        private readonly string _filename;
        public string OriginalContent { get; private set; }

        public FileContent(string filename)
        {
            _filename = filename;
            OriginalContent = File.ReadAllText(_filename);
        }

        public void RemoveAndWrite(string text)
        {
            File.WriteAllText(_filename, OriginalContent.Replace(text, ""));
        }

        public void RemoveAllAndWrite(List<string> texts)
        {
            string result = OriginalContent;
            foreach (string text in texts)
            {
                result = result.Replace(text, "");
            }
            File.WriteAllText(_filename, result);
        }

        public void RevertWrite()
        {
            File.WriteAllText(_filename, OriginalContent);
        }
    }
}
