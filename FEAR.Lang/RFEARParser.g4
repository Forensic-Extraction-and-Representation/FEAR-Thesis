/*
 * Forensic Extraction and Representation parser grammar
 * Allan Korol
 */

parser grammar RFEARParser;
options {tokenVocab=RFEARLexer;}

program
  : rulesetDefinitionBlock ruleBlock* EOF
  ;

entityUri
    : IDENTIFIER ':' IDENTIFIER
    | IRI
    ;

ruleBlock
    : ruleType=(Observed|Derived) ruleName=IDENTIFIER With? OpenBrace swrlRule CloseBrace dependsOnClauseBlock? ';'
    ;

swrlRule
    : swrlAntecedent Indirection swrlConsequent
    ;

swrlAntecedent: atom (Conjunct atom)*;
swrlConsequent: atom (Conjunct atom)*;

atom: classAtom | propertyAtom | builtinAtom;

classAtom: iri '(' variable ')';
propertyAtom: iri '(' subjectVariable=variable ',' objectVariable=argument ')';
builtinAtom: swrlbIri '(' arg1=argument ',' arg2=argument ')';

argument: variable | literal | iri;

variable: VAR;
literal: STRING_LITERAL | NUMBER_LITERAL;

iri: entityUri;
swrlbIri: Swrlb ':' IDENTIFIER;
swrlbOrIdentifier: Swrlb | IDENTIFIER;

dependsOnClauseBlock
    : Depends_on dependsOnClause (Comma dependsOnClause)*
    ;

dependsOnClause
    : ruleName=IDENTIFIER
    ;

includeStatement
    : Include onto=swrlbOrIdentifier (Comma onto=swrlbOrIdentifier)* ';'
    ;

prefixStatement
    : Prefix_ OpenParen prefixLabel=STRING_LITERAL Comma prefixUri=STRING_LITERAL CloseParen ';'
    ;

rulesetDefinitionBlock
    : rulesetDefinitionStatement
    ontologyStatement+
    includeStatement?
    prefixStatement*
    ;

rulesetDefinitionStatement
    : Define Ruleset OpenParen rulsetName=STRING_LITERAL Comma rulesetCategory=STRING_LITERAL CloseParen ';'
    ;

ontologyStatement
    : Ontology OpenParen ontUrl=STRING_LITERAL Comma ontPrefix=STRING_LITERAL (Comma ontologyPrefixStatement)* CloseParen ';'
    ;

ontologyPrefixStatement
    : IDENTIFIER ':' STRING_LITERAL
    ;