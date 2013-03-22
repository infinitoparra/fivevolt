
#ifndef _serial_h_
#define _serial_h_

#include "common.h"
#include <stdint.h>
#include <avr/pgmspace.h>

#ifndef RX_BUFFER_SIZE
	#define RX_BUFFER_SIZE 64
#endif

#if defined(__AVR_ATmegaXXUX__)
	#define UART0_UCSRA UCSR1A
	#define UART0_UCSRB UCSR1B
	#define UART0_U2X   U2X1
	#define UART0_USRRH UBRR1H
	#define UART0_USRRL UBRR1L
	#define UART0_RXEN  RXEN1
	#define UART0_TXEN  TXEN1
	#define UART0_RXCIE RXCIE1
	#define UART0_UDR   UDR1
	#define UART0_UDRE  UDRE1
	#define serial_RX_vect USART1_RX_vect
#else
	#error "Not supported"
#endif

#ifdef SERIAL_DISABLED
#define serial_begin(baud) {}
#define serial_write(ch) {}
#define serial_write_str(p) {}
#define serial_write_str_P(p) {}
#define serial_write_uint8(val, radix) {}
#define serial_write_uint16(val, radix) {}
#define serial_write_uint32(val, radix) {}
#define serial_write_crlf() {}
#define serial_write_memory(buffer, length) {}
#define serial_available() {}
#define serial_skip(num) {}
#define serial_peek() {}
#define serial_peek_n(depth) {}
#define serial_read_line(buffer, bufferSize) false
#define serial_rxDataReady() 0
#define serial_flush() {}

#else
void serial_begin(uint16_t baud);
void serial_write(uint8_t ch);
void serial_write_str(const char* p);
void serial_write_str_P(PGM_P p);
void serial_write_uint8(uint8_t val, uint8_t radix);
void serial_write_uint16(uint16_t val, uint8_t radix);
void serial_write_uint32(uint32_t val, uint8_t radix);
void serial_write_crlf(void);
void serial_write_memory(uint8_t *buffer, uint16_t length);
uint16_t serial_available(void);
void serial_skip(uint16_t num);
uint8_t serial_peek(void);
uint8_t serial_peek_n(uint16_t depth);
bool serial_read_line(char* buffer, uint16_t bufferSize);
uint8_t serial_rxDataReady(void);
void serial_flush(void);
#endif

#endif
