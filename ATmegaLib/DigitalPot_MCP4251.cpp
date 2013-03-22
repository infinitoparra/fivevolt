
#include "DigitalPot_MCP4251.h"

DigitalPotMCP4251::DigitalPotMCP4251(xSPI &spi, PORT_struct &port, uint8_t pin) :
	xSPIDevice(spi, port, pin) {
}

void DigitalPotMCP4251::begin() {
	xSPIDevice::begin();
}

void DigitalPotMCP4251::write(uint8_t wiper, uint8_t val) {
	beginTransfer();

	DigitalPotMCP4251_Address_t address;
	if(wiper == 0) {
		address = DigitalPotMCP4251_Address_Wiper0;
	} else {
		address = DigitalPotMCP4251_Address_Wiper1;
	}

	writeCommand(address, DigitalPotMCP4251_CommandBits_WriteData, val);

	endTransfer();
}

uint8_t DigitalPotMCP4251::read(uint8_t wiper) {
	beginTransfer();

	DigitalPotMCP4251_Address_t address;
	if(wiper == 0) {
		address = DigitalPotMCP4251_Address_Wiper0;
	} else {
		address = DigitalPotMCP4251_Address_Wiper1;
	}

	uint8_t result = writeCommand(address, DigitalPotMCP4251_CommandBits_ReadData, 0);

	endTransfer();

	return result;
}

uint8_t DigitalPotMCP4251::writeCommand(
	DigitalPotMCP4251_Address_t address, 
	DigitalPotMCP4251_CommandBits_t command,
	uint16_t data) {

	xSPIDevice::write( ((uint8_t)address<<4) | ((uint8_t)command<<2) | (data>>8) );
	return xSPIDevice::write( (uint8_t)data );
}
