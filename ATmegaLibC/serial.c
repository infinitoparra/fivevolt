
#include "serial.h"
#include "io.h"
#include <avr/interrupt.h>
#include <string.h>

#ifndef SERIAL_DISABLED

uint8_t _rxBuffer[RX_BUFFER_SIZE];
uint8_t _rxBufferHead;
uint8_t _rxBufferTail;


void serial_begin(uint16_t baud) {
	_rxBufferHead = _rxBufferTail = 0;
	
	#if defined(__AVR_ATmegaXXUX__)
		io_input_begin(&PORTD, 2);
		io_output_begin(&PORTD, 3);
	#else
		#error Not Supported
	#endif

	#if defined(__AVR_ATmegaXXX__)
		uint16_t baud_setting;
		bool use_u2x;

		// U2X mode is needed for baud rates higher than (CPU Hz / 16)
		if (baud > F_CPU / 16) {
			use_u2x = true;
		} else {
			// figure out if U2X mode would allow for a better connection

			// calculate the percent difference between the baud-rate specified and
			// the real baud rate for both U2X and non-U2X mode (0-255 error percent)
			uint8_t nonu2x_baud_error = abs((int)(255-((F_CPU/(16*(((F_CPU/8/baud-1)/2)+1))*255)/baud)));
			uint8_t u2x_baud_error = abs((int)(255-((F_CPU/(8*(((F_CPU/4/baud-1)/2)+1))*255)/baud)));

			// prefer non-U2X mode because it handles clock skew better
			use_u2x = (nonu2x_baud_error > u2x_baud_error);
		}

		if (use_u2x) {
			UART0_UCSRA = 1 << UART0_U2X;
			baud_setting = (F_CPU / 4 / baud - 1) / 2;
		} else {
			UART0_UCSRA = 0;
			baud_setting = (F_CPU / 8 / baud - 1) / 2;
		}

		// assign the baud_setting, a.k.a. ubbr (USART Baud Rate Register)
		UART0_USRRH = baud_setting >> 8;
		UART0_USRRL = baud_setting;

		UART0_UCSRB |= (1<<UART0_RXEN);
		UART0_UCSRB |= (1<<UART0_TXEN);
		UART0_UCSRB |= (1<<UART0_RXCIE);
	#else
		#error Not Supported
	#endif

	serial_flush();
}

void serial_write(uint8_t ch) {
#if defined(__AVR_ATmegaXXX__)
	while (!(UART0_UCSRA & (1 << UART0_UDRE)));
	UART0_UDR = ch;
#else
	#error Not Supported
#endif
}

void serial_write_str(const char* p) {
	while(*p) {
		serial_write(*p++);
	}
}

void serial_write_str_P(PGM_P p) {
	uint8_t i;
	char c;
	for (i = 0; (c=pgm_read_byte(&p[i])); i++)  
	{       
		serial_write(c);
	}
}

uint8_t serial_rxDataReady() {
#if defined(__AVR_ATmegaXXX__)
	uint8_t data = UART0_UDR;
#else
	#error Not Supported
#endif
	uint8_t i = (_rxBufferHead + 1) % RX_BUFFER_SIZE;

	// if we should be storing the received character into the location
	// just before the tail (meaning that the head would advance to the
	// current location of the tail), we're about to overflow the buffer
	// and so we don't write the character or advance the head.
	if (i != _rxBufferTail) {
		_rxBuffer[_rxBufferHead] = data;
		_rxBufferHead = i;
		return data;
	}

	return -1;
}

void serial_flush() {
	// don't reverse this or there may be problems if the RX interrupt
	// occurs after reading the value of rx_buffer_head but before writing
	// the value to rx_buffer_tail; the previous value of rx_buffer_head
	// may be written to rx_buffer_tail, making it appear as if the buffer
	// don't reverse this or there may be problems if the RX interrupt
	// occurs after reading the value of rx_buffer_head but before writing
	// the value to rx_buffer_tail; the previous value of rx_buffer_head
	// may be written to rx_buffer_tail, making it appear as if the buffer
	// were full, not empty.
	_rxBufferHead = _rxBufferTail;
}

uint8_t serial_peek() {
	// if the head isn't ahead of the tail, we don't have any characters
	if (_rxBufferHead == _rxBufferTail) {
		return -1;
	}
	unsigned char c = _rxBuffer[_rxBufferTail];
	return c;
}

uint8_t serial_peek_n(uint16_t depth) {
	if (depth >= RX_BUFFER_SIZE) {
		return -1;
	}
	if (serial_available() <= depth) {
		return -1;
	}
	unsigned char c = _rxBuffer[(_rxBufferTail + depth) % RX_BUFFER_SIZE];
	return c;
}

uint16_t serial_available() {
	return (RX_BUFFER_SIZE + (int)_rxBufferHead - (int)_rxBufferTail) % RX_BUFFER_SIZE;
}

void serial_skip(uint16_t num) {
	cli();
	if (serial_available() <= num) {
		serial_flush();
	} else {
		_rxBufferTail = (_rxBufferTail + num) % RX_BUFFER_SIZE;
	}
	sei();
}

bool serial_read_line(char* buffer, uint16_t bufferSize) {
	uint16_t i;
	for(i=0; i<bufferSize-1; i++) {
		uint8_t c = serial_peek_n(i);
		if(c == (uint8_t)-1) {
			return false;
		}
		*buffer++ = c;
		if(c == '\n' || c == '\r') {
			break;
		}
	}
	serial_skip(i + 1);
	*buffer++ = '\0';
	return true;
}

void serial_write_uint8(uint8_t val, uint8_t radix) {
	char temp[5];
	utoa(val, temp, radix);
	if(radix == 16 && strlen(temp)==1) {
		temp[2] = '\0';
		temp[1] = temp[0];
		temp[0] = '0';
	}
	serial_write_str(temp);
}

void serial_write_uint16(uint16_t val, uint8_t radix) {
	char temp[10];
	utoa(val, temp, radix);
	serial_write_str(temp);
}

void serial_write_uint32(uint32_t val, uint8_t radix) {
	char temp[15];
	ultoa(val, temp, radix);
	serial_write_str(temp);
}

void serial_write_crlf() {
	serial_write('\r');
	serial_write('\n');
}

void serial_write_memory(uint8_t *buffer, uint16_t length) {
	for(uint16_t i=0; i<length; i+=16) {
		for(uint16_t j=0; j<16 && i+j<length; j++) {
			serial_write_uint8(buffer[i+j], 16);
			serial_write(' ');
		}
		serial_write_crlf();
	}
}

#endif