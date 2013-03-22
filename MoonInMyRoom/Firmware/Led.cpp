
#include "Led.h"

Led::Led(volatile uint8_t &port, uint8_t pin) : m_pin(port, pin) {
	
}

void Led::begin() { 
	m_pin.begin();
}

void Led::on(uint8_t level) {
	m_level = level;
}

void Led::on(float level) {
	float t = level * 255.0f;
	if(t > 255.0f) on((uint8_t)0xff);
	else if(t < 0.0f) on((uint8_t)0x00);
	else on((uint8_t)t);
}

void Led::loop() {
	m_time += 10;
	if(m_time < m_level) {
		m_pin.set(false);
	} else {
		m_pin.set(true);
	}
}
