
#include <stdlib.h>
#include <math.h>
#include <string.h>
#include <avr/interrupt.h>
#include "IO.h"
#include "Serial.h"
#include "LCD_HD44780.h"
#include "QuadratureEncoder.h"
#include "DAC.h"
#include "DMA.h"
#include "Timer.h"
#include "SPI.h"
#include "DigitalPot_MCP4251.h"
#include "Utils.h"

#define SERIAL_PORT      USARTF1

#define SPI_PORT         SPIC

#define DACA_TIMER       TCC0
#define DACA_POT_CS_PORT PORTC
#define DACA_POT_CS_PIN  PIN3_bm

#define DAC_DATA_LEN     1024
#define DAC_GAIN_WIPER   0
#define DAC_OFFSET_WIPER 1
#define DAC_POT_MIN      0
#define DAC_POT_MAX      255

/* peripherals */
Serial serial(SERIAL_PORT);
xSPI spi(SPI_PORT);

/* DAC A */
xTimer dacATimer(DACA_TIMER, TC_CLKSEL_DIV1024_gc, 1000);
uint16_t dacA_Data[DAC_DATA_LEN];
xDAC dacA(DACA);
xDMAMemorySource dacA_DMASource(dacA_Data, DAC_DATA_LEN * sizeof(uint16_t));
xDMA dacA_DMA(DMA.CH0, dacA_DMASource, dacA, dacATimer, DMA_CH_BURSTLEN_2BYTE_gc, true);
DigitalPotMCP4251 dacA_pot(spi, DACA_POT_CS_PORT, DACA_POT_CS_PIN);

void setDACAGain(float gain);
void setDACAOffset(float offset);

int main() {
	setClockTo32MHz();

	serial.begin(115200);
	serial.printLine("BEGIN");
	spi.begin();

	dacA_pot.begin();
	setDACAGain(0.0);
	setDACAOffset(0.0);
	dacA.begin();
	dacA_DMA.begin(xDMA::RepeatForever);

	PMIC.CTRL |= PMIC_LOLVLEN_bm | PMIC_MEDLVLEN_bm | PMIC_HILVLEN_bm; // interrupt controller enable

	sei();

	// BEGIN populate dac buffer
	for(int16_t i=0; i<DAC_DATA_LEN; i++) {
		float x = (2.0f * M_PI) * ((float)i / (float)DAC_DATA_LEN);
		float y = sin(x);
		//dacA_Data[i] = (uint16_t)map(y, -1.0f, 1.0f, 0.0f, (float)xDAC::Max);
		dacA_Data[i] = i * 4; // (uint16_t)map((float)i, 0.0f, (float)DAC_DATA_LEN, 0.0f, (float)xDAC::Max);
	}

	dacATimer.begin();
	// END populate dac buffer

	serial.print(">");

	while(1) {
		_delay_ms(100);
		if(serial.available() > 0) {
			char cmd[50];

			if(serial.readLine(cmd, 50)) {
				strtrim(cmd);

				// daca gain ##0.###
				if(strncmp(cmd, "daca gain ", 10) == 0) {
					float gain = atof(cmd + 10);
					setDACAGain(gain);
					serial.printLine(dacA_pot.read(DAC_GAIN_WIPER));
				} 
				
				// daca offset ##0.###
				else if(strncmp(cmd, "daca offset ", 12) == 0) {
					float offset = atof(cmd + 12);
					setDACAOffset(offset);
					serial.printLine(dacA_pot.read(DAC_OFFSET_WIPER));
				} 
				
				// unknown command
				else {
					serial.printLine("Unknown Command");
					serial.printLine(cmd);
				}
				serial.print(">");
			}
		}
	}

	serial.printLine("END");
}

void setDACAGain(float gain) {
	if(gain < -12.0f || gain > 12.0f) {
		serial.printLine("-DACA gain must be between -12.0 and 12.0");
		return;
	}

	uint8_t v = clamp( map(gain, -12.0f, 12.0f, (float)DAC_POT_MIN, (float)DAC_POT_MAX), DAC_POT_MIN, DAC_POT_MAX);
	dacA_pot.write(DAC_GAIN_WIPER, v);

	serial.print("+DACA Gain: ");
	serial.printLine(gain);
}

void setDACAOffset(float offset) {
	if(offset < -12.0f || offset > 12.0f) {
		serial.printLine("-DACA offset must be between -12.0 and 12.0");
		return;
	}

	uint8_t v = clamp( map(offset, -12.0f, 12.0f, (float)DAC_POT_MIN, (float)DAC_POT_MAX), DAC_POT_MIN, DAC_POT_MAX);
	dacA_pot.write(DAC_OFFSET_WIPER, v);

	serial.print("+DACA Offset: ");
	serial.printLine(offset);
}

/*
ISR(PORTD_INT0_vect)
{
}
*/

ISR(USARTF1_RXC_vect)
{
	uint8_t data = serial.rxDataReady();
	if(data != (uint8_t)-1) {
		if(data == '\r') {
			serial.printLine();
		} else if(data == '\n') {
		} else {
			serial.print((char)data);
		}
	}
}


extern "C" void __cxa_pure_virtual() { while (1); }
