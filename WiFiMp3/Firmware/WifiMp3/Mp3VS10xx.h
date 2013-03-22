
#ifndef _Mp3VS10xx_h_
#define _Mp3VS10xx_h_

#include <HardwareSPI.h>
#include "RingBuffer.h"

#define SCI_OP_READ   0x03
#define SCI_OP_WRITE  0x02

//VS10xx SCI Registers
#define SCI_MODE 0x00
#define SCI_STATUS 0x01
#define SCI_BASS 0x02
#define SCI_CLOCKF 0x03
#define SCI_DECODE_TIME 0x04
#define SCI_AUDATA 0x05
#define SCI_WRAM 0x06
#define SCI_WRAMADDR 0x07
#define SCI_HDAT0 0x08
#define SCI_HDAT1 0x09
#define SCI_AIADDR 0x0A
#define SCI_VOL 0x0B
#define SCI_AICTRL0 0x0C
#define SCI_AICTRL1 0x0D
#define SCI_AICTRL2 0x0E
#define SCI_AICTRL3 0x0F

#define SM_DIFF          0x0001 /* Differential (0 normal in-phase audio, 1 left channel inverted) */
#define SM_LAYER12       0x0002 /* Allow MPEG layers I & II (0 no, 1 yes) */
#define SM_RESET         0x0004 /* Soft reset (0 no reset, 1 reset) */
#define SM_OUTOFWAV      0x0008 /* Jump out of WAV decoding (0 no, 1 yes) */
#define SM_EARSPEAKER_LO 0x0010 /* EarSpeaker low setting (0 off, 1 active) */
#define SM_TESTS         0x0020 /* Allow SDI tests (0 not allowed, 1 allowed) */
#define SM_STREAM        0x0040 /* Stream mode (0 no, 1 yes) */
#define SM_EARSPEAKER_HI 0x0080 /* EarSpeaker high setting (0 off, 1 active) */
#define SM_DACT          0x0100 /* DCLK active edge (0 rising, 1 falling) */
#define SM_SDIORD        0x0200 /* SDI bit order (0 MSb ﬁrst, 1 MSb last) */
#define SM_SDISHARE      0x0400 /* Share SPI chip select (0 no, 1 yes) */
#define SM_SDINEW        0x0800 /* VS1002 native SPI modes (0 no, 1 yes) */
#define SM_ADPCM         0x1000 /* ADPCM recording active (0 no, 1 yes) */
#define SM_ADPCM_HP      0x2000 /* ADPCM high-pass ﬁlter active (0 no, 1 yes) */
#define SM_LINE_IN       0x4000 /* ADPCM recording selector (0 microphone, 1 line in) */
#define SM_CLK_RANGE     0x8000 /* Input clock range (0 12..13 MHz, 1 24..26 MHz) */

#define XTAL_0_0 0x0000
#define XTAL_1_5 0x2000
#define XTAL_2_0 0x4000
#define XTAL_2_5 0x6000
#define XTAL_3_0 0x8000
#define XTAL_3_5 0xa000
#define XTAL_4_0 0xc000
#define XTAL_4_5 0xe000

// Size of audio FIFO is 2048 stereo (2x16 bit) samples. If DREQ is high,
// VS1053B can take, at a minimum, 32 bytes of SDI data. DREQ is low when stream
// buffer is full.
#define Mp3VS10xx_FifoSize    2048
#define Mp3VS10xx_MinDreqSize 32

#define Mp3VS10xx_MinZerosBetweenSongs 2052

class Mp3VS10xx {
public:
  Mp3VS10xx(HardwareSPI& spi, uint8 xcs, uint8 xdcs, uint8 dreq, uint8 reset);
  void begin();

  void setVolume(uint8 leftchannel, uint8 rightchannel);
  uint8 isBufferFull() { return digitalRead(m_dreq) ? 0 : 1; }
  void send(RingBuffer* buffer, uint16 len);
  void send(uint8 *buffer, uint16 len);
  void send(uint8 d);
  void endOfSong();
  void debug();
  void writeRegister(uint8 addressbyte, uint8 highbyte, uint8 lowbyte);
  void writeRegister(uint8 addressbyte, uint16 value) { writeRegister(addressbyte, (value >> 8) & 0xff, value & 0xff); }
  uint16 readRegister(uint8 addressbyte);

protected:
  void xdcsOff() { /*delay_us(100);*/ digitalWrite(m_xdcs, 1); }
  void xdcsOn() { digitalWrite(m_xdcs, 0); /*delay_us(100);*/ }
  void xcsOff() { delay_us(100); digitalWrite(m_xcs, 1); }
  void xcsOn() { digitalWrite(m_xcs, 0); delay_us(100);  }
  void resetOff() { delay_us(100); digitalWrite(m_xcs, 1); }
  void resetOn() { digitalWrite(m_xcs, 0); delay_us(100);  }
  void waitForDreq() { while(isBufferFull()); }
  
  HardwareSPI *m_pspi;
  uint8 m_xcs;
  uint8 m_xdcs;
  uint8 m_dreq;
  uint8 m_reset;
};

#endif


