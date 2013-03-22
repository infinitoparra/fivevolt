
#include "SoftwareSPI.h"

SoftwareSPI::SoftwareSPI(uint8 sckPin, uint8 csPin, uint8 misoPin, uint8 mosiPin) {
  m_sckPin = sckPin;
  m_csPin = csPin;
  m_misoPin = misoPin;
  m_mosiPin = mosiPin;
  m_sckDev = PIN_MAP[m_sckPin].gpio_device;
  m_sckBit = PIN_MAP[m_sckPin].gpio_bit;
  m_mosiDev = PIN_MAP[m_mosiPin].gpio_device;
  m_mosiBit = PIN_MAP[m_mosiPin].gpio_bit;
  m_csDev = PIN_MAP[m_csPin].gpio_device;
  m_csBit = PIN_MAP[m_csPin].gpio_bit;
}

void SoftwareSPI::begin() {
  pinMode(m_sckPin, OUTPUT);
  pinMode(m_csPin, OUTPUT);
  pinMode(m_misoPin, INPUT);
  pinMode(m_mosiPin, OUTPUT);
}

void SoftwareSPI::writeBits(uint16 val, uint8 bits) {
  for(uint8 b=bits-1; b!=255; b--) {
    gpio_write_bit(m_sckDev, m_sckBit, 0);
    gpio_write_bit(m_mosiDev, m_mosiBit, (val & (1<<b)) ? 1 : 0);
    gpio_write_bit(m_sckDev, m_sckBit, 1);
  }
}

