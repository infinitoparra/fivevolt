
#include "Timer.h"
#include <stdint.h>

const uint16_t xTimer::MaxPeriod = 0xffff;

xTimer::xTimer(TC0_t &timer, TC_CLKSEL_t clkSel, uint16_t per) {
	_timer = &timer;
	_clkSel = clkSel;
	_per = per;
}

void xTimer::begin() {
	// Enable overflow interrupt
	_timer->INTCTRLA = ( _timer->INTCTRLA & TC0_OVFINTLVL_gm ) | TC_OVFINTLVL_MED_gc; 

	_timer->CTRLA = _clkSel;
	_timer->PER = _per;
}

DMA_CH_TRIGSRC_t xTimer::getDMATriggerSource() {
	if(_timer == &TCC0) {
		return DMA_CH_TRIGSRC_TCC0_OVF_gc;
	}
	if(_timer == &TCD0) {
		return DMA_CH_TRIGSRC_TCD0_OVF_gc;
	}
	if(_timer == &TCE0) {
		return DMA_CH_TRIGSRC_TCE0_OVF_gc;
	}
	if(_timer == &TCF0) {
		return DMA_CH_TRIGSRC_TCF0_OVF_gc;
	}
	return DMA_CH_TRIGSRC_OFF_gc;
}
