using System;
using System.Collections.Concurrent;
using System.IO;
using GZipTest.Domain;

namespace GZipTest
{
    public static class DecompressedFile
    {
        private const int BlockSize = 1048576;  // Мегабайт
        
        /* public void Save(BlockingCollection<DataBlock> collection, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                // Запишем наши блоки
                foreach (var i in collection.GetConsumingEnumerable().)
                {
                    fileStream.Write(i.Data, 0, i.Size);
                }
            }
        } */

        public static void Load(string path, ref BlockingCollection<DataBlock> collection)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var index = 0;

                while (fileStream.Position < fileStream.Length)
                {
                    var array = new byte[BlockSize];
                    var length = fileStream.Read(array, 0, BlockSize);

                    collection.Add(new DataBlock
                    {
                        Index = index,
                        Size = length,
                        Data = array
                    });

                    index++;
                }
            }

            collection.CompleteAdding();
        }
    }
}