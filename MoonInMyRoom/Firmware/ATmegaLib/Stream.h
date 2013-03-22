
#include <stdint.h>
#include "Print.h"

#ifndef _stream_h_
#define _stream_h_

class Stream : public Print {
public:
	virtual uint16_t available() = 0;
    virtual uint8_t read() = 0;
	virtual uint8_t peek() = 0;
    virtual void flush() = 0;
	virtual void skip(uint16_t num) = 0;
	virtual void skipUntil(uint8_t b);
};

#endif