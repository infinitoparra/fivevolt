
#ifndef LED_H_
#define LED_H_

#include <IO.h>
#include <avr/io.h>

class Led {
public:
	Led(volatile uint8_t &port, uint8_t pin);
	void begin();
	void on(float level);
	void on(uint8_t level);
	void loop();

private:
	OutputPin m_pin;
	uint8_t m_level;
	uint8_t m_time;
};

#endif