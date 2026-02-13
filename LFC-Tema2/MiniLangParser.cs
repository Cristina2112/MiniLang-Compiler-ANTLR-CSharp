using System;
using System.Collections;
using System.Collections.Generic;

namespace LFC_Tema2
{
    public class MiniLangParser
    {
        private ArrayList tokens;
        private int currentToken;
        private ArrayList errors;
        private FunctionDefNode currentFunctionNode;

        public MiniLangParser(ArrayList tokens)
        {
            this.tokens = tokens;
            this.currentToken = 0;
            this.errors = new ArrayList();
            this.currentFunctionNode = null;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            try
            {
                while (!IsAtEnd())
                {
                    if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
                    {
                        // verifica daca e functie sau declaratie variabila globala
                        if (PeekAhead(1) != null && ((Token)PeekAhead(1)).Type == "IDENTIFIER")
                        {
                            if (PeekAhead(2) != null && ((Token)PeekAhead(2)).Type == "LPAREN")
                            {
                                // functie
                                var func = ParseFunctionDef();
                                if (func != null)
                                    program.Functions.Add(func);
                            }
                            else if (PeekAhead(2) != null && (((Token)PeekAhead(2)).Type == "ASSIGN" || ((Token)PeekAhead(2)).Type == "COMMA" || ((Token)PeekAhead(2)).Type == "SEMICOLON"))
                            {
                                // variabila globala
                                var vars = ParseGlobalVarDecl();
                                if (vars != null)
                                {
                                    for (int i = 0; i < vars.Count; i++)
                                        program.GlobalVariables.Add(vars[i]);
                                }
                            }
                        }
                    }
                    else if (PeekType() == "VOID")
                    {
                        var func = ParseFunctionDef();
                        if (func != null)
                            program.Functions.Add(func);
                    }
                    else if (PeekType() == "CONST")
                    {
                        var vars = ParseGlobalVarDecl();
                        if (vars != null)
                        {
                            for (int i = 0; i < vars.Count; i++)
                                program.GlobalVariables.Add(vars[i]);
                        }
                    }
                    else
                    {
                        Advance();
                    }

                    if (currentToken >= tokens.Count)
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Parse error: {ex.Message}");
            }

            return program;
        }

        private FunctionDefNode ParseFunctionDef()
        {
            var node = new FunctionDefNode();
            currentFunctionNode = node;
            int startLine = CurrentToken().LineNumber;

            // tip returnat al functiei
            if (PeekType() == "VOID")
            {
                node.ReturnType = Advance().Lexeme;
            }
            else if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
            {
                node.ReturnType = Advance().Lexeme;
            }

            // numele functiei
            if (PeekType() == "IDENTIFIER")
            {
                node.FunctionName = Advance().Lexeme;
            }

            // lista parametri functie
            if (PeekType() == "LPAREN")
            {
                Advance();
                while (PeekType() != "RPAREN" && !IsAtEnd())
                {
                    var param = new Parameter();
                    if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
                    {
                        param.Type = Advance().Lexeme;
                        if (PeekType() == "IDENTIFIER")
                        {
                            param.Name = Advance().Lexeme;
                            node.Parameters.Add(param);
                        }
                    }

                    if (PeekType() == "COMMA")
                        Advance();
                    else
                        break;
                }

                if (PeekType() == "RPAREN")
                    Advance();
            }

            // corpul functiei
            if (PeekType() == "LBRACE")
            {
                Advance();
                while (PeekType() != "RBRACE" && !IsAtEnd())
                {
                    // declarare variabile sau statements
                    if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING" || PeekType() == "CONST")
                    {
                        var vars = ParseVarDecl();
                        if (vars != null)
                        {
                            for (int i = 0; i < vars.Count; i++)
                                node.LocalVariables.Add((VariableNode)vars[i]);
                        }
                    }
                    else
                    {
                        string stmtType = ParseStatementAndGetType();
                        if (!string.IsNullOrEmpty(stmtType))
                            node.Statements.Add(stmtType);
                    }
                }

                if (PeekType() == "RBRACE")
                    Advance();
            }

            node.LineNumber = startLine;
            currentFunctionNode = null;
            return node;
        }

        private ArrayList ParseGlobalVarDecl()
        {
            var variables = new ArrayList();
            bool isConst = false;

            if (PeekType() == "CONST")
            {
                isConst = true;
                Advance();
            }

            string type = null;
            if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
            {
                type = Advance().Lexeme;
            }

            while (PeekType() == "IDENTIFIER" || PeekType() == "COMMA")
            {
                if (PeekType() == "COMMA")
                {
                    Advance();
                    continue;
                }

                var variable = new VariableNode();
                variable.Type = type;
                variable.IsConstant = isConst;
                variable.Name = Advance().Lexeme;

                if (PeekType() == "ASSIGN")
                {
                    Advance();
                    variable.Value = ParseExpression();
                }

                variables.Add(variable);

                if (PeekType() != "COMMA")
                    break;
            }

            if (PeekType() == "SEMICOLON")
                Advance();

            return variables;
        }

        private ArrayList ParseVarDecl()
        {
            var variables = new ArrayList();
            bool isConst = false;

            if (PeekType() == "CONST")
            {
                isConst = true;
                Advance();
            }

            string type = null;
            if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
            {
                type = Advance().Lexeme;
            }

            while (PeekType() == "IDENTIFIER" || PeekType() == "COMMA")
            {
                if (PeekType() == "COMMA")
                {
                    Advance();
                    continue;
                }

                var variable = new VariableNode();
                variable.Type = type;
                variable.IsConstant = isConst;
                variable.Name = Advance().Lexeme;

                if (PeekType() == "ASSIGN")
                {
                    Advance();
                    variable.Value = ParseExpression();
                }

                variables.Add(variable);

                if (PeekType() != "COMMA")
                    break;
            }

            if (PeekType() == "SEMICOLON")
                Advance();

            return variables;
        }

        private void ParseStatement()
        {
            if (PeekType() == "IF")
                ParseIfStatement();
            else if (PeekType() == "WHILE")
                ParseWhileStatement();
            else if (PeekType() == "FOR")
                ParseForStatement();
            else if (PeekType() == "RETURN")
                ParseReturnStatement();
            else if (PeekType() == "IDENTIFIER" || PeekType() == "INCREMENT" || PeekType() == "DECREMENT")
                ParseAssignmentOrCall();
            else if (PeekType() == "SEMICOLON")
                Advance();
            else
                Advance();
        }
        // Metoda care parseaza statement si returneaza tipul acestuia
        private string ParseStatementAndGetType()
        {
            if (PeekType() == "IF")
            {
                ParseIfStatement();
                return "IF";
            }
            else if (PeekType() == "WHILE")
            {
                ParseWhileStatement();
                return "WHILE";
            }
            else if (PeekType() == "FOR")
            {
                ParseForStatement();
                return "FOR";
            }
            else if (PeekType() == "RETURN")
            {
                ParseReturnStatement();
                return "RETURN";
            }
            else if (PeekType() == "IDENTIFIER" || PeekType() == "INCREMENT" || PeekType() == "DECREMENT")
            {
                ParseAssignmentOrCall();
                return "ASSIGNMENT";
            }
            else if (PeekType() == "SEMICOLON")
            {
                Advance();
                return "EMPTY";
            }
            else
            {
                Advance();
                return "";
            }
        }
        private void ParseIfStatement()
        {
            if (PeekType() == "IF")
            {
                Advance();
                if (PeekType() == "LPAREN")
                {
                    Advance();
                    ParseExpression();
                    if (PeekType() == "RPAREN")
                        Advance();
                }

                if (PeekType() == "LBRACE")
                {
                    Advance();
                    while (PeekType() != "RBRACE" && !IsAtEnd())
                        ParseStatement();
                    if (PeekType() == "RBRACE")
                        Advance();
                }

                if (PeekType() == "ELSE")
                {
                    Advance();
                    if (PeekType() == "LBRACE")
                    {
                        Advance();
                        while (PeekType() != "RBRACE" && !IsAtEnd())
                            ParseStatement();
                        if (PeekType() == "RBRACE")
                            Advance();
                    }
                }
            }
        }

        private void ParseWhileStatement()
        {
            if (PeekType() == "WHILE")
            {
                Advance();
                if (PeekType() == "LPAREN")
                {
                    Advance();
                    ParseExpression();
                    if (PeekType() == "RPAREN")
                        Advance();
                }

                if (PeekType() == "LBRACE")
                {
                    Advance();
                    while (PeekType() != "RBRACE" && !IsAtEnd())
                        ParseStatement();
                    if (PeekType() == "RBRACE")
                        Advance();
                }
            }
        }

        private void ParseForStatement()
        {
            if (PeekType() == "FOR")
            {
                Advance();
                if (PeekType() == "LPAREN")
                {
                    Advance();
                    
                    if (PeekType() == "INT" || PeekType() == "FLOAT" || PeekType() == "DOUBLE" || PeekType() == "STRING")
                    {
                        var vars = ParseVarDecl();
                        if (vars != null && currentFunctionNode != null)
                        {
                            for (int i = 0; i < vars.Count; i++)
                                currentFunctionNode.LocalVariables.Add(vars[i]);
                        }
                    }
                    else
                    {
                        ParseExpression();
                        if (PeekType() == "SEMICOLON")
                            Advance();
                    }
                    
                    ParseExpression();
                    if (PeekType() == "SEMICOLON")
                        Advance();
                    ParseExpression();
                    if (PeekType() == "RPAREN")
                        Advance();
                }

                if (PeekType() == "LBRACE")
                {
                    Advance();
                    while (PeekType() != "RBRACE" && !IsAtEnd())
                        ParseStatement();
                    if (PeekType() == "RBRACE")
                        Advance();
                }
            }
        }

        private void ParseReturnStatement()
        {
            if (PeekType() == "RETURN")
            {
                int returnLine = CurrentToken().LineNumber;
                Advance();
                string expr = ParseExpression();
                if (currentFunctionNode != null)
                {
                    var returnNode = new ReturnNode();
                    returnNode.Expression = expr;
                    returnNode.LineNumber = returnLine;
                    currentFunctionNode.Returns.Add(returnNode);
                }
                if (PeekType() == "SEMICOLON")
                    Advance();
            }
        }

        private void ParseAssignmentOrCall()
        {
            if (PeekType() == "IDENTIFIER")
            {
                string name = CurrentToken().Lexeme;
                int lineNum = CurrentToken().LineNumber;
                Advance();
                if (PeekType() == "LPAREN")
                {
                    Advance();
                    var call = new FunctionCallNode();
                    call.FunctionName = name;
                    call.LineNumber = lineNum;
                    while (PeekType() != "RPAREN" && !IsAtEnd())
                    {
                        string arg = ParseExpression();
                        call.Arguments.Add(arg);
                        if (PeekType() == "COMMA")
                            Advance();
                        else
                            break;
                    }
                    if (PeekType() == "RPAREN")
                        Advance();
                    else
                        errors.Add($"Syntax error at line {lineNum}: Missing closing parenthesis ')' in function call '{name}'");
                    if (currentFunctionNode != null)
                        currentFunctionNode.FunctionCalls.Add(call);
                }
                else if (IsAssignmentOperator(PeekType()))
                {
                    string op = CurrentToken().Lexeme;
                    Advance();
                    ParseExpression();
                    if (currentFunctionNode != null)
                    {
                        var assign = new AssignmentNode();
                        assign.VariableName = name;
                        assign.Operator = op;
                        assign.LineNumber = lineNum;
                        currentFunctionNode.Assignments.Add(assign);
                    }
                }
                else if (PeekType() == "INCREMENT" || PeekType() == "DECREMENT")
                {
                    Advance();
                }
            }
            else if (PeekType() == "INCREMENT" || PeekType() == "DECREMENT")
            {
                Advance();
                if (PeekType() == "IDENTIFIER")
                    Advance();
            }

            if (PeekType() == "SEMICOLON")
                Advance();
            else if (PeekType() != "RBRACE" && PeekType() != "EOF" && !IsAtEnd())
            {
                Token curr = CurrentToken();
                if (curr != null)
                    errors.Add($"Syntax error at line {curr.LineNumber}: Missing semicolon ';'");
            }
        }

        private string ParseExpression()
        {
            var expr = "";
            int parenCount = 0;

            while (!IsAtEnd() && (parenCount > 0 || (PeekType() != "SEMICOLON" && PeekType() != "RPAREN" && PeekType() != "COMMA" && PeekType() != "RBRACE" && !IsStatementKeyword(PeekType()))))
            {
                if (PeekType() == "LPAREN")
                    parenCount++;
                else if (PeekType() == "RPAREN")
                {
                    if (parenCount == 0)
                        break;
                    parenCount--;
                }

                if (PeekType() == "IDENTIFIER" && currentFunctionNode != null)
                {
                    object next = PeekAhead(1);
                    if (next != null && ((Token)next).Type == "LPAREN")
                    {
                        string funcName = CurrentToken().Lexeme;
                        int funcLine = CurrentToken().LineNumber;
                        expr += CurrentToken().Lexeme + " ";
                        Advance();
                        expr += CurrentToken().Lexeme + " ";
                        Advance();
                        parenCount++;

                        var call = new FunctionCallNode();
                        call.FunctionName = funcName;
                        call.LineNumber = funcLine;

                        int argParenCount = 1;
                        string currentArg = "";
                        while (!IsAtEnd() && argParenCount > 0)
                        {
                            if (PeekType() == "LPAREN")
                                argParenCount++;
                            else if (PeekType() == "RPAREN")
                            {
                                argParenCount--;
                                if (argParenCount == 0)
                                {
                                    if (!string.IsNullOrEmpty(currentArg.Trim()))
                                        call.Arguments.Add(currentArg.Trim());
                                    expr += CurrentToken().Lexeme + " ";
                                    Advance();
                                    parenCount--;
                                    break;
                                }
                            }
                            else if (PeekType() == "COMMA" && argParenCount == 1)
                            {
                                call.Arguments.Add(currentArg.Trim());
                                currentArg = "";
                                expr += CurrentToken().Lexeme + " ";
                                Advance();
                                continue;
                            }

                            currentArg += CurrentToken().Lexeme + " ";
                            expr += CurrentToken().Lexeme + " ";
                            Advance();
                        }

                        currentFunctionNode.FunctionCalls.Add(call);
                        continue;
                    }
                    else
                    {
                        var usage = new VariableUsageNode();
                        usage.VariableName = CurrentToken().Lexeme;
                        usage.LineNumber = CurrentToken().LineNumber;
                        currentFunctionNode.VariableUsages.Add(usage);
                    }
                }

                expr += CurrentToken().Lexeme + " ";
                Advance();

                if (parenCount == 0 && (PeekType() == "SEMICOLON" || PeekType() == "RPAREN" || PeekType() == "COMMA" || PeekType() == "RBRACE" || IsStatementKeyword(PeekType())))
                    break;
            }

            return expr.Trim();
        }

        private bool IsAssignmentOperator(string tokenType)
        {
            return tokenType == "ASSIGN" || tokenType == "PLUS_ASSIGN" || tokenType == "MINUS_ASSIGN" || 
                   tokenType == "MUL_ASSIGN" || tokenType == "DIV_ASSIGN" || tokenType == "MOD_ASSIGN";
        }

        private bool IsStatementKeyword(string tokenType)
        {
            return tokenType == "IF" || tokenType == "ELSE" || tokenType == "WHILE" || tokenType == "FOR" || tokenType == "RETURN";
        }

        private Token CurrentToken()
        {
            return currentToken < tokens.Count ? (Token)tokens[currentToken] : null;
        }

        //tip token curent
        private string PeekType()
        {
            return currentToken < tokens.Count ? ((Token)tokens[currentToken]).Type : "EOF";
        }

        //verifica daca dupa token urm un nume de variabila
        private object PeekAhead(int offset)
        {
            return currentToken + offset < tokens.Count ? tokens[currentToken + offset] : null;
        }

        //trece la tokenul urmator
        private Token Advance()
        {
            return currentToken < tokens.Count ? (Token)tokens[currentToken++] : null;
        }

        //verifica daca am ajuns la finalul listei de tokeni
        private bool IsAtEnd()
        {
            return currentToken >= tokens.Count;
        }

        //lista erori
        public ArrayList GetErrors()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < errors.Count; i++)
                result.Add(errors[i]);
            return result;
        }
    }

    // AST 

    //radacina
    public class ProgramNode
    {
        public ArrayList Functions { get; set; }
        public ArrayList GlobalVariables { get; set; }

        public ProgramNode()
        {
            Functions = new ArrayList();
            GlobalVariables = new ArrayList();
        }
    }

    //nod pt functie
    public class FunctionDefNode
    {
        public string ReturnType { get; set; }
        public string FunctionName { get; set; }
        public ArrayList Parameters { get; set; }
        public ArrayList LocalVariables { get; set; }
        public ArrayList Statements { get; set; } = new ArrayList();
        public ArrayList FunctionCalls { get; set; } = new ArrayList();
        public ArrayList Assignments { get; set; } = new ArrayList();
        public ArrayList Returns { get; set; } = new ArrayList();
        public ArrayList VariableUsages { get; set; } = new ArrayList();


        public int LineNumber { get; set; }

        public FunctionDefNode()
        {
            Parameters = new ArrayList();
            LocalVariables = new ArrayList();
            Statements = new ArrayList();
            FunctionCalls = new ArrayList();
            Assignments = new ArrayList();
            Returns = new ArrayList();
            VariableUsages = new ArrayList();
        }
    }
    //nod pt parametru 
    public class Parameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    //nod pt variabila
    public class VariableNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsConstant { get; set; }

        public override string ToString()
        {
            string result = Type + " " + Name;
            if (Value != null)
                result += " = " + Value;
            return result;
        }
    }

    public class VariableUsageNode
    {
        public string VariableName { get; set; }
        public int LineNumber { get; set; }
    }

    public class FunctionCallNode
    {
        public string FunctionName { get; set; }
        public ArrayList Arguments { get; set; }
        public int LineNumber { get; set; }

        public FunctionCallNode()
        {
            Arguments = new ArrayList();
        }
    }

    public class AssignmentNode
    {
        public string VariableName { get; set; }
        public string Operator { get; set; }
        public int LineNumber { get; set; }
    }

    public class ReturnNode
    {
        public string Expression { get; set; }
        public int LineNumber { get; set; }
    }
}
