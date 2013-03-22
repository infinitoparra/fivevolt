
#include <avr/io.h>
#include "DMA.h"

#ifndef _timer_h_
#define _timer_h_

class xTimer : public xDMATrigger {
public:
	static const uint16_t MaxPeriod;

	xTimer(TC0_t &timer, TC_CLKSEL_t clkSel, uint16_t per);
	void begin();
	virtual DMA_CH_TRIGSRC_t getDMATriggerSource();

private:
	TC0_t *_timer;
	TC_CLKSEL_t _clkSel;
	uint16_t _per;
};

#endif
