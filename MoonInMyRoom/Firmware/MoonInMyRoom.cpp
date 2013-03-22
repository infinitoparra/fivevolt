
#define DEFGLOBALS
#include "global.h"
#undef DEFGLOBALS
#include <math.h>
#include <stdlib.h>

#define MOON_PHASE_LENGTH 29.530588853f

uint8_t g_fadeAnimation[6][7] = {
	{ 0x20, 0x70, 0xFF, 0x70, 0x20, 0x00 },
	{ 0x00, 0x20, 0x70, 0xFF, 0x70, 0x20 },
	{ 0x20, 0x00, 0x20, 0x70, 0xFF, 0x70 },
	{ 0x70, 0x20, 0x00, 0x20, 0x70, 0xFF },
	{ 0xFF, 0x70, 0x20, 0x00, 0x20, 0x70 },
	{ 0x70, 0xFF, 0x70, 0x20, 0x00, 0x20 }
};

uint32_t g_ledAnimationTimer = 0;
uint8_t g_ledAnimationFrame;
uint32_t g_ledAnimationSpeed = 5000;
float g_phase;
uint32_t g_lastButtonCheck = 0;

void setup();
void loop();
void processSerial();
void processCommand(const char* line);
void updateMoon();
float computeMoonPhase(DateTime &dt);
void updateAnimation();
void modeSelect(uint8_t mode);

int main(void)
{
	setup();
	
    while(1)
    {
        loop();
    }
}

void setup() {
	uint8_t resetReason = MCUSR;
	MCUSR = 0;
	
	g_serial.begin(UART_BAUD);
	g_serial.printLine_P(PSTR("Init UART...OK"));
		
	if(resetReason != 0) {
		g_serial.print_P(PSTR("Reset Reason: "));
		g_serial.printLine(resetReason, 16);
	}
	
	g_modeButton.begin();

	g_serial.print_P(PSTR("Init LEDs..."));
	for(int i = 0; i < LED_COUNT; i++) {
		g_leds[i]->begin();
		g_leds[i]->on(1.0f);
		g_leds[i]->loop();
		g_serial.print('.');
		_delay_ms(100);
	}			
	g_serial.printLine_P(PSTR("OK"));
	
	// initialize timer
	g_serial.print_P(PSTR("Init Timer..."));
	OCR0A = 100;
	TCCR0A = 0<<WGM00 | 1<<WGM01;
	TCCR0B = 0<<WGM02 | 0<<CS02 | 1<<CS01 | 0<<CS00;
	TIMSK0 = 1<<OCIE0A;
	g_serial.printLine_P(PSTR("OK"));

	sei();

	// init RTC
	g_serial.print_P(PSTR("Init RTC..."));
	g_rtc.begin();
	g_serial.print('.');
	DateTime dt = g_rtc.now();
	g_serial.print('.');
	char dtBuffer[25];
	dt.toIso8601(dtBuffer);
	g_serial.print(dtBuffer);
	g_serial.printLine_P(PSTR(" OK"));

	modeSelect(MODE_MOON);

	g_serial.print_P(PSTR("Updating Moon..."));
	updateMoon();
	g_serial.printLine_P(PSTR("OK"));
	
	if(g_echoOn) {
		g_serial.print(">");
	}
}

void loop() {
	processSerial();
	for(int i = 0; i < LED_COUNT; i++) {
		g_leds[i]->loop();
	}		

	if(g_timer - g_lastButtonCheck > 100) {
		if(g_modeButton.loop()) {
			modeSelect(g_mode+1);
		}
		g_lastButtonCheck = g_timer;
	}

	if(g_timer - g_ledAnimationTimer > g_ledAnimationSpeed) {
		updateAnimation();
		g_ledAnimationTimer = g_timer;
	}
}

void modeSelect(uint8_t mode) {
	g_mode = mode;
	if(g_mode > MODE_MAX) {
		g_mode = 0;
	}
	g_serial.print_P(PSTR("#ModeSet="));
	g_serial.printLine(g_mode);
	g_ledAnimationFrame = 0;
	
	switch(g_mode) {
	case MODE_MOON:
		g_ledAnimationSpeed = 10000;
		break;
	case MODE_CYCLE:
		g_ledAnimationSpeed = 1000;
		break;
	case MODE_FADE:
		g_ledAnimationSpeed = 10000;
		break;
	}
}

void updateAnimation() {
	uint8_t maxFrame;
	
	if(g_mode == MODE_MOON) {
		if(g_ledAnimationFrame == 0) {
			updateMoon();
			maxFrame = 100;
		}			
	}
	else if(g_mode == MODE_CYCLE) {
		for(uint8_t i=0; i<LED_COUNT; i++) 
		{
			g_leds[i]->on(g_ledAnimationFrame == i ? 1.0f : 0.0f);
		}			
		
		maxFrame = LED_COUNT;
	}
	
	else if(g_mode == MODE_FADE) {
		for(uint8_t i=0; i<LED_COUNT; i++) 
		{
			g_leds[i]->on(g_fadeAnimation[g_ledAnimationFrame][i]);
		}		
		
		maxFrame = 6;
	}
	
	g_ledAnimationFrame++;
	if(g_ledAnimationFrame > maxFrame) {
		g_ledAnimationFrame = 0;
	}
}

