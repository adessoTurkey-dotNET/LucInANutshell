using System.Reflection;

namespace Ads.LuceneIndexer.Interfaces
{
    public abstract class APathProvider
    {
        public string SetPath(string? suffix)
        {
            var def = Path.GetFullPath(Path.Combine($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}", @"..\..\..\settings"));

            return suffix == null ? def : $"{def}{suffix}";
        }
    }
}
