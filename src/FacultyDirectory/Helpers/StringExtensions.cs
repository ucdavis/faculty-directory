namespace FacultyDirectory.Helpers
{

    public static class StringExtensions
    {
        public static string NullIfEmpty(this string str)
        {
            // return the string, or null if the string is empty

            return string.IsNullOrWhiteSpace(str) ? str : null;
        }
    }
}