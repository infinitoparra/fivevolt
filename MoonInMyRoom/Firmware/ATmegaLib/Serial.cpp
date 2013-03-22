
#include <stdlib.h>
#include <math.h>
#include <avr/interrupt.h>
#include "Utils.h"
#include "Serial.h"

/**
 * Creates the serial driver.
 * @param usart Can be USARTC0, USARTC1, USARTD0, USARTD1, USARTE0, USARTE1, USARTF0, or USARTF1.
 */
#if defined(__AVR_ATmegaXXX__)
Serial::Serial(
		volatile uint8_t *ubrrh, volatile uint8_t *ubrrl,
		volatile uint8_t *ucsra, volatile uint8_t *ucsrb,
		volatile uint8_t *udr,
		uint8_t rxen, uint8_t txen, uint8_t rxcie, uint8_t udre, uint8_t u2x) {
	_ubrrh = ubrrh;
	_ubrrl = ubrrl;
	_ucsra = ucsra;
	_ucsrb = ucsrb;
	_udr = udr;
	_rxen = rxen;
	_txen = txen;
	_rxcie = rxcie;
	_udre = udre;
	_u2x = u2x;
}

#elif defined(__AVR_ATxmega128A1__)
Serial::Serial(USART_t &usart) {
	_usart = &usart;
}
#else
	#error Not Supported
#endif

/**
 * Sets the pins for input or output (rx, tx respectivly), set the baud rate settings
 * and enables the USART.
 * The pins are 3 for tx and 2 for rx on USARTX0 on the respective port and 7 for tx and
 * 6 for rx on USARTX1. The ports are C, D, E, F.
 */
void Serial::begin(uint32_t baud) {
#if defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__) || defined(__AVR_ATmegaXX8__)
	InputPin rxPin(PORTD, 0);
	rxPin.begin();
	OutputPin txPin(PORTD, 1);
	txPin.begin();
#elif defined(__AVR_ATmega8U2__)
	InputPin rxPin(PORTD, 2);
	rxPin.begin();
	OutputPin txPin(PORTD, 3);
	txPin.begin();
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
		*_ucsra = 1 << _u2x;
		baud_setting = (F_CPU / 4 / baud - 1) / 2;
	} else {
		*_ucsra = 0;
		baud_setting = (F_CPU / 8 / baud - 1) / 2;
	}

	// assign the baud_setting, a.k.a. ubbr (USART Baud Rate Register)
	*_ubrrh = baud_setting >> 8;
	*_ubrrl = baud_setting;

	sbi(*_ucsrb, _rxen);
	sbi(*_ucsrb, _txen);
	sbi(*_ucsrb, _rxcie);
#elif defined(__AVR_ATxmega128A1__)
	if(_usart == &USARTC0) {
		PORTC.DIRSET = PIN3_bm; // tx
		PORTC.DIRCLR = PIN2_bm; // rx
	} else if(_usart == &USARTC1) {
		PORTC.DIRSET = PIN7_bm;
		PORTC.DIRCLR = PIN6_bm;
	} else if(_usart == &USARTD0) {
		PORTD.DIRSET = PIN3_bm;
		PORTD.DIRCLR = PIN2_bm;
	} else if(_usart == &USARTD1) {
		PORTD.DIRSET = PIN7_bm;
		PORTD.DIRCLR = PIN6_bm;
	} else if(_usart == &USARTE0) {
		PORTE.DIRSET = PIN3_bm;
		PORTE.DIRCLR = PIN2_bm;
	} else if(_usart == &USARTE1) {
		PORTE.DIRSET = PIN7_bm;
		PORTE.DIRCLR = PIN6_bm;
	} else if(_usart == &USARTF0) {
		PORTF.DIRSET = PIN3_bm;
		PORTF.DIRCLR = PIN2_bm;
	} else if(_usart == &USARTF1) {
		PORTF.DIRSET = PIN7_bm;
		PORTF.DIRCLR = PIN6_bm;
	}

	uint8_t _charSize = USART_CHSIZE_8BIT_gc;
	uint8_t _parityMode = USART_PMODE_DISABLED_gc;
	uint8_t _twoStopBits = false;

	calculateBestSettings(baud, &_bscale, &_bsel, &_clk2x);
	_usart->BAUDCTRLA = (uint8_t)_bsel;
	_usart->BAUDCTRLB = (_bscale << USART_BSCALE0_bp) | (_bsel >> 8);
	
	_usart->CTRLB = USART_RXEN_bm | USART_TXEN_bm | (_clk2x == 0 ? 0 : USART_CLK2X_bm);
	_usart->CTRLA = (_usart->CTRLA & ~USART_RXCINTLVL_gm) | USART_RXCINTLVL_HI_gc;
	_usart->CTRLC = (uint8_t) _charSize | _parityMode | (_twoStopBits ? USART_SBMODE_bm : 0);
#else
	#error Not Supported
#endif
}

void Serial::setCTSPin(InputPin &ctsPin) {
	_ctsEnabled = true;
	_ctsPin = &ctsPin;
}

