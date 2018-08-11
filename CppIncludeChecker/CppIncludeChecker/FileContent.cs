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
        private byte[] _originalContentBytes;
        private FileAttributes _originalFileAttributes;

        public FileContent(string filename)
        {
            _filename = filename;
            _originalFileAttributes = File.GetAttributes(_filename);

            _originalContentBytes = File.ReadAllBytes(_filename);
            OriginalContent = File.ReadAllText(_filename);
        }

        public void RemoveAndWrite(string text)
        {
            RemoveReadOnlyAttribute(_filename);
            File.WriteAllText(_filename, OriginalContent.Replace(text, ""));
        }

        public void RemoveAllAndWrite(List<string> texts)
        {
            string result = OriginalContent;
            foreach (string text in texts)
            {
                result = result.Replace(text, "");
            }
            RemoveReadOnlyAttribute(_filename);
            File.WriteAllText(_filename, result);
        }

        public void RevertWrite()
        {
            RemoveReadOnlyAttribute(_filename);
            File.WriteAllBytes(_filename, _originalContentBytes);
            File.SetAttributes(_filename, _originalFileAttributes);
        }

        private static void RemoveReadOnlyAttribute(string filename)
        {
            var originalAttributes = File.GetAttributes(filename);
            File.SetAttributes(filename, originalAttributes & ~FileAttributes.ReadOnly);
        }
    }
}
