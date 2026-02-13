using System;
using System.Collections;

namespace LFC_Tema2
{
    //variabila
    public class Symbol
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsConstant { get; set; }
        public int LineNumber { get; set; }

        public Symbol(string name, string type, string value = null, bool isConstant = false, int lineNumber = 0)
        {
            Name = name;
            Type = type;
            Value = value;
            IsConstant = isConstant;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            string result = $"{Type} {Name}";
            if (Value != null)
                result += $" = {Value}";
            return result;
        }
    }
    //functie
    public class FunctionSymbol
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public ArrayList Parameters { get; set; }
        public ArrayList LocalVariables { get; set; }
        public ArrayList ControlStructures { get; set; }
        public bool IsMain { get; set; }
        public bool IsRecursive { get; set; }
        public int LineNumber { get; set; }

        public FunctionSymbol(string name, string returnType, int lineNumber = 0)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = new ArrayList();
            LocalVariables = new ArrayList();
            ControlStructures = new ArrayList();
            IsMain = name == "main";
            IsRecursive = false;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            string funcType = IsMain ? "main" : "non-main";
            string recursiveType = IsRecursive ? "recursive" : "iterative";

            string result = $"[{funcType}, {recursiveType}] {ReturnType} {Name}(";
            for (int i = 0; i < Parameters.Count; i++)
            {
                Symbol param = (Symbol)Parameters[i];
                result += param.Type + " " + param.Name;
                if (i < Parameters.Count - 1)
                    result += ", ";
            }
            result += ")";
            return result;
        }
    }
}
