using System;

namespace Компилятор
{
    class LexicalAnalyzer
    {
        public const byte
            star = 21,
            slash = 60,
            equal = 16,
            comma = 20,
            semicolon = 14,
            colon = 5,
            point = 61,
            arrow = 62,
            leftpar = 9,
            rightpar = 4,
            lbracket = 11,
            rbracket = 12,
            flpar = 63,
            frpar = 64,
            later = 65,
            greater = 66,
            laterequal = 67,
            greaterequal = 68,
            latergreater = 69,
            plus = 70,
            minus = 71,
            lcomment = 72,
            rcomment = 73,
            assign = 51,
            twopoints = 74,
            ident = 2,
            floatc = 82,
            intc = 15,
            stringc = 83,
            charc = 84,
            casesy = 31,
            elsesy = 32,
            filesy = 57,
            gotosy = 33,
            thensy = 52,
            typesy = 34,
            untilsy = 53,
            dosy = 54,
            withsy = 37,
            ifsy = 56,
            insy = 100,
            ofsy = 101,
            orsy = 102,
            tosy = 103,
            endsy = 104,
            varsy = 105,
            divsy = 106,
            andsy = 107,
            notsy = 108,
            forsy = 109,
            modsy = 110,
            nilsy = 111,
            setsy = 112,
            beginsy = 113,
            whilesy = 114,
            arraysy = 115,
            constsy = 116,
            labelsy = 117,
            downtosy = 118,
            packedsy = 119,
            recordsy = 120,
            repeatsy = 121,
            programsy = 122,
            functionsy = 123,
            proceduresy = 124,
            integersy = 125,
            realsy = 126,
            booleansy = 127,
            charsy = 128,
            stringsy = 129;

        private const int MAX_IDENT_LENGTH = 127;
        private const int MAX_STRING_LENGTH = 255;

        private byte _symbol;
        private TextPosition _tokenPosition;
        private string _addressName;
        private int _integerValue;
        private double _floatValue;
        private char _charValue;
        private string _stringValue;

        private Keywords _keywords;

        private bool _savedToken;
        private byte _savedSymbol;

        public byte Symbol
        {
            get
            {
                return _symbol;
            }
        }

        public TextPosition TokenPosition
        {
            get
            {
                return _tokenPosition;
            }
        }

        public string AddressName
        {
            get
            {
                return _addressName;
            }
        }

        public int IntegerValue
        {
            get
            {
                return _integerValue;
            }
        }

        public double FloatValue
        {
            get
            {
                return _floatValue;
            }
        }

        public char CharValue
        {
            get
            {
                return _charValue;
            }
        }

        public string StringValue
        {
            get
            {
                return _stringValue;
            }
        }

        public LexicalAnalyzer()
        {
            _keywords = new Keywords();
            _symbol = 0;
            _tokenPosition = new TextPosition(0, 0);
            _addressName = string.Empty;
            _integerValue = 0;
            _floatValue = 0.0;
            _charValue = '\0';
            _stringValue = string.Empty;
            _savedToken = false;
            _savedSymbol = 0;
        }

        public byte NextSym()
        {
            if (_savedToken)
            {
                _savedToken = false;
                _symbol = _savedSymbol;
                return _symbol;
            }

            while (!InputOutput.EndOfFile && InputOutput.Ch == ' ')
            {
                InputOutput.NextCh();
            }

            if (InputOutput.EndOfFile)
            {
                _symbol = 0;
                return _symbol;
            }

            _tokenPosition.LineNumber = InputOutput.PositionNow.LineNumber;
            _tokenPosition.CharNumber = InputOutput.PositionNow.CharNumber;

            char currentChar = InputOutput.Ch;

            if (char.IsLetter(currentChar) || currentChar == '_')
            {
                return ScanIdentifierOrKeyword();
            }

            if (char.IsDigit(currentChar))
            {
                return ScanNumber();
            }

            if (currentChar == '\'')
            {
                return ScanCharOrString();
            }

            if (currentChar == '"')
            {
                return ScanString();
            }

            return ScanOperatorOrDelimiter(currentChar);
        }

        private byte ScanIdentifierOrKeyword()
        {
            string name = string.Empty;

            while (!InputOutput.EndOfFile &&
                   (char.IsLetterOrDigit(InputOutput.Ch) || InputOutput.Ch == '_'))
            {
                name = name + InputOutput.Ch;
                InputOutput.NextCh();
            }

            if (name.Length > MAX_IDENT_LENGTH)
            {
                InputOutput.Error(209, _tokenPosition);
                name = name.Substring(0, MAX_IDENT_LENGTH);
            }

            byte keywordCode = _keywords.FindKeyword(name);
            if (keywordCode > 0)
            {
                _symbol = keywordCode;
            }
            else
            {
                _symbol = ident;
                _addressName = name;
            }

            return _symbol;
        }

        private byte ScanNumber()
        {
            _integerValue = 0;
            bool overflow = false;

            while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch) && !overflow)
            {
                byte digit = (byte)(InputOutput.Ch - '0');

                if (_integerValue > Int16.MaxValue / 10 ||
                    (_integerValue == Int16.MaxValue / 10 && digit > Int16.MaxValue % 10))
                {
                    InputOutput.Error(203, _tokenPosition);
                    overflow = true;
                }
                else
                {
                    _integerValue = 10 * _integerValue + digit;
                    InputOutput.NextCh();
                }
            }

