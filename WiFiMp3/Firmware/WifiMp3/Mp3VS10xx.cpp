
#include <io.h>
#include <wirish.h>
#include "Mp3VS10xx.h"
#include "global.h"

#define DEBUG_MP3

Mp3VS10xx::Mp3VS10xx(HardwareSPI& spi, uint8 xcs, uint8 xdcs, uint8 dreq, uint8 reset) {
  m_pspi = &spi;
  m_xcs = xcs;
  m_xdcs = xdcs;
  m_dreq = dreq;
  m_reset = reset;
}

void Mp3VS10xx::begin() {
  pinMode(m_dreq, INPUT);
  pinMode(m_xcs, OUTPUT);
  pinMode(m_xdcs, OUTPUT);
  pinMode(m_reset, OUTPUT);
  resetOff();
  xcsOff();
  xdcsOff();
  
  // todo: m_pspi->begin(SPI_281_250KHZ, MSBFIRST, SPI_MODE_0);
  m_pspi->begin(SPI_1_125MHZ, MSBFIRST, SPI_MODE_0);
  setVolume(20, 20); //Set initial volume (20 = -10dB)
  
  uint16 mode = readRegister(SCI_MODE);
  mode |= SM_LAYER12;
  writeRegister(SCI_MODE, mode);
  
  SerialDebug.println("check status");
  debug();
  
  //Now that we have the VS1053 up and running, increase the VS1053 internal clock multiplier and up our SPI rate
  writeRegister(SCI_CLOCKF, XTAL_3_0); //Set multiplier to 3.0x

  //From page 12 of datasheet, max SCI reads are CLKI/7. Input clock is 12.288MHz. 
  //Internal clock multiplier is now 3x.
  //Therefore, max SPI speed is 5MHz. 2.25MHz will be safe.
  // todo: m_pspi->begin(SPI_2_25MHZ, MSBFIRST, SPI_MODE_0); // todo: increase to 4.5MHz?
  delay(100);

  SerialDebug.println("mp3 init complete");
  debug();
}

void Mp3VS10xx::debug() {
  #ifdef DEBUG_MP3
    //Let's check the status of the VS1053
    uint16 mode = readRegister(SCI_MODE);
    SerialDebug.print("SCI_MODE: 0x");
    SerialDebug.println(mode, 16);
    
    uint16 status = readRegister(SCI_STATUS);
    SerialDebug.print("SCI_STATUS: 0x");
    SerialDebug.println(status, 16);
  
    int vsVersion = (status >> 4) & 0x000F; //Mask out only the four version bits
    SerialDebug.print("VS Version (VS1053 is 4) = ");
    SerialDebug.println(vsVersion, 10); //The 1053B should respond with 4. VS1001 = 0, VS1011 = 1, VS1002 = 2, VS1003 = 3
  
    uint16 clock = readRegister(SCI_CLOCKF);
    SerialDebug.print("SCI_CLOCKF: 0x");
    SerialDebug.println(clock, 16);
    
    uint16 hdat0 = readRegister(SCI_HDAT0);
    SerialDebug.print("SCI_HDAT0: 0x");
    SerialDebug.println(hdat0, 16);

    uint16 hdat1 = readRegister(SCI_HDAT1);
    SerialDebug.print("SCI_HDAT1: 0x");
    SerialDebug.println(hdat1, 16);

    SerialDebug.println("------");
  #endif
}

void Mp3VS10xx::send(RingBuffer* buffer, uint16 len) {
  xdcsOn();
  for(uint16 i=0; i<len; i++) {
    m_pspi->transfer(buffer->remove());
  }
  xdcsOff();
}

void Mp3VS10xx::send(uint8 *buffer, uint16 len) {
  xdcsOn();
  for(uint16 i=0; i<len; i++) {
    m_pspi->transfer(buffer[i]);
  }
  xdcsOff();
}

void Mp3VS10xx::send(uint8 d) {
  xdcsOn();
  m_pspi->transfer(d);
  xdcsOff();
}

void Mp3VS10xx::endOfSong() {
  xdcsOn();
  for(uint16 i = 0; i < Mp3VS10xx_MinZerosBetweenSongs; i++) {
    waitForDreq();
    m_pspi->transfer(0);
  }
  while(isBufferFull());
  xdcsOff();
}

//Write to VS10xx register
//SCI: Data transfers are always 16bit. When a new SCI operation comes in 
//DREQ goes low. We then have to wait for DREQ to go high again.
//XCS should be low for the full duration of operation.
void Mp3VS10xx::writeRegister(uint8 addressbyte, uint8 highbyte, uint8 lowbyte) {
  waitForDreq();
  xcsOn();

  //SCI consists of instruction byte, address byte, and 16-bit data word.
  m_pspi->transfer(SCI_OP_WRITE); //Write instruction
  m_pspi->transfer(addressbyte);
  m_pspi->transfer(highbyte);
  m_pspi->transfer(lowbyte);
  waitForDreq();
  xcsOff();
}

//Read the 16-bit value of a VS10xx register
uint16 Mp3VS10xx::readRegister(uint8 addressbyte) {
  xdcsOff();
  waitForDreq();
  xcsOn();

  //SCI consists of instruction byte, address byte, and 16-bit data word.
  m_pspi->transfer(SCI_OP_READ); //Read instruction
  delay_us(50);
  m_pspi->transfer(addressbyte);
  delay_us(50);
  
  uint8 response1 = m_pspi->transfer(0xFF); //Read the first byte
  waitForDreq();
  delay_us(50);
  uint8 response2 = m_pspi->transfer(0xFF); //Read the second byte
  waitForDreq();

  xcsOff();

  uint16 resultvalue = ((uint16)response1) << 8;
  resultvalue |= response2;
  return resultvalue;
}

void Mp3VS10xx::setVolume(uint8 leftchannel, uint8 rightchannel) {
  writeRegister(SCI_VOL, leftchannel, rightchannel);
}