#if defined(__AVR_ATxmega128A1__)
uint32_t Serial::calculateBaud(int8_t bscale, uint16_t bsel, uint8_t clk2x) {
	if(bscale >= 0) {
		if(!clk2x) {
			return F_CPU / (pow2(bscale) * 16 * (bsel + 1));
		} else {
			return F_CPU / (pow2(bscale) * 8 * (bsel + 1));
		}
	} else {
		if(!clk2x) {
			return F_CPU / (16 * ((pow2(-bscale) * bsel) + 1));
		} else {
			return F_CPU / (8 * ((pow2(-bscale) * bsel) + 1));
		}
	}
}

int32_t Serial::calculateBSEL(uint32_t baud, int8_t bscale, uint8_t clk2x) {
	if(bscale >= 0) {
		if(!clk2x) {
			return round(((float)F_CPU / ((float)pow2(bscale) * 16.0f * (float)baud)) - 1.0f);
		} else {
			return round(((float)F_CPU / ((float)pow2(bscale) * 8.0f * (float)baud)) - 1.0f);
		}
	} else {
		if(!clk2x) {
			return round((((float)F_CPU / (16.0f * (float)baud)) - 1.0f) / (float)pow2(-bscale));
		} else {
			return round((((float)F_CPU / (8.0f * (float)baud)) - 1.0f) / (float)pow2(-bscale));
		}
	}
}

void Serial::calculateBestSettings(uint32_t baud, int8_t *pbscale, uint16_t *pbsel, uint8_t *pclk2x) {
	*pbscale = 0;
	*pbsel = 0;
	*pclk2x = 0;
	uint16_t lowestError = 65535;
	int8_t bscales[] = { 0, 1, -1, 2, -2, 3, -3, 4, -4, 5, -5, 6, -6, 7, -7 };
	for(uint8_t clk2x=0; clk2x<=1; clk2x++) {
		for(int8_t bscaleIdx=0; bscaleIdx<15; bscaleIdx++) {
			int8_t bscale = bscales[bscaleIdx];
			int32_t bsel = calculateBSEL(baud, bscale, clk2x);
			if(bsel < 0 || bsel > 4095) {
				continue;
			}
			uint32_t calculatedBaud = calculateBaud(bscale, bsel, clk2x);
			uint16_t error = abs((float)((int32_t)calculatedBaud - (int32_t)baud));
			if(error < lowestError) {
				*pbscale = bscale;
				*pbsel = bsel;
				*pclk2x = clk2x;
				lowestError = error;
				if(((float)lowestError / (float)baud) < 0.005) { // 0.05%
					return;
				}
			}
		}
	}
}

uint16_t Serial::pow2(uint8_t exp) {
	int result = 1;
	while(exp--) {
		result *= 2;
	}
	return result;
}
#endif

void Serial::write(uint8_t ch) {
	if(_ctsEnabled) {
		while(_ctsPin->isSet());
	}

#if defined(__AVR_ATmegaXXX__)
	while (!((*_ucsra) & (1 << _udre)));
	*_udr = ch;
#elif defined(__AVR_ATxmega128A1__)
	while((_usart->STATUS & USART_DREIF_bm) == 0);
	_usart->DATA = ch;
#else
	#error Not Supported
#endif
}

uint16_t Serial::available() {
	return (RX_BUFFER_SIZE + (int)_rxBufferHead - (int)_rxBufferTail) % RX_BUFFER_SIZE;
}

uint8_t Serial::read() {
	// if the head isn't ahead of the tail, we don't have any characters
	if (_rxBufferHead == _rxBufferTail) {
		return -1;
	} else {
		unsigned char c = _rxBuffer[_rxBufferTail];
		_rxBufferTail = (_rxBufferTail + 1) % RX_BUFFER_SIZE;
		return c;
	}
}

uint8_t Serial::peek() {
	// if the head isn't ahead of the tail, we don't have any characters
	if (_rxBufferHead == _rxBufferTail) {
		return -1;
	}
	unsigned char c = _rxBuffer[_rxBufferTail];
	return c;
}

uint8_t Serial::peek(uint16_t depth) {
	if (depth >= RX_BUFFER_SIZE) {
		return -1;
	}
	if (available() <= depth) {
		return -1;
	}
	unsigned char c = _rxBuffer[(_rxBufferTail + depth) % RX_BUFFER_SIZE];
	return c;
}

void Serial::flush() {
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

uint8_t Serial::rxDataReady() { 
#if defined(__AVR_ATmegaXXX__)
	uint8_t data = *_udr;
#elif defined(__AVR_ATxmega128A1__)
	uint8_t data = _usart->DATA;
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

void Serial::skip(uint16_t num) {
	cli();
	if (available() <= num) {
		flush();
	} else {
		_rxBufferTail = (_rxBufferTail + num) % RX_BUFFER_SIZE;
	}
	sei();
}

bool Serial::readLine(char* buffer, uint16_t bufferSize) {
	uint16_t i;
	for(i=0; i<bufferSize-1; i++) {
		uint8_t c = peek(i);
		if(c == (uint8_t)-1) {
			return false;
		}
		*buffer++ = c;
		if(c == '\n' || c == '\r') {
			break;
		}
	}
	skip(i + 1);
	*buffer++ = '\0';
	return true;
}
