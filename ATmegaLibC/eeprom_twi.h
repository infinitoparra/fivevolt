
#ifndef _eeprom_twi_h_
#define _eeprom_twi_h_

#include <stdint.h>

#define EEPROM_PARAMS volatile uint8_t *pinRegSda, volatile uint8_t *ddrSda, volatile uint8_t *portSda, uint8_t pinSda, volatile uint8_t *ddrScl, volatile uint8_t *portScl, uint8_t pinScl

void eeprom_twi_begin(EEPROM_PARAMS);
void eeprom_twi_write(EEPROM_PARAMS, uint16_t addr, uint8_t data);
void eeprom_twi_write_block(EEPROM_PARAMS, uint16_t addr, uint8_t *data, uint8_t dataLength);
uint8_t eeprom_twi_read(EEPROM_PARAMS, uint16_t addr);
void eeprom_twi_read_block(EEPROM_PARAMS, uint16_t addr, uint8_t *data, uint16_t dataLength);

#endif

