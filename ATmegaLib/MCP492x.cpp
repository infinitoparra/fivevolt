
#include "MCP492x.h"
#include "SPI.h"
#include "Utils.h"

MCP4921::MCP4921(SPI &spi, OutputPin &csPin) 
	: SPIDevice(spi, csPin) {

}

void MCP4921::begin(uint16_t inputBufferControl, uint16_t outputGain) {
	SPIDevice::begin();
	_command = inputBufferControl | outputGain | MCP492x_ENABLE;
	sendCommand();
}

void MCP4921::setValue(int16_t val) {
	int16_t newVal = clamp(val, MCP492x_MIN, MCP492x_MAX);
	_command &= ~MCP492x_VALUE_MASK;
	_command |= newVal & MCP492x_VALUE_MASK;
	sendCommand();
	_val = newVal;
}

volatile int16_t MCP4921::getValue() {
	return _val;
}

void MCP4921::sendCommand() {
	_spi->setBitOrder(SPI_BITORDER_MSBFIRST);
	_spi->setDataMode(SPI_MODE0);
	_spi->setClockDivider(SPI_CLOCK_DIV128);
	beginTransfer();
	write((_command >> 8) & 0xff);
	write((_command >> 0) & 0xff);
	endTransfer();
}
