namespace matechat.util
{
    public static class JsonUtil
    {
        public static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\\", "\\\\");
        }
    }
}
