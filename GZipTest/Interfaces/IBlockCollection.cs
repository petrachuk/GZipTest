using System.Collections.Generic;
using GZipTest.Domain;

namespace GZipTest.Interfaces
{
    public interface IBlockCollection
    {
        int Capacity { get; }
        int Count { get; }
        bool IsAddingCompleted { get; }
        bool IsCompleted  { get; }

        IEnumerable<DataBlock> Collection { get; }

        void Add(DataBlock item);

        void CompleteAdding();
    }
}
