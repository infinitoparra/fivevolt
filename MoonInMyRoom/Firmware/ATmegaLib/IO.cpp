
#include <stdlib.h>
#include <avr/io.h>
#include <math.h>
#include <util/delay.h>
#include "IO.h"

#if defined(__AVR_ATmegaXXX__)
volatile uint8_t* IODevice::getPinRegisterFromPort(volatile uint8_t* port) {
	#if defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
		if(port == &PORTA) {
			return &PINA;
		}
	#endif
	if(port == &PORTB) {
		return &PINB;
	}
	if(port == &PORTC) {
		return &PINC;
	}
	if(port == &PORTD) {
		return &PIND;
	}
	return NULL;
}

volatile uint8_t* IODevice::getDdrRegisterFromPort(volatile uint8_t* port) {
	#if defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
		if(port == &PORTA) {
			return &DDRA;
		}
	#endif
	if(port == &PORTB) {
		return &DDRB;
	}
	if(port == &PORTC) {
		return &DDRC;
	}
	if(port == &PORTD) {
		return &DDRD;
	}
	return NULL;
}

uint8_t IODevice::getPCINTNumber(volatile uint8_t* port, uint8_t pin) {
	#if defined(__AVR_ATmega328P__)
		if(port == &PORTB) {
			switch(pin) {
				case 0: return 0;
				case 1: return 1;
				case 2: return 2;
				case 3: return 3;
				case 4: return 4;
				case 5: return 5;
				case 6: return 6;
				case 7: return 7;
			}
		}
		if(port == &PORTC) {
			switch(pin) {
				case 0: return 8;
				case 1: return 9;
				case 2: return 10;
				case 3: return 11;
				case 4: return 12;
				case 5: return 13;
				case 6: return 14;
			}
		}
		if(port == &PORTD) {
			switch(pin) {
				case 0: return 16;
				case 1: return 17;
				case 2: return 18;
				case 3: return 19;
				case 4: return 20;
				case 5: return 21;
				case 6: return 22;
				case 7: return 23;
			}
		}
	#elif defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
		if(port == &PORTA) {
			switch(pin) {
				case 0: return 0;
				case 1: return 1;
				case 2: return 2;
				case 3: return 3;
				case 4: return 4;
				case 5: return 5;
				case 6: return 6;
				case 7: return 7;
			}
		}
		if(port == &PORTB) {
			switch(pin) {
				case 0: return 8;
				case 1: return 9;
				case 2: return 10;
				case 3: return 11;
				case 4: return 12;
				case 5: return 13;
				case 6: return 14;
				case 7: return 15;
			}
		}
		if(port == &PORTC) {
			switch(pin) {
				case 0: return 16;
				case 1: return 17;
				case 2: return 18;
				case 3: return 19;
				case 4: return 20;
				case 5: return 21;
				case 6: return 22;
				case 7: return 23;
			}
		}
		if(port == &PORTD) {
			switch(pin) {
				case 0: return 24;
				case 1: return 25;
				case 2: return 26;
				case 3: return 27;
				case 4: return 28;
				case 5: return 29;
				case 6: return 30;
				case 7: return 31;
			}
		}
	#endif
	return -1;
}

uint8_t IODevice::getPICIE(volatile uint8_t* port, uint8_t pin) {
	uint8_t pcint = getPCINTNumber(port, pin);
	if(pcint >= 0 && pcint <= 7) {
		return PCIE0;
	}
	#if defined(PCIE1)
		if(pcint >= 8 && pcint <= 14) {
			return PCIE1;
		}
	#endif
	#if defined(PCIE2)
		if(pcint >= 16 && pcint <= 23) {
			return PCIE2;
		}
	#endif
	return -1;
}

volatile uint8_t* IODevice::getPCMSK(volatile uint8_t* port, uint8_t pin) {
	uint8_t pcint = getPCINTNumber(port, pin);
	if(pcint >= 0 && pcint <= 7) {
		return &PCMSK0;
	}
	if(pcint >= 8 && pcint <= 14) {
		return &PCMSK1;
	}
	#if defined(PCMSK2)
		if(pcint >= 16 && pcint <= 23) {
			return &PCMSK2;
		}
	#endif
	return NULL;
}

