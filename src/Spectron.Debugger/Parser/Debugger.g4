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
        : 'A' | 'B' | 'C' | 'D' | 'E' | 'F' | 'H' | 'L'
        | 'A\'' | 'B\'' | 'C\'' | 'D\'' | 'E\'' | 'H\'' | 'L\''
        | 'IXH' | 'IXL' | 'IYH' | 'IYL'
        | 'AF' | 'BC' | 'DE' | 'HL'
        | 'AF\'' | 'BC\'' | 'DE\'' | 'HL\''
        | 'SP' | 'PC' | 'IX' | 'IY'
        | 'I' | 'R'
        ;

HELP
        : 'HELP'
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

OUT
        : 'OUT'
        ;

IN
        : 'IN'
        ;

CLEAR
        : 'CLEAR'
        ;

GOTO
        : 'GOTO'
        ;

LIST
        : 'LIST'
        ;

program
        : statement (';' statement)* ';'?
        ;

statement
        : helpstmt
        | clearstmt
        | printstmt
        | pokestmt
        | peekfunc
        | outfunc
        | infunc
        | assign
        | gotostmt
        | liststmt
        ;

assign
        : REG '=' expression
        ;

helpstmt
        : HELP (functionName=PRINT | POKE | PEEK | IN | OUT)?
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

outfunc
        : OUT address=expression COMMA value=expression
        ;

infunc
        : IN address=expression
        ;

clearstmt
        : CLEAR
        ;

gotostmt
        : GOTO address=expression
        ;

liststmt
        : LIST address=expression?
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

