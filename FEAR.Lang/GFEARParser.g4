/*
 * Forensic Extraction and Representation parser grammar
 * Allan Korol
 */

parser grammar GFEARParser;
options {tokenVocab=FEARLexer;}

program
  : codifierDefinitionBlock resultEntityBlock EOF
  ;

//enumConstants
//    : enumConstant (',' enumConstant)*
//    ;

entityUri
    : IDENTIFIER ':' IDENTIFIER
    ;

//entityTypeUri
//   : entityUri ('[' ']')?
//   ;

//enumConstant
//     : identifier arguments?
//     ;

//variableDeclarators
//    : variableDeclarator (',' variableDeclarator)*
//    ;

//variableDeclarator
//    : variableDeclaratorId ('=' variableInitializer)?
//    ;

//variableDeclaratorId
//    : identifier ('[' ']')*
//    ;

//variableInitializer
//    : arrayInitializer
//    | expression
//    ;

//arrayInitializer
//    : '{' (variableInitializer (',' variableInitializer)* ','? )? '}'
//    ;

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

identifiedByEntityClause
    : condition=(Required|Optional) Entity propertyUri=entityUri entityName=IDENTIFIER
    ;
    
identifiedByPropertyClause
    : condition=(Required|Optional) propertyUri=entityUri
    ;
    
identifiedByClause
    : identifiedByEntityClause
    | identifiedByPropertyClause
    ;

identifiedByClauseBlock
    : Identified By identifiedByClause (Comma identifiedByClause)*
    ;

resultEntityBlock
    : Result resultName=IDENTIFIER iriFormatClause? As typeUri=entityUri With block identifiedByClauseBlock? ';'
    ;

block
    : '{' blockStatement* '}'
    ;

blockStatement
    : statement
    ;


identifier
    : IDENTIFIER
    ;

createPropertyCollectionStatement
    : Create Property propertyNameUri=entityUri With Collection collectionName=IDENTIFIER ';'
    ;
    
createPropertyValueStatement
    : Create Property propertyNameUri=entityUri (As typeUri=entityUri)? With valueName=expression ';'
    ;

createPropertyRelationshipStatement
    : Create Property propertyNameUri=entityUri With iriFormatClause ';'
    ;
   
createPropertyRelationshipUriStatement
    : Create Property propertyNameUri=entityUri With relUri=entityUri ';'
    ;

createPropertyEntityStatement
    : Create Property propertyNameUri=entityUri With Entity entityName=IDENTIFIER ';'
    ;

createPropertyStatement
    : createPropertyValueStatement
    | createPropertyEntityStatement
    | createPropertyRelationshipStatement
    | createPropertyRelationshipUriStatement
    | createPropertyCollectionStatement
    ;

createCollectionStatement
    : Create Collection entityName=IDENTIFIER As collectionType=entityUri (Of expectedType=entityUri)? ';'
    ;

createEntityStatement
    : Create Entity entityName=IDENTIFIER iriFormatClause? As entityType=entityUri With block (identifiedByClauseBlock ';')?
    ;

appendEntityStatement
    : Append Entity entityName=IDENTIFIER To collectionName=IDENTIFIER ';'
    ;

appendPropertyValueStatement
    : Append valueName=expression To collectionName=IDENTIFIER ';'
    ;
    
appendPropertyRelationshipStatement
    : Append iriFormatClause To collectionName=IDENTIFIER ';'
    ;
    
acceptsStatement
    : Accepts OpenParen IDENTIFIER (Comma IDENTIFIER)* CloseParen ';'
    ;

includeStatement
    : Include IDENTIFIER (Comma IDENTIFIER)* ';'
    ;

prefixStatement
    : Prefix_ OpenParen prefixLabel=STRING_LITERAL Comma prefixUri=STRING_LITERAL CloseParen ';'
    ;

codifierDefinitionBlock
    : codifierDefinitionStatement
    ontologyStatement+
    includeStatement?
    prefixStatement*
    acceptsStatement?
    ;

codifierDefinitionStatement
    : Define Codifier OpenParen codifierName=STRING_LITERAL Comma codifierCategory=STRING_LITERAL CloseParen ';'
    ;

iriFormatClause
    : LessThan STRING_LITERAL MoreThan
    ;

ontologyStatement
    : Ontology OpenParen ontUrl=STRING_LITERAL Comma ontPrefix=STRING_LITERAL (Comma ontologyPrefixStatement)* CloseParen ';'
    ;

ontologyPrefixStatement
    : IDENTIFIER ':' STRING_LITERAL
    ;

statement
    : blockLabel=block
    | createEntityStatement
    | createPropertyStatement
    | createCollectionStatement
    | If parExpression statement (Else statement)?
    | Foreach foreachControl statement
    | appendEntityStatement
    | appendPropertyRelationshipStatement
    | appendPropertyValueStatement
    | SemiColon
//    | statementExpression=expression ';'
//    | identifierLabel=identifier ':' statement
    ;

foreachControl
    : identifier 'in' collectionExpression=expression
    ;

parExpression
    : '(' expression ')'
    ;

//expressionList
//    : expression (',' expression)*
//    ;

//methodCall
//    : identifier arguments
//    ;

expression
    : primary
    | expression bop='.'
      (
         identifier
      )
//    | prefix=('+'|'-'|'!') expression
    | expression bop=('*'|'/'|'%') expression  
    | expression bop=('+'|'-') expression 
//    | expression ('<' '<' | '>' '>' '>' | '>' '>') expression 
//    | expression bop=('<=' | '>=' | '>' | '<') expression 
//    | expression bop=('==' | '!=') expression  
//    | expression bop='&' expression
//    | expression bop='^' expression  
//    | expression bop='|' expression  
//    | expression bop='&&' expression 
//    | expression bop='||' expression 
//    | <assoc=right> expression bop='?' expression ':' expression 
//    | <assoc=right> expression
//      bop='='
//      expression

    ;

primary
    : '(' expression ')'
    | literal
    | identifier
    ;
   
//arguments
//    : '(' expressionList? ')'
//    ;