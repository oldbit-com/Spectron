grammar Debugger;

options {
    caseInsensitive = true;
}

INT
        : [0-9]+
        ;
HEX
        : ('0x' | '$') [0-9a-f]+ | [0-9a-f]+ [h]
        ;

BIN
        : ('0b') [01]+ | [01]+ [b]
        ;

WS
        : [ \t\r\n]+ -> skip
        ;
COMMA
        : ','
        ;

REG
        : 'A' | 'B' | 'C' | 'D' | 'E' | 'H' | 'L'
        | 'A\'' | 'B\'' | 'C\'' | 'D\'' | 'E\'' | 'H\'' | 'L\''
        | 'IXH' | 'IXL' | 'IYH' | 'IYL'
        | 'AF' | 'BC' | 'DE' | 'HL'
        | 'AF\'' | 'BC\'' | 'DE\'' | 'HL\''
        | 'SP' | 'PC' | 'IX' | 'IY'
        | 'I' | 'R'
        ;

PRINT
        : 'PRINT'
        | '?'
        ;

POKE
        : 'POKE'
        ;

PEEK
        : 'PEEK'
        ;

program
        : statement (';' statement)* ';'?
        ;

statement
        : expression
        | printstmt
        | pokestmt
        | peekfunc
        | assign
        ;

assign:
        REG '=' expression
        ;

printstmt
        : PRINT (expression (',' expression)*)?
        ;

pokestmt
        : POKE address=expression COMMA value=expression
        ;

peekfunc
        : PEEK address=expression
        ;

expression
        :
        //expression ('+' | '-') expression   # AddSub
          INT                                 # Int
        | HEX                                 # Hex
        | BIN                                 # Bin
        | REG                                 # Reg
       // | '(' expression ')'                  # Parens
        ;

