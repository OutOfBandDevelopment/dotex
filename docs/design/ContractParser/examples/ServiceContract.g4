grammar ServiceContract;

/* Parser */

contracts : (COMMENT| service | dto )+ EOF; // | enum

service		:  NAME '@service' COMMENT* NEWLINE* operation*; 
operation	:  FUNCTIONTYPE NAME COMMENT* NEWLINE* parameter*;

input : '>' typeDefinition ;
output : '<' typeDefinition ;

parameter	:  (input | output) ;

dto			: NAME '@dto' COMMENT* NEWLINE* property*; 
property	: typeDefinition;

/*
enum			: NAME '@enum' COMMENT* NEWLINE* enumValues*; 
enumValues	: NAME (|'=' ( DIGIT+ | '0x' DIGIT+ |NAME | OBJECTTYPE)) COMMENT* NEWLINE*;
*/

typeDefinition : NAME ':' (NAME | OBJECTTYPE) COMMENT* NEWLINE*;


/* Lexer */

fragment LOWERCASE	: [a-z] ;
fragment UPPERCASE	: [A-Z] ;
fragment DIGIT		: [0-9] ;
fragment OTHER		: [_]	;

SEPERATOR			: '-' ;
FUNCTIONTYPE		: '+' | '*' ;
PARAMETERTYPE		: '>' | '<' ;

NEWLINE             : ('\r'? '\n' | '\r')+ -> skip;

COMMENT				: '|' .*? NEWLINE;


NAME	: (LOWERCASE | UPPERCASE)(LOWERCASE | UPPERCASE | DIGIT | OTHER)* ;
OBJECTTYPE	: (NAME | NAME '<' NAME '>') ;

WS			: [ \t\u000C]+ -> skip ; 

/*
TestService @service
|you can put comments here

	+ TestReadOperation1
		> Parameter1 : string
		> Parameter2 : TestDto
		< Result1 : string
		< Result2 : string
				
	+ TestWriteOperation1
		> Parameter1 : string
		> Parameter2 : TestDto
		< Result1 : string
		< Result2 : string

TestDto @dto
	Property1 : List<string>
	Property2 : List<int>
*/
