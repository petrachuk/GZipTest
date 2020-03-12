using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace GZipTest
{
    public class Processing : IDisposable
    {
        private const int BlockSize = 1048576;

        private static BlockingCollection<DataBlock> SrcQueue { get; set; }
        private static BlockingCollection<DataBlock> DstQueue { get; set; }
        bool _disposed;

        private FileInfo SrcFile { get; set; }
        

        public Processing()
        {
            SrcQueue = new BlockingCollection<DataBlock>(Environment.ProcessorCount + 1);
            DstQueue = new BlockingCollection<DataBlock>();
            SrcFile = new FileInfo("D:\\Distrib\\O365HomePremRetail.img");
            // SrcFile = new FileInfo("D:\\Temp\\voyna-i-mir-tom-1.txt");
        }

        public int Start()
        {
            // Загрузка файла в память
            var read = new Thread(Reader);
            read.Start();
            
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

            return 0;
        }

        private void Reader()
        {
            using (var fileStream = new FileStream(SrcFile.FullName, FileMode.Open))
            {
                var index = 0;

                while (fileStream.Position < fileStream.Length)
                {
                    var array = new byte[BlockSize];
                    var length = fileStream.Read(array, 0, BlockSize);

                    SrcQueue.Add(new DataBlock
                    {
                        Index = index,
                        Size = length,
                        Data = array
                    });

                    index++;
                }

                SrcQueue.CompleteAdding();
            }
        }

        private static void Compression()
        {
            while (!SrcQueue.IsCompleted)
            {
                DataBlock dataBlock;

                try
                {
                    dataBlock = SrcQueue.Take();
                }
                catch
                {
                    Console.WriteLine("!!!");
                    return;
                }

                byte[] resultBytes;

                using (var memoryStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, false))
                    {
                        zipStream.Write(dataBlock.Data, 0, dataBlock.Size);
                    }

                    resultBytes = memoryStream.ToArray();
                }

                /* DstQueue.Add(new DataBlock
                {
                    Index = dataBlock.Index,
                    Size = resultBytes.Length,
                    Data = resultBytes
                }); */

                Console.WriteLine(
                    $"Блок: {dataBlock.Index}; исх: {dataBlock.Size}; новый: {resultBytes.Length}; разница: {dataBlock.Size - resultBytes.Length}; очередь: {SrcQueue.Count}");

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
                SrcQueue?.Dispose();
                DstQueue?.Dispose();
            }
      
            _disposed = true;
        }
        #endregion
    }
}
