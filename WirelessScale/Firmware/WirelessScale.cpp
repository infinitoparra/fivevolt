
#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdlib.h>
#include <string.h>
#include "Cpu.h"
#include "SevenSegment.h"
#include "Serial.h"
#include "IO.h"
#include "XBee.h"
#include "Utils.h"
#include "XBeeFiveVoltProtocol.h"

/*
 * Peripherals
 */

/*
 * PORTB - Selects the segments for the LED.
 *   PIN0-6 - 7-segment segments
 *   PIN7   - decimal point
 */
#define DIGIT_SEGMENT_PORT   PORTB

/*
 * PORTC
 *   PIN0 (PCINT8)  - Digit 0 (least significant)
 *   PIN1 (PCINT9)  - Digit 1
 *   PIN2 (PCINT10) - Digit 2
 *   PIN3 (PCINT11) - Digit 3 (Most significant)
 */
#define DIGIT_ENABLE_PORT    PORTC
#define DIGIT_ENABLE_DIGIT0  PINC0
#define DIGIT_ENABLE_DIGIT1  PINC1
#define DIGIT_ENABLE_DIGIT2  PINC2
#define DIGIT_ENABLE_DIGIT3  PINC3

/*
 * PORTD
 *   PIN0 - Serial RXD
 *   PIN1 - Serial TXD
 *   PIN2 - Serial !CTS
 *   PIN3 - Serial RTS
 *   PIN4 - XBee Sleep RQ
 *   PIN5 - XBee Reset
 */
#define SERIAL_CTS_PORT      PORTD
#define SERIAL_CTS_PIN       PIND2
#define SERIAL_RTS_PORT      PORTD
#define SERIAL_RTS_PIN       PIND3
#define XBEE_SLEEP_RQ_PORT   PORTD
#define XBEE_SLEEP_RQ_PIN    PIND4
#define XBEE_RESET_PORT      PORTD
#define XBEE_RESET_PIN       PIND5
#define ACTIVE_LED_PORT      PORTD
#define ACTIVE_LED_PIN       PIND7

#define SERIAL_RTS_ON   0
#define SERIAL_RTS_OFF  1

#define XBEE_RESET_OFF 1
#define XBEE_RESET_ON  0

Serial serial(UART0);
InputPin serialCTSPin(SERIAL_CTS_PORT, SERIAL_CTS_PIN);
OutputPin serialRTSPin(SERIAL_RTS_PORT, SERIAL_RTS_PIN);
OutputPin xbeeSleepRQPin(XBEE_SLEEP_RQ_PORT, XBEE_SLEEP_RQ_PIN);
OutputPin xbeeResetPin(XBEE_RESET_PORT, XBEE_RESET_PIN);
OutputPin activeLed(ACTIVE_LED_PORT, ACTIVE_LED_PIN);
XBeeFV xbee(serial);
InputPin digitEnable0(DIGIT_ENABLE_PORT, DIGIT_ENABLE_DIGIT0);
InputPin digitEnable1(DIGIT_ENABLE_PORT, DIGIT_ENABLE_DIGIT1);
InputPin digitEnable2(DIGIT_ENABLE_PORT, DIGIT_ENABLE_DIGIT2);
InputPin digitEnable3(DIGIT_ENABLE_PORT, DIGIT_ENABLE_DIGIT3);
InputPort digitSegmentPort(DIGIT_SEGMENT_PORT);
char tempLcd[] = "???.?";
char lastTempLcd[] = "???.?";
char currentLcd[] = "???.?";
volatile bool batteryLow = false;

// values in 10s of pounds so that we don't need floats
int tempLcdValue = 0;
volatile int currentLcdValue = 0;

void sleep();
void sendWeight(int weight);

struct WeightPacket {
	uint8_t user;
	int weight;
};

