
#include <avr/io.h>
#include <util/delay.h>
#include <avr/interrupt.h>
#include <string.h>
#include <stdlib.h>
#include <avr/pgmspace.h>
#include "Utils.h"
#include "SPI.h"
#include "MCP492x.h"
#include "IO.h"
#include "LCD_HD44780.h"
#include "Serial.h"
#include "ADC.h"
#include "QuadratureEncoder.h"

#define LOAD_MIN    0
#define LOAD_MAX    2500
#define SERIAL_BAUD 9600

// CPU: ATmega644PA

/*
 * PORTA
 *   PIN0 - Load Current Sense
 *   PIN1 - Load Volatage Sense
 *   PIN2 - External Current Sense
 *   PIN3 - External Voltage Sense
 */
#define LOAD_CURRENT_SENSE_PORT PORTA
#define LOAD_CURRENT_SENSE_PIN  PIN0
#define LOAD_VOLTAGE_SENSE_PORT PORTA
#define LOAD_VOLTAGE_SENSE_PIN  PIN1
#define EXT_CURRENT_SENSE_PORT  PORTA
#define EXT_CURRENT_SENSE_PIN   PIN2
#define EXT_VOLTAGE_SENSE_PORT  PORTA
#define EXT_VOLTAGE_SENSE_PIN   PIN3
 
/*
 * PORTB
 *   PIN0 - Current Control Quadrature Encoder A
 *   PIN1 - Current Control Quadrature Encoder B
 *   PIN2 - !DAC Chip Select
 *   PIN3 - MOSI
 *   PIN4 - MISO
 *   PIN5 - SCK
 */
#define SPI_DAC_CS_PORT      PORTB
#define SPI_DAC_CS_PIN       PIN4
#define CURRENT_CONTROL_PORT PORTB
#define CURRENT_CONTROL_A    PIN1
#define CURRENT_CONTROL_B    PIN0

/*
 * PORTC
 *   PIN0-7 - LCD Data
 */
#define LCD_DATA_PORT    PORTC

/*
 * PORTD
 *   PIN0 - Serial RX
 *   PIN1 - Serial TX
 *   PIN4 - CPU Active LED
 *   PIN5 - LCD RS Pin
 *   PIN6 - LCD RW Pin
 *   PIN7 - LCD Enable
 */
#define CPU_ACTIVE_LED_PORT PORTD
#define CPU_ACTIVE_LED_PIN  PIN4
#define LCD_RS_PORT         PORTD
#define LCD_RS_PIN          PIN5
#define LCD_RW_PORT         PORTD
#define LCD_RW_PIN          PIN6
#define LCD_ENABLE_PORT     PORTD
#define LCD_ENABLE_PIN      PIN7

Serial serial(UART0);

SPI spi;

#define DAC_MIN  MCP492x_MIN
#define DAC_MAX  MCP492x_MAX
OutputPin dacCsPin(SPI_DAC_CS_PORT, SPI_DAC_CS_PIN);
MCP4921 dac(spi, dacCsPin);

ADCPin loadCurrentSensePin(LOAD_CURRENT_SENSE_PORT, LOAD_CURRENT_SENSE_PIN);
ADCPin loadVoltageSensePin(LOAD_VOLTAGE_SENSE_PORT, LOAD_VOLTAGE_SENSE_PIN);
ADCPin extCurrentSensePin(EXT_CURRENT_SENSE_PORT, EXT_CURRENT_SENSE_PIN);
ADCPin extVoltageSensePin(EXT_VOLTAGE_SENSE_PORT, EXT_VOLTAGE_SENSE_PIN);

#define CURRENT_CONTROL_MIN -100
#define CURRENT_CONTROL_MAX 100
InputPin currentControlA(CURRENT_CONTROL_PORT, CURRENT_CONTROL_A);
InputPin currentControlB(CURRENT_CONTROL_PORT, CURRENT_CONTROL_B);
QuadratureEncoder currentControl(currentControlA, currentControlB);

OutputPort lcdDataPort(LCD_DATA_PORT);
OutputPin lcdEnablePin(LCD_ENABLE_PORT, LCD_ENABLE_PIN);
OutputPin lcdRsPin(LCD_RS_PORT, LCD_RS_PIN);
OutputPin lcdRwPin(LCD_RW_PORT, LCD_RW_PIN);
LCD lcd(lcdDataPort, lcdEnablePin, lcdRsPin, &lcdRwPin);

