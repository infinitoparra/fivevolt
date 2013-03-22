
#include "ADC.h"

ADCPin::ADCPin(volatile uint8_t &port, uint8_t pin) {
	_port = &port;
	_pin = pin;
	_vref = 5.0f;
}

void ADCPin::begin() {
	InputPin pin(*_port, _pin);
	pin.begin();

	ADCSRA |= _BV(ADPS2);
	ADCSRA |= _BV(ADPS1);
	ADCSRA |= _BV(ADPS0);
	ADCSRA |= _BV(ADEN);
}

volatile uint16_t ADCPin::read() {
	uint8_t low, high;
	uint8_t mux = 0x00;

	#if defined(__AVR_ATmega1284P__) | defined(__AVR_ATmega644P__)
		if(_port == &PORTA) {
			if(_pin == PIN0) mux = 0x00;
			else if(_pin == PIN1) mux = 0x01;
			else if(_pin == PIN2) mux = 0x02;
			else if(_pin == PIN3) mux = 0x03;
			else if(_pin == PIN4) mux = 0x04;
			else if(_pin == PIN5) mux = 0x05;
			else if(_pin == PIN6) mux = 0x06;
			else if(_pin == PIN7) mux = 0x07;
		}
	#else
		#error Not Supported
	#endif

	ADMUX = mux;

	// start conversion
	ADCSRA |= _BV(ADSC);

	// wait for conversion
	while (bit_is_set(ADCSRA, ADSC));

	// we have to read ADCL first; doing so locks both ADCL
	// and ADCH until ADCH is read.  reading ADCL second would
	// cause the results of each conversion to be discarded,
	// as ADCL and ADCH would be locked when it completed.
	low = ADCL;
	high = ADCH;

	// combine the two bytes
	return (high << 8) | low;
}

volatile int16_t ADCPin::readV() {
	return (int16_t)((int32_t)read() * (int32_t)_vref / 1024);
}
