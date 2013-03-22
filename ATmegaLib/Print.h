
#include <stdint.h>
#include <avr/pgmspace.h>

#ifndef _print_h_
#define _print_h_

#define DEC 10
#define HEX 16
#define OCT 8
#define BIN 2
#define BYTE 0

class Print {
public:
	void print(const char* p);
	void print_P(PGM_P p);
	void print(char c);
	void print(int i, int radix = DEC);
	void print(uint16_t i, int radix = DEC);
	void print(uint32_t i, int radix = DEC);
	void print(float f);
	void printLine();
	void printLine(const char* p);
	void printLine_P(PGM_P p);
	void printLine(char c);
	void printLine(int i, int radix = DEC);
	void printLine(uint16_t i, int radix = DEC);
	void printLine(uint32_t i, int radix = DEC);
	void printLine(float f);
	virtual void write(uint8_t c) = 0;
	virtual void write(uint8_t *pData, uint16_t length);
};

#endif
