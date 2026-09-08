/*
 * Forensic Extraction and Representation parser grammar
 * Allan Korol
 */

parser grammar CFEARParser;
options {tokenVocab=FEARLexer;}

program
  : collectorStatement collectorUse* blockStatement+ EOF
  ;

enumDeclaration
    : Enum identifier '{' enumConstants? ';' '}'
    ;

enumConstants
    : enumConstant (',' enumConstant)*
    ;

enumConstant
    : identifier arguments?
    ;


fieldDeclaration
    : typeType variableDeclarators ';'
    ;

variableDeclarators
    : variableDeclarator (',' variableDeclarator)*
    ;

variableDeclarator
    : variableDeclaratorId ('=' variableInitializer)?
    ;

variableDeclaratorId
    : identifier ('[' ']')*
    ;

variableInitializer
    : arrayInitializer
    | expression
    ;

arrayInitializer
    : '{' (variableInitializer (',' variableInitializer)* ','? )? '}'
    ;

literal
    : integerLiteral
    | floatLiteral
    | CHAR_LITERAL
    | STRING_LITERAL
    | BOOL_LITERAL
    | NULL_LITERAL
    ;

integerLiteral
    : DECIMAL_LITERAL
    | HEX_LITERAL
    | OCT_LITERAL
    | BINARY_LITERAL
    ;

floatLiteral
    : FLOAT_LITERAL
    | HEX_FLOAT_LITERAL
    ;

block
    : '{' blockStatement* '}'
    ;

blockStatement
    : localVariableDeclaration ';'
    | statement
    ;

localVariableDeclaration
    : typeType variableDeclarators
    ;

identifier
    : IDENTIFIER
    | primitiveTypeIdentifiers
    ;

diskSearchStatement
    : Search OpenParen folder=STRING_LITERAL Comma match=STRING_LITERAL CloseParen ';'
    ;

networkSearchStatement
    : Search OpenParen bpf=STRING_LITERAL CloseParen ';'
    ;

acceptsStatement
    : Accepts OpenParen STRING_LITERAL CloseParen ';'
    ;

collectorStatement
    : Define Disk collectorDefinition ';' diskSearchStatement
    |   Define Memory collectorDefinition ';' acceptsStatement
    | Define Network collectorDefinition ';' networkSearchStatement
    ;

collectorDefinition
    : Collector OpenParen STRING_LITERAL Comma STRING_LITERAL Comma STRING_LITERAL CloseParen
    ;

collectorUse
  : Collector identifier Assign Collector OpenParen STRING_LITERAL CloseParen ';'
  ;

interpreterStatement
  : Interpreter_ identifier Assign Interpreter_ OpenParen STRING_LITERAL CloseParen ';'
  ;

moduleStatement
  : Module identifier Assign Module OpenParen STRING_LITERAL CloseParen ';'
  ;

statement
    : blockLabel=block
    | interpreterStatement
    | moduleStatement
    | If parExpression statement (Else statement)?
    | For '(' forControl ')' statement
    | Foreach '(' foreachControl ')' statement
    | While parExpression statement
    | Return expression? ';'
    | Break identifier? ';'
    | Continue identifier? ';'
    | SemiColon
    | statementExpression=expression ';'
    | identifierLabel=identifier ':' statement
    ;

    
forControl
    : forInit? ';' expression? ';' forUpdate=expressionList?
    ;

forInit
    : localVariableDeclaration
    | expressionList
    ;

foreachControl
    :typeType variableDeclaratorId 'in' expression
    ;

parExpression
    : '(' expression ')'
    ;

expressionList
    : expression (',' expression)*
    ;

methodCall
    : identifier arguments
    ;

expression
    : primary
    | New primitiveType '(' ')'
    | expression '[' expression ']'
    | expression bop='.'
      (
         identifier
       | methodCall
      )
    | methodCall
    | expression postfix=('++' | '--')
    | prefix=('+'|'-'|'++'|'--'|'~'|'!') expression
    | expression bop=('*'|'/'|'%') expression  
    | expression bop=('+'|'-') expression 
    | expression ('<' '<' | '>' '>' '>' | '>' '>') expression 
    | expression bop=('<=' | '>=' | '>' | '<') expression 
    | expression bop=('==' | '!=') expression  
    | expression bop='&' expression
    | expression bop='^' expression  
    | expression bop='|' expression  
    | expression bop='&&' expression 
    | expression bop='||' expression 
    | <assoc=right> expression bop='?' expression ':' expression 
    | <assoc=right> expression
      bop='='
      expression

    ;

primary
    : '(' expression ')'
    | literal
    | identifier
    ;


typeType
    : primitiveType ('[' ']')*
    | identifier
    ;

primitiveType
    : Boolean
    | Char
    | Byte
    | Short
    | Int
    | Long
    | Float
    | Double
    | Object
    | List
    | Result
    | String
    ;

primitiveTypeIdentifiers
    : Result
    | List
    ;
    
arguments
    : '(' expressionList? ')'
    ;