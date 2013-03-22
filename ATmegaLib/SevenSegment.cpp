
#include "SevenSegment.h"
#include "SevenSegmentAlphabet.h"

char SevenSegment::segmentsToChar(uint8_t val) {
	return (char)pgm_read_byte(SevenSegmentSegToChar + (val & 0x7f));
}

uint8_t SevenSegment::charToSegments(char ch) {
	return (uint8_t)pgm_read_byte(SevenSegmentCharToSeg + (ch & 0x7f));
}

