
#include "SPI.h"

#ifndef _DigitalPot_MCP4251_h_
#define _DigitalPot_MCP4251_h_

typedef enum DigitalPotMCP4251_Address_enum {
	DigitalPotMCP4251_Address_Wiper0 = 0x00,
	DigitalPotMCP4251_Address_Wiper1 = 0x01,
	DigitalPotMCP4251_Address_TCON   = 0x04,
	DigitalPotMCP4251_Address_Status = 0x05
} DigitalPotMCP4251_Address_t;

typedef enum DigitalPotMCP4251_CommandBits_enum {
	DigitalPotMCP4251_CommandBits_WriteData = 0b00,
	DigitalPotMCP4251_CommandBits_Increment = 0b01,
	DigitalPotMCP4251_CommandBits_Decrement = 0b10,
	DigitalPotMCP4251_CommandBits_ReadData  = 0b11
} DigitalPotMCP4251_CommandBits_t;

class DigitalPotMCP4251 : private xSPIDevice {
public:
	DigitalPotMCP4251(xSPI &spi, PORT_struct &port, uint8_t pin);
	void begin();
	void write(uint8_t wiper, uint8_t val);
	uint8_t read(uint8_t wiper);

private:
	uint8_t writeCommand(
		DigitalPotMCP4251_Address_t address, 
		DigitalPotMCP4251_CommandBits_t command,
		uint16_t data);
};

#endif
