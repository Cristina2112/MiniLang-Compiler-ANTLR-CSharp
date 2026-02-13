using System;
using System.Collections;

namespace LFC_Tema2
{
    public class CompilerVisitor
    {
        private SymbolTable symbolTable;
        private TypeChecker typeChecker;
        private ArrayList tokens;
        private ArrayList errors;
        private ArrayList globalVars;
        private ArrayList functions;
        private ArrayList localVars;
        private FunctionSymbol currentFunction;

        public CompilerVisitor()
        {
            symbolTable = new SymbolTable();
            typeChecker = new TypeChecker(symbolTable);
            tokens = new ArrayList();
            errors = new ArrayList();
            globalVars = new ArrayList();
            functions = new ArrayList();
            localVars = new ArrayList();
            currentFunction = null;
        }

        //parcurge AST construit de parser (nu modif clasele nodurilor)
        public void Visit(ProgramNode program)
        {
            // pas1 colecteza variabile globale si functii
            for (int i = 0; i < program.GlobalVariables.Count; i++)
            {
                VariableNode variable = (VariableNode)program.GlobalVariables[i];
                VisitGlobalVariable(variable);
            }

            for (int i = 0; i < program.Functions.Count; i++)
            {
                FunctionDefNode function = (FunctionDefNode)program.Functions[i];
                VisitFunctionDefinition(function);
            }

            // pas 2 validare semantica
            ValidateSemantics(program);
        }

        private void VisitGlobalVariable(VariableNode variable)
        {
            var symbol = new Symbol(variable.Name, variable.Type, variable.Value, variable.IsConstant);
            symbolTable.AddGlobalVariable(symbol);
            globalVars.Add(variable.ToString());
        }

        private void VisitFunctionDefinition(FunctionDefNode funcDef)
        {
            var funcSymbol = new FunctionSymbol(funcDef.FunctionName, funcDef.ReturnType, funcDef.LineNumber);

            // adauga parametri
            for (int i = 0; i < funcDef.Parameters.Count; i++)
            {
                Parameter param = (Parameter)funcDef.Parameters[i];
                var paramSymbol = new Symbol(param.Name, param.Type);
                funcSymbol.Parameters.Add(paramSymbol);
            }

            // adauga variab locale
            for (int i = 0; i < funcDef.LocalVariables.Count; i++)
            {
                VariableNode localVar = (VariableNode)funcDef.LocalVariables[i];
                var varSymbol = new Symbol(localVar.Name, localVar.Type, localVar.Value, localVar.IsConstant);
                funcSymbol.LocalVariables.Add(varSymbol);
            }

            symbolTable.AddFunction(funcSymbol);
            functions.Add(funcSymbol.ToString());
        }

        private void ValidateSemantics(ProgramNode program)
        {
            ArrayList firstPassErrors = symbolTable.GetErrors();
            for (int i = 0; i < firstPassErrors.Count; i++)
            {
                errors.Add(firstPassErrors[i]);
            }

            symbolTable.ValidateMainFunction();

            for (int i = 0; i < program.GlobalVariables.Count; i++)
            {
                VariableNode variable = (VariableNode)program.GlobalVariables[i];
                ValidateVariableDeclaration(variable);
            }

            for (int i = 0; i < program.Functions.Count; i++)
            {
                FunctionDefNode function = (FunctionDefNode)program.Functions[i];
                currentFunction = symbolTable.GetFunctionByName(function.FunctionName);
                ValidateFunctionDefinition(function);
            }

            functions.Clear();
            ArrayList allFuncs = symbolTable.GetAllFunctions();
            for (int i = 0; i < allFuncs.Count; i++)
            {
                functions.Add(((FunctionSymbol)allFuncs[i]).ToString());
            }

            ArrayList typeErrors = typeChecker.GetErrors();
            for (int i = 0; i < typeErrors.Count; i++)
            {
                errors.Add(typeErrors[i]);
            }

            ArrayList symTableErrors = symbolTable.GetErrors();
            for (int i = 0; i < symTableErrors.Count; i++)
            {
                if (!errors.Contains(symTableErrors[i]))
                {
                    errors.Add(symTableErrors[i]);
                }
            }
        }
        private void ValidateVariableDeclaration(VariableNode variable)
        {
            if (variable.Value != null)
            {
                typeChecker.CheckVariableDeclaration(variable.Name, variable.Type, variable.Value, 0);
            }
        }