OutputPin cpuActiveLedPin(CPU_ACTIVE_LED_PORT, CPU_ACTIVE_LED_PIN);

/*****************************************************************/
volatile int16_t g_loadCurrentTarget = 0; // * 1000
volatile int16_t g_loadCurrentSense = 0; // * 1000
float g_loadCurrentAdj = 1.0f;
int16_t g_loadCurrentAdjZero = 0; // * 1000

int16_t g_loadVoltage = 0; // * 1000
float g_loadVoltageAdj = 4.022f;
int16_t g_loadVoltageAdjZero = 24; // * 1000

volatile int16_t g_extCurrent = 0; // * 1000
float g_extCurrentAdj = 0.51026f;
int16_t g_extCurrentAdjZero = -3.5; // * 1000

int16_t g_extVoltage = 0; // * 1000
float g_extVoltageAdj = 4.014f;
int16_t g_extVoltageAdjZero = 27; // * 1000

bool g_echoOn = true;
bool g_rawEnabled = false;

/*****************************************************************/
void init();
void processSerialPort();
void processCommand(const char* line);
void setLoad(int16_t load); // * 1000
const char* formatCurrent(int16_t current, char* buffer);
const char* formatVoltage(int16_t voltage, char* buffer);
const char* formatWatts(int16_t watts, char* buffer);
void updateLcd();
void adjustLoad();
void updateInputs();
int16_t getExtCurrent();
int16_t getExtVoltage();
int16_t getLoadCurrent();
int16_t getLoadVoltage();
void updateLoadFromCurrentControl();
int16_t estimateLoadCurrentFromDacValue(int16_t dacValue); // * 1000
int16_t calculateWatts(int16_t current, int16_t volts); // * 1000
void printRaw();

/*****************************************************************/
int main() {
	init();
	
	int loopCount = 0;
	while(1) {
		adjustLoad();

		if((loopCount % 100) == 0) {
			updateLoadFromCurrentControl();
			updateInputs();
			processSerialPort();
			updateLcd();
		}

		if(g_rawEnabled && ((loopCount % 1000) == 0)) {
			printRaw();
		}

		_delay_ms(1);
		loopCount++;
	}


	cpuActiveLedPin.clear();
	
	return 0;
}

void updateInputs() {
	g_extCurrent = getExtCurrent() * 0.1f + g_extCurrent * 0.9f;
	g_extVoltage = getExtVoltage() * 0.1f + g_extVoltage * 0.9f;
	g_loadCurrentSense = getLoadCurrent() * 0.1f + g_loadCurrentSense * 0.9f;
	g_loadVoltage = getLoadVoltage() * 0.1f + g_loadVoltage * 0.9f;
}

void init() {
	uint8_t resetReason = MCUSR;
	MCUSR = 0;

	cpuActiveLedPin.begin();
	cpuActiveLedPin.clear();

	lcd.begin();
	lcd.setXY(0, 0);
	lcd.print_P(PSTR("Dummy Load v1.0"));

	_delay_ms(500);
	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Init Serial Port    "));
	serial.begin(SERIAL_BAUD);
	serial.printLine_P(PSTR("!DummyLoad v1.0"));

	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Init DAC            "));
	spi.begin();
	dac.begin(MCP492x_UNBUFFERED, MCP492x_GAIN_1X);
	dac.setValue(0);

	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Init Current Ctrl   "));
	currentControl.begin();
	currentControl.setMin(CURRENT_CONTROL_MIN);
	currentControl.setMax(CURRENT_CONTROL_MAX);

	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Init Input Pins     "));
	int16_t vref = 5000;

	loadCurrentSensePin.begin();
	loadCurrentSensePin.setVRef(vref);
	
	loadVoltageSensePin.begin();
	loadVoltageSensePin.setVRef(vref);
	
	extCurrentSensePin.begin();
	extCurrentSensePin.setVRef(vref);
	
	extVoltageSensePin.begin();
	extVoltageSensePin.setVRef(vref);

	if(resetReason != 0) {
		serial.print_P(PSTR("-Reset Reason: "));
		serial.printLine(resetReason, 16);
	}
	if(g_echoOn) {
		serial.print(">");
	}

	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Enable Interrupts   "));
	sei();

	cpuActiveLedPin.set();
	lcd.setXY(0, 1);
	lcd.print_P(PSTR("Init Complete       "));
	_delay_ms(500);
}

