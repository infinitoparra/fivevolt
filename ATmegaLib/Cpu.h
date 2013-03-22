
#ifndef _cpu_h_
#define _cpu_h_

#include <avr/sleep.h>

class Cpu {
public:
	void setSleepMode(uint8_t sleepMode);
	void sleep();
};

extern Cpu cpu;

#endif
