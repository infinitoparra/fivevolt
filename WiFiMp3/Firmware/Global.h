
#ifndef _global_h_
#define _global_h_

#include <gpio.h>
#include <delay.h>
#include <boards.h>
#include "IoPin.h"
#include "Vs1053Mp3Ic.h"
#include "Nokia6100ColorLcd.h"
#include "XBeeWifi.h"
#include "SdCard.h"
#include "SerialPort.h"
#include "StreamReaderWriter.h"
#include "HardwareSpi.h"

#ifdef DEFGLOBAL
#define GLOBAL
#define GLOBALINIT(t,name,params) t name params
#else
#define GLOBAL extern
#define GLOBALINIT(t,name,params) extern t name
#endif

/*
 * GPIOA
 *   5 - Debug Led
 */
#define DEBUG_LED_PORT      GPIOA
#define DEBUG_LED_PIN       5
#define WIFI_USART          USART2
#define WIFI_USART_TX_PORT  GPIOA
#define WIFI_USART_TX_PIN   2
#define WIFI_USART_RX_PORT  GPIOA
#define WIFI_USART_RX_PIN   3

/*
 * GPIOB
 *  10 - Debug Serial Port TX (USART3)
 *  11 - Debug Serial Port RX (USART3)
 */
#define DEBUG_USART         USART3
#define DEBUG_USART_TX_PORT GPIOB
#define DEBUG_USART_TX_PIN  10
#define DEBUG_USART_RX_PORT GPIOB
#define DEBUG_USART_RX_PIN  11

GLOBALINIT(Vs1053Mp3Ic, g_mp3, ());
GLOBALINIT(SdCard, g_sdcard, ());

// lcd
GLOBALINIT(Spi, g_lcdSpi, (SPI1));
GLOBALINIT(Nokia6100ColorLcd, g_lcd, (&g_lcdSpi));

// wifi
GLOBALINIT(IoPin, g_serialWifiTxPin, (WIFI_USART_TX_PORT, WIFI_USART_TX_PIN));
GLOBALINIT(IoPin, g_serialWifiRxPin, (WIFI_USART_RX_PORT, WIFI_USART_RX_PIN));
GLOBALINIT(SerialPort, g_serialWifi, (WIFI_USART, &g_serialWifiTxPin, &g_serialWifiRxPin));
GLOBALINIT(XBeeWifi, g_wifi, (&g_serialWifi, STM32_PCLK1));

// debug USART
GLOBALINIT(IoPin, g_serialDebugTxPin, (DEBUG_USART_TX_PORT, DEBUG_USART_TX_PIN));
GLOBALINIT(IoPin, g_serialDebugRxPin, (DEBUG_USART_RX_PORT, DEBUG_USART_RX_PIN));
GLOBALINIT(SerialPort, g_serialDebug, (DEBUG_USART, &g_serialDebugTxPin, &g_serialDebugRxPin));
GLOBALINIT(StreamReaderWriter, g_debug, (&g_serialDebug));

#endif
