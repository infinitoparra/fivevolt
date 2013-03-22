
#include "io.h"
#include "serial.h"

volatile uint8_t* getDdrRegisterFromPort(volatile uint8_t* port) {
	#if defined(PORTA)
		if(port == &PORTA) {
			return &DDRA;
		}
	#endif
	#if defined(PORTB)
		if(port == &PORTB) {
			return &DDRB;
		}
	#endif
	#if defined(PORTC)
		if(port == &PORTC) {
			return &DDRC;
		}
	#endif
	#if defined(PORTD)
		if(port == &PORTD) {
			return &DDRD;
		}
	#endif
	#if defined(PORTF)
		if(port == &PORTF) {
			return &DDRF;
		}
	#endif
	return (volatile uint8_t*)0;
}

void io_input_begin(volatile uint8_t *port, uint8_t pin) {
	volatile uint8_t* ddr = getDdrRegisterFromPort(port);
	*port &= ~(1<<pin);
	*ddr &= ~(1<<pin);
}

void io_output_begin(volatile uint8_t *port, uint8_t pin) {
	volatile uint8_t* ddr = getDdrRegisterFromPort(port);
	*port |= (1<<pin);
	*ddr |= (1<<pin);
}

