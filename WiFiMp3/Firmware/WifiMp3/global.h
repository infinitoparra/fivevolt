
#ifndef _global_h_
#define _global_h_

#include <libmaple.h>

/*
 * D2  - PA0  - WIFI USART CTS
 * D3  - PA1  - WIFI USART RTS
 * D1  - PA2  - WIFI USART TX
 * D0  - PA3  - WIFI USART RX
 * D10 - PA4  - SD SPI CS
 * D13 - PA5  - SD SPI SCK
 * D12 - PA6  - SD SPI MISO
 * D11 - PA7  - SD SPI MOSI
 * D6  - PA8  - 
 * D7  - PA9  - Debug USART TX
 * D8  - PA10 - Debug USART TX
 *     - PA11 - 
 *     - PA12 - 
 * D39 - PA13 - 
 * D40 - PA14 - 
 * D41 - PA15 -
 *
 * D27 - PB0  -
 * D28 - PB1  -
 *     - PB2  -
 * D42 - PB3  - LCD SPI SCK
 * D43 - PB4  -
 * D4  - PB5  - LCD SPI MOSI
 * D5  - PB6  - LCD SPI MISO
 * D9  - PB7  - LCD Reset
 * D14 - PB8  - 
 * D24 - PB9  -
 * D29 - PB10 - IPOD TX
 * D30 - PB11 - IPOD RX
 * D31 - PB12 - 
 * D32 - PB13 - MP3 SPI SCK
 * D33 - PB14 - MP3 SPI MISO
 * D34 - PB15 - MP3 SPI MOSI
 *
 * D15 - PC0  - 
 * D16 - PC1  -
 * D17 - PC2  - 
 * D18 - PC3  - 
 * D19 - PC4  -
 * D20 - PC5  -
 * D35 - PC6  - MP3 DREQ
 * D36 - PC7  - MP3 XDCS
 * D37 - PC8  - MP3 XCS
 * D38 - PC9  - MP3 xRESET
 * D26 - PC10 -
 *     - PC11 -
 *     - PC12 -
 * D21 - PC13 -
 * D22 - PC14 -
 * D23 - PC15 -
 *
 * D25 - PD2  - LCD SPI CS
 */

#define isPrintable(ch) ( ((ch)>=0x20 && (ch)<=0x7e) ? true : false )

//#define WIFI_DEBUG
//#define SD_DEBUG
#define IPOD_DEBUG

// TODO change to SPI 3
#define LCD_SPI_SCK_PIN    42 /* D42 - PB3 */
#define LCD_SPI_CS_PIN     25 /* D25 - PD2 */
#define LCD_SPI_MISO_PIN   5 /* D5 - PB6 */
#define LCD_SPI_MOSI_PIN   4 /* D4 - PB5 */
#define LCD_RESET          9 /* D9 - PB7 */

// TODO move to the SDIO pins?
#define SD_SPI             1 /* SCK (D13 - PA5), MISO (D12 - PA6), MOSI (D11 - PA7), CS (D10 - PA4) */
#define SD_SPI_CS          10

#define MP3_SPI            2 /* SCK (D32 - PB13), MISO (D33 - PB14), MOSI (D34 - PB15) */
#define MP3_XCS            37 /* PC8 - Control Chip Select Pin (for accessing SPI Control/Status registers) */
#define MP3_XDCS           36 /* PC7 - Data Chip Select / BSYNC Pin */
#define MP3_DREQ           35 /* PC6 - Data Request Pin: Player asks for more data */
#define MP3_RESET          31 /* PC9 - xRESET */ /* TODO: change to 38 for prod */

#define SerialWifi         Serial2
#define WIFI_BAUD          9600
#define WIFI_USART_TX      1 /* D1 - PA2 */
#define WIFI_USART_RX      0 /* D0 - PA3 */
#define WIFI_USART_CTS     2 /* D2 - PA0 */
#define WIFI_USART_RTS     3 /* D3 - PA1 */

#define SerialDebug        Serial1
#define DEBUG_BAUD         19200
#define DEBUG_USART_TX     7 /* D7 - PA9 */
#define DEBUG_USART_RX     8 /* D8 - PA10 */

#define SerialIpod         Serial3
#define IPOD_USART_RX      30
#define IPOD_USART_TX      29

#pragma pack(push, 1)

typedef struct _WifiSettings {
  char ssid[25];
  uint8 encryption;
  char securityKey[30];
  uint8 ipAddrMode;
  uint32 ipAddr;
  uint32 subnetMask;
  uint32 gateway;
} WifiSettings;

typedef struct _WifiMp3Settings {
  WifiSettings wifi;
} WifiMp3Settings;

#pragma pack(pop)

#endif
