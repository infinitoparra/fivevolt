
#ifndef _TLC5925_h_
#define _TLC5925_h_

#include "IO.h"

class TLC5925 {
public:
	TLC5925(OutputPin &dataPin, OutputPin &clkPin, OutputPin &latchPin);
	void begin();
	void write(uint16_t val);

private:
	OutputPin *_dataPin;
	OutputPin *_clkPin;
	OutputPin *_latchPin;
};

#endif
