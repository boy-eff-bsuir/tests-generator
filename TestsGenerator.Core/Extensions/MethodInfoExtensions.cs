using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Providers;

namespace TestsGenerator.Core.Extensions
{
    public static class MethodInfoExtensions
    {
        public static MethodDto ToMethodDto(this KeyValuePair<string, MethodInfo> pair)
        {
            return new MethodDto(pair.Key, pair.Value.Parameters, pair.Value.Count, pair.Value.ReturnType);
        }
    }
}