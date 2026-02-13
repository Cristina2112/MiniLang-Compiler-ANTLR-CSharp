using System;
using System.Collections;

namespace LFC_Tema2
{

    //se asigură că numele există și sunt unice
    public class SymbolTable
    {
        private ArrayList globalVariables;
        private ArrayList functions;
        private ArrayList errors;

        public SymbolTable()
        {
            globalVariables = new ArrayList();
            functions = new ArrayList();
            errors = new ArrayList();
        }

        //  management variabile globale
        public void AddGlobalVariable(Symbol symbol)
        {
            for (int i = 0; i < globalVariables.Count; i++)
            {
                Symbol existing = (Symbol)globalVariables[i];
                if (existing.Name == symbol.Name)
                {
                    errors.Add($"Error: Global variable '{symbol.Name}' already declared at line {symbol.LineNumber}");
                    return;
                }
            }
            globalVariables.Add(symbol);
        }

        public Symbol GetGlobalVariable(string name)
        {
            for (int i = 0; i < globalVariables.Count; i++)
            {
                Symbol sym = (Symbol)globalVariables[i];
                if (sym.Name == name)
                    return sym;
            }
            return null;
        }

        public ArrayList GetAllGlobalVariables()
        {
            return new ArrayList(globalVariables);
        }

        // management functii
        public void AddFunction(FunctionSymbol function)
        {
            string key = GenerateFunctionKey(function.Name, function.Parameters);
            for (int i = 0; i < functions.Count; i++)
            {
                FunctionSymbol existing = (FunctionSymbol)functions[i];
                string existingKey = GenerateFunctionKey(existing.Name, existing.Parameters);
                if (existingKey == key)
                {
                    errors.Add($"Error: Function '{function.Name}' with same parameters already declared at line {function.LineNumber}");
                    return;
                }
            }
            functions.Add(function);
        }

        public FunctionSymbol GetFunction(string name, ArrayList paramTypes)
        {
            string key = GenerateFunctionKey(name, paramTypes);
            for (int i = 0; i < functions.Count; i++)
            {
                FunctionSymbol func = (FunctionSymbol)functions[i];
                string funcKey = GenerateFunctionKey(func.Name, paramTypes);
                if (funcKey == key)
                    return func;
            }
            return null;
        }

        public FunctionSymbol GetFunctionByName(string name)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                FunctionSymbol func = (FunctionSymbol)functions[i];
                if (func.Name == name)
                    return func;
            }
            return null;
        }

        public ArrayList GetAllFunctions()
        {
            return new ArrayList(functions);
        }

        private string GenerateFunctionKey(string name, ArrayList parameters)
        {
            string paramStr = "";
            for (int i = 0; i < parameters.Count; i++)
            {
                Symbol param = (Symbol)parameters[i];
                if (i > 0)
                    paramStr += ",";
                paramStr += param.Type;
            }
            return $"{name}({paramStr})";
        }

      
        public void AddError(string error)
        {
            errors.Add(error);
        }

        public ArrayList GetErrors()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < errors.Count; i++)
            {
                result.Add(errors[i]);
            }
            return result;
        }

        public bool HasErrors()
        {
            return errors.Count > 0;
        }

        public void ClearErrors()
        {
            errors.Clear();
        }

        // validare functie main unica
        public bool ValidateMainFunction()
        {
            int mainCount = 0;
            for (int i = 0; i < functions.Count; i++)
            {
                FunctionSymbol func = (FunctionSymbol)functions[i];
                if (func.IsMain)
                    mainCount++;
            }

            if (mainCount == 0)
            {
                errors.Add("Error: Program must contain exactly one 'main' function");
                return false;
            }
            if (mainCount > 1)
            {
                errors.Add("Error: Program must contain exactly one 'main' function");
                return false;
            }
            return true;
        }

        // verificare daca o functie este definita
        public bool IsFunctionDefined(string name)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                FunctionSymbol func = (FunctionSymbol)functions[i];
                if (func.Name == name)
                    return true;
            }
            return false;
        }
    }
}
