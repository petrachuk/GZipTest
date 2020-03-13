using System;
using System.Collections.Generic;
using GZipTest.Domain;
using GZipTest.Interfaces;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public abstract class BaseFile : IBlockCollection, IFileActions, IDisposable
    {
        private bool _disposed;
        internal readonly BlockingCollection<CachedBlock> _collection;

        #region Constructors
        public BaseFile()
        {
            _collection = new BlockingCollection<CachedBlock>();
        }

        public BaseFile(int boundedCapacity)
        {
            _collection = new BlockingCollection<CachedBlock>(boundedCapacity);
        }
        #endregion

        #region IBlockCollection
        public int Capacity => _collection.BoundedCapacity;
        public int Count => _collection.Count;
        public bool IsAddingCompleted => _collection.IsAddingCompleted;
        public bool IsCompleted => _collection.IsCompleted;

        public IEnumerable<DataBlock> Collection => _collection.GetConsumingEnumerable()
            .Select(x => new DataBlock {Index = x.Index, Size = x.Size, Data = GetBytes(x.Path)});

        public void Add(DataBlock dataBlock)
        {
            if (_collection.IsAddingCompleted) throw new Exception("Collection sealed");

            var tempFileName = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(dataBlock.Data, 0, dataBlock.Size);
            }

            _collection.Add(new CachedBlock { Index = dataBlock.Index, Size = dataBlock.Size, Path = tempFileName });
        }

        public void CompleteAdding()
        {
            if (_collection.IsAddingCompleted) throw new Exception("Collection sealed");
            _collection.CompleteAdding();
        }

        private static byte[] GetBytes(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }
        #endregion

        public virtual void Save(string path)
        {
            throw new NotImplementedException();
        }

        public virtual BlockingCollection<CachedBlock> Load(string path)
        {
            throw new NotImplementedException();
        }

        #region IDisposable
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
   
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; 
      
            if (disposing) {
                
                // Удалим временные файлы
                if (_collection != null)
                {
                    foreach (var i in _collection.GetConsumingEnumerable())
                        File.Delete(i.Path);
                }

                // Освободим коллекцию
                _collection?.Dispose();
            }
      
            _disposed = true;
        }
        ~BaseFile()
        {
            Dispose(false);
        }
        #endregion
    }
}