// Computes moon phase based upon Bradley E. Schaefer's moon phase algorithm.
// returns a value of 0 to 1
float computeMoonPhase(DateTime &dt) {
	int year = dt.year();
	int month = dt.month();
	int day = dt.day();
    
	// Convert the year into the format expected by the algorithm.
	float transformedYear = year - floor((12 - month) / 10);
  
	// Convert the month into the format expected by the algorithm.
	int8_t transformedMonth = month + 9;
	if (transformedMonth >= 12) {
		transformedMonth = transformedMonth - 12;
	}
  
	// Logic to compute moon phase as a fraction between 0 and 1
	float term1 = floor(365.25f * (transformedYear + 4712));
	float term2 = floor(30.6f * transformedMonth + 0.5);
	float term3 = floor(floor((transformedYear / 100) + 49) * 0.75f) - 38;
   
	float intermediate = term1 + term2 + day + 59;
	if (intermediate > 2299160) {
		intermediate = intermediate - term3;
	}
   
	float normalizedPhase = (intermediate - 2451550.1) / MOON_PHASE_LENGTH;
	normalizedPhase = normalizedPhase - floor(normalizedPhase);
	if (normalizedPhase < 0) {
		normalizedPhase = normalizedPhase + 1;
	}
   
	// Return the result as a value between 0 and 1
	return normalizedPhase;
}

#define PHASE_PER_SLICE (0.5f / (float)LED_COUNT)
void updateMoon() {
	if(g_mode != MODE_MOON) {
		return;
	}
	
	DateTime dt = g_rtc.now();
	float phase = computeMoonPhase(dt);
	g_phase = phase;
	
	// full moon
	if(phase > 0.49 && phase < 0.51) {
		for(uint8_t i=0; i<LED_COUNT; i++) 
		{
			g_leds[i]->on(1.0f);
		}
	}
	
	// waxing
	else if(phase < 0.5f) {
		int8_t i, j;
		float f;
		for(f = 0.0f, i = 0; f < 1.0f; f += PHASE_PER_SLICE, i++) {
			if(phase < (f + PHASE_PER_SLICE)) {
				for(j = 0; j < i; j++) {
					g_leds[j]->on(1.0f);
				}
				g_leds[j++]->on((phase - f) * (1.0f / PHASE_PER_SLICE));
				for(; j < LED_COUNT; j++) {
					g_leds[j]->on(0.0f);
				}
				break;
			}				
		}
	}
	
	// waning
	else if(phase > 0.5f) {
		int8_t i, j;
		float f;
		for(f = 1.0f, i = 0; f > 0.0f; f -= PHASE_PER_SLICE, i++) {
			if(phase > (f - PHASE_PER_SLICE)) {
				for(j = 0; j < i; j++) {
					g_leds[5-j]->on(1.0f);
				}
				g_leds[5-j]->on((f - phase) * (1.0f / PHASE_PER_SLICE));
				j++;
				for(; j < LED_COUNT; j++) {
					g_leds[5-j]->on(0.0f);
				}
				break;
			}				
		}
	}
}

void processSerial() {
	char buffer[100];
	if(g_serial.readLine(buffer, 100)) {
		processCommand(buffer);
	}
}

void processCommand(const char* line) {
	char dtBuffer[25];

	if(line[0] == '\n' || line[0] == '\r') {
		// do nothing
	} else if(strncasecmp_P(line, PSTR("set "), 4) == 0) {
		DateTime dt;
		if(DateTime::tryParseIso8601(line + 4, &dt)) {
			dt.toIso8601(dtBuffer);
			g_serial.print_P(PSTR("#set "));
			g_serial.printLine(dtBuffer);
			
			g_rtc.adjust(dt);
			dt = g_rtc.now();
			dt.toIso8601(dtBuffer);
			updateMoon();
			g_serial.print_P(PSTR("+OK "));
			g_serial.printLine(dtBuffer);
		} else {
			g_serial.printLine_P(PSTR("-Invalid Format"));
		}		
	} else if(strncasecmp_P(line, PSTR("now"), 3) == 0) {
		DateTime dt = g_rtc.now();
		dt.toIso8601(dtBuffer);
		g_serial.print_P(PSTR("+OK "));
		g_serial.print(dtBuffer);
		g_serial.print(' ');
		g_serial.printLine(g_phase);
	} else if(strncasecmp_P(line, PSTR("mode "), 5) == 0) {
		uint8_t mode = atoi(line + 5);
		modeSelect(mode);
		g_serial.print_P(PSTR("+OK "));
		g_serial.printLine(g_mode);
	} else {
		g_serial.printLine_P(PSTR("-ERROR: Invalid Command"));
	}
	
	if(g_echoOn) {
		g_serial.print(">");
	}
}			

ISR(TIMER0_COMPA_vect)
{
	g_timer++;
}	

ISR(USART_RX_vect) {
	uint8_t data = g_serial.rxDataReady();
	if(data == (uint8_t)-1) {
		g_serial.flush();
	}
	else if(data == '\r') {
		if(g_echoOn) {
			g_serial.printLine();
		}
	} else if(data == '\n') {
		// do nothing
	} else {
		if(g_echoOn) {
			g_serial.print((char)data);
		}
	}
}

extern "C" void __cxa_pure_virtual() { while (1); }
