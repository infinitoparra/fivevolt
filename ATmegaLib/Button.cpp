
#include "Button.h"

Button::Button(volatile uint8_t &port, uint8_t pin) : m_pin(port, pin) {
	
}

void Button::begin() {
	m_pin.begin();
	m_isPressed = false;
	m_checkCount = 0;
}

uint8_t Button::loop() {
	uint8_t result = false;
	if(!m_pin.isSet()) { // button is active low
		m_checkCount++;
		if(m_checkCount > 10) {
			m_checkCount = 10;
			if(!m_isPressed) {
				result = true;
			}
			m_isPressed = true;
		}
	} else {
		m_checkCount = 0;
		m_isPressed = false;
	}
	return result;
}