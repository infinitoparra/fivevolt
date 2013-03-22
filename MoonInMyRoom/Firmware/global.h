
#ifndef GLOBAL_H_
#define GLOBAL_H_

#include <Common.h>
#include <Serial.h>
#include <RTClib.h>
#include <Button.h>
#include <util/delay.h>
#include <avr/interrupt.h>
#include <avr/io.h>
#include "Led.h"

#ifdef DEFGLOBALS
	#define GLOBAL
	#define GLOBAL_INIT(a, b) a = b
#else
	#define GLOBAL extern
	#define GLOBAL_INIT(a, b) a
#endif

/*
 * PORTB
 *  0 - Button - Mode Select
 */
#define MODE_SELECT_PORT  PORTB
#define MODE_SELECT_PIN   0

/*
 * PORTC
 *  4 - i2c SDA
 *  5 - i2c SCL
 */

/*
 * PORTD
 *  0 - USART RX
 *  1 - USART TX
 *  2 - LED1
 *  3 - LED2
 *  4 - LED3
 *  5 - LED4
 *  6 - LED5
 *  7 - LED6
 */
#define LED1_PORT  PORTD
#define LED1_PIN   2
#define LED2_PORT  PORTD
#define LED2_PIN   3
#define LED3_PORT  PORTD
#define LED3_PIN   4
#define LED4_PORT  PORTD
#define LED4_PIN   5
#define LED5_PORT  PORTD
#define LED5_PIN   6
#define LED6_PORT  PORTD
#define LED6_PIN   7

#define MODE_MOON  0
#define MODE_CYCLE 1
#define MODE_FADE  2
#define MODE_MAX   2

#define UART_BAUD 9600
GLOBAL Serial g_serial(UART0);
GLOBAL_INIT(uint8_t g_echoOn, true);

Button g_modeButton(MODE_SELECT_PORT, MODE_SELECT_PIN);

#define LED_COUNT 6
GLOBAL Led g_led1(LED1_PORT, LED1_PIN);
GLOBAL Led g_led2(LED2_PORT, LED2_PIN);
GLOBAL Led g_led3(LED3_PORT, LED3_PIN);
GLOBAL Led g_led4(LED4_PORT, LED4_PIN);
GLOBAL Led g_led5(LED5_PORT, LED5_PIN);
GLOBAL Led g_led6(LED6_PORT, LED6_PIN);

GLOBAL Led* g_leds[] = { &g_led1, &g_led2, &g_led3, &g_led4, &g_led5, &g_led6 };

GLOBAL RTC_DS1307 g_rtc;

GLOBAL_INIT(uint32_t g_timer, 0);
GLOBAL_INIT(uint8_t g_mode, MODE_MOON);

#endif