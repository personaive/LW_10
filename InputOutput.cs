using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    struct TextPosition
    {
        private uint _lineNumber;
        private byte _charNumber;

        public TextPosition(uint ln, byte c)
        {
            _lineNumber = ln;
            _charNumber = c;
        }

        public uint LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                _lineNumber = value;
            }
        }

        public byte CharNumber
        {
            get
            {
                return _charNumber;
            }
            set
            {
                _charNumber = value;
            }
        }
    }

    struct Err
    {
        private TextPosition _errorPosition;
        private byte _errorCode;

        public Err(TextPosition pos, byte code)
        {
            _errorPosition = pos;
            _errorCode = code;
        }

        public TextPosition ErrorPosition
        {
            get
            {
                return _errorPosition;
            }
            set
            {
                _errorPosition = value;
            }
        }

        public byte ErrorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                _errorCode = value;
            }
        }
    }

    class InputOutput
    {
        private const byte ERRMAX = 9;

        private static char _ch;
        private static TextPosition _positionNow;
        private static List<Err> _errors;
        private static string _line;
        private static byte _lastInLine;
        private static uint _errorCount;
        private static StreamReader _file;
        private static bool _endOfFile;
        private static bool _linePrinted;
        private static bool _suppressOutput;

        public static char Ch
        {
            get
            {
                return _ch;
            }
            set
            {
                _ch = value;
            }
        }

        public static TextPosition PositionNow
        {
            get
            {
                return _positionNow;
            }
            set
            {
                _positionNow = value;
            }
        }

        public static List<Err> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
            }
        }

        public static bool EndOfFile
        {
            get
            {
                return _endOfFile;
            }
        }

        public static bool EndOfLine
        {
            get
            {
                return _positionNow.CharNumber == _lastInLine;
            }
        }

        public static bool SuppressOutput
        {
            get
            {
                return _suppressOutput;
            }
            set
            {
                _suppressOutput = value;
            }
        }

        public static uint ErrorCount
        {
            get
            {
                return _errorCount;
            }
        }

        public static void Initialize(StreamReader file)
        {
            _file = file;
            _errorCount = 0;
            _endOfFile = false;
            _linePrinted = false;
            _suppressOutput = false;
            _line = " ";
            _lastInLine = 0;
            _ch = ' ';
            _positionNow = new TextPosition(0, 0);
            _errors = new List<Err>();
            ReadNextLine();
            _positionNow.LineNumber = 1;
            _positionNow.CharNumber = 0;
            _ch = _line[0];
        }

        public static void NextCh()
        {
            if (_endOfFile)
            {
                return;
            }

            if (_positionNow.CharNumber == _lastInLine)
            {
                if (!_suppressOutput && !_linePrinted)
                {
                    Console.WriteLine($"{_positionNow.LineNumber,4}: {_line}");
                    _linePrinted = true;
                }

                if (!_suppressOutput && _errors.Count > 0)
                {
                    PrintErrors();
                }

                ReadNextLine();

                if (_endOfFile)
                {
                    return;
                }

                _positionNow.LineNumber = _positionNow.LineNumber + 1;
                _positionNow.CharNumber = 0;
                _linePrinted = false;
            }
            else
            {
                _positionNow.CharNumber = (byte)(_positionNow.CharNumber + 1);
            }

            _ch = _line[_positionNow.CharNumber];
        }

        public static void Close()
        {
            if (!_suppressOutput && !_linePrinted && _positionNow.LineNumber > 0)
            {
                Console.WriteLine($"{_positionNow.LineNumber,4}: {_line}");
                _linePrinted = true;
            }

            if (!_suppressOutput && _errors.Count > 0)
            {
                PrintErrors();
            }

            Console.WriteLine($"Компиляция завершена: ошибок — {_errorCount}!");
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        private static void ReadNextLine()
        {
            if (!_file.EndOfStream)
            {
                _line = _file.ReadLine() + " ";
                _lastInLine = (byte)(_line.Length - 1);
                _errors = new List<Err>();
            }
            else
            {
                _endOfFile = true;
            }
        }

        private static void PrintErrors()
        {
            foreach (Err item in _errors)
            {
                _errorCount = _errorCount + 1;

                string num = _errorCount < 10 ? $"0{_errorCount}" : $"{_errorCount}";

                int prefixLength = 6;
                int errorPrefixLength = 6;
                int spacesCount = prefixLength + item.ErrorPosition.CharNumber - errorPrefixLength;
                if (spacesCount < 0)
                {
                    spacesCount = 0;
                }

                string spaces = new string(' ', spacesCount);

                Console.WriteLine(
                    $"**{num}**{spaces}^ ошибка код {item.ErrorCode}: " +
                    $"{GetErrorMessage(item.ErrorCode)}");
            }
        }

        private static string GetErrorMessage(byte code)
        {
            if (code == 201)
            {
                return "Неожиданный символ";
            }
            if (code == 202)
            {
                return "Неверный идентификатор";
            }
            if (code == 203)
            {
                return "Целая константа превышает допустимый предел";
            }
            if (code == 204)
            {
                return "Незакрытый комментарий";
            }
            if (code == 205)
            {
                return "Ошибка в символьной/строковой константе";
            }
            if (code == 206)
            {
                return "Ошибка в вещественной константе";
            }
            if (code == 207)
            {
                return "Недопустимый символ в числе";
            }
            if (code == 208)
            {
                return "Конец файла внутри комментария";
            }
            if (code == 209)
            {
                return "Слишком длинный идентификатор";
            }
            if (code == 210)
            {
                return "Ожидался другой токен";
            }
            if (code == 211)
            {
                return "Ожидался тип (integer, real, boolean, char, string)";
            }
            if (code == 212)
            {
                return "Ожидался оператор";
            }
            if (code == 213)
            {
                return "Ожидался операнд";
            }
            if (code == 214)
            {
                return "Ожидалось to или downto";
            }
            if (code == 215)
            {
                return "Переменная уже объявлена";
            }
            if (code == 216)
            {
                return "Несовместимые типы в присваивании";
            }
            if (code == 217)
            {
                return "Переменная цикла for должна быть integer или char";
            }
            if (code == 218)
            {
                return "Необъявленная переменная";
            }
            return "Неизвестная ошибка";
        }

        public static void Error(byte code, TextPosition position)
        {
            if (_errors.Count <= ERRMAX)
            {
                _errors.Add(new Err(position, code));
            }
        }
    }
}