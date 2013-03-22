
#include <avr/io.h>
#include <stdint.h>
#include "Utils.h"
#include "DMA.h"

#ifndef _dac_h_
#define _dac_h_

class xDAC : public xDMADest {
public:
	static const uint16_t Max;

	xDAC(DAC_t &dac);
	void begin();
	virtual void* getDMAAddress() { return (void*)&_dac->CH0DATA; }
	virtual DMA_CH_DESTRELOAD_t getDMAReload() { return DMA_CH_DESTRELOAD_BURST_gc; }
	virtual DMA_CH_DESTDIR_t getDMADirection() { return DMA_CH_DESTDIR_INC_gc; }

	inline void write(uint16_t val) {
		waitForDataRegisterEmpty();
		_dac->CH0DATA = val;
	}

	inline void write(float val) {
		waitForDataRegisterEmpty();
		_dac->CH0DATA = (uint16_t)(clamp(val, 0.0f, 1.0f) * (float)xDAC::Max);
	}

private:
	DAC_t* _dac;

	inline void waitForDataRegisterEmpty() {
		while( (_dac->STATUS & DAC_CH0DRE_bm) == false );
	}
};

#endif
