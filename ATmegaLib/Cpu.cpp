
#include "Cpu.h"

Cpu cpu;

void Cpu::setSleepMode(uint8_t sleepMode) {
	set_sleep_mode(sleepMode);
}

void Cpu::sleep() {
	sleep_mode();
}

