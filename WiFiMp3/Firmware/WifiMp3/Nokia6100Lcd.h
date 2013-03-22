
#ifndef _nokia6100lcd_h_
#define _nokia6100lcd_h_

#include "SoftwareSPI.h"

// Font sizes 
#define FONT_SMALL  0 
#define FONT_MEDIUM 1 
#define FONT_LARGE  2

#define RED         0xF00
#define GREEN       0x0F0
#define BLUE        0x00F
#define YELLOW      0xFF0
#define CYAN        0x0FF
#define MAGENTA     0xF0F
#define BLACK       0x000
#define WHITE       0xFFF

// Epson S1D15G10 Command Set 
#define DISON       0xaf
#define DISOFF      0xae
#define DISNOR      0xa6
#define DISINV      0xa7
#define COMSCN      0xbb
#define DISCTL      0xca
#define SLPIN       0x95
#define SLPOUT      0x94
#define PASET       0x75
#define CASET       0x15
#define DATCTL      0xbc
#define RGBSET8     0xce
#define RAMWR       0x5c
#define RAMRD       0x5d
#define PTLIN       0xa8
#define PTLOUT      0xa9
#define RMWIN       0xe0
#define RMWOUT      0xee
#define ASCSET      0xaa
#define SCSTART     0xab
#define OSCON       0xd1
#define OSCOFF      0xd2
#define PWRCTR      0x20
#define VOLCTR      0x81
#define VOLUP       0xd6
#define VOLDOWN     0xd7
#define TMPGRD      0x82
#define EPCTIN      0xcd
#define EPCOUT      0xcc
#define EPMWR       0xfc
#define EPMRD       0xfd
#define EPSRRD1     0x7c
#define EPSRRD2     0x7d
#define NOP         0x25

class Nokia6100Lcd {
public:
  Nokia6100Lcd(SoftwareSPI &spi, uint8 resetPin);
  void begin();
  void sendCMD(uint8 data);
  void sendData(uint8 data);
  void fill(uint16 color, uint8 x1, uint8 y1, uint8 x2, uint8 y2);
  void setBox(uint8 x1, uint8 y1, uint8 x2, uint8 y2);
  void putPixel(uint16 color, uint8 x, uint8 y);
  void drawChar(char c, uint8 x, uint8 y, uint8 size, uint16 fColor, uint16 bColor);
  void drawString(const char *str, uint8 x, uint8 y, uint8 size, uint16 fColor, uint16 bColor);
  void drawColorBar();
  void gotoXY(uint8 x, uint8 y);
  
protected:
  SoftwareSPI *m_pSpi;
  uint8 m_resetPin;
};

#endif

