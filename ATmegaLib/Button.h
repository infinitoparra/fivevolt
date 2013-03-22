
#ifndef BUTTON_H_
#define BUTTON_H_

#include <IO.h>

class Button {
public:
	Button(volatile uint8_t &port, uint8_t pin);
	void begin();
	uint8_t loop();
	
private:
	InputPin m_pin;	
	uint8_t m_checkCount;
	uint8_t m_isPressed;
};

#endif