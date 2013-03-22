
#include "IO.h"
#include "Serial.h"
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdlib.h>

#define SERIAL_BAUD 9600

/*
 * PORTB
 *   PIN0 - Debug LED
 *   PIN1 - Hall Effect Sensor 1
 */
#define DEBUG_LED_PORT      PORTB
#define DEBUG_LED_PIN       PIN0
#define HALL_SENSOR_1_PORT  PORTB
#define HALL_SENSOR_1_PIN   PIN1

/*
 * PORTD
 *   PIN6 - Intake Valve
 *   PIN7 - Exhaust Valve
 */
#define VALVE_INTAKE_PORT   PORTD
#define VALVE_INTAKE_PIN    PIN6
#define VALVE_EXHAUST_PORT  PORTD
#define VALVE_EXHAUST_PIN   PIN7

Serial serial(UART0);

OutputPin debugLedPin(DEBUG_LED_PORT, DEBUG_LED_PIN);
InputPin hallSensor1(HALL_SENSOR_1_PORT, HALL_SENSOR_1_PIN);
OutputPin valveIntakePin(VALVE_INTAKE_PORT, VALVE_INTAKE_PIN);
OutputPin valveExhaustPin(VALVE_EXHAUST_PORT, VALVE_EXHAUST_PIN);

bool g_echoOn = true;
#define TICKS_PER_MS     (10)
#define TICKS_PER_SECOND (TICKS_PER_MS*1000)
uint32_t g_time = 0;
uint32_t g_intakeOpen   =   10;
uint32_t g_intakeClose  =  700;
bool g_intakeState;
uint32_t g_exhaustOpen  =  701;
uint32_t g_exhaustClose = 1400;
bool g_exhaustState;

#define REV_COUNT_INTERVAL (TICKS_PER_SECOND*1)
uint32_t g_revCountTime = 0;
uint16_t g_revCount = 0;
float g_rpms;

void processSerialPort();
void processCommand(const char* line);

int main() {
	uint8_t resetReason = MCUSR;
	MCUSR = 0;

	serial.begin(SERIAL_BAUD);
	serial.printLine_P(PSTR("!AirMotorEcu v1.0"));
	if(resetReason != 0) {
		serial.print_P(PSTR("-Reset Reason: "));
		serial.printLine(resetReason, 16);
	}

	debugLedPin.begin();
	valveIntakePin.begin();
	valveExhaustPin.begin();
	hallSensor1.begin();
	hallSensor1.enableInterrupt();

	// initialize timer (counter0 set to 100us)
	OCR0A = 100;
	TCCR0A = 0<<WGM00 | 1<<WGM01;
	TCCR0B = 0<<WGM02 | 0<<CS02 | 1<<CS01 | 0<<CS00;
	TIMSK0 = 1<<OCIE0A;

	sei();

	while(1) {
		processSerialPort();
		
		int i = hallSensor1.isSet() ? 1 : 0;
		debugLedPin.set(i);
		valveIntakePin.set(g_intakeState);
		valveExhaustPin.set(g_exhaustState);
	}

	return 0;
}

void processSerialPort() {
	char buffer[100];
	if(serial.readLine(buffer, 100)) {
		processCommand(buffer);
	}
}

void processCommand(const char* line) {
	if(line[0] == '\n' || line[0] == '\r') {
		// do nothing
	} else if(line[0] == '*') {
		serial.print('*');
		serial.print(((int)g_rpms));
		serial.printLine();
	} else if(strncasecmp_P(line, PSTR("setairon "), 9) == 0) {
		g_intakeOpen = atoi(line + 9);
		serial.print_P(PSTR("+OK "));
		serial.printLine(g_intakeOpen);
	} else if(strncasecmp_P(line, PSTR("setairoff "), 10) == 0) {
		g_intakeClose = atoi(line + 10);
		serial.print_P(PSTR("+OK "));
		serial.printLine(g_intakeClose);
	} else if(strncasecmp_P(line, PSTR("setexhauston "), 13) == 0) {
		g_exhaustOpen = atoi(line + 13);
		serial.print_P(PSTR("+OK "));
		serial.printLine(g_exhaustOpen);
	} else if(strncasecmp_P(line, PSTR("setexhaustoff "), 14) == 0) {
		g_exhaustClose = atoi(line + 14);
		serial.print_P(PSTR("+OK "));
		serial.printLine(g_exhaustClose);
	} else if(strncasecmp_P(line, PSTR("echooff"), 7) == 0) {
		g_echoOn = false;
		serial.printLine_P(PSTR("+OK"));
	} else if(strncasecmp_P(line, PSTR("echoon"), 6) == 0) {
		g_echoOn = true;
		serial.printLine_P(PSTR("+OK"));
	} else {
		serial.printLine_P(PSTR("-ERROR: Invalid Command"));
	}
	if(g_echoOn) {
		serial.print(">");
	}
}

// PCINT0...7
ISR(PCINT0_vect) {
	if(!hallSensor1.isSet()) {
		g_time = 0;
		g_revCount++;
		valveIntakePin.set(1);
		g_intakeState = true;
		valveExhaustPin.set(1);
		g_exhaustState = true;
	}
}

ISR(TIMER0_COMPA_vect)
{
	g_time++;
	if(g_time == g_intakeOpen) {
		valveIntakePin.set(0);
		g_intakeState = false;
	}
	else if(g_time == g_intakeClose) {
		valveIntakePin.set(1);
		g_intakeState = true;
	}
	else if(g_time == g_exhaustOpen) {
		valveExhaustPin.set(0);
		g_exhaustState = false;
	}	
	else if(g_time == g_exhaustClose) {
		valveExhaustPin.set(1);
		g_exhaustState = true;
	}

	g_revCountTime++;
	if(g_revCountTime > REV_COUNT_INTERVAL) {
		float f = ((float)g_revCount * (float)(60 / (REV_COUNT_INTERVAL / TICKS_PER_SECOND))); 
		g_rpms = (g_rpms * 0.5f) + (f * 0.5f);
		g_revCountTime = 0;
		g_revCount = 0;
	}
}

ISR(USART_RX_vect) {
	uint8_t data = serial.rxDataReady();
	if(data == (uint8_t)-1) {
		serial.flush();
	}
	else if(data == '\r') {
		if(g_echoOn) {
			serial.printLine();
		}
	} else if(data == '\n') {
		// do nothing
	} else {
		if(g_echoOn) {
			serial.print((char)data);
		}
	}
}

extern "C" void __cxa_pure_virtual() { while (1); }
