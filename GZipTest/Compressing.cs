using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Domain;

namespace GZipTest
{
    /// <summary>
    /// Сжатие/распаковка данных
    /// </summary>
    public class Compressing : IDisposable
    {
        private bool _disposed;

        private const int DefaultBlockSize = 1048576;   // Мегабайт

        private readonly bool _mode;

        private FileInfo SrcFile { get; }
        private BlockingCollection<IndexedBlock> _src;

        private FileInfo DstFile { get; }
        private ConcurrentDictionary<int, DataBlock> _dst;

        #region Constructors
        public Compressing(FileInfo inFile, FileInfo outFile, bool compressing)
        {
            _mode = compressing;

            SrcFile = inFile;
            _src = new BlockingCollection<IndexedBlock>();

            DstFile = outFile;
            _dst = new ConcurrentDictionary<int, DataBlock>();
        }
        #endregion

        private void Reader()
        {
            FileInOut.Read(SrcFile, ref _src, DefaultBlockSize, !_mode);
        }

        private void Writer()
        {
            var totalBlocks = (int) (SrcFile.Length / DefaultBlockSize) +
                              (SrcFile.Length % DefaultBlockSize > 0 ? 1 : 0);

            FileInOut.Write(DstFile, ref _dst, totalBlocks, _mode);
        }

        private void Processing()
        {
            foreach (var dataBlock in _src.GetConsumingEnumerable())
            {
                byte[] resultBytes;

                using (var dst = new MemoryStream())
                {
                    if (_mode)
                    {
                        using (var compress = new GZipStream(dst, CompressionMode.Compress, false))
                        {
                            compress.Write(dataBlock.Data, 0, dataBlock.Size);
                        }
                    }
                    else
                    {
                        using (var src = new MemoryStream(dataBlock.Data, 0, dataBlock.Size))
                        using (var compress = new GZipStream(src, CompressionMode.Decompress, false))
                        {
                            compress.CopyTo(dst);
                        }
                    }

                    resultBytes = dst.ToArray();
                }

                // Отправка в буфер обработанных
                _dst.TryAdd(dataBlock.Index, new DataBlock
                {
                    Size = resultBytes.Length,
                    Data = resultBytes
                });
            }
        }

        public void Start()
        {
            // Процесс загрузки файла
            var reader = new Thread(Reader) {Priority = ThreadPriority.AboveNormal};
            reader.Start();

            // Процесс записи файла
            var writer = new Thread(Writer);
            writer.Start();

            // Сжатие блоков
            var threads = new Thread[Environment.ProcessorCount];

            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i] = new Thread(Processing);
                threads[i].Start();
            }

            // Дождемся всех
            reader.Join();
            for (var i = 0; i < 8; i++) threads[i].Join();
            writer.Join();
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
                _src?.Dispose();
            }

            _src = null;
            _dst = null;
      
            _disposed = true;
        }
        
        ~Compressing()
        {
            Dispose(false);
        }
        #endregion
    }
}
