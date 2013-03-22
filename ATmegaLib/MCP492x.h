
#ifndef _mcp492x_h_
#define _mcp492x_h_

#include "SPI.h"

#define MCP492x_MIN 0
#define MCP492x_MAX 4095

#define MCP492x_DACA       0x0000
#define MCP492x_DACB       0x8000

#define MCP492x_BUFFERED   0x4000
#define MCP492x_UNBUFFERED 0x0000

#define MCP492x_GAIN_1X    0x2000
#define MCP492x_GAIN_2X    0b00000000

#define MCP492x_SHUTDOWN   0x0000
#define MCP492x_ENABLE     0x1000

#define MCP492x_VALUE_MASK 0x0fff

class MCP4921 : protected SPIDevice {
public:
	MCP4921(SPI &spi, OutputPin &csPin);	
	void begin(uint16_t inputBufferControl, uint16_t outputGain);
	void setValue(int16_t val);
	volatile int16_t getValue();

private:
	void sendCommand();

	uint16_t _command;
	volatile int16_t _val;
};

#endif
