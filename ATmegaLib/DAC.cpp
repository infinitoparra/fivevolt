
#include "DAC.h"

const uint16_t xDAC::Max = 4095;

/**
 * Setup the DAC.
 * @param dac Can be DACA or DACB.
 */
xDAC::xDAC(DAC_t &dac) {
	_dac = &dac;
}

void xDAC::begin() {
    _dac->CTRLC = DAC_REFSEL_AVCC_gc;  // Use AVCC as Voltage Reference
    _dac->CTRLB = DAC_CHSEL_SINGLE_gc; // Use Single Conversion Mode
	_dac->CTRLA = DAC_CH0EN_bm | DAC_ENABLE_bm; // Enable Channel 0 and Enable the (entire) module
}