int main() {
	activeLed.begin();
	activeLed.set(true);

	serialCTSPin.begin();
	serialRTSPin.begin();
	xbeeSleepRQPin.begin();
	xbeeResetPin.begin();

	DDRC = 0xff;
	PORTC = 0x00;

	serial.begin(9600);
	serial.setCTSPin(serialCTSPin);
	xbee.begin();
	xbee.setSleepPin(xbeeSleepRQPin);
	digitEnable0.begin();
	digitEnable0.enableInterrupt();
	digitEnable1.begin();
	digitEnable1.enableInterrupt();
	digitEnable2.begin();
	digitEnable2.enableInterrupt();
	digitEnable3.begin();
	digitEnable3.enableInterrupt();
	digitSegmentPort.begin();

	sei();

	serialRTSPin.set(SERIAL_RTS_ON);
	xbeeResetPin.set(XBEE_RESET_ON);
	_delay_ms(500);
	xbeeResetPin.set(XBEE_RESET_OFF);
	xbee.wakeUp();

	xbee.sendCapabilities(XBEEFV_CAPABILITY_SCALE);
	_delay_ms(5000);

	sleep();

	int lastValue = 0;
	unsigned long time = 0;
	unsigned long timeout = 30000;
	while(1) {
		if(currentLcdValue != lastValue) {
			cli();
			lastValue = currentLcdValue;
			sei();
			if(lastValue > 100) {
				sendWeight(lastValue);
			}
			_delay_ms(5000);
			lastValue = currentLcdValue = 0;
			sleep();
			time = 0;
		}
		if(batteryLow) {
			xbee.sendLowBattery();
			_delay_ms(5000);
			batteryLow = false;
			sleep();
			time = 0;
		}

		_delay_ms(10);
		time += 10;
		if(time > timeout) {
			xbee.sendMessage(XBEEFV_MESSAGELEVEL_INFO, "Timeout");
			_delay_ms(5000);
			sleep();
			time = 0;
		}
	}

	return 0;
}

void sendWeight(int weight) {
	WeightPacket packet;
	packet.user = XBEEFV_USER_UNKNOWN;
	packet.weight = weight;
	xbee.sendCapabilityData(XBEE_ADDR_COORDINATOR, XBEE_NETADDR_UNKNOWN, XBEEFV_CAPABILITY_SCALE, (uint8_t*)&packet, sizeof(WeightPacket));
}

void sleep() {
	activeLed.set(false);
	xbee.sleep();
	cpu.setSleepMode(SLEEP_MODE_PWR_SAVE);
	cpu.sleep();

	activeLed.set(true);
	xbee.wakeUp();
}

SIGNAL(USART_RX_vect) {
	serial.rxDataReady();
	xbee.rxDataReady();
}

// PCINT8...14
SIGNAL(PCINT1_vect) {
	// read data
	_delay_us(2000); // each digit is pulsed for 3.5ms so wait for middle of pulse to read data
	uint8_t data = digitSegmentPort.read();
	uint8_t digit;
	if(digitEnable0.isSet()) {
		digit = 0;
	}
	else if(digitEnable1.isSet()) {
		digit = 1;
	}
	else if(digitEnable2.isSet()) {
		digit = 2;
	}
	else if(digitEnable3.isSet()) {
		digit = 3;
	} else {
		digit = 0xff;
	}

	// process data
	char dataCh = SevenSegment::segmentsToChar(data & 0x7f);
	if(dataCh == '?') {
		/* DEBUG
		char str[20];
		strcpy(str, "Unknown char 0x");
		itoa(data, str+strlen(str), 16);
		strcat(str, " at digit ");
		itoa(digit, str+strlen(str), 10);
		xbee.sendMessage(XBEEFV_MESSAGELEVEL_ERROR, str);
		*/
	}
	else
	{
		if(digit == 0) {
			tempLcd[0] = dataCh;
		}
		else if(digit == 1) {
			tempLcd[1] = dataCh;
		}
		else if(digit == 2) {
			tempLcd[2] = dataCh;
		}
		else if(digit == 3) {
			tempLcd[3] = dataCh;
			tempLcd[4] = '\0';

			// check that we get two consistant readings
			if(!strcmp(lastTempLcd, tempLcd)) {
				strcpy(currentLcd, tempLcd);
				if(!strcasecmp(currentLcd, "batt")) {
					batteryLow = true;
				}
				else {
					batteryLow = false;
					if(currentLcd[0] == 'o'
						|| currentLcd[1] == 'o'
						|| currentLcd[2] == 'o'
						|| currentLcd[3] == 'o') {
						currentLcdValue = tempLcdValue;
					} else {
						// DEBUG: xbee.sendMessage(XBEEFV_MESSAGELEVEL_DEBUG, currentLcd);
						tempLcdValue = atoi(currentLcd);
					}
				}
			} else {
				strcpy(lastTempLcd, tempLcd);
			}
		}
	}
}

extern "C" void __cxa_pure_virtual() { while (1); }
