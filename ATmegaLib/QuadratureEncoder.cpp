
#include "QuadratureEncoder.h"

const uint8_t QuadratureEncoder::_sequence[] = { 0, 1, 3, 2 };

QuadratureEncoder::QuadratureEncoder(InputPin &a, InputPin &b) {
	_a = &a;
	_b = &b;
	_value = 0;
	_min = 0;
	_max = 100;
}

void QuadratureEncoder::begin() {
	_a->begin();
	_a->enableInterrupt();

	_b->begin();
	_b->enableInterrupt();
}

void QuadratureEncoder::update() {
	_delay_ms(1);
	uint8_t out = (_a->isSet() ? 1 : 0) | (_b->isSet() ? 2 : 0);
	if(_sequence[_lastOutput] == out) {
		return;
	}

	uint8_t currentOutput = 0;
	for(uint8_t i=0; i<4; i++) {
		if(_sequence[i] == out) {
			currentOutput = i;
			break;
		}
	}

	int newValue = _value;
	if((currentOutput == _lastOutput + 1)
		|| (currentOutput == 0 && _lastOutput == 3)) {
		newValue++;
		if(newValue > _max) {
			newValue = _max;
		}
	} else if((currentOutput == _lastOutput - 1)
		|| (currentOutput == 3 && _lastOutput == 0)) {
		newValue--;
		if(newValue < _min) {
			newValue = _min;
		}
	}
	
	_lastOutput = currentOutput;
	_value = newValue;
}

