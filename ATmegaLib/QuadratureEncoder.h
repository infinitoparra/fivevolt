
#include <avr/io.h>
#include <util/delay.h>
#include "IO.h"

#ifndef _QuadratureEncoder_h_
#define _QuadratureEncoder_h_

class QuadratureEncoder {
public:
	QuadratureEncoder(InputPin &a, InputPin &b);
	void begin();
	void update();
	int getValue() { return _value; }
	void setValue(int v) { _value = v; }
	void setMin(int min) { _min = min; }
	void setMax(int max) { _max = max; }

private:
	InputPin *_a;
	InputPin *_b;
	uint8_t _lastOutput;
	int _value;
	int _min;
	int _max;
	static const uint8_t _sequence[];
};

#endif
