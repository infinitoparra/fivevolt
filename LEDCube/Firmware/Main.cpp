
#include <avr/io.h>
#include <util/delay.h>
#include "IO.h"
#include "TLC5925.h"
#include "Demux.h"

/*
 * PORTB
 *   PIN0 - Test Pin
 */
#define TESTPIN_PORT  PORTB
#define TESTPIN_PIN   PIN0

/*
 * PORTC
 *   PIN0 - Column Led Driver (data)
 *   PIN1 - Column Led Driver (clk)
 *   PIN2 - Column Led Driver (latch)
 *   PIN3 - Row Demux (addr 0)
 *   PIN4 - Row Demux (addr 1)
 *   PIN5 - Row Demux (addr 2)
 */
#define LEDDRIVER_DATA_PORT   PORTC
#define LEDDRIVER_DATA_PIN    PIN0
#define LEDDRIVER_CLK_PORT    PORTC
#define LEDDRIVER_CLK_PIN     PIN1
#define LEDDRIVER_LATCH_PORT  PORTC
#define LEDDRIVER_LATCH_PIN   PIN2
#define DEMUX0_PORT           PORTC
#define DEMUX0_PIN            PIN3
#define DEMUX1_PORT           PORTC
#define DEMUX1_PIN            PIN4
#define DEMUX2_PORT           PORTC
#define DEMUX2_PIN            PIN5

InputPin testPin(TESTPIN_PORT, TESTPIN_PIN);

OutputPin dataPin(LEDDRIVER_DATA_PORT, LEDDRIVER_DATA_PIN);
OutputPin clkPin(LEDDRIVER_CLK_PORT, LEDDRIVER_CLK_PIN);
OutputPin latchPin(LEDDRIVER_LATCH_PORT, LEDDRIVER_LATCH_PIN);
TLC5925 columns(dataPin, clkPin, latchPin);

OutputPin demux0(DEMUX0_PORT, DEMUX0_PIN);
OutputPin demux1(DEMUX1_PORT, DEMUX1_PIN);
OutputPin demux2(DEMUX2_PORT, DEMUX2_PIN);
Demux3to8 row(demux0, demux1, demux2);

int main() {
	uint16_t i;

	testPin.begin();
	columns.begin();
	row.begin();

	
	for(i=0; ; i++) {
		if(testPin.isSet()) {
			row.write(0);
		} else {
			row.write(1);
		}
		columns.write(0xffff);
	}

	return 0;
}
