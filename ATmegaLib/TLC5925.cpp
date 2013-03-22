
#include "TLC5925.h"

TLC5925::TLC5925(OutputPin &dataPin, OutputPin &clkPin, OutputPin &latchPin) {
	_dataPin = &dataPin;
	_clkPin = &clkPin;
	_latchPin = &latchPin;
}

void TLC5925::begin() {
	_dataPin->begin();
	_clkPin->begin();
	_latchPin->begin();
}

void TLC5925::write(uint16_t val) {
	for(uint8_t b=0; b<16; b++) {
		if((val & 0x01) == 0x01) {
			_dataPin->set();
		}
		else {
			_dataPin->clear();
		}
		_clkPin->set();
		_clkPin->clear();
		val = val >> 1;
	}
	_latchPin->set();
	_latchPin->clear();
}

