
#include "DMA.h"

const uint8_t xDMA::RepeatForever = 0;

xDMA::xDMA(
	DMA_CH_t &dma, 
	xDMASource &source, 
	xDMADest &dest, 
	xDMATrigger &trigger,
	DMA_CH_BURSTLEN_t burstMode,
	bool useRepeat) {
	_dma = &dma;
	_source = &source;
	_dest = &dest;
	_trigger = &trigger;
	_burstMode = burstMode;
	_useRepeat = useRepeat;
}

void xDMA::begin(uint8_t repeatCount) {
	DMA.CTRL |= DMA_ENABLE_bm;

	void *srcAddr = _source->getDMAAddress();
	_dma->SRCADDR0 = (( (uint32_t) srcAddr) >> 0*8 ) & 0xFF;
	_dma->SRCADDR1 = (( (uint32_t) srcAddr) >> 1*8 ) & 0xFF;
	_dma->SRCADDR2 = (( (uint32_t) srcAddr) >> 2*8 ) & 0xFF;

	void *destAddr = _dest->getDMAAddress();
	_dma->DESTADDR0 = (( (uint32_t) destAddr) >> 0*8 ) & 0xFF;
	_dma->DESTADDR1 = (( (uint32_t) destAddr) >> 1*8 ) & 0xFF;
	_dma->DESTADDR2 = (( (uint32_t) destAddr) >> 2*8 ) & 0xFF;

	_dma->ADDRCTRL = (uint8_t) _source->getDMAReload()
		| _source->getDMADirection()
		| _dest->getDMAReload() 
		| _dest->getDMADirection();
	_dma->TRFCNT = _source->getDMABlockSize();
	_dma->CTRLA = _burstMode | ( _useRepeat ? DMA_CH_REPEAT_bm : 0);

	if(_useRepeat) {
		_dma->REPCNT = repeatCount;
	}

	_dma->TRIGSRC = _trigger->getDMATriggerSource();
	_dma->CTRLA |= DMA_CH_SINGLE_bm;
	_dma->CTRLA |= DMA_CH_ENABLE_bm;
}

xDMAMemorySource::xDMAMemorySource(void* p, uint16_t len) {
	_p = p;
	_len = len;
}
