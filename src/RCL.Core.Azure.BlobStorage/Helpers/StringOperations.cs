namespace RCL.Core.Azure.BlobStorage
{
    internal static class StringOperations
    {
        public static string GetValueFromConnectionString(string text, string separator, string key)
        {
            string[] strlist = text.Split(separator,StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            for (int i = 0; i < strlist.Count(); i++)
            {
                int index = strlist[i].IndexOf('=');
                string k = strlist[i].Substring(0, index);
                string v = strlist[i].Substring(index + 1);

                dict.Add(k, v);
            }

            return dict[key];
        }
    }
}
