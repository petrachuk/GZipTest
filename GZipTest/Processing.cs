using GZipTest.Domain;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public class Processing : IDisposable
    {
        bool _disposed;

        private BlockingCollection<DataBlock> _src;
        private ConcurrentDictionary<int, ShortBlock> _dst;

        private const int BlockSize = 1048576;

        private FileInfo SrcFile { get; set; }
        private FileInfo DstFile { get; set; }

        private long TotalBlocks;

        public Processing()
        {
            _src = new BlockingCollection<DataBlock>(Environment.ProcessorCount + 1);
            _dst = new ConcurrentDictionary<int, ShortBlock>();

            SrcFile = new FileInfo("D:\\Distrib\\aimp_4.51.2084.exe");
            DstFile = new FileInfo("D:\\Temp\\test.txt");
            // SrcFile = new FileInfo("D:\\Temp\\voyna-i-mir-tom-1.txt");

            TotalBlocks = SrcFile.Length / BlockSize + (SrcFile.Length % BlockSize > 0 ? 1 : 0);
        }

        public int Start()
        {
            


            // Загрузка файла в память
            var read = new Thread(Reader);
            read.Start();
            
            var compress0 = new Thread(Compression);
            compress0.Start();

            var compress1 = new Thread(Compression);
            compress1.Start();

            var compress2 = new Thread(Compression);
            compress2.Start();

            var compress3 = new Thread(Compression);
            compress3.Start();

            var compress4 = new Thread(Compression);
            compress4.Start();

            var compress5 = new Thread(Compression);
            compress5.Start();

            var compress6 = new Thread(Compression);
            compress6.Start();

            var compress7 = new Thread(Compression);
            compress7.Start();

            var write = new Thread(Writer);
            write.Start();

            return 0;
        }

        private void Reader()
        {
            DecompressedFile.Load(SrcFile.FullName, ref _src);
        }

        private void Compression()
        {
            foreach (var dataBlock in _src.GetConsumingEnumerable())
            {
                byte[] resultBytes;

                using (var memoryStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, false))
                    {
                        zipStream.Write(dataBlock.Data, 0, dataBlock.Size);
                    }

                    resultBytes = memoryStream.ToArray();
                }

                _dst.TryAdd(dataBlock.Index, new ShortBlock
                {
                    Size = resultBytes.Length,
                    Data = resultBytes
                });

                Console.WriteLine(
                    $"Блок: {dataBlock.Index}; исх: {dataBlock.Size}; новый: {resultBytes.Length}; разница: {dataBlock.Size - resultBytes.Length}; очередь: {_src.Count}");
            }
        }

        private void Writer()
        {
            var index = 0;

            using (var fileStream = new FileStream(DstFile.FullName, FileMode.Create, FileAccess.Write))
            {
                while (index < TotalBlocks)
                {
                    while (_dst.TryRemove(index, out var block))
                    {
                        fileStream.Write(block.Data, 0, block.Size);
                        index++;
                        Console.WriteLine(index);
                    }
                }
            }
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
                // SrcQueue?.Dispose();
            }
      
            _disposed = true;
        }
        #endregion
    }
}
