using System.Collections.Generic;

namespace CppIncludeChecker
{
    public class Util
    {
        public static List<string> FilterOut(List<string> sources, List<string> by)
        {
            List<string> output = new List<string>();
            foreach (string include in sources)
            {
                bool isInFilter = false;
                foreach (string filter in by)
                {
                    if (include.Contains(filter))
                    {
                        isInFilter = true;
                        break;
                    }
                }
                if (isInFilter == false)
                {
                    output.Add(include);
                }
            }
            return output;
        }
    }
}
