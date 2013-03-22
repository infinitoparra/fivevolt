
#include "Print.h"
#include "Utils.h"
#include <stdlib.h>

void Print::print(const char* p) {
	while(*p) {
		write(*p++);
	}
}

void Print::print_P(PGM_P p) {
	uint8_t i;
	char c;
	for (i = 0; (c=pgm_read_byte(&p[i])); i++)  
	{       
		write(c);
	}
}

void Print::print(char c) {
	write(c);
}

void Print::print(int i, int radix) {
	char temp[20];
	itoa(i, temp, radix);
	print(temp);
}

void Print::print(uint16_t i, int radix) {
	char temp[20];
	utoa(i, temp, radix);
	print(temp);
}

void Print::print(uint32_t i, int radix) {
	char temp[20];
	ultoa(i, temp, radix);
	print(temp);
}

void Print::print(float f) {
	char temp[20];
	ftoa(f, temp); // Utils.h/Utils.cpp
	print(temp);
}

void Print::printLine() {
	write('\r');
	write('\n');
}

void Print::printLine(const char* p) {
	print(p);
	printLine();
}

void Print::printLine_P(PGM_P p) {
	print_P(p);
	printLine();
}

void Print::printLine(char c) {
	print(c);
	printLine();
}

void Print::printLine(int i, int radix) {
	print(i, radix);
	printLine();
}

void Print::printLine(uint16_t i, int radix) {
	print(i, radix);
	printLine();
}

void Print::printLine(uint32_t i, int radix) {
	print(i, radix);
	printLine();
}

void Print::printLine(float f) {
	print(f);
	printLine();
}

void Print::write(uint8_t *pData, uint16_t length) {
	uint8_t *p = pData;
	for(uint16_t i=0; i<length; i++) {
		write(*(p++));
	}
}
