using System;
using System.IO;

namespace GZipTest
{
    /// <summary>
    /// Класс для разбора коммандной строки
    /// </summary>
    public class ArgsParser
    {
        private readonly string[] _args;

        #region Constructors
        public ArgsParser(string[] args)
        {
            _args = args;
        }
        #endregion

        public void TryParse(out FileInfo srcFile, out FileInfo dstFile, out bool mode)
        {
            //
            // Проверки синтаксиса
            //
            if (_args.Length != 3) throw new Exception("Parameters count error");

            if(_args[0] != "compress" && _args[0] != "decompress") throw new Exception("Unknown working mode");

            //
            // Режим работы
            //
            mode = _args[0] == "compress";

            //
            // Пробуем получить файлы
            //
            srcFile = new FileInfo(_args[1]);
            if (!srcFile.Exists) throw new Exception("Source file not found");

            dstFile = new FileInfo(_args[2]);
            if (dstFile.Exists) throw new Exception("Destination file already exists");
        }
    }
}
