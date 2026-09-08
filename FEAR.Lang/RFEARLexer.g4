/*
 * Forensic Extraction and Representation lexer grammar
 * Allan Korol
 */

lexer grammar RFEARLexer;
channels { ERROR }

/// Keywords

Swrlb                   : 'swrlb';
Segment                 : 'segment';
Define                  : 'define';
Ruleset                 : 'ruleset';
Derived                 : 'derived';
Observed                : 'observed';
Ontology                : 'ontology';
Include                 : 'include';
Prefix_                 : 'prefix';
Depends_on              : 'depends on';
With                    : 'with';

OpenParen               : '(';
CloseParen              : ')';
OpenBrace               : '{';
CloseBrace              : '}';
SemiColon               : ';';
Comma                   : ',';
Colon                   : ':';
Conjunct                : '^';
Indirection             : '->';
LeftAngle              : '<';
RightAngle             : '>';

Whitespace              : [ \t\r\n]+ -> channel(HIDDEN) ;
MultiLineComment        : '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment       : '//' ~[\r\n\u2028\u2029]* -> channel(HIDDEN);
Newline                 : '\r'? '\n' -> skip ;

IRI: '<' [a-zA-Z_][a-zA-Z0-9_:./#-]* '>';
VAR: '?' [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL          :'"' (~["\\\r\n] | EscapeSequence)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Identifiers

IDENTIFIER              : Letter LetterOrDigit*;

// Fragment rules

fragment Digits
    : [0-9] ([0-9_]* [0-9])?
    ;

fragment LetterOrDigit
    : Letter
    | [0-9]
    | '-' // For uri-prefixes
    ;


fragment EscapeSequence
    : '\\' 'u005c'? [btnfr"'\\]
    | '\\' 'u005c'? ([0-3]? [0-7])? [0-7]
    ;

fragment Letter
    : [a-zA-Z$_] // these are the "java letters" below 0x7F
    | ~[\u0000-\u007F\uD800-\uDBFF] // covers all characters above 0x7F which are not a surrogate
    | [\uD800-\uDBFF] [\uDC00-\uDFFF] // covers UTF-16 surrogate pairs encodings for U+10000 to U+10FFFF
    ;