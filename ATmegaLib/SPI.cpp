
#include "SPI.h"
#include <util/delay.h>

#if defined(__AVR_ATmega328P__)
	#define SPI_SS_PORT   PORTB
	#define SPI_SS_PIN    PIN2
	#define SPI_MOSI_PORT PORTB
	#define SPI_MOSI_PIN  PIN3
	#define SPI_MISO_PORT PORTB
	#define SPI_MISO_PIN  PIN4
	#define SPI_SCK_PORT  PORTB
	#define SPI_SCK_PIN   PIN5
#elif defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)
	#define SPI_SS_PORT   PORTB
	#define SPI_SS_PIN    PIN4
	#define SPI_MOSI_PORT PORTB
	#define SPI_MOSI_PIN  PIN5
	#define SPI_MISO_PORT PORTB
	#define SPI_MISO_PIN  PIN6
	#define SPI_SCK_PORT  PORTB
	#define SPI_SCK_PIN   PIN7
#endif

#if defined(__AVR_ATmega328P__) || defined(__AVR_ATmega644P__) || defined(__AVR_ATmega1284P__)

SPI::SPI() {
}

void SPI::begin() {
	OutputPin sck(SPI_SCK_PORT, SPI_SCK_PIN);
	OutputPin mosi(SPI_MOSI_PORT, SPI_MOSI_PIN);
	InputPin miso(SPI_MISO_PORT, SPI_MISO_PIN);
	OutputPin ss(SPI_SS_PORT, SPI_SS_PIN);

	sck.begin();
	mosi.begin();
	miso.begin();
	ss.begin();

	sck.clear();
	mosi.clear();
	ss.set();	

	// Warning: if the SS pin ever becomes a LOW INPUT then SPI 
	// automatically switches to Slave, so the data direction of 
	// the SS pin MUST be kept as OUTPUT.
	SPCR = (1<<MSTR) | (1<<SPE);
}

void SPI::setBitOrder(uint8_t bitOrder) {
	if(bitOrder == SPI_BITORDER_LSBFIRST) {
		SPCR |= _BV(DORD);
	} else {
		SPCR &= ~(_BV(DORD));
	}
}

void SPI::setDataMode(uint8_t mode) {
	SPCR = (SPCR & ~SPI_MODE_MASK) | mode;
}

void SPI::setClockDivider(uint8_t rate) {
	SPCR = (SPCR & ~SPI_CLOCK_MASK) | (rate & SPI_CLOCK_MASK);
	SPSR = (SPSR & ~SPI_2XCLOCK_MASK) | (rate & SPI_2XCLOCK_MASK);
}

SPIDevice::SPIDevice(SPI &spi, OutputPin &csPin) {
	_csPin = &csPin;
	_spi = &spi;
}

void SPIDevice::begin() {
	_csPin->begin();
	_csPin->set();
}

void SPIDevice::beginTransfer() {
	_csPin->clear();
}

void SPIDevice::endTransfer() {
	_csPin->set();
}

uint8_t SPIDevice::write(uint8_t val) {
	SPDR = val;
	while (!(SPSR & _BV(SPIF)));
	return SPDR;
}

#elif defined(__AVR_ATxmega128A1__)

#define SPI_MOSI_PIN PIN5_bm
#define SPI_MISO_PIN PIN6_bm
#define SPI_SCK_PIN  PIN7_bm

SPI::SPI(SPI_t &spi) {
	_spi = &spi;
}

void SPI::begin() {
	PORT_struct *port = getPort();

	port->DIRSET = SPI_MOSI_PIN;
	port->DIRCLR = SPI_MISO_PIN;
	port->DIRSET = SPI_SCK_PIN;

	_spi->CTRL = SPI_ENABLE_bm | SPI_MASTER_bm;
	_spi->INTCTRL = SPI_INTLVL_OFF_gc;
}

PORT_struct* SPI::getPort() {
	if(_spi == &SPIC) {
		return &PORTC;
	} else if(_spi == &SPID) {
		return &PORTD;
	} else if(_spi == &SPIE) {
		return &PORTE;
	} else if(_spi == &SPIF) {
		return &PORTF;
	}
	return (PORT_struct*)-1;
}

SPIDevice::SPIDevice(xSPI &spi, PORT_struct &port, uint8_t pin) {
	_spi = &spi;
	_port = &port;
	_pin = pin;
}

void SPIDevice::begin() {
	_port->DIRSET = _pin;
	_port->PIN4CTRL = PORT_OPC_WIREDANDPULL_gc;
	_port->OUTSET = _pin;
}

void SPIDevice::beginTransfer() {
	SPI_PRESCALER_t clockDivision = SPI_PRESCALER_DIV128_gc;
	bool clk2x = false;
	bool lsbFirst = false;
	SPI_MODE_t mode = SPI_MODE_0_gc;
	SPI_t *spi = _spi->getSPI();

	spi->CTRL = (spi->CTRL & ~(SPI_PRESCALER_gm | SPI_CLK2X_bm | SPI_DORD_bm | SPI_MODE_gm))
		| clockDivision                // SPI prescaler.
	    | (clk2x ? SPI_CLK2X_bm : 0)   // SPI Clock double.
	    | (lsbFirst ? SPI_DORD_bm : 0) // Data order.
		| mode;                        // SPI mode.
	_port->OUTCLR = _pin;
}

void SPIDevice::endTransfer() {
	_port->OUTSET = _pin;
}

uint8_t SPIDevice::write(uint8_t val) {
	SPI_t *spi = _spi->getSPI();

	spi->DATA = val;
	while(!(spi->STATUS & SPI_IF_bm)); // wait for complete
	return spi->DATA;
}

#else

#error "Not Supported"

#endif
