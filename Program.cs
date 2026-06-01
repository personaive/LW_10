using System;
using System.IO;

namespace Компилятор
{
    class Program
    {
        static void Main()
        {
            Console.Write("Введите путь к файлу: ");
            string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден.");
                return;
            }

            StreamReader file = new StreamReader(path);
            InputOutput.Initialize(file);

            LexicalAnalyzer lexer = new LexicalAnalyzer();
            Parser parser = new Parser(lexer);

            Console.WriteLine();
            parser.Parse();

            InputOutput.Close();

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}