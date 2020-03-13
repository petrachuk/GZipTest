using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Domain;

namespace GZipTest
{
    public class CompressedFile : BaseFile
    {
        private bool _disposed;
        
        #region Constructors
        public CompressedFile() : base()
        {
        }

        public CompressedFile(int boundedCapacity) : base(boundedCapacity)
        {
        }
        #endregion
        
        public override void Save(string path)
        {
            if (!IsAddingCompleted) throw new Exception("Collection not complete");

            //
            // Формирование заголовка файла
            //
            var header = Collection.OrderBy(x => x.Index).Aggregate(string.Empty, (current, item) => current + (item.Size + ";"));
            header = header.Substring(0, header.Length - 1);
            var headerBytes = Encoding.ASCII.GetBytes(header);

            //
            // Длина заголовка
            //
            var headerLength = headerBytes.Length;
            var headerLengthBytes = BitConverter.GetBytes(headerLength);
            if (BitConverter.IsLittleEndian) Array.Reverse(headerLengthBytes);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                // Запишем длину заголовка (4 байта)
                fileStream.Write(headerLengthBytes, 0, headerLengthBytes.Length);

                // Запишем заголовок
                fileStream.Write(headerBytes, 0, headerBytes.Length);

                // Запишем наши блоки
                foreach (var i in Collection.OrderBy(x => x.Index))
                {
                    fileStream.Write(i.Data, 0, i.Size);
                }
            }
        }

        public override BlockingCollection<CachedBlock> Load(string path)
        {
            if (IsAddingCompleted) throw new Exception("Collection sealed");

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    // Прочитаем длину заголовка
                    var headerLengthBytes = new byte[4];
                    fileStream.Read(headerLengthBytes, 0, 4);
                    if (BitConverter.IsLittleEndian) Array.Reverse(headerLengthBytes);
                    var headerLength = BitConverter.ToInt32(headerLengthBytes, 0);

                    // Прочитаем заголовок
                    var headerBytes = new byte[headerLength];
                    fileStream.Read(headerBytes, 0, headerBytes.Length);
                    var header = System.Text.Encoding.ASCII.GetString(headerBytes);

                    // Получим блоки данных
                    var index = 0;
                    
                    foreach (var headerItem in header.Split(';'))
                    {
                        var blockLength = int.Parse(headerItem);

                        var data = new byte[blockLength];
                        fileStream.Read(data, 0, data.Length);

                        Add(new DataBlock { Index = index, Size = blockLength, Data = data});

                        index++;
                    }
                }
            }

            CompleteAdding();

            return _collection;
        }
        
        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return; 
            _disposed = true;
            base.Dispose(disposing);
        }
        #endregion
    }
}