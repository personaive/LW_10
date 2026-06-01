using System;
using System.Collections.Generic;

namespace Компилятор
{
    class Parser
    {
        private LexicalAnalyzer _lexer;
        private byte _currentToken;
        private SemanticAnalyzer _semantic;

        public Parser(LexicalAnalyzer lexer)
        {
            _lexer = lexer;
            _currentToken = 0;
            _semantic = new SemanticAnalyzer(lexer);
        }

        public void Parse()
        {
            NextToken();
            ParseProgram();
        }

        private void NextToken()
        {
            _currentToken = _lexer.NextSym();
        }

        private bool CurrentIs(byte token)
        {
            return _currentToken == token;
        }

        private void Expect(byte token)
        {
            if (CurrentIs(token))
            {
                NextToken();
            }
            else
            {
                TextPosition pos = InputOutput.PositionNow;
                pos.LineNumber = _lexer.TokenPosition.LineNumber;
                pos.CharNumber = _lexer.TokenPosition.CharNumber;
                InputOutput.Error(210, pos);
                SkipToSynchronization();
            }
        }

        private void SkipToSynchronization()
        {
            while (!InputOutput.EndOfFile &&
                   !CurrentIs(LexicalAnalyzer.semicolon) &&
                   !CurrentIs(LexicalAnalyzer.endsy) &&
                   !CurrentIs(LexicalAnalyzer.beginsy) &&
                   !CurrentIs(LexicalAnalyzer.untilsy) &&
                   _currentToken != 0)
            {
                NextToken();
            }

            if (CurrentIs(LexicalAnalyzer.semicolon))
            {
                NextToken();
            }
        }

        private void ParseProgram()
        {
            if (CurrentIs(LexicalAnalyzer.programsy))
            {
                NextToken();
            }

            if (CurrentIs(LexicalAnalyzer.ident))
            {
                NextToken();
            }

            if (CurrentIs(LexicalAnalyzer.semicolon))
            {
                NextToken();
            }

            ParseBlock();

            if (CurrentIs(LexicalAnalyzer.point))
            {
                NextToken();
            }
        }

        private void ParseBlock()
        {
            if (CurrentIs(LexicalAnalyzer.varsy))
            {
                ParseVarDeclaration();
            }

            if (CurrentIs(LexicalAnalyzer.beginsy))
            {
                ParseCompoundStatement();
            }
        }

        private void ParseVarDeclaration()
        {
            Expect(LexicalAnalyzer.varsy);

            bool hasMoreVars = true;

            while (CurrentIs(LexicalAnalyzer.ident) && hasMoreVars)
            {
                List<string> names = ParseIdentListWithNames();
                Expect(LexicalAnalyzer.colon);
                byte type = ParseType();
                Expect(LexicalAnalyzer.semicolon);

                for (int i = 0; i < names.Count; i = i + 1)
                {
                    _semantic.AddSymbol(names[i], type, _lexer.TokenPosition);
                }

                if (!CurrentIs(LexicalAnalyzer.ident))
                {
                    hasMoreVars = false;
                }
            }
        }

        private List<string> ParseIdentListWithNames()
        {
            List<string> names = new List<string>();

            if (CurrentIs(LexicalAnalyzer.ident))
            {
                names.Add(_lexer.AddressName);
                NextToken();
            }

            while (CurrentIs(LexicalAnalyzer.comma))
            {
                NextToken();

                if (CurrentIs(LexicalAnalyzer.ident))
                {
                    names.Add(_lexer.AddressName);
                    NextToken();
                }
            }

            return names;
        }

        private byte ParseType()
        {
            byte lexerType = _currentToken;

            if (CurrentIs(LexicalAnalyzer.integersy))
            {
                NextToken();
            }
            else if (CurrentIs(LexicalAnalyzer.realsy))
            {
                NextToken();
            }
            else if (CurrentIs(LexicalAnalyzer.booleansy))
            {
                NextToken();
            }
            else if (CurrentIs(LexicalAnalyzer.charsy))
            {
                NextToken();
            }
            else if (CurrentIs(LexicalAnalyzer.stringsy))
            {
                NextToken();
            }
            else
            {
                TextPosition pos = InputOutput.PositionNow;
                pos.LineNumber = _lexer.TokenPosition.LineNumber;
                pos.CharNumber = _lexer.TokenPosition.CharNumber;
                InputOutput.Error(211, pos);
                return SemanticAnalyzer.TYPE_ERROR;
            }

            return _semantic.ConvertLexerTypeToSemanticType(lexerType);
        }

