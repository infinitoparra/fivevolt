
#ifndef _Nokia6100ColorLcd_h_
#define _Nokia6100ColorLcd_h_

#include "HardwareSpi.h"

class Nokia6100ColorLcd {
public:
	Nokia6100ColorLcd(Spi *pSpi);
	void enable();

protected:
	Spi *m_pSpi;
};

#endif
