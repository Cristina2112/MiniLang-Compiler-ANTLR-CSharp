using System;
using System.Collections;

namespace LFC_Tema2
{
    //se asigură că operațiile pe care le faci cu acele nume sunt valide dpdv matematic și logic
    public class TypeChecker
    {
        private SymbolTable symbolTable;
        private ArrayList errors;

        public TypeChecker(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
            this.errors = new ArrayList();
        }

        public bool IsNumericType(string type)
        {
            return type == "int" || type == "float" || type == "double";
        }

        public bool IsCompatibleType(string sourceType, string targetType)
        {
            if (sourceType == targetType)
                return true;

            if (IsNumericType(sourceType) && IsNumericType(targetType))
                return true;

            return false;
        }

        public void CheckTypeCompatibility(string sourceType, string targetType, int lineNumber)
        {
            if (!IsCompatibleType(sourceType, targetType))
            {
                errors.Add("Type error at line " + lineNumber + ": Cannot convert from '" + sourceType + "' to '" + targetType + "'");
            }
        }

        public void CheckVariableDeclaration(string varName, string declaredType, string initializationValue, int lineNumber)
        {
            if (initializationValue == null)
                return;

            string valueType = DetermineExpressionType(initializationValue);
            if (!IsCompatibleType(valueType, declaredType))
            {
                errors.Add("Type error at line " + lineNumber + ": Cannot initialize '" + declaredType + "' with '" + valueType + "'");
            }
        }

        public string DetermineExpressionType(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return "unknown";

            double dummy;
            if (double.TryParse(expression, out dummy))
            {
                return expression.Contains(".") ? "double" : "int";
            }

            if (expression.StartsWith("\"") && expression.EndsWith("\""))
                return "string";

            return "unknown";
        }

        public ArrayList GetErrors()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < errors.Count; i++)
                result.Add(errors[i]);
            return result;
        }

        public void AddError(string error)
        {
            errors.Add(error);
        }

        public void ClearErrors()
        {
            errors.Clear();
        }
    }
}
