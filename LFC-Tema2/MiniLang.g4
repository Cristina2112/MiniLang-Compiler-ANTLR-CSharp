grammar MiniLang;

// Parser rules
program: (functionDef | globalVarDecl)* EOF;

functionDef: returnType IDENTIFIER LPAREN paramList? RPAREN LBRACE functionBody RBRACE;

returnType: TYPE | VOID;

paramList: param (COMMA param)*;

param: TYPE IDENTIFIER;

functionBody: (varDecl | statement)*;

globalVarDecl: (CONST)? TYPE varList SEMICOLON;

varList: varInit (COMMA varInit)*;

varInit: IDENTIFIER (ASSIGN expression)?;

varDecl: (CONST)? TYPE varList SEMICOLON;

statement: assignment SEMICOLON
         | ifStmt
         | whileStmt
         | forStmt
         | returnStmt SEMICOLON
         | functionCall SEMICOLON
         | SEMICOLON;

assignment: IDENTIFIER (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MUL_ASSIGN | DIV_ASSIGN | MOD_ASSIGN) expression
          | IDENTIFIER (INCREMENT | DECREMENT);

ifStmt: IF LPAREN expression RPAREN LBRACE statement* RBRACE (ELSE LBRACE statement* RBRACE)?;

whileStmt: WHILE LPAREN expression RPAREN LBRACE statement* RBRACE;

forStmt: FOR LPAREN assignment? SEMICOLON expression? SEMICOLON assignment? RPAREN LBRACE statement* RBRACE;

returnStmt: RETURN expression?;

functionCall: IDENTIFIER LPAREN argList? RPAREN;

argList: expression (COMMA expression)*;

expression: logicalOr;

logicalOr: logicalAnd (LOGICAL_OR logicalAnd)*;

logicalAnd: relational (LOGICAL_AND relational)*;

relational: additive ((EQ | NEQ | LT | GT | LTE | GTE) additive)*;

additive: multiplicative ((PLUS | MINUS) multiplicative)*;

multiplicative: unary ((MUL | DIV | MOD) unary)*;

unary: (NOT | MINUS | PLUS)* primary;

primary: LPAREN expression RPAREN
        | NUMBER
        | STRING
        | IDENTIFIER
        | functionCall
        | INCREMENT IDENTIFIER
        | DECREMENT IDENTIFIER
        | IDENTIFIER INCREMENT
        | IDENTIFIER DECREMENT;

// Lexer rules
TYPE: 'int' | 'float' | 'double' | 'string';
VOID: 'void';
CONST: 'const';
IF: 'if';
ELSE: 'else';
FOR: 'for';
WHILE: 'while';
RETURN: 'return';

LPAREN: '(';
LBRACE: '{';
RPAREN: ')';
RBRACE: '}';
SEMICOLON: ';';
COMMA: ',';

ASSIGN: '=';
PLUS_ASSIGN: '+=';
MINUS_ASSIGN: '-=';
MUL_ASSIGN: '*=';
DIV_ASSIGN: '/=';
MOD_ASSIGN: '%=';

PLUS: '+';
MINUS: '-';
MUL: '*';
DIV: '/';
MOD: '%';

LT: '<';
GT: '>';
LTE: '<=';
GTE: '>=';
EQ: '==';
NEQ: '!=';

LOGICAL_AND: '&&';
LOGICAL_OR: '||';
NOT: '!';

INCREMENT: '++';
DECREMENT: '--';

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
NUMBER: [0-9]+ ('.' [0-9]+)?;
STRING: '"' (~["\\\r\n] | '\\' .)* '"';

COMMENT_LINE: '//' ~[\r\n]* -> skip;
COMMENT_BLOCK: '/*' .*? '*/' -> skip;
WS: [ \t\r\n]+ -> skip;