        private void ValidateFunctionDefinition(FunctionDefNode funcDef)
        {
            var funcSymbol = symbolTable.GetFunctionByName(funcDef.FunctionName);
            if (funcSymbol == null)
                return;

            string[] paramNames = new string[funcDef.Parameters.Count];
            for (int i = 0; i < funcDef.Parameters.Count; i++)
            {
                string paramName = ((Parameter)funcDef.Parameters[i]).Name;
                for (int j = 0; j < i; j++)
                {
                    if (paramNames[j] == paramName)
                    {
                        errors.Add($"Error: Duplicate parameter '{paramName}' in function '{funcDef.FunctionName}' at line {funcDef.LineNumber}");
                    }
                }
                paramNames[i] = paramName;
            }

            string[] localVarNames = new string[funcDef.LocalVariables.Count];
            for (int i = 0; i < funcDef.LocalVariables.Count; i++)
            {
                string localVarName = ((VariableNode)funcDef.LocalVariables[i]).Name;
                
                for (int j = 0; j < funcDef.Parameters.Count; j++)
                {
                    if (paramNames[j] == localVarName)
                    {
                        errors.Add($"Error: Local variable '{localVarName}' conflicts with parameter in function '{funcDef.FunctionName}' at line {funcDef.LineNumber}");
                    }
                }

                for (int j = 0; j < i; j++)
                {
                    if (localVarNames[j] == localVarName)
                    {
                        errors.Add($"Error: Duplicate local variable '{localVarName}' in function '{funcDef.FunctionName}' at line {funcDef.LineNumber}");
                    }
                }
                localVarNames[i] = localVarName;

                var localVar = (VariableNode)funcDef.LocalVariables[i];
                if (localVar.Value != null)
                {
                    typeChecker.CheckVariableDeclaration(localVar.Name, localVar.Type, localVar.Value, 0);
                }
            }

            if (funcDef.ReturnType != "void")
            {
                if (funcDef.Returns.Count == 0)
                {
                    errors.Add($"Error: Function '{funcDef.FunctionName}' must have a return statement at line {funcDef.LineNumber}");
                }
            }

            for (int i = 0; i < funcDef.Returns.Count; i++)
            {
                ReturnNode ret = (ReturnNode)funcDef.Returns[i];
                if (funcDef.ReturnType == "void" && !string.IsNullOrEmpty(ret.Expression))
                {
                    errors.Add($"Error: void function '{funcDef.FunctionName}' cannot return a value at line {ret.LineNumber}");
                }
                else if (funcDef.ReturnType != "void" && !string.IsNullOrEmpty(ret.Expression))
                {
                    string returnType = typeChecker.DetermineExpressionType(ret.Expression);
                    if (returnType != "unknown" && !typeChecker.IsCompatibleType(returnType, funcDef.ReturnType))
                    {
                        errors.Add($"Error: Return type mismatch in function '{funcDef.FunctionName}' at line {ret.LineNumber}");
                    }
                }
            }

            for (int i = 0; i < funcDef.FunctionCalls.Count; i++)
            {
                FunctionCallNode call = (FunctionCallNode)funcDef.FunctionCalls[i];
                ValidateFunctionCall(call, funcDef.FunctionName);
            }

            for (int i = 0; i < funcDef.Assignments.Count; i++)
            {
                AssignmentNode assign = (AssignmentNode)funcDef.Assignments[i];
                ValidateAssignment(assign, funcDef);
            }

            for (int i = 0; i < funcDef.VariableUsages.Count; i++)
            {
                VariableUsageNode usage = (VariableUsageNode)funcDef.VariableUsages[i];
                ValidateVariableUsage(usage, funcDef);
            }

            if (funcDef.FunctionName == "main")
            {
                if (funcDef.ReturnType != "void")
                {
                    errors.Add($"Error: main function must return void at line {funcDef.LineNumber}");
                }
                if (funcDef.Parameters.Count > 0)
                {
                    errors.Add($"Error: main function cannot have parameters at line {funcDef.LineNumber}");
                }
            }
        }

