
#ifndef _io_h_
#define _io_h_

#include "common.h"

void io_input_begin(volatile uint8_t *port, uint8_t pin);

void io_output_begin(volatile uint8_t *port, uint8_t pin);
#define io_output_set(port, pin) { port |= (1<<pin); }
#define io_output_clear(port, pin) { port &= ~(1<<pin); }
#define io_output_set_val(port, pin, val) { \
	if(val) {                       \
		io_output_set(port,pin);    \
	} else {                        \
		io_output_clear(port,pin);  \
	}                               \
}

#endif