        private void ParseCompoundStatement()
        {
            Expect(LexicalAnalyzer.beginsy);

            if (!CurrentIs(LexicalAnalyzer.endsy))
            {
                ParseStatement();

                bool hasMoreStatements = true;

                while (CurrentIs(LexicalAnalyzer.semicolon) && hasMoreStatements)
                {
                    NextToken();

                    if (CurrentIs(LexicalAnalyzer.endsy))
                    {
                        hasMoreStatements = false;
                    }
                    else
                    {
                        ParseStatement();
                    }
                }
            }

            Expect(LexicalAnalyzer.endsy);
        }

        private void ParseStatement()
        {
            if (CurrentIs(LexicalAnalyzer.ident))
            {
                ParseAssignment();
            }
            else if (CurrentIs(LexicalAnalyzer.forsy))
            {
                ParseForStatement();
            }
            else if (CurrentIs(LexicalAnalyzer.whilesy))
            {
                ParseWhileStatement();
            }
            else if (CurrentIs(LexicalAnalyzer.repeatsy))
            {
                ParseRepeatStatement();
            }
            else if (CurrentIs(LexicalAnalyzer.beginsy))
            {
                ParseCompoundStatement();
            }
            else if (CurrentIs(LexicalAnalyzer.ifsy))
            {
                ParseIfStatement();
            }
            else
            {
                TextPosition pos = InputOutput.PositionNow;
                pos.LineNumber = _lexer.TokenPosition.LineNumber;
                pos.CharNumber = _lexer.TokenPosition.CharNumber;
                InputOutput.Error(212, pos);
                SkipToSynchronization();
            }
        }

        private void ParseAssignment()
        {
            string varName = string.Empty;
            TextPosition varPosition = _lexer.TokenPosition;

            if (CurrentIs(LexicalAnalyzer.ident))
            {
                varName = _lexer.AddressName;
                _semantic.CheckDeclared(varName, varPosition);
                NextToken();
            }

            Expect(LexicalAnalyzer.assign);
            byte exprType = ParseExpression();
            _semantic.CheckAssignment(varName, exprType, varPosition);
        }

        private byte ParseExpression()
        {
            byte leftType = ParseSimpleExpression();

            if (CurrentIs(LexicalAnalyzer.equal) ||
                CurrentIs(LexicalAnalyzer.later) ||
                CurrentIs(LexicalAnalyzer.greater) ||
                CurrentIs(LexicalAnalyzer.laterequal) ||
                CurrentIs(LexicalAnalyzer.greaterequal) ||
                CurrentIs(LexicalAnalyzer.latergreater))
            {
                NextToken();
                ParseSimpleExpression();
                return SemanticAnalyzer.TYPE_BOOLEAN;
            }

            return leftType;
        }

        private byte ParseSimpleExpression()
        {
            byte leftType = ParseTerm();

            bool hasMoreOps = true;

            while (hasMoreOps &&
                   (CurrentIs(LexicalAnalyzer.plus) ||
                    CurrentIs(LexicalAnalyzer.minus) ||
                    CurrentIs(LexicalAnalyzer.orsy)))
            {
                byte op = _currentToken;
                NextToken();
                byte rightType = ParseTerm();

                if (op == LexicalAnalyzer.orsy)
                {
                    leftType = SemanticAnalyzer.TYPE_BOOLEAN;
                }
                else if (leftType == SemanticAnalyzer.TYPE_REAL ||
                         rightType == SemanticAnalyzer.TYPE_REAL)
                {
                    leftType = SemanticAnalyzer.TYPE_REAL;
                }
            }

            return leftType;
        }

