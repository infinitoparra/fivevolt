
#include "Stream.h"

void Stream::skipUntil(uint8_t b) {
	while(available() > 0) {
		if(peek() == b) {
			break;
		}
		read();
	}
}