void updateLoadFromCurrentControl() {
	int val = currentControl.getValue();
	if(val > 1 || val < -1) {
		int sign = val > 0 ? 1 : -1;
		currentControl.setValue(0);
		val = val / 2;
		setLoad(g_loadCurrentTarget + (sign * abs(val*val)));
	}
}

void adjustLoad() {
	int16_t estimatedLoadCurrent;
	int16_t dacValue;
	int16_t loadCurrentSense;

	loadCurrentSense = getLoadCurrent();
	dacValue = dac.getValue();

	if(g_loadCurrentTarget > loadCurrentSense) {
		estimatedLoadCurrent = estimateLoadCurrentFromDacValue(dacValue + 1);
		if((estimatedLoadCurrent - g_loadCurrentTarget) < 50) {
			dac.setValue(dacValue + 1);
		} else {
			dac.setValue(dacValue - 1);
		}
	} else if(g_loadCurrentTarget < loadCurrentSense) {
		estimatedLoadCurrent = estimateLoadCurrentFromDacValue(dacValue - 1);
		if((g_loadCurrentTarget - estimatedLoadCurrent) < 50) {
			dac.setValue(dacValue - 1);
		} else {
			dac.setValue(dacValue + 1);
		}
	}
}

int16_t estimateLoadCurrentFromDacValue(int16_t dacValue) {
	return (int16_t)((int32_t)dacValue * (int32_t)1500 / DAC_MAX);
}

void updateLcd() {
	char lcdBuffer[40];
	char numberFormatBuffer[21];
	int16_t dacValue;
	float extWatts;
	float loadWatts;

	dacValue = dac.getValue();
	extWatts = calculateWatts(g_extCurrent, g_extVoltage);
	loadWatts = calculateWatts(g_loadCurrentSense, g_loadVoltage);

	lcd.setXY(0, 0);
	strcpy_P(lcdBuffer, PSTR("External"));
	strcatfill(lcdBuffer, 20, ' ');
	lcd.print(lcdBuffer);

	lcd.setXY(0, 1);
	strcpy(lcdBuffer, formatVoltage(g_extVoltage, numberFormatBuffer));
	strcatfill(lcdBuffer, 7, ' ');
	strcat(lcdBuffer, formatCurrent(g_extCurrent, numberFormatBuffer));
	strcatfill(lcdBuffer, 13, ' ');
	strcat(lcdBuffer, formatWatts(extWatts, numberFormatBuffer));
	strcatfill(lcdBuffer, 20, ' ');
	lcd.print(lcdBuffer);

	lcd.setXY(0, 2);
	strcpy_P(lcdBuffer, PSTR("Load"));
	strcatfill(lcdBuffer, 7, ' ');
	strcat_P(lcdBuffer, PSTR("-> "));
	strcat(lcdBuffer, formatCurrent(g_loadCurrentTarget, numberFormatBuffer));
	strcatfill(lcdBuffer, 20, ' ');
	lcd.print(lcdBuffer);

	lcd.setXY(0, 3);
	strcpy(lcdBuffer, formatVoltage(g_loadVoltage, numberFormatBuffer));
	strcatfill(lcdBuffer, 7, ' ');
	strcat(lcdBuffer, formatCurrent(g_loadCurrentSense, numberFormatBuffer));
	strcatfill(lcdBuffer, 13, ' ');
	strcat(lcdBuffer, formatWatts(loadWatts, numberFormatBuffer));
	strcatfill(lcdBuffer, 20, ' ');
	lcd.print(lcdBuffer);
}

int16_t calculateWatts(int16_t current, int16_t volts) {
	float c = (float)current / 1000.0f;
	float v = (float)volts / 1000.0f;
	return c * v * 1000.0f;
}

int16_t getLoadCurrent() {
	int16_t result = ((float)loadCurrentSensePin.readV() * g_loadCurrentAdj) + g_loadCurrentAdjZero;
	if(result < 0) {
		result = 0;
	}
	return result;
}

int16_t getLoadVoltage() {
	int16_t result = ((float)loadVoltageSensePin.readV() * g_loadVoltageAdj) + g_loadVoltageAdjZero;
	if(result < 0) {
		result = 0;
	}
	return result;
}

int16_t getExtCurrent() {
	int16_t result = ((float)extCurrentSensePin.readV() * g_extCurrentAdj) + g_extCurrentAdjZero;
	if(result < 0) {
		result = 0;
	}
	return result;
}

int16_t getExtVoltage() {
	int16_t result = ((float)extVoltageSensePin.readV() * g_extVoltageAdj) + g_extVoltageAdjZero;
	if(result < 0) {
		result = 0;
	}
	return result;
}