        private byte ParseTerm()
        {
            byte leftType = ParseFactor();

            bool hasMoreOps = true;

            while (hasMoreOps &&
                   (CurrentIs(LexicalAnalyzer.star) ||
                    CurrentIs(LexicalAnalyzer.slash) ||
                    CurrentIs(LexicalAnalyzer.divsy) ||
                    CurrentIs(LexicalAnalyzer.modsy) ||
                    CurrentIs(LexicalAnalyzer.andsy)))
            {
                byte op = _currentToken;
                NextToken();
                byte rightType = ParseFactor();

                if (op == LexicalAnalyzer.andsy)
                {
                    leftType = SemanticAnalyzer.TYPE_BOOLEAN;
                }
                else if (leftType == SemanticAnalyzer.TYPE_REAL ||
                         rightType == SemanticAnalyzer.TYPE_REAL)
                {
                    leftType = SemanticAnalyzer.TYPE_REAL;
                }
            }

            return leftType;
        }

        private byte ParseFactor()
        {
            if (CurrentIs(LexicalAnalyzer.ident))
            {
                string name = _lexer.AddressName;
                TextPosition pos = _lexer.TokenPosition;
                _semantic.CheckDeclared(name, pos);
                byte type = _semantic.GetIdentifierType(name);
                NextToken();
                return type;
            }
            else if (CurrentIs(LexicalAnalyzer.intc))
            {
                NextToken();
                return SemanticAnalyzer.TYPE_INTEGER;
            }
            else if (CurrentIs(LexicalAnalyzer.floatc))
            {
                NextToken();
                return SemanticAnalyzer.TYPE_REAL;
            }
            else if (CurrentIs(LexicalAnalyzer.charc))
            {
                NextToken();
                return SemanticAnalyzer.TYPE_CHAR;
            }
            else if (CurrentIs(LexicalAnalyzer.leftpar))
            {
                NextToken();
                byte type = ParseExpression();
                Expect(LexicalAnalyzer.rightpar);
                return type;
            }
            else if (CurrentIs(LexicalAnalyzer.notsy))
            {
                NextToken();
                ParseFactor();
                return SemanticAnalyzer.TYPE_BOOLEAN;
            }
            else
            {
                TextPosition pos = InputOutput.PositionNow;
                pos.LineNumber = _lexer.TokenPosition.LineNumber;
                pos.CharNumber = _lexer.TokenPosition.CharNumber;
                InputOutput.Error(213, pos);
                return SemanticAnalyzer.TYPE_ERROR;
            }
        }

        private void ParseForStatement()
        {
            Expect(LexicalAnalyzer.forsy);

            string varName = string.Empty;
            TextPosition varPosition = _lexer.TokenPosition;

            if (CurrentIs(LexicalAnalyzer.ident))
            {
                varName = _lexer.AddressName;
                _semantic.CheckDeclared(varName, varPosition);
                _semantic.CheckForVariable(varName, varPosition);
                NextToken();
            }

            Expect(LexicalAnalyzer.assign);
            ParseExpression();

            if (CurrentIs(LexicalAnalyzer.tosy) ||
                CurrentIs(LexicalAnalyzer.downtosy))
            {
                NextToken();
            }
            else
            {
                TextPosition pos = InputOutput.PositionNow;
                pos.LineNumber = _lexer.TokenPosition.LineNumber;
                pos.CharNumber = _lexer.TokenPosition.CharNumber;
                InputOutput.Error(214, pos);
            }

            ParseExpression();
            Expect(LexicalAnalyzer.dosy);
            ParseStatement();
        }

        private void ParseWhileStatement()
        {
            Expect(LexicalAnalyzer.whilesy);
            ParseExpression();
            Expect(LexicalAnalyzer.dosy);
            ParseStatement();
        }

        private void ParseRepeatStatement()
        {
            Expect(LexicalAnalyzer.repeatsy);

            if (!CurrentIs(LexicalAnalyzer.untilsy))
            {
                ParseStatement();

                bool hasMoreStatements = true;

                while (CurrentIs(LexicalAnalyzer.semicolon) && hasMoreStatements)
                {
                    NextToken();

                    if (CurrentIs(LexicalAnalyzer.untilsy))
                    {
                        hasMoreStatements = false;
                    }
                    else
                    {
                        ParseStatement();
                    }
                }
            }

            Expect(LexicalAnalyzer.untilsy);
            ParseExpression();
        }

        private void ParseIfStatement()
        {
            Expect(LexicalAnalyzer.ifsy);
            ParseExpression();
            Expect(LexicalAnalyzer.thensy);
            ParseStatement();

            if (CurrentIs(LexicalAnalyzer.elsesy))
            {
                NextToken();
                ParseStatement();
            }
        }
    }
}