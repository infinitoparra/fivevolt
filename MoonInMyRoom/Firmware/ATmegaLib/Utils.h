
#include <stdint.h>

#ifndef _Utils_h_
#define _Utils_h_

#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif

#define clamp(x,min,max) ( ((x)<(min)) ? (min) : (((x)>(max)) ? (max) : (x)) )
#define max(a,b) ( ((a)>(b)) ? (a) : (b) )

#if defined(__AVR_ATxmega128A1__)
void setClockTo32MHz();
#endif

void ftoa(float f, char* pstr);

// converts a float to a string.
//   digits - number of digits to the right of decimal
void ftoa(float f, char* pstr, uint8_t digits);

float map(float x, float in_min, float in_max, float out_min, float out_max);
int16_t map(int16_t x, int16_t in_min, int16_t in_max, int16_t out_min, int16_t out_max);
bool isWhitespace(char c);
void strtrim(char *str);

// fills the remaining characters in a string with fillChar up to totalChars
//  so if totalChars is 20 buffer will have 20 characters filled in and the
//  21 character will be null
void strcatfill(char *buffer, uint8_t totalChars, char fillChar);

#endif
