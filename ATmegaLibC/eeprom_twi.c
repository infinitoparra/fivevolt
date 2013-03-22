
#include "eeprom_twi.h"
#include "io.h"
#include <util/delay.h>

#define CONTROL_CODE 0xa0
#define EEPROM_WRITE 0x00
#define EEPROM_READ  0x01
#define PULSE_TIME 1

#define eeprom_twi_sda_high() { *portSda |= (1<<pinSda); }
#define eeprom_twi_sda_low() { *portSda &= ~(1<<pinSda); }
#define eeprom_twi_sda_read() (*pinRegSda & (1<<pinSda))
#define eeprom_twi_scl_high() { *portScl |= (1<<pinScl); }
#define eeprom_twi_scl_low() { *portScl &= ~(1<<pinScl); }
#define eeprom_twi_sda_in() { *ddrSda &= ~(1<<pinSda); }
#define eeprom_twi_sda_out() { *ddrSda |= (1<<pinSda); }

void eeprom_twi_recv_ack(volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *portScl, uint8_t pinScl) {
	eeprom_twi_scl_low();
	_delay_us(1);
	eeprom_twi_sda_low();
	eeprom_twi_sda_in();
	_delay_us(PULSE_TIME);
	eeprom_twi_scl_high();
	// todo: check ack
	_delay_us(PULSE_TIME);
	eeprom_twi_sda_out();
	eeprom_twi_scl_low();
	_delay_us(PULSE_TIME);
}

void eeprom_twi_send_byte(volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *portScl, uint8_t pinScl, uint8_t b) {
	uint8_t i;
	
	eeprom_twi_sda_out();
	for(i=0; i<8; i++) {
		eeprom_twi_scl_low();
		_delay_us(PULSE_TIME);
		if((b & 0x80) == 0x80) {
			eeprom_twi_sda_high();
		} else {
			eeprom_twi_sda_low();
		}
		b <<= 1;
		_delay_us(PULSE_TIME);
		eeprom_twi_scl_high();
		_delay_us(PULSE_TIME);
	}

	eeprom_twi_recv_ack(ddrSda, portSda, pinSda, portScl, pinScl);
	_delay_us(30);
}

uint8_t eeprom_twi_recv_byte(
	volatile uint8_t *pinRegSda, volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *portScl, uint8_t pinScl,
	bool more) {
	uint8_t i;
	uint8_t result = 0;

	eeprom_twi_sda_in();
	for(i=0; i<8; i++) {
		eeprom_twi_scl_low();
		_delay_us(PULSE_TIME);
		eeprom_twi_scl_high();
		_delay_us(PULSE_TIME);
		result = result << 1;
		result |= ((eeprom_twi_sda_read()!=0) ? 0x01 : 0x00);
		_delay_us(PULSE_TIME);
	}
	
	// send ack (continue)
	eeprom_twi_scl_low();
	_delay_us(1);
	if(more) {
		eeprom_twi_sda_low();
	} else {
		eeprom_twi_sda_high();
	}
	eeprom_twi_sda_out();
	_delay_us(PULSE_TIME);
	eeprom_twi_scl_high();
	_delay_us(PULSE_TIME);
	eeprom_twi_scl_low();
	_delay_us(30);

	return result;
}

void eeprom_twi_begin(EEPROM_PARAMS) {
	eeprom_twi_sda_out();
	*ddrScl |= (1<<pinScl); // set SCL as output
	// set SCL first so that we don't trigger a start or stop condition
	eeprom_twi_scl_high();
	eeprom_twi_sda_high();
}

void eeprom_twi_start(volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *portScl, uint8_t pinScl) {
	eeprom_twi_sda_high();
	eeprom_twi_sda_out();
	eeprom_twi_scl_high();
	_delay_us(PULSE_TIME);
	eeprom_twi_sda_low();
	_delay_us(PULSE_TIME);
}

void eeprom_twi_stop(volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *portScl, uint8_t pinScl) {
	eeprom_twi_sda_low();
	eeprom_twi_sda_out();
	_delay_us(PULSE_TIME);
	eeprom_twi_scl_high();
	_delay_us(4*PULSE_TIME);
	eeprom_twi_sda_high();
	_delay_us(PULSE_TIME);
}

void eeprom_twi_write(EEPROM_PARAMS, uint16_t addr, uint8_t data) {
	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_WRITE);

	// address
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 8) & 0xff);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 0) & 0xff);

	// send data
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, data);

	eeprom_twi_stop(ddrSda, portSda, pinSda, portScl, pinScl);
}

void eeprom_twi_write_block(EEPROM_PARAMS, uint16_t addr, uint8_t *data, uint8_t dataLength) {
	uint8_t i;

	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_WRITE);

	// address
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 8) & 0xff);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 0) & 0xff);

	// send data
	for(i=0; i<dataLength; i++) {
		eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, data[i]);
	}

	eeprom_twi_stop(ddrSda, portSda, pinSda, portScl, pinScl);
}

uint8_t eeprom_twi_read(EEPROM_PARAMS, uint16_t addr) {
	uint8_t result = 0;

	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_WRITE);

	// address
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 8) & 0xff);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 0) & 0xff);

	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);

	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_READ);

	// read byte
	result = eeprom_twi_recv_byte(pinRegSda, ddrSda, portSda, pinSda, portScl, pinScl, false);

	eeprom_twi_stop(ddrSda, portSda, pinSda, portScl, pinScl);

	return result;
}

void eeprom_twi_read_block(EEPROM_PARAMS, uint16_t addr, uint8_t *data, uint16_t dataLength) {
	uint16_t i;

	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_WRITE);

	// address
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 8) & 0xff);
	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, (addr >> 0) & 0xff);

	eeprom_twi_start(ddrSda, portSda, pinSda, portScl, pinScl);

	eeprom_twi_send_byte(ddrSda, portSda, pinSda, portScl, pinScl, CONTROL_CODE | (0<<1) | EEPROM_READ);

	// read byte
	for(i=0; i<dataLength; i++) {
		bool more = (i != dataLength-1);
		data[i] = eeprom_twi_recv_byte(pinRegSda, ddrSda, portSda, pinSda, portScl, pinScl, more);
	}

	eeprom_twi_stop(ddrSda, portSda, pinSda, portScl, pinScl);
}