            if (overflow)
            {
                _integerValue = 0;
                while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
                {
                    InputOutput.NextCh();
                }
            }

            if (!InputOutput.EndOfFile && InputOutput.Ch == '.')
            {
                InputOutput.NextCh();

                if (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
                {
                    return ScanFloatFraction();
                }
                else
                {
                    _savedToken = true;
                    _savedSymbol = point;
                    _symbol = intc;
                    return _symbol;
                }
            }

            _symbol = intc;
            return _symbol;
        }

        private byte ScanFloatFraction()
        {
            double fraction = 0.0;
            double divisor = 10.0;

            while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
            {
                fraction = fraction + (InputOutput.Ch - '0') / divisor;
                divisor = divisor * 10.0;
                InputOutput.NextCh();
            }

            _floatValue = _integerValue + fraction;

            if (!InputOutput.EndOfFile && (InputOutput.Ch == 'e' || InputOutput.Ch == 'E'))
            {
                InputOutput.NextCh();
                ScanExponent();
            }

            _symbol = floatc;
            return _symbol;
        }

        private void ScanExponent()
        {
            int expSign = 1;
            int exponent = 0;

            if (!InputOutput.EndOfFile && (InputOutput.Ch == '+' || InputOutput.Ch == '-'))
            {
                if (InputOutput.Ch == '-')
                {
                    expSign = -1;
                }
                InputOutput.NextCh();
            }

            if (!InputOutput.EndOfFile && !char.IsDigit(InputOutput.Ch))
            {
                InputOutput.Error(206, InputOutput.PositionNow);
                return;
            }

            bool overflow = false;

            while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch) && !overflow)
            {
                exponent = 10 * exponent + (InputOutput.Ch - '0');
                if (exponent > 308)
                {
                    InputOutput.Error(206, InputOutput.PositionNow);
                    overflow = true;
                }
                else
                {
                    InputOutput.NextCh();
                }
            }

            if (overflow)
            {
                while (!InputOutput.EndOfFile && char.IsDigit(InputOutput.Ch))
                {
                    InputOutput.NextCh();
                }
                exponent = 0;
            }

            _floatValue = _floatValue * Math.Pow(10.0, expSign * exponent);
        }

        private byte ScanCharOrString()
        {
            InputOutput.NextCh();

            if (InputOutput.EndOfFile)
            {
                InputOutput.Error(205, _tokenPosition);
                _symbol = charc;
                _charValue = '\0';
                return _symbol;
            }

            if (InputOutput.Ch == '\'')
            {
                InputOutput.Error(205, _tokenPosition);
                InputOutput.NextCh();
                _symbol = charc;
                _charValue = '\0';
                return _symbol;
            }

            _charValue = InputOutput.Ch;
            InputOutput.NextCh();

            if (InputOutput.EndOfFile)
            {
                InputOutput.Error(205, _tokenPosition);
                _symbol = charc;
                return _symbol;
            }

            if (InputOutput.Ch == '\'')
            {
                InputOutput.NextCh();
                _symbol = charc;
                return _symbol;
            }

            InputOutput.Error(205, _tokenPosition);

            bool closed = false;
            while (!InputOutput.EndOfFile && !closed)
            {
                if (InputOutput.Ch == '\'')
                {
                    InputOutput.NextCh();
                    closed = true;
                }
                else if (InputOutput.EndOfLine)
                {
                    closed = true;
                    InputOutput.NextCh();
                }
                else
                {
                    InputOutput.NextCh();
                }
            }

            _symbol = charc;
            return _symbol;
        }

        private byte ScanString()
        {
            InputOutput.NextCh();

            _stringValue = string.Empty;
            bool stringClosed = false;
            bool overflow = false;
            bool errorClosed = false;

            while (!InputOutput.EndOfFile && !stringClosed && !overflow && !errorClosed)
            {
                if (InputOutput.Ch == '"')
                {
                    stringClosed = true;
                }
                else if (_stringValue.Length >= MAX_STRING_LENGTH)
                {
                    InputOutput.Error(205, _tokenPosition);
                    overflow = true;
                }
                else if (InputOutput.EndOfLine)
                {
                    InputOutput.Error(205, _tokenPosition);
                    errorClosed = true;
                    InputOutput.NextCh();
                }
                else
                {
                    _stringValue = _stringValue + InputOutput.Ch;
                    InputOutput.NextCh();
                }
            }

            if (overflow)
            {
                while (!InputOutput.EndOfFile && !stringClosed && !errorClosed)
                {
                    if (InputOutput.Ch == '"')
                    {
                        InputOutput.NextCh();
                        stringClosed = true;
                    }
                    else if (InputOutput.EndOfLine)
                    {
                        errorClosed = true;
                        InputOutput.NextCh();
                    }
                    else
                    {
                        InputOutput.NextCh();
                    }
                }
            }
            else if (stringClosed)
            {
                InputOutput.NextCh();
            }
            else if (!errorClosed)
            {
                InputOutput.Error(205, _tokenPosition);
            }

            _symbol = stringc;
            return _symbol;
        }

        private byte ScanOperatorOrDelimiter(char currentChar)
        {
            if (currentChar == '<')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == '=')
                {
                    _symbol = laterequal;
                    InputOutput.NextCh();
                }
                else if (!InputOutput.EndOfFile && InputOutput.Ch == '>')
                {
                    _symbol = latergreater;
                    InputOutput.NextCh();
                }
                else
                {
                    _symbol = later;
                }
            }
            else if (currentChar == '>')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == '=')
                {
                    _symbol = greaterequal;
                    InputOutput.NextCh();
                }
                else
                {
                    _symbol = greater;
                }
            }
            else if (currentChar == ':')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == '=')
                {
                    _symbol = assign;
                    InputOutput.NextCh();
                }
                else
                {
                    _symbol = colon;
                }
            }
            else if (currentChar == '.')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == '.')
                {
                    _symbol = twopoints;
                    InputOutput.NextCh();
                }
                else
                {
                    _symbol = point;
                }
            }
            else if (currentChar == ';')
            {
                _symbol = semicolon;
                InputOutput.NextCh();
            }
            else if (currentChar == ',')
            {
                _symbol = comma;
                InputOutput.NextCh();
            }
            else if (currentChar == '(')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == '*')
                {
                    _symbol = lcomment;
                    InputOutput.NextCh();
                    SkipComment();
                    return NextSym();
                }
                else
                {
                    _symbol = leftpar;
                }
            }
            else if (currentChar == ')')
            {
                _symbol = rightpar;
                InputOutput.NextCh();
            }
            else if (currentChar == '*')
            {
                InputOutput.NextCh();
                if (!InputOutput.EndOfFile && InputOutput.Ch == ')')
                {
                    _symbol = rcomment;
                    InputOutput.NextCh();
                }
                else
                {
                    _symbol = star;
                }
            }
            else if (currentChar == '{')
            {
                _symbol = flpar;
                InputOutput.NextCh();
                SkipCurlyComment();
                return NextSym();
            }
            else if (currentChar == '}')
            {
                _symbol = frpar;
                InputOutput.NextCh();
            }
            else if (currentChar == '+')
            {
                _symbol = plus;
                InputOutput.NextCh();
            }
            else if (currentChar == '-')
            {
                _symbol = minus;
                InputOutput.NextCh();
            }
            else if (currentChar == '/')
            {
                _symbol = slash;
                InputOutput.NextCh();
            }
            else if (currentChar == '=')
            {
                _symbol = equal;
                InputOutput.NextCh();
            }
            else if (currentChar == '^')
            {
                _symbol = arrow;
                InputOutput.NextCh();
            }
            else if (currentChar == '[')
            {
                _symbol = lbracket;
                InputOutput.NextCh();
            }
            else if (currentChar == ']')
            {
                _symbol = rbracket;
                InputOutput.NextCh();
            }
            else
            {
                _symbol = 0;
                InputOutput.Error(201, _tokenPosition);
                InputOutput.NextCh();
            }

            return _symbol;
        }

        private void SkipComment()
        {
            int level = 1;
            bool done = false;
            TextPosition commentStart = _tokenPosition;
            InputOutput.SuppressOutput = true;

            while (!done && level > 0 && !InputOutput.EndOfFile)
            {
                if (InputOutput.Ch == '(')
                {
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == '*')
                    {
                        level = level + 1;
                        InputOutput.NextCh();
                    }
                }
                else if (InputOutput.Ch == '*')
                {
                    InputOutput.NextCh();
                    if (!InputOutput.EndOfFile && InputOutput.Ch == ')')
                    {
                        level = level - 1;
                        InputOutput.NextCh();
                    }
                }
                else
                {
                    InputOutput.NextCh();
                }

                if (InputOutput.EndOfFile)
                {
                    done = true;
                }

                if (level <= 0)
                {
                    done = true;
                }
            }

            InputOutput.SuppressOutput = false;

            if (level > 0)
            {
                InputOutput.Error(204, commentStart);
            }
        }

        private void SkipCurlyComment()
        {
            int level = 1;
            bool done = false;
            TextPosition commentStart = _tokenPosition;
            InputOutput.SuppressOutput = true;

            while (!done && level > 0 && !InputOutput.EndOfFile)
            {
                if (InputOutput.Ch == '{')
                {
                    level = level + 1;
                    InputOutput.NextCh();
                }
                else if (InputOutput.Ch == '}')
                {
                    level = level - 1;
                    if (level > 0)
                    {
                        InputOutput.NextCh();
                    }
                }
                else
                {
                    InputOutput.NextCh();
                }

                if (InputOutput.EndOfFile)
                {
                    done = true;
                }

                if (level <= 0)
                {
                    done = true;
                }
            }

            InputOutput.SuppressOutput = false;

            if (level > 0)
            {
                InputOutput.Error(204, commentStart);
            }
            else
            {
                InputOutput.NextCh();
            }
        }
    }
}