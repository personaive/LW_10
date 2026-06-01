using System.Collections.Generic;

namespace Компилятор
{
    class Keywords
    {
        private Dictionary<byte, Dictionary<string, byte>> _keywordTable;

        public Dictionary<byte, Dictionary<string, byte>> KeywordTable
        {
            get
            {
                return _keywordTable;
            }
        }

        public Keywords()
        {
            _keywordTable = new Dictionary<byte, Dictionary<string, byte>>();

            Dictionary<string, byte> tmp;

            tmp = new Dictionary<string, byte>();
            tmp["do"] = LexicalAnalyzer.dosy;
            tmp["if"] = LexicalAnalyzer.ifsy;
            tmp["in"] = LexicalAnalyzer.insy;
            tmp["of"] = LexicalAnalyzer.ofsy;
            tmp["or"] = LexicalAnalyzer.orsy;
            tmp["to"] = LexicalAnalyzer.tosy;
            _keywordTable[2] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["end"] = LexicalAnalyzer.endsy;
            tmp["var"] = LexicalAnalyzer.varsy;
            tmp["div"] = LexicalAnalyzer.divsy;
            tmp["and"] = LexicalAnalyzer.andsy;
            tmp["not"] = LexicalAnalyzer.notsy;
            tmp["for"] = LexicalAnalyzer.forsy;
            tmp["mod"] = LexicalAnalyzer.modsy;
            tmp["nil"] = LexicalAnalyzer.nilsy;
            tmp["set"] = LexicalAnalyzer.setsy;
            _keywordTable[3] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["then"] = LexicalAnalyzer.thensy;
            tmp["else"] = LexicalAnalyzer.elsesy;
            tmp["case"] = LexicalAnalyzer.casesy;
            tmp["file"] = LexicalAnalyzer.filesy;
            tmp["goto"] = LexicalAnalyzer.gotosy;
            tmp["type"] = LexicalAnalyzer.typesy;
            tmp["with"] = LexicalAnalyzer.withsy;
            tmp["char"] = LexicalAnalyzer.charsy;
            tmp["real"] = LexicalAnalyzer.realsy;
            _keywordTable[4] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["begin"] = LexicalAnalyzer.beginsy;
            tmp["while"] = LexicalAnalyzer.whilesy;
            tmp["array"] = LexicalAnalyzer.arraysy;
            tmp["const"] = LexicalAnalyzer.constsy;
            tmp["label"] = LexicalAnalyzer.labelsy;
            tmp["until"] = LexicalAnalyzer.untilsy;
            _keywordTable[5] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["downto"] = LexicalAnalyzer.downtosy;
            tmp["packed"] = LexicalAnalyzer.packedsy;
            tmp["record"] = LexicalAnalyzer.recordsy;
            tmp["repeat"] = LexicalAnalyzer.repeatsy;
            tmp["string"] = LexicalAnalyzer.stringsy;
            _keywordTable[6] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["program"] = LexicalAnalyzer.programsy;
            tmp["integer"] = LexicalAnalyzer.integersy;
            tmp["boolean"] = LexicalAnalyzer.booleansy;
            _keywordTable[7] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["function"] = LexicalAnalyzer.functionsy;
            _keywordTable[8] = tmp;

            tmp = new Dictionary<string, byte>();
            tmp["procedure"] = LexicalAnalyzer.proceduresy;
            _keywordTable[9] = tmp;
        }

        public byte FindKeyword(string name)
        {
            string lowerName = name.ToLower();
            byte length = (byte)lowerName.Length;

            if (_keywordTable.ContainsKey(length))
            {
                if (_keywordTable[length].ContainsKey(lowerName))
                {
                    return _keywordTable[length][lowerName];
                }
            }

            return 0;
        }
    }
}