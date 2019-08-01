using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private Encoding _writeEncoding;

        public FileModifier(string filename, Encoding writeEncoding)
        {
            Filename = filename;
            _writeEncoding = writeEncoding;
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
            if (_writeEncoding == null)
            {
                File.WriteAllText(Filename, result);
            }
            else
            {
                File.WriteAllText(Filename, result, _writeEncoding);
            }
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
