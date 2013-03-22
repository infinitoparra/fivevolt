
#ifndef _quad_eeprom_h_
#define _quad_eeprom_h_

class QuadEEPROM {
public:
  void begin();
  void updateValues();

private:
  static float readFloat(int address);
  static void writeFloat(int address, float value);
  static void updateFloat(int address, float value);
};

#endif