uint8_t IODevice::getPCINT(volatile uint8_t* port, uint8_t pin) {
	if(port == &PORTB) {
		switch(pin) {
			case 0: return PCINT0;
			case 1: return PCINT1;
			case 2: return PCINT2;
			case 3: return PCINT3;
			case 4: return PCINT4;
			case 5: return PCINT5;
			case 6: return PCINT6;
			case 7: return PCINT7;
		}
	}
	if(port == &PORTC) {
		switch(pin) {
			#if defined(PCINT8)
			case 0: return PCINT8;
			case 1: return PCINT9;
			case 2: return PCINT10;
			case 3: return PCINT11;
			case 4: return PCINT12;
			#endif
			#if defined(PCINT13)
			case 5: return PCINT13;
			case 6: return PCINT14;
			#endif
		}
	}
	#if defined(PCINT13)
		if(port == &PORTD) {
			switch(pin) {
				case 0: return PCINT16;
				case 1: return PCINT17;
				case 2: return PCINT18;
				case 3: return PCINT19;
				case 4: return PCINT20;
				case 5: return PCINT21;
				case 6: return PCINT22;
				case 7: return PCINT23;
			}
		}
	#endif
	return -1;
}
#endif

#if defined(__AVR_ATmegaXXX__)
Pin::Pin(volatile uint8_t &port, uint8_t pin) {
	_pPin = getPinRegisterFromPort(&port);
	_pPort = &port;
	_pDdr = getDdrRegisterFromPort(&port);
	_pin = pin;
}
#elif defined(__AVR_ATxmega128A1__)
Pin::Pin(PORT_struct &port, uint8_t pin) {
	_port = &port;
	_pin = pin;
}
#endif

#if defined(__AVR_ATmegaXXX__)
OutputPin::OutputPin(volatile uint8_t &port, uint8_t pin) : Pin(port, pin) {
}
#elif defined(__AVR_ATxmega128A1__)
OutputPin::OutputPin(PORT_struct &port, uint8_t pin) : Pin(port, pin) {
}
#endif

#if defined(__AVR_ATmegaXXX__)
InputPin::InputPin(volatile uint8_t &port, uint8_t pin) : Pin(port, pin) {
}
#elif defined(__AVR_ATxmega128A1__)
InputPin::InputPin(PORT_struct &port, uint8_t pin) : Pin(port, pin) {
}
#endif

void OutputPin::begin() {
	#if defined(__AVR_ATmegaXXX__)
		*_pPort |= (1<<_pin);
		*_pDdr |= (1<<_pin);
	#elif defined(__AVR_ATxmega128A1__)
		_port->DIRSET = _pin;
	#endif
}

void OutputPin::pulseOut(uint8_t data, uint16_t pulseWidth) {
	for(uint8_t i=0; i<8; i++) {
		if(data & 0x80) {
			set();
		} else {
			clear();
		}
		_delay_ms(pulseWidth);
		data <<= 1;
	}
}

void InputPin::begin() {
	#if defined(__AVR_ATmegaXXX__)
		*_pPort &= ~(1<<_pin);
		*_pDdr &= ~(1<<_pin);
	#elif defined(__AVR_ATxmega128A1__)
		_port->DIRCLR = _pin;
	#endif
}

void InputPin::enableInterrupt() {
	#if defined(__AVR_ATmegaXXX__)
		PCICR |= (1<<getPICIE(_pPort, _pin));
		*getPCMSK(_pPort, _pin) |= (1<<getPCINT(_pPort, _pin));
	#else
		#error Not Supported
	#endif
}

#if defined(__AVR_ATmegaXXX__)
Port::Port(volatile uint8_t &port) {
	_pPin = getPinRegisterFromPort(&port);
	_pPort = &port;
	_pDdr = getDdrRegisterFromPort(&port);
}
#endif

void Port::begin() {
}

#if defined(__AVR_ATmegaXXX__)
InputPort::InputPort(volatile uint8_t &port) : Port(port) {
}
#endif

void InputPort::begin() {
	Port::begin();
	#if defined(__AVR_ATmegaXXX__)
		*_pPort = 0;
		*_pDdr = 0;
	#else
		#error Not Supported
	#endif
}

#if defined(__AVR_ATmegaXXX__)
OutputPort::OutputPort(volatile uint8_t &port) : Port(port) {
}
#endif

void OutputPort::begin() {
	Port::begin();
	#if defined(__AVR_ATmegaXXX__)
		*_pDdr = 0xff;
	#else
		_dataPort->DIRSET = 0xff;
	#endif
}
