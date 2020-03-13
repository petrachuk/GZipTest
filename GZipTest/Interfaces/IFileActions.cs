using GZipTest.Domain;
using System.Collections.Concurrent;

namespace GZipTest.Interfaces
{
    public interface IFileActions
    {
        void Save(string path);
        BlockingCollection<CachedBlock> Load(string path);
    }
}