        private void ValidateFunctionCall(FunctionCallNode call, string callerName)
        {
            if (call.FunctionName == "main")
            {
                errors.Add($"Error: main function cannot be called at line {call.LineNumber}");
                return;
            }

            FunctionSymbol calledFunc = symbolTable.GetFunctionByName(call.FunctionName);
            if (calledFunc == null)
            {
                errors.Add($"Error: Function '{call.FunctionName}' is not defined at line {call.LineNumber}");
                return;
            }

            if (calledFunc.Parameters.Count != call.Arguments.Count)
            {
                errors.Add($"Error: Function '{call.FunctionName}' expects {calledFunc.Parameters.Count} arguments but got {call.Arguments.Count} at line {call.LineNumber}");
                return;
            }

            for (int i = 0; i < call.Arguments.Count; i++)
            {
                string argValue = (string)call.Arguments[i];
                string argType = typeChecker.DetermineExpressionType(argValue);
                Symbol param = (Symbol)calledFunc.Parameters[i];
                if (argType != "unknown" && !typeChecker.IsCompatibleType(argType, param.Type))
                {
                    errors.Add($"Error: Argument type mismatch for parameter '{param.Name}' in call to '{call.FunctionName}' at line {call.LineNumber}");
                }
            }

            if (call.FunctionName == callerName)
            {
                FunctionSymbol callerFunc = symbolTable.GetFunctionByName(callerName);
                if (callerFunc != null)
                    callerFunc.IsRecursive = true;
            }
        }

        private void ValidateAssignment(AssignmentNode assign, FunctionDefNode funcDef)
        {
            Symbol varSymbol = FindVariable(assign.VariableName, funcDef);
            if (varSymbol == null)
            {
                errors.Add($"Error: Variable '{assign.VariableName}' is not declared at line {assign.LineNumber}");
                return;
            }

            if (varSymbol.IsConstant)
            {
                errors.Add($"Error: Cannot assign to constant variable '{assign.VariableName}' at line {assign.LineNumber}");
            }
        }

        private Symbol FindVariable(string name, FunctionDefNode funcDef)
        {
            for (int i = 0; i < funcDef.LocalVariables.Count; i++)
            {
                VariableNode v = (VariableNode)funcDef.LocalVariables[i];
                if (v.Name == name)
                    return new Symbol(v.Name, v.Type, v.Value, v.IsConstant);
            }

            for (int i = 0; i < funcDef.Parameters.Count; i++)
            {
                Parameter p = (Parameter)funcDef.Parameters[i];
                if (p.Name == name)
                    return new Symbol(p.Name, p.Type);
            }

            return symbolTable.GetGlobalVariable(name);
        }

        private void ValidateVariableUsage(VariableUsageNode usage, FunctionDefNode funcDef)
        {
            Symbol varSymbol = FindVariable(usage.VariableName, funcDef);
            if (varSymbol == null)
            {
                errors.Add($"Error: Variable '{usage.VariableName}' is not declared at line {usage.LineNumber}");
            }
        }

        public void ProcessTokens(ArrayList tokenList)
        {
            for (int i = 0; i < tokenList.Count; i++)
            {
                tokens.Add(((Token)tokenList[i]).ToString());
            }
        }

        public ArrayList GetTokens()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < tokens.Count; i++)
                result.Add(tokens[i]);
            return result;
        }

        public ArrayList GetGlobalVariables()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < globalVars.Count; i++)
                result.Add(globalVars[i]);
            return result;
        }

        public ArrayList GetFunctions()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < functions.Count; i++)
                result.Add(functions[i]);
            return result;
        }

        public ArrayList GetErrors()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < errors.Count; i++)
                result.Add(errors[i]);
            return result;
        }
    }
}
