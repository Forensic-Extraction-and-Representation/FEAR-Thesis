/*
 * Forensic Extraction and Representation lexer grammar
 * Allan Korol
 */

lexer grammar FEARLexer;
channels { ERROR }

/// Keywords

Break                   : 'break';
Else                    : 'else';
Var                     : 'var';
Return                  : 'return';
Void                    : 'void';
Continue                : 'continue';
For                     : 'for';
Foreach                 : 'foreach';
While                   : 'while';
With                    : 'with';
Create                  : 'create';
Collection              : 'collection';
Append                  : 'append';
Function_               : 'function';
If                      : 'if';
In                      : 'in';
To                      : 'to';
As                      : 'as';
Of                      : 'of';
New                     : 'new';

EndL                    : 'L';
EndB                    : 'B';
Iri                     : 'iri';
Segment                 : 'segment';
Define                  : 'define';
Internal                : 'internal';
Interpreter_            : 'interpreter';
Collector               : 'collector';
Codifier                : 'codifier';
Module                  : 'module';
Codify                  : 'codify';
Ontology                : 'ontology';
Entity                  : 'entity';
Property                : 'property';
Include                 : 'include';
Identified              : 'identified';
By                      : 'by';
Prefix_                 : 'prefix';
Accepts                 : 'accepts';
Search                  : 'search';
Result                  : 'result';
Required                : 'required';
Optional                : 'optional';
InterpreterSkip         : 'S';

OpenBracket             : '[';
CloseBracket            : ']';
OpenParen               : '(';
CloseParen              : ')';
OpenBrace               : '{';
CloseBrace              : '}';
SemiColon               : ';';
Comma                   : ',';
Assign                  : '=';
QuestionMark            : '?';
Colon                   : ':';
Dot                     : '.';
PlusPlus                : '++';
MinusMinus              : '--';
Plus                    : '+';
Minus                   : '-';
BitNot                  : '~';
Not                     : '!';
Multiply                : '*';
Divide                  : '/';
Modulus                 : '%';
LessThan                : '<';
MoreThan                : '>';
LessThanEquals          : '<=';
GreaterThanEquals       : '>=';
Equals_                 : '==';
NotEquals               : '!=';
RightShiftArithmetic    : '>>';
LeftShiftArithmetic     : '<<';
BitAnd                  : '&';
BitXOr                  : '^';
BitOr                   : '|';
And                     : '&&';
Or                      : '||';
Arrow                   : '=>';
ColonColon              : '::';

NULL_LITERAL            : 'null';


DECIMAL_LITERAL         : ('0' | [1-9] (Digits? | '_'+ Digits)) [lL]?;
HEX_LITERAL             : '0' [xX] [0-9a-fA-F] ([0-9a-fA-F_]* [0-9a-fA-F])? [lL]?;
OCT_LITERAL             : '0' '_'* [0-7] ([0-7_]* [0-7])? [lL]?;
BINARY_LITERAL          : '0' [bB] [01] ([01_]* [01])? [lL]?;

FLOAT_LITERAL           : (Digits '.' Digits? | '.' Digits) ExponentPart? [fFdD]?
                        | Digits (ExponentPart [fFdD]? | [fFdD])
                        ;

HEX_FLOAT_LITERAL       : '0' [xX] (HexDigits '.'? | HexDigits? '.' HexDigits) [pP] [+-]? Digits [fFdD]?;

BOOL_LITERAL            : 'true'
                        | 'false'
                        ;

CHAR_LITERAL            :'\'' (~['\\\r\n] | EscapeSequence) '\'';
STRING_LITERAL          :'"' (~["\\\r\n] | EscapeSequence)* '"';

Whitespace              : [ \t\r\n]+ -> channel(HIDDEN) ;
MultiLineComment        : '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment       : '//' ~[\r\n\u2028\u2029]* -> channel(HIDDEN);
Newline                 : '\r'? '\n' -> skip ;

UnexpectedCharacter     : . -> channel(ERROR);

// Identifiers

IDENTIFIER              : Letter LetterOrDigit*;

// Fragment rules

fragment ExponentPart
    : [eE] [+-]? Digits
    ;

fragment EscapeSequence
    : '\\' 'u005c'? [btnfr"'\\]
    | '\\' 'u005c'? ([0-3]? [0-7])? [0-7]
    | '\\' 'u'+ HexDigit HexDigit HexDigit HexDigit
    ;

fragment HexDigits
    : HexDigit ((HexDigit | '_')* HexDigit)?
    ;

fragment HexDigit
    : [0-9a-fA-F]
    ;

fragment Digits
    : [0-9] ([0-9_]* [0-9])?
    ;

fragment LetterOrDigit
    : Letter
    | [0-9]
    | '-' // For uri-prefixes
    ;
    
fragment Letter
    : [a-zA-Z$_] // these are the "java letters" below 0x7F
    | ~[\u0000-\u007F\uD800-\uDBFF] // covers all characters above 0x7F which are not a surrogate
    | [\uD800-\uDBFF] [\uDC00-\uDFFF] // covers UTF-16 surrogate pairs encodings for U+10000 to U+10FFFF
    ;