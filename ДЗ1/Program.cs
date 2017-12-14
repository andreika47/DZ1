using System;
using System.IO;

namespace ДЗ1
{
    enum FillType
    {
        Zero,          // Заполнение нулями
        Random          // Заполнение случайными байтами
    }

    // Класс для работы с файлами 
    static class FileExtensions
    {
        const int BUFFER_SIZE = 1024;

        // Заполнение файла нулями
        public static void FillZero(this FileStream file)
        {
            file.Seek(0, SeekOrigin.Begin);
            var bytesToRewrite = file.Length;
            var zeroArray = new byte[BUFFER_SIZE];
            while (bytesToRewrite > BUFFER_SIZE)
            {
                file.Write(zeroArray, 0, BUFFER_SIZE);
                bytesToRewrite -= BUFFER_SIZE;
            }
            file.Write(new byte[bytesToRewrite], 0, (int)bytesToRewrite);
        }

        // Заполнение файла случайными байтами
        public static void FillRandom(this FileStream file)
        {
            file.Seek(0, SeekOrigin.Begin);
            var bytesToRewrite = file.Length;
            var buffer = new byte[BUFFER_SIZE];
            var random = new Random();
            while (bytesToRewrite > BUFFER_SIZE)
            {
                random.NextBytes(buffer);               // Заполнение буфера случайными байтами
                file.Write(buffer, 0, BUFFER_SIZE);
                bytesToRewrite -= BUFFER_SIZE;
            }
            buffer = new byte[(int)bytesToRewrite];
            random.NextBytes(buffer);
            file.Write(new byte[bytesToRewrite], 0, (int)bytesToRewrite);
        }
    }

    // Класс, осуществляющий безопасное удаление файла/директории
    class FileShredder
    {

        public uint Iterations { get; set; }            // Число итераций перезаписи файла
        public FillType FillType { get; set; }          // Тип заполнения

        // Безопасное удаление директории
        private void _ShredDirectory(string path)
        {
            // Удаление всех файлов в директории
            foreach (var file in Directory.EnumerateFiles(path))
            {
                _ShredFile(file);
            }

            // Удаление всех поддиректорий
            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                _ShredDirectory(directory);
            }
            Directory.Delete(path);             // Удаление директории в ОС
        }

        // Безопасное удаление файла
        private void _ShredFile(string path)
        {
            using (var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                for (var i = Iterations; i > 0; i--)
                {
                    file.Seek(0, SeekOrigin.Begin);    // Переход в начало файла
                    switch (FillType)
                    {
                        case FillType.Zero:
                            file.FillZero();     // Заполнение нулями
                            break;
                        case FillType.Random:
                            file.FillRandom();    // Заполнение случайными байтами
                            break;
                    }
                    file.Flush();
                }
            }
            File.Delete(path);     // Удаление файла в ОС
        }

        // Безопасное удаление файла или директории
        public void Shred(string path)
        {
            if (Directory.Exists(path))
            {
                _ShredDirectory(path);
            }
            else if (File.Exists(path))
            {
                _ShredFile(path);
            }
            else
            {
                throw new FileNotFoundException(path);
            }
        }

        public FileShredder(uint iterations = 1, FillType fillType = FillType.Zero)
        {
            Iterations = iterations;
            FillType = FillType;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 3)
            {
                string path = "";
                uint iterations = 1;
                FillType fillType = FillType.Zero;

                path = args[0];
                iterations = uint.Parse(args[1]);
                fillType = args[2].Equals("0") ? FillType.Zero : FillType.Random; // если передан 0, то заполянем нулями, иначе случайными байтами

                var fileShredder = new FileShredder(iterations, fillType);
                try
                {
                    fileShredder.Shred(path);
                }
                catch (FileNotFoundException exception)
                {
                    Console.WriteLine($"File or directory with path {exception.Message} not found");
                }
            }
            else
            {
                Console.WriteLine("Wrong arguments!");
                return;
            }
        }
    }
}
