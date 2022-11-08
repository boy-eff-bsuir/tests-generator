namespace TestsGenerator.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Mocked(this string str)
        {
            return "Mock<" + str + ">";
        }

        public static string Underscored(this string str)
        {
            return "_" + str;
        }
    }
}