const char* formatCurrent(int16_t current, char* buffer) {
	if(current >= 1000) {
		ftoa((float)current / 1000.0f, buffer, 2);
		strcat(buffer, "A");
	}
	else {
		itoa(current, buffer, 10);
		strcat(buffer, "mA");
	}
	return buffer;
}

const char* formatVoltage(int16_t voltage, char* buffer) {
	if(voltage >= 1000) {
		ftoa((float)voltage / 1000.0f, buffer, 2);
		strcat(buffer, "V");
	}
	else {
		itoa(voltage, buffer, 10);
		strcat(buffer, "mV");
	}
	return buffer;
}

const char* formatWatts(int16_t watts, char* buffer) {
	if(watts >= 1000) {
		ftoa((float)watts / 1000.0f, buffer, 2);
		strcat(buffer, "W");
	}
	else {
		itoa(watts, buffer, 10);
		strcat(buffer, "mW");
	}
	return buffer;
}

void processSerialPort() {
	char rxLineBuffer[RX_BUFFER_SIZE];

	if(serial.readLine(rxLineBuffer, RX_BUFFER_SIZE)) {
		processCommand(rxLineBuffer);
	}
}

void printRaw() {
	int16_t dacValue = dac.getValue();
	serial.print('+');
	serial.print(loadCurrentSensePin.readV());
	serial.print(',');
	serial.print(loadVoltageSensePin.readV());
	serial.print(',');
	serial.print(extCurrentSensePin.readV());
	serial.print(',');
	serial.print(extVoltageSensePin.readV());
	serial.print(',');
	serial.print(dacValue);
	serial.print(',');
	serial.print(estimateLoadCurrentFromDacValue(dacValue));
	serial.printLine();
}

void processCommand(const char* line) {
	char numberFormatBuffer[21];

	if(line[0] == '\n' || line[0] == '\r') {
		// do nothing
	} else if(line[0] == '*') {
		serial.print('*');
		serial.print(dac.getValue());
		serial.print(',');
		serial.print(g_loadCurrentSense);
		serial.print(',');
		serial.print(g_loadCurrentTarget);
		serial.print(',');
		serial.print(g_extCurrent);
		serial.print(',');
		serial.print(g_loadVoltage);
		serial.print(',');
		serial.print(g_extVoltage);
		serial.printLine();
	} else if(strncasecmp_P(line, PSTR("raw"), 3) == 0) {
		g_rawEnabled = !g_rawEnabled;
		if(g_rawEnabled) {
			serial.printLine_P(PSTR("+raw on"));
		} else {
			serial.printLine_P(PSTR("+raw off"));
		}
	} else if(strncasecmp_P(line, PSTR("setload "), 8) == 0) {
		int16_t newValue = (int16_t)(atof(line + 8) * 1000.0f);
		setLoad(newValue);
		serial.print_P(PSTR("+OK "));
		serial.printLine(newValue);
	} else if(strncasecmp_P(line, PSTR("getdac"), 6) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(dac.getValue());
	} else if(strncasecmp_P(line, PSTR("getloadsense"), 12) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatCurrent(g_loadCurrentSense, numberFormatBuffer));
	} else if(strncasecmp_P(line, PSTR("getloadtarget"), 13) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatCurrent(g_loadCurrentTarget, numberFormatBuffer));
	} else if(strncasecmp_P(line, PSTR("getloadest"), 10) == 0) {
		int16_t est = estimateLoadCurrentFromDacValue(dac.getValue());
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatCurrent(est, numberFormatBuffer));
	} else if(strncasecmp_P(line, PSTR("getloadv"), 8) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatVoltage(g_loadVoltage, numberFormatBuffer));
	} else if(strncasecmp_P(line, PSTR("getextv"), 7) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatVoltage(g_extVoltage, numberFormatBuffer));
	} else if(strncasecmp_P(line, PSTR("getextcurrent"), 13) == 0) {
		serial.print_P(PSTR("+OK "));
		serial.printLine(formatCurrent(g_extCurrent, numberFormatBuffer));
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

void setLoad(int16_t load) {
	if(load < LOAD_MIN) {
		load = LOAD_MIN;
	}
	if(load > LOAD_MAX) {
		load = LOAD_MAX;
	}
	g_loadCurrentTarget = load;
}

// PCINT8...14
ISR(PCINT1_vect) {
	currentControl.update();
}

ISR(USART0_RX_vect) {
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

