
#ifndef _demux_h_
#define _demux_h_

#include "IO.h"

class Demux3to8 {
public:
	Demux3to8(OutputPin &addr0, OutputPin &addr1, OutputPin &addr2);
	void begin();
	void write(uint8_t val);

private:
	OutputPin *_addr0;
	OutputPin *_addr1;
	OutputPin *_addr2;
};

#endif
