
#include <avr/io.h>
#include "IO.h"

#ifndef _adc_h_
#define _adc_h_

class ADCPin {
public:
	ADCPin(volatile uint8_t &port, uint8_t pin);
	
	void begin();
	volatile uint16_t read();
	volatile int16_t readV(); // * 1000
	void setVRef(int16_t vref) { _vref = vref; } // * 1000

private:
	volatile uint8_t *_port;
	uint8_t _pin;
	int16_t _vref; // * 1000
};

#endif
