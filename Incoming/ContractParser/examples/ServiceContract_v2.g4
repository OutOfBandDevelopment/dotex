grammar ServiceContract;

/* Parser */
contracts : (COMMENT| service | dto | enumeration)+ EOF;

	service	:  NAME '@service' COMMENT* NEWLINE* operation*; 
		operation	:  FUNCTIONTYPE NAME COMMENT* NEWLINE* parameter*;
			parameter	:  (input | output) ;
				input	: '>' typeDefinition ;
				output	: '<' typeDefinition ;

	dto		: NAME '@dto' COMMENT* NEWLINE* property*; 
		
	enumeration	: NAME '@enum' COMMENT* NEWLINE* enumerationValue*; 
		enumerationValue : NAME ('=' NUMERIC_VALUE)? COMMENT* NEWLINE*;

	//database :  NAME '@db' COMMENT* NEWLINE+ operation*; 
		
	//table		: NAME '@table' COMMENT* NEWLINE* tablerow*; 
	//	tablerow : NAME ':' (NAME | OBJECTTYPE) '(' DIGIT+ ')' COMMENT* NEWLINE*;

typeDefinition : NAME ':' (NAME | OBJECTTYPE) COMMENT* NEWLINE*;
property : typeDefinition;

/* Lexer */

fragment LOWERCASE		: [a-z];
fragment UPPERCASE		: [A-Z];
fragment DIGIT			: [0-9];
fragment OTHER			: [_];

fragment BINARY			: 'b'[01]+;
fragment HEXADECIMAL	: ('0x' | 'h')[0-9A-Fa-f]+;
fragment OCTAL			: 'o'[0-7]+;

FUNCTIONTYPE			: '+' | '*' | '-';
PARAMETERTYPE			: '>' | '<';

NUMERIC_VALUE			: BINARY | OCTAL | HEXADECIMAL | [0-9]+ ;
NEWLINE					: ('\r'? '\n' | '\r')+ -> skip;
COMMENT					: '|' .*? NEWLINE;

NAME					: (LOWERCASE | UPPERCASE)(LOWERCASE | UPPERCASE | DIGIT | OTHER)* ;
OBJECTTYPE				: (NAME | NAME '<' NAME (',' NAME)* '>' | NAME '[]') ; //TODO: this doesn't support multiple type parameters

WS						: [ \t\u000C]+ -> skip ; 