
#include <avr/io.h>
#include "Common.h"

#ifndef _io_h_
#define _io_h_

class IODevice {
public:
	#if defined(__AVR_ATmegaXXX__)
		static volatile uint8_t* getPinRegisterFromPort(volatile uint8_t* port);
		static volatile uint8_t* getDdrRegisterFromPort(volatile uint8_t* port);
		static uint8_t getPICIE(volatile uint8_t* port, uint8_t pin);
		static uint8_t getPCINT(volatile uint8_t* port, uint8_t pin);
		static volatile uint8_t* getPCMSK(volatile uint8_t* port, uint8_t pin);
	#endif

private:
	#if defined(__AVR_ATmegaXXX__)
		static uint8_t getPCINTNumber(volatile uint8_t* port, uint8_t pin);
	#endif
};

class Pin : public IODevice {
public:
	bool isSet() { 
		#if defined(__AVR_ATmegaXXX__)
			return *_pPin & _BV(_pin);
		#elif defined(__AVR_ATxmega128A1__)
			return _port->IN & _pin;
		#else
			#error Not Supported
		#endif
	}

protected:
	#if defined(__AVR_ATmegaXXX__)
		Pin(volatile uint8_t &port, uint8_t pin);
	#elif defined(__AVR_ATxmega128A1__)
		Pin(PORT_struct &port, uint8_t pin);
	#else
		#error Not Supported
	#endif

	#if defined(__AVR_ATmegaXXX__)
		volatile uint8_t* _pPin;
		volatile uint8_t* _pPort;
		volatile uint8_t* _pDdr;
	#elif defined(__AVR_ATxmega128A1__)
		PORT_struct *_port;
	#else
		#error Not Supported
	#endif
	uint8_t _pin;
};

class OutputPin : public Pin {
public:
	#if defined(__AVR_ATmegaXXX__)
		OutputPin(volatile uint8_t &port, uint8_t pin);
	#elif defined(__AVR_ATxmega128A1__)
		OutputPin(PORT_struct &port, uint8_t pin);
	#else
		#error Not Supported
	#endif

	void begin();

	void clear() { 
		#if defined(__AVR_ATmegaXXX__)
			*_pPort &= ~(1<<_pin);
		#elif defined(__AVR_ATxmega128A1__)
			_port->OUTCLR = _pin; 
		#else
			#error Not Supported
		#endif
	}
	void set() { 
		#if defined(__AVR_ATmegaXXX__)
			*_pPort |= (1<<_pin);
		#elif defined(__AVR_ATxmega128A1__)
			_port->OUTSET = _pin; 
		#else
			#error Not Supported
		#endif
	}
	void set(bool val) { if(val) { set(); } else { clear(); } }
	void toggle() { 
		#if defined(__AVR_ATmegaXXX__)
			if(isSet()) {
				clear();
			} else {
				set();
			}
		#elif defined(__AVR_ATxmega128A1__)
			_port->OUTTGL = _pin; 
		#else
			#error Not Supported
		#endif
	}

	void pulseOut(uint8_t data, uint16_t pulseWidth);
};

class InputPin : public Pin {
public:
	#if defined(__AVR_ATmegaXXX__)
		InputPin(volatile uint8_t &port, uint8_t pin);
	#elif defined(__AVR_ATxmega128A1__)
		InputPin(PORT_struct &port, uint8_t pin);
	#else
		#error Not Supported
	#endif

	void begin();
	void enableInterrupt();
};

class Port : public IODevice {
public:
	#if defined(__AVR_ATmegaXXX__)
		Port(volatile uint8_t &port);
	#elif defined(__AVR_ATxmega128A1__)
		Port(PORT_struct &port);
	#else
		#error Not Supported
	#endif

	void begin();
	
protected:
	#if defined(__AVR_ATmegaXXX__)
		volatile uint8_t* _pPin;
		volatile uint8_t* _pPort;
		volatile uint8_t* _pDdr;
	#endif
};

class OutputPort : public Port {
public:
	#if defined(__AVR_ATmegaXXX__)
		OutputPort(volatile uint8_t &port);
	#else
		#error Not Supported
	#endif

	void begin();
	void write(uint8_t data) { *_pPort = data; }
};

class InputPort : public Port {
public:
	#if defined(__AVR_ATmegaXXX__)
		InputPort(volatile uint8_t &port);
	#else
		#error Not Supported
	#endif

	void begin();
	uint8_t read() { return *_pPin; }
};

class IOPort : public Port {
};

#endif
