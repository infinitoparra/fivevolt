
#include "Demux.h"

Demux3to8::Demux3to8(OutputPin &addr0, OutputPin &addr1, OutputPin &addr2) {
	_addr0 = &addr0;
	_addr1 = &addr1;
	_addr2 = &addr2;
}

void Demux3to8::begin() {
	_addr0->begin();
	_addr1->begin();
	_addr2->begin();
}

void Demux3to8::write(uint8_t val) {
	_addr0->set(val & 0x01);
	_addr1->set(val & 0x02);
	_addr2->set(val & 0x04);
}
