
#ifndef _sevensegment_h_
#define _sevensegment_h_

#include <stdint.h>

class SevenSegment {
public:
	static char segmentsToChar(uint8_t val);
	static uint8_t charToSegments(char ch);
};

#endif
