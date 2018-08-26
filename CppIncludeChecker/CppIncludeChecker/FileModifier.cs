using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace CppIncludeChecker
{
    public class FileModifier
    {
        public List<string> RemovedStrings { get; private set; }
        public string Filename { get; private set; }
        public string OriginalContent { get; private set; }
        private byte[] _originalContentBytes;
        private FileAttributes _originalFileAttributes;

        public FileModifier(string filename)
        {
            Filename = filename;
            _originalFileAttributes = File.GetAttributes(Filename);

            _originalContentBytes = File.ReadAllBytes(Filename);
            OriginalContent = File.ReadAllText(Filename);
        }

        public void RemoveAndWrite(string text)
        {
            RemoveAndWrite(new List<string> { text });
        }

        public void RemoveAndWrite(List<string> texts)
        {
            Debug.Assert(RemovedStrings == null);
            RemovedStrings = texts;

            RemoveReadOnlyAttribute(Filename);
            string result = OriginalContent;
            foreach (string text in texts)
            {
                result = RemoveIncludeLine(result, text);
            }
            File.WriteAllText(Filename, result);

        }

        public void RevertAndWrite()
        {
            RemoveReadOnlyAttribute(Filename);
            File.WriteAllBytes(Filename, _originalContentBytes);
            File.SetAttributes(Filename, _originalFileAttributes);
            RemovedStrings = null;
        }

        private static void RemoveReadOnlyAttribute(string filename)
        {
            var originalAttributes = File.GetAttributes(filename);
            File.SetAttributes(filename, originalAttributes & ~FileAttributes.ReadOnly);
        }

        public static string RemoveIncludeLine(string input, string include)
        {
            string pattern = string.Format("{0}.*\n", Regex.Escape(include));
            return Regex.Replace(input, pattern, "");
        }
    }
}
