
#ifndef _Serial_h_
#define _Serial_h_

#include <avr/io.h>
#include "Stream.h"
#include "IO.h"

#define RX_BUFFER_SIZE 128

class Serial : public Stream {
public:
#if defined(__AVR_ATmegaXXX__)
	Serial(
		volatile uint8_t *ubrrh, volatile uint8_t *ubrrl,
		volatile uint8_t *ucsra, volatile uint8_t *ucsrb,
		volatile uint8_t *udr,
		uint8_t rxen, uint8_t txen, uint8_t rxcie, uint8_t udre, uint8_t u2x);
#elif defined(__AVR_ATxmega128A1__)
	Serial(USART_t &usart);
#else
	#error Not Supported
#endif
	void setCTSPin(InputPin &ctsPin);
	void begin(uint32_t baud);
	virtual void write(uint8_t ch);
	virtual uint16_t available();
    virtual uint8_t read();
	virtual uint8_t peek();
    virtual void flush();
	virtual void skip(uint16_t num);
	uint8_t peek(uint16_t depth);
	bool readLine(char* buffer, uint16_t bufferSize);
#if defined(__AVR_ATxmega128A1__)
	uint16_t getBSEL() { return _bsel; }
	int8_t getBSCALE() { return _bscale; }
	uint8_t getCLK2X() { return _clk2x; }
#endif
	uint8_t rxDataReady();

private:
#if defined(__AVR_ATxmega128A1__)
	uint32_t calculateBaud(int8_t bscale, uint16_t bsel, uint8_t clk2x);
	int32_t calculateBSEL(uint32_t baud, int8_t bscale, uint8_t clk2x);
	void calculateBestSettings(uint32_t baud, int8_t *pbscale, uint16_t *pbsel, uint8_t *pclk2x);
	uint16_t pow2(uint8_t exp);
#endif

#if defined(__AVR_ATmegaXXX__)
	volatile uint8_t *_ubrrh;
    volatile uint8_t *_ubrrl;
    volatile uint8_t *_ucsra;
    volatile uint8_t *_ucsrb;
    volatile uint8_t *_udr;
    uint8_t _rxen;
    uint8_t _txen;
    uint8_t _rxcie;
    uint8_t _udre;
    uint8_t _u2x;
#elif defined(__AVR_ATxmega128A1__)
	USART_t* _usart;
	uint16_t _bsel;
	int8_t _bscale;
	uint8_t _clk2x;
#else
	#error Not Supported
#endif

	bool _ctsEnabled;
	InputPin *_ctsPin;
	uint8_t _rxBuffer[RX_BUFFER_SIZE];
  	uint8_t _rxBufferHead;
	uint8_t _rxBufferTail;
};

#if defined(__AVR_ATmega328P__)
	#define UART0  &UBRR0H, &UBRR0L, &UCSR0A, &UCSR0B, &UDR0, RXEN0, TXEN0, RXCIE0, UDRE0, U2X0
#elif defined(__AVR_ATmega644P__)
	#define UART0  &UBRR0H, &UBRR0L, &UCSR0A, &UCSR0B, &UDR0, RXEN0, TXEN0, RXCIE0, UDRE0, U2X0
#elif defined(__AVR_ATmega1284P__)
	#define UART0  &UBRR0H, &UBRR0L, &UCSR0A, &UCSR0B, &UDR0, RXEN0, TXEN0, RXCIE0, UDRE0, U2X0
#elif defined(__AVR_ATmega8U2__)
	#define UART0  &UBRR1H, &UBRR1L, &UCSR1A, &UCSR1B, &UDR1, RXEN1, TXEN1, RXCIE1, UDRE1, U2X1
#endif

#endif
