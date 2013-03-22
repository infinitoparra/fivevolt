
#ifndef _SoftwareSPI_h_
#define _SoftwareSPI_h_

#include <libmaple.h>
#include <io.h>

class SoftwareSPI {
public:
  SoftwareSPI(uint8 sckPin, uint8 csPin, uint8 misoPin, uint8 mosiPin);
  void begin();
  void writeCS(uint8 val) { gpio_write_bit(m_csDev, m_csBit, val); }
  void writeMOSI(uint8 val) { gpio_write_bit(m_mosiDev, m_mosiBit, val); }
  void writeSCK(uint8 val) { gpio_write_bit(m_sckDev, m_sckBit, val); }
  void writeBits(uint16 val, uint8 bits);
  
protected:
  uint8 m_sckPin;
  uint8 m_csPin;
  uint8 m_misoPin;
  uint8 m_mosiPin;
  gpio_dev *m_sckDev;
  uint8 m_sckBit;
  gpio_dev *m_csDev;
  uint8 m_csBit;
  gpio_dev *m_mosiDev;
  uint8 m_mosiBit;
};

#endif
