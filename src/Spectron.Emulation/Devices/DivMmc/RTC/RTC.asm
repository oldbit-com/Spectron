; RTC.SYS for ESXDOS emulation
;
; This is a simple emulated RTC driver for the ESXDOS system. It uses port 0x3B11 to get the time
; and date from the emulator system.
;
; OUTPUT
;   BC constains Date
;       yyyyyyyM_MMMddddd
;		year-1980 (7 bits) + month (4 bits) + day (5 bits)
;   DE contains Time
;		HHHHHmmm_mmmsssss
;	    hours (5 bits) + minutes (6 bits) + seconds/2 (5 bits)
;
;  Assmbler: sjasmplus

            DEVICE ZXSPECTRUM48

            OUTPUT "RTC.SYS"

            ORG 0x2700

PORT        EQU 0x3B11

YEAR        EQU 0x01
MONTH       EQU 0x02
DAY         EQU 0x03
HOUR        EQU 0x04
MINUTE      EQU 0x05
SECOND      EQU 0x06

START:
            push af
            push hl

            ld bc,PORT

            ; Get year
            ld a,YEAR
            call SEND
            ld h,a              ; h = 0yyy_yyyy

            ; Get month
            ld a,MONTH
            call SEND
            ld l,a              ; l = 0000_mmmm
            sla l               ; l = 000m_mmm0
            sla l               ; l = 00mm_mm00
            sla l               ; l = 0mmm_m000
            sla l               ; l = mmmm_0000
            sla l               ; Carry=m, l = mmm0_0000
            rl h                ; l = yyyy_yyym - move year to positiona and include 1 bit from month

            ; Get day
            ld a,DAY
            call SEND
            or l                ; a = mmm0_0000 | 000d_dddd, include 5 day bits, l alrady contains 3 bits from month
            ld l,a              ; l = mmmd_dddd

            ; Get hour
            ld a,HOUR
            call SEND
            ld d,a              ; d = 000H_HHHH
            sla d               ; d = 00HH_HHH0
            sla d               ; d = 0HHH_HH00
            sla d               ; d = HHHH_H000

            ; Get minutes
            ld a,MINUTE
            call SEND
            ld e,a              ; e = 00mm_mmmm
            srl a               ; a = 000m_mmmm
            srl a               ; a = 0000_mmmm
            srl a               ; a = 00000_mmm
            or d                ; a = 00000_mmm | HHHH_H000
            ld d,a              ; d = HHHH_Hmmm - result contains 5 bits from hours and 3 bits from minutes
            ld a,e              ; a = 00mm_mmmm
            rrca                ; a = m00m_mmmm
            rrca                ; a = mm00_mmmm
            rrca                ; a = mmm0_0mmm
            and 0xE0            ; a = mmm0_0000 - keep 3 bits from minutes
            ld e,a              ; e = mmm0_0000

            ; Get seconds
            ld a,SECOND
            call SEND
            or e                ; a = mmm0_0000 | 000s_ssss
            ld e,a              ; e = mmms_ssss

            ld b,h
            ld c,l

            pop hl
            pop af

            ret

; Sends the command to the emulated RTC device and receives the value back.
; INPUT
;   A = date/time component to read
; OUTPUT
;   A = date/time component value
SEND:
            out (c),a
            in a,(c)

            ret

