using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CppIncludeChecker
{
    class FileContent
    {
        private readonly string _filename;
        private readonly string _originalContent;
        public FileContent(string filename)
        {
            _filename = filename;
            _originalContent = File.ReadAllText(_filename);
        }

        public void RemoveAndWrite(string text)
        {
            File.WriteAllText(_filename, _originalContent.Replace(text, ""));
        }

        public void RemoveAllAndWrite(List<string> texts)
        {
            string result = _originalContent;
            foreach (string text in texts)
            {
                result = result.Replace(text, "");
            }
            File.WriteAllText(_filename, result);
        }

        public void RevertWrite()
        {
            File.WriteAllText(_filename, _originalContent);
        }
    }
}
