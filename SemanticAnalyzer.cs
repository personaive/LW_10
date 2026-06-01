using System.Collections.Generic;

namespace Компилятор
{
    struct SymbolInfo
    {
        private string _name;
        private byte _type;
        private TextPosition _position;

        public SymbolInfo(string name, byte type, TextPosition position)
        {
            _name = name;
            _type = type;
            _position = position;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public byte Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public TextPosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }
    }

    class SemanticAnalyzer
    {
        public const byte TYPE_INTEGER = 1;
        public const byte TYPE_REAL = 2;
        public const byte TYPE_BOOLEAN = 3;
        public const byte TYPE_CHAR = 4;
        public const byte TYPE_STRING = 5;
        public const byte TYPE_ERROR = 0;

        private List<SymbolInfo> _symbolTable;
        private LexicalAnalyzer _lexer;

        public SemanticAnalyzer(LexicalAnalyzer lexer)
        {
            _symbolTable = new List<SymbolInfo>();
            _lexer = lexer;
        }

        public byte GetIdentifierType(string name)
        {
            string lowerName = name.ToLower();

            for (int i = 0; i < _symbolTable.Count; i = i + 1)
            {
                if (_symbolTable[i].Name == lowerName)
                {
                    return _symbolTable[i].Type;
                }
            }

            return TYPE_ERROR;
        }

        public bool IsDeclared(string name)
        {
            string lowerName = name.ToLower();

            for (int i = 0; i < _symbolTable.Count; i = i + 1)
            {
                if (_symbolTable[i].Name == lowerName)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddSymbol(string name, byte type, TextPosition position)
        {
            if (IsDeclared(name))
            {
                InputOutput.Error(215, position);
                return;
            }

            SymbolInfo info = new SymbolInfo(name.ToLower(), type, position);
            _symbolTable.Add(info);
        }

        public byte ConvertLexerTypeToSemanticType(byte lexerType)
        {
            if (lexerType == LexicalAnalyzer.integersy)
            {
                return TYPE_INTEGER;
            }
            if (lexerType == LexicalAnalyzer.realsy)
            {
                return TYPE_REAL;
            }
            if (lexerType == LexicalAnalyzer.booleansy)
            {
                return TYPE_BOOLEAN;
            }
            if (lexerType == LexicalAnalyzer.charsy)
            {
                return TYPE_CHAR;
            }
            if (lexerType == LexicalAnalyzer.stringsy)
            {
                return TYPE_STRING;
            }

            return TYPE_ERROR;
        }

        public void CheckAssignment(string name, byte expressionType, TextPosition position)
        {
            byte varType = GetIdentifierType(name);

            if (varType == TYPE_ERROR)
            {
                return;
            }

            if (varType == TYPE_REAL && expressionType == TYPE_INTEGER)
            {
                return;
            }

            if (varType != expressionType)
            {
                InputOutput.Error(216, position);
            }
        }

        public void CheckForVariable(string name, TextPosition position)
        {
            byte varType = GetIdentifierType(name);

            if (varType == TYPE_ERROR)
            {
                return;
            }

            if (varType != TYPE_INTEGER && varType != TYPE_CHAR)
            {
                InputOutput.Error(217, position);
            }
        }

        public void CheckDeclared(string name, TextPosition position)
        {
            if (!IsDeclared(name))
            {
                InputOutput.Error(218, position);
            }
        }

        public void Reset()
        {
            _symbolTable.Clear();
        }
    }
}