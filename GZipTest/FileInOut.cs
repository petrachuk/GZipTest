using System;
using System.Collections.Concurrent;
using System.IO;
using GZipTest.Domain;

namespace GZipTest
{
    /// <summary>
    /// Работа с файловой системой
    /// </summary>
    public static class FileInOut
    {
        /// <summary>
        /// Чтение файла в коллекцию блоков
        /// </summary>
        /// <param name="srcFile">Исходный файл</param>
        /// <param name="collection">Коллекция блоков</param>
        /// <param name="compressed">Признак сжатия</param>
        /// <param name="defaultBlockSize">Размер блока по умолчанию</param>
        public static void Read(FileInfo srcFile, ref BlockingCollection<IndexedBlock> collection, int defaultBlockSize, bool compressed)
        {
            using (var fileStream = new FileStream(srcFile.FullName, FileMode.Open, FileAccess.Read))
            {
                var index = 0;
                var blockSize = defaultBlockSize;

                while (fileStream.Position < fileStream.Length)
                {
                    if (compressed)
                    {
                        // Заголовок блока
                        var blockHeader = new byte[4];
                        fileStream.Read(blockHeader, 0, 4);
                        blockSize = BitConverter.ToInt32(blockHeader, 0);
                    }

                    var array = new byte[blockSize];
                    var length = fileStream.Read(array, 0, blockSize);

                    collection.Add(new IndexedBlock
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

        /// <summary>
        /// Запись коллекции блоков в файл
        /// </summary>
        /// <param name="dstFile">Конечный файл</param>
        /// <param name="collection">Коллекция блоков</param>
        /// <param name="totalBlocks">Общее число блоков</param>
        /// <param name="compressed">Признак сжатия</param>
        public static void Write(FileInfo dstFile, ref ConcurrentDictionary<int, DataBlock> collection, int totalBlocks, bool compressed)
        {
            var index = 0;

            using (var fileStream = new FileStream(dstFile.FullName, FileMode.Create, FileAccess.Write))
            {
                while (index < totalBlocks)
                {
                    while (collection.TryRemove(index, out var block))
                    {
                        if (compressed)
                        {
                            // Размер блока
                            var sizeBytes = BitConverter.GetBytes(block.Size);
                            fileStream.Write(sizeBytes, 0, sizeBytes.Length);
                        }

                        // Блок
                        fileStream.Write(block.Data, 0, block.Size);

                        index++;
                    }
                }
            }
        }
    }
}
