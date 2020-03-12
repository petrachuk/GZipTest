using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class ResultWriter
    {
        // Не Queue
        private static List<ResultBlock> Queue { get; set; } = new List<ResultBlock>();

        private long blockNumber;

        public ResultWriter(long blockNumber)
        {
            this.blockNumber = blockNumber;
        }

        public void Add(DataBlock dataBlock)
        {
            var tempFileName = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(dataBlock.Data, 0, dataBlock.Size);
            }

            Queue.Add(new ResultBlock { Index = dataBlock.Index, Size = dataBlock.Size, Path = tempFileName });

            if (Queue.Count == blockNumber) CompleteAdding();
        }

        public void CompleteAdding()
        {
            //
            // Формирование заголовка файла
            //
            string header = string.Empty;

            foreach (var item in Queue.OrderBy(x => x.Index))
            {
                header += item.Size + ";";
            }

            header = header.Substring(0, header.Length - 1);

            ///
            /// Переведем заголовок в байты
            ///
            var headerBytes = Encoding.ASCII.GetBytes(header);

            var headerLength = headerBytes.Length;
            byte[] headerLengthBytes = BitConverter.GetBytes(headerLength);
            if (BitConverter.IsLittleEndian) Array.Reverse(headerLengthBytes);

            using (var fileStream = new FileStream("C:\\Temp\\archive.my", FileMode.Create, FileAccess.Write))
            {
                // Запишем длину заголовка
                fileStream.Write(headerLengthBytes, 0, headerLengthBytes.Length);

                // Запишем заголовок
                fileStream.Write(headerBytes, 0, headerBytes.Length);

                foreach (var i in Queue.OrderBy(x => x.Index))
                {
                    using (var srcFileStream = new FileStream(i.Path, FileMode.Open))
                    {
                        var tmp = new byte[srcFileStream.Length];
                        srcFileStream.Read(tmp, 0, tmp.Length);
                        fileStream.Write(tmp, 0, tmp.Length);
                    }

                    File.Delete(i.Path);
                }
            }

            using (var fileStream = new FileStream("C:\\Temp\\archive.my", FileMode.Open))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    byte[] dlinna = new byte[4];
                    fileStream.Read(dlinna, 0, 4);
                    if (BitConverter.IsLittleEndian) Array.Reverse(dlinna);
                    int dlinnaZagolovka = BitConverter.ToInt32(dlinna, 0);
                    Console.WriteLine(dlinnaZagolovka);

                    byte[] zagolovokBytes = new byte[dlinnaZagolovka];
                    fileStream.Read(zagolovokBytes, 0, dlinnaZagolovka);
                    var zagolovok = System.Text.Encoding.ASCII.GetString(zagolovokBytes);
                    Console.WriteLine(zagolovok);

                    foreach (var i in zagolovok.Split(';'))
                    {
                        var blockLenght = int.Parse(i);

                        byte[] tmp = new byte[blockLenght];
                        fileStream.Read(tmp, 0, blockLenght);
                        Console.WriteLine(System.Text.Encoding.ASCII.GetString(tmp));


                    }
                }

            }

        }
    }
}
