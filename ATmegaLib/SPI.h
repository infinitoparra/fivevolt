
#ifndef _spi_h_
#define _spi_h_

#include <avr/io.h>
#include "IO.h"

#if defined(__AVR_ATmega328P__) || defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
#define SPI_BITORDER_LSBFIRST 0
#define SPI_BITORDER_MSBFIRST 1

#define SPI_CLOCK_DIV4 0x00
#define SPI_CLOCK_DIV16 0x01
#define SPI_CLOCK_DIV64 0x02
#define SPI_CLOCK_DIV128 0x03
#define SPI_CLOCK_DIV2 0x04
#define SPI_CLOCK_DIV8 0x05
#define SPI_CLOCK_DIV32 0x06
//#define SPI_CLOCK_DIV64 0x07

#define SPI_MODE0 0x00
#define SPI_MODE1 0x04
#define SPI_MODE2 0x08
#define SPI_MODE3 0x0C

#define SPI_MODE_MASK 0x0C  // CPOL = bit 3, CPHA = bit 2 on SPCR
#define SPI_CLOCK_MASK 0x03  // SPR1 = bit 1, SPR0 = bit 0 on SPCR
#define SPI_2XCLOCK_MASK 0x01  // SPI2X = bit 0 on SPSR
#else
	#error Not Supported
#endif

class SPI {
public:
#if defined(__AVR_ATmega328P__) || defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
	SPI();
#elif defined(__AVR_ATxmega128A1__)
	SPI(SPI_t &spi);
#endif

	void begin();

#if defined(__AVR_ATmega328P__) || defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
	void setBitOrder(uint8_t bitOrder);
	void setDataMode(uint8_t mode);
  	void setClockDivider(uint8_t rate);
#endif

#if defined(__AVR_ATxmega128A1__)
	PORT_struct* getPort();
	SPI_t* getSPI() { return _spi; }
#endif

private:
#if defined(__AVR_ATxmega128A1__)
	SPI_t *_spi;
#endif
};

class SPIDevice {
public:
	SPIDevice(SPI &spi, OutputPin &csPin);
	void begin();
	void beginTransfer();
	void endTransfer();
	uint8_t write(uint8_t val);


protected:
	SPI *_spi;
	OutputPin *_csPin;
};
	
#endif
