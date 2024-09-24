grammar Nx;

parse: block EOF;

block: stat*;

stat:
    var_declaration
    | if_stat
    | while_stat
    | return
    | expr
    | OTHER { throw new System.Exception("Unknown Character: " + $OTHER.text); };

var_declaration: LET ID ASSIGN expr;

if_stat:
    IF expr stat_block 
    ( ELSE IF expr stat_block )*
    ( ELSE stat_block )?;

stat_block: OBRACE block CBRACE | stat;

while_stat: WHILE expr stat_block;

obj_literal: OBRACE (atom COLON expr (COMMA atom COLON expr)*)? COMMA? CBRACE;

array_literal: OBRACK (expr (COMMA expr)*)? CBRACK;

fn_literal : FN (ID (COMMA ID)*)? LAMBDA stat_block;

return : RETURN expr;

// https://en.cppreference.com/w/c/language/operator_precedence
expr:
    ID OPAR (expr (COMMA expr)*)? CPAR       # fnCallExpr
    | expr OBRACK expr CBRACK                # indexExpr // Maybe use atom here for the first expr
    | expr DOT ID                            # memberExpr // Maybe use atom here for the first expr
    | array_literal                          # arrayExpr
    | obj_literal                            # objExpr
    | fn_literal                             # fnExpr
    | <assoc=right> expr POW expr            # powExpr
    | MINUS expr                             # unaryMinusExpr
    | NOT expr                               # notExpr
    | expr op = (MULT | DIV | MOD) expr      # multiplicationExpr
    | expr op = (PLUS | MINUS) expr          # additiveExpr
    | expr op = (LTEQ | GTEQ | LT | GT) expr # relationalExpr
    | expr op = (EQ | NEQ) expr              # equalityExpr
    | expr AND expr                          # andExpr
    | expr OR expr                           # orExpr
    | <assoc=right> expr ASSIGN expr         # assignExpr 
    | atom                                   # atomExpr;

atom:
    OPAR expr CPAR   # parExpr
    | (INT | FLOAT)  # numberAtom
    | (TRUE | FALSE) # boolAtom
    | ID             # idAtom
    | STRING         # stringAtom
    | NIL            # nilAtom;

OR       : '||' ;
AND      : '&&' ;
EQ       : '==' ;
NEQ      : '!=' ;
GT       : '>' ;
LT       : '<' ;
GTEQ     : '>=' ;
LTEQ     : '<=' ;
PLUS     : '+' ;
MINUS    : '-' ;
MULT     : '*' ;
DIV      : '/' ;
MOD      : '%' ;
POW      : '^' ;
NOT      : '!' ;

DOT      : '.' ;
COMMA    : ',' ;
COLON    : ':' ;
ASSIGN   : '=' ;
OPAR     : '(' ;
CPAR     : ')' ;
OBRACE   : '{' ;
CBRACE   : '}' ;
OBRACK   : '[' ;
CBRACK   : ']' ;

LAMBDA   : '->' ;

TRUE     : 'true' ;
FALSE    : 'false' ;
NIL      : 'nil' ;
IF       : 'if' ;
ELSE     : 'else' ;
WHILE    : 'while' ;
LOG      : 'log' ;
FN       : 'fn' ;
RETURN   : 'ret' ;
LET      : 'let' ;

ID       : [a-zA-Z_] [a-zA-Z_0-9]* ;
INT      : [0-9]+ ;
FLOAT    : [0-9]+ '.' [0-9]* | '.' [0-9]+ ;
STRING   : '"' (~["\r\n] | '""')* '"' ;
COMMENT  : '//' ~[\r\n]* -> skip ;
SPACE    : [ \t\r\n] -> skip ;
OTHER    : . ;