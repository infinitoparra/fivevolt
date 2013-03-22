
#include <avr/io.h>

#ifndef _dma_h_
#define _dma_h_

class xDMASource;
class xDMADest;
class xDMATrigger;

class xDMA {
public:
	static const uint8_t RepeatForever;

	xDMA(
		DMA_CH_t &dma, 
		xDMASource &source, 
		xDMADest &dest,
		xDMATrigger &trigger,
		DMA_CH_BURSTLEN_t burstMode,
		bool useRepeat);
	void begin(uint8_t repeatCount);

private:
	DMA_CH_t *_dma;
	xDMASource *_source;
	xDMADest *_dest;
	xDMATrigger *_trigger;
	DMA_CH_BURSTLEN_t _burstMode;
	bool _useRepeat;
};

class xDMASource {
public:
	virtual void* getDMAAddress() = 0;
	virtual DMA_CH_SRCRELOAD_t getDMAReload() = 0;
	virtual DMA_CH_SRCDIR_t getDMADirection() = 0;
	virtual uint16_t getDMABlockSize() = 0;
};

class xDMADest {
public:
	virtual void* getDMAAddress() = 0;
	virtual DMA_CH_DESTRELOAD_t getDMAReload() = 0;
	virtual DMA_CH_DESTDIR_t getDMADirection() = 0;
};

class xDMATrigger {
public:
	virtual DMA_CH_TRIGSRC_t getDMATriggerSource() = 0;	
};

class xDMAMemorySource : public xDMASource {
public:
	xDMAMemorySource(void* p, uint16_t len);
	virtual void* getDMAAddress() { return _p; }
	virtual DMA_CH_SRCRELOAD_t getDMAReload() { return DMA_CH_SRCRELOAD_BLOCK_gc; }
	virtual DMA_CH_SRCDIR_t getDMADirection() { return DMA_CH_SRCDIR_INC_gc; }
	virtual uint16_t getDMABlockSize() { return _len; }

private:
	void *_p;
	uint16_t _len;
};

#endif
