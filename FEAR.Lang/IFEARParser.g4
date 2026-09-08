/*
 * Forensic Extraction and Representation parser grammar
 * Allan Korol
 */

parser grammar IFEARParser;
options {tokenVocab=FEARLexer;}

program
    : interpreter_definition interpreter_source EOF
    ;

interpreter_source
    : statement*
    ;

fieldDeclaration
    : Internal variableDeclarators ';'
    ;

variableDeclarators
    : variableDeclaratorId (',' variableDeclaratorId)*
    ;

variableDeclaratorId
    : IDENTIFIER
    ;

statement
    : interpreter_use
    | codify_line
    | fieldDeclaration
    | interpreterStatement
    | skipStatement
    | assignment_line
    ;

codify_line
    : Codify (Object | Property) variable_reference As (uri_identifier)+ ';'
    ;

assignment_line
    : variable_reference Assign assignment_action ';'
    ;

assignment_action
    : (variable_reference | numericLiteral | stringLiteral) (assignment_function assignment_action)?
    ;

assignment_function
    : (Plus | Minus)
    ;

uri_identifier
    : (Colon IDENTIFIER)
    ;

interpreter_definition
    : Define Interpreter_ OpenParen stringLiteral Comma stringLiteral Comma stringLiteral CloseParen ';'
    ;

skipStatement
    : InterpreterSkip Colon numericLiteral ';'
    ;

interpreterStatement
    : variable_reference Assign numericLiteral? Colon numericLiteral Colon type_specifier Colon ('L' | 'B')? ';'
    ;

type_specifier
    : type_conversion
    | IDENTIFIER
    ;

type_conversion
: IDENTIFIER TypeConversion IDENTIFIER
;

interpreter_use
    : Interpreter_ variable_reference Assign Interpreter_ OpenParen stringLiteral CloseParen ';'
    ;

stringLiteral
    : STRING_LITERAL
    ; 
		
variable_reference
    : IDENTIFIER
    ;

numericLiteral
    : '-'? DECIMAL_LITERAL
    ;