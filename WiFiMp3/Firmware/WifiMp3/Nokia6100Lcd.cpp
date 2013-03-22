
#include "WProgram.h"
#include "Nokia6100Lcd.h"
#include "Nokia6100LcdFonts.h"

Nokia6100Lcd::Nokia6100Lcd(SoftwareSPI &spi, uint8 resetPin) {
  m_pSpi = &spi;
  m_resetPin = resetPin;
}

void Nokia6100Lcd::begin() {
  m_pSpi->begin();
  pinMode(m_resetPin, OUTPUT);

  // Hardware Reset LCD
  digitalWrite(m_resetPin, 0);
  delay(10);
  digitalWrite(m_resetPin, 1);
  delay(100);
  
  // Initial state
  m_pSpi->writeCS(1);
  m_pSpi->writeMOSI(1);
  m_pSpi->writeSCK(1);
  
  // Display control
  sendCMD(DISCTL);
  sendData(0x00); // P1: 0x00 = 2 divisions, switching period=8 (default) 
  sendData(0x20); // P2: 0x20 = nlines/4 - 1 = 132/4 - 1 = 32)
  sendData(0x00); // P3: 0x00 = no inversely highlighted lines 
  
  sendCMD(COMSCN);
  sendData(0x01);  // P1: 0x01 = Scan 1->80, 160<-81
  
  sendCMD(OSCON);
  delay(10);
  
  sendCMD(SLPOUT);
  delay(10);
  
  sendCMD(VOLCTR);      // electronic volume, this is kinda contrast/brightness
  sendData(0x24);       // this might be different for individual LCDs
  sendData(0x03);    
  
  sendCMD(PWRCTR);      // power ctrl
  sendData(0x0f);       // everything on, no external reference resistors
  delay(100);

  //sendCMD(DISINV);      // display mode
  //delay(10);
  
  sendCMD(DATCTL);
  sendData(0x01); // P1: 0x01 = page address inverse, col address normal, address scan in col direction
  sendData(0x00); // P2: 0x00 = RGB sequence (default value)
  sendData(0x02); // P3: 0x02 = Grayscale -> 16 (selects 12-bit color, type A, RRRRGGGG, BBBBRRRR, GGGGBBBB)
  delay(10);
  
  sendCMD(NOP);
  
  fill(BLACK, 0, 0, 131, 131);
  sendCMD(DISON);     // display on
}

void Nokia6100Lcd::gotoXY(uint8 x, uint8 y) {
  sendCMD(PASET);   // column start/end ram
  sendData(y);
  sendData(131);

  sendCMD(CASET);   // page start/end ram
  sendData(x);
  sendData(131);
}

//*****************************************************************************
// Description:    Fills an area starting @ xs,ys to width @ heighth with color  
//                    param 1: Color of Pixel, RRRGGGBB format
//                    param 2: x start direction: 0-131
//                    param 3: y start direction: 0-131
//                    param 4: width: 0-131
//                    param 5: heighth: 0-131
//
// Example:        fill(BLACK, 0, 0, 132, 132);  // fill entire screen with black color
//					
// Notes:          No bounds checking	
//*******************************************************************************
void Nokia6100Lcd::fill(uint16 color, uint8 x, uint8 y, uint8 width, uint8 height) {
  uint16 i;
  uint16 len = ((uint16)(height+2) * (uint16)width) / 2;
  
  setBox(x, y, x + width, y + height);
  uint8 a = color >> 4; // RRRRGGGG
  uint8 b = ((color & 0xf) << 4) | ((color >> 8) & 0xf); // BBBBRRRR
  uint8 c = color; // GGGGBBBB
  sendCMD(RAMWR);
  for(i = 0; i < len; i++) {
    sendData(a);
    sendData(b);
    sendData(c);
  }
}

void Nokia6100Lcd::putPixel(uint16 color, uint8 x, uint8 y) {
  setBox(x, y, x, y);
  
  sendCMD(RAMWR);
  // second pixel ignored because of bounding box
  sendData(color >> 4); // RRRRGGGG
  sendData(((color & 0xf) << 4) | ((color >> 8) & 0xf)); // BBBBRRRR
  sendData(color); // GGGGBBBB
}

void Nokia6100Lcd::drawChar(char c, uint8 x, uint8 y, uint8 size, uint16 fColor, uint16 bColor) {
  int8 i, j; 
  uint8 pixelRow; 
  uint8 mask; 
  uint16 word0; 
  uint16 word1; 
   
  uint8 *pFont = (uint8*)FONT_TABLE[size];
  uint8 nCols = FONT_COLS[size]; 
  uint8 nRows = FONT_ROWS[size]; 
  uint8 nBytes = FONT_BYTES[size]; 

  // get pointer to the last byte of the desired character
  uint8 *pChar = pFont + (nBytes * (c - 0x20)) + nBytes - 1; 
  
  setBox(x, y, x + nCols - 1, y + nRows - 1);
  
  // WRITE MEMORY 
  sendCMD(RAMWR); 
  
  // loop on each row, working backwards from the bottom to the top 
  for (i = nRows - 1; i >= 0; i--) { 
    // copy pixel row from font table and then decrement row
    pixelRow = *pChar--; 
    
    // loop on each pixel in the row (left to right) 
    // Note: we do two pixels each loop 
    mask = 0x80; 
    for (j = 0; j < nCols; j += 2) { 
      // if pixel bit set, use foreground color; else use the background color 
      // now get the pixel color for two successive pixels 
      if ((pixelRow & mask) == 0) 
        word0 = bColor; 
      else 
        word0 = fColor;
      mask = mask >> 1;
      
      if ((pixelRow & mask) == 0) 
        word1 = bColor; 
      else 
        word1 = fColor; 
      mask = mask >> 1; 
      
      // use this information to output three data bytes 
      sendData((word0 >> 4) & 0xFF); 
      sendData(((word0 & 0xF) << 4) | ((word1 >> 8) & 0xF)); 
      sendData(word1 & 0xFF); 
    }
  }
  // terminate the Write Memory command 
  sendCMD(NOP);
}

void Nokia6100Lcd::drawString(const char *str, uint8 x, uint8 y, uint8 size, uint16 fColor, uint16 bColor) {
  while(*str) {
    char c = *str;
    drawChar(c, x, y, size, fColor, bColor);
    x += FONT_COLS[size];
    if (x > 131) break; 
    str++;
  }
}

void Nokia6100Lcd::setBox(uint8 x1, uint8 y1, uint8 x2, uint8 y2) {
  sendCMD(CASET);  // page start/end ram
  sendData(x1);
  sendData(x2);
  
  sendCMD(PASET);  // column start/end ram
  sendData(y1);
  sendData(y2);
}

void Nokia6100Lcd::drawColorBar() {
  fill(RED, 0, 0, 131, 33);
  fill(GREEN, 0, 34, 131, 33);
  fill(BLUE, 0, 67, 131, 33);
  fill(WHITE, 0, 100, 131, 33);
}

void Nokia6100Lcd::sendCMD(uint8 data) {
  m_pSpi->writeSCK(1);
  m_pSpi->writeCS(1);
  m_pSpi->writeCS(0);

  m_pSpi->writeBits(0x0000 | ((uint16)data), 9);
}

void Nokia6100Lcd::sendData(uint8 data) {
  m_pSpi->writeSCK(1);
  m_pSpi->writeCS(1);
  m_pSpi->writeCS(0);

  m_pSpi->writeBits(0x0100 | ((uint16)data), 9);
}


