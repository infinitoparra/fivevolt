
#include "Utils.h"
#include <avr/io.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

#ifdef __AVR_ATxmega128A1__
void setClockTo32MHz() {
	CCP = CCP_IOREG_gc;                     // disable register security for oscillator update
	OSC.CTRL = OSC_RC32MEN_bm;              // enable 32MHz oscillator
	while(!(OSC.STATUS & OSC_RC32MRDY_bm)); // wait for oscillator to be ready
	CCP = CCP_IOREG_gc;                     // disable register security for clock update
	CLK.CTRL = CLK_SCLKSEL_RC32M_gc;        // switch to 32MHz clock
}
#endif

void ftoa(float f, char* pstr) {
	ftoa(f, pstr, 5);
}

void ftoa(float number, char* pstr, uint8_t digits) {
	float rounding;
	unsigned long intPart;
	float remainder;
	int toPrint;

	if (number < 0.0)
	{
		*pstr++ = '-';
		number = -number;
	}

	// Round correctly so that print(1.999, 2) prints as "2.00"
	rounding = 0.5;
	for (uint8_t i=0; i<digits; i++) {
		rounding /= 10.0;
	}

	number += rounding;

	// Extract the integer part of the number and print it
	intPart = (unsigned long)floor(number);
	ultoa(intPart, pstr, 10);
	while(*pstr != '\0') { 
		pstr++;
	}

	// Print the decimal point, but only if there are digits beyond
	if (digits > 0) {
		*pstr++ = '.';
	}

	// Extract digits from the remainder one at a time
	remainder = number - (float)intPart;
	while (digits-- > 0)
	{
		remainder *= 10.0f;
		toPrint = floor(remainder);
		*pstr++ = toPrint + '0';
		remainder -= toPrint; 
	} 

	*pstr++ = '\0';
}

float map(float x, float in_min, float in_max, float out_min, float out_max) {
	return ( ((x - in_min) * (out_max - out_min)) / (in_max - in_min) + out_min );
}

int16_t map(int16_t x, int16_t in_min, int16_t in_max, int16_t out_min, int16_t out_max) {
	return ( ((int32_t)(x - in_min) * (int32_t)(out_max - out_min)) / (in_max - in_min) + out_min );
}

bool isWhitespace(char c) {
	if(c == ' '
		|| c == '\t'
		|| c == '\r'
		|| c == '\n') return true;
	return false;
}

void strtrim(char *str) {
	char* write = str;
	char* lastGoodChar;
	while(*str && isWhitespace(*str)) {
		str++;
	}
	lastGoodChar = str;
	while(*str) {
		if(!isWhitespace(*str)) {
			lastGoodChar = str + 1;
		}
		*write++ = *str++;
	}
	*lastGoodChar = '\0';
}

void strcatfill(char *buffer, uint8_t totalChars, char fillChar) {
	uint8_t	i = strlen(buffer);
	for(; i < totalChars; i++) {
		buffer[i] = fillChar;
	}
	buffer[i] = '\0';
}


