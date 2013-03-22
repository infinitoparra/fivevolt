
#include <avr/eeprom.h>
#include <EEPROM.h>
#include "Eeprom.h"
#include "Pid.h"

extern Pid g_rollPid, g_pitchPid, g_yawPid;
extern float g_userControlGain;
extern float g_pidWindupGuard;

void QuadEEPROM::begin() {
  int i = 0;
  g_rollPid.P = readFloat(i); 
  i += 4;
  g_rollPid.I = readFloat(i); 
  i += 4;
  g_rollPid.D = readFloat(i); 
  i += 4;
  g_pitchPid.P = readFloat(i); 
  i += 4;
  g_pitchPid.I = readFloat(i); 
  i += 4;
  g_pitchPid.D = readFloat(i); 
  i += 4;
  g_yawPid.P = readFloat(i); 
  i += 4;
  g_yawPid.I = readFloat(i); 
  i += 4;
  g_yawPid.D = readFloat(i); 
  i += 4;
  g_userControlGain = readFloat(i);
  i += 4;
  g_pidWindupGuard = readFloat(i);
  i += 4;
}

void QuadEEPROM::updateValues() {
  int i = 0;
  updateFloat(i, g_rollPid.P);
  i += 4;
  updateFloat(i, g_rollPid.I);
  i += 4;
  updateFloat(i, g_rollPid.D);
  i += 4;
  updateFloat(i, g_pitchPid.P);
  i += 4;
  updateFloat(i, g_pitchPid.I);
  i += 4;
  updateFloat(i, g_pitchPid.D);
  i += 4;
  updateFloat(i, g_yawPid.P);
  i += 4;
  updateFloat(i, g_yawPid.I);
  i += 4;
  updateFloat(i, g_yawPid.D);
  i += 4;
  updateFloat(i, g_userControlGain);
  i += 4;
  updateFloat(i, g_pidWindupGuard);
}

float QuadEEPROM::readFloat(int address) {
  union floatStore {
    uint8_t floatByte[4];
    float floatVal;
  } 
  floatOut;

  for (int i = 0; i < 4; i++) 
    floatOut.floatByte[i] = EEPROM.read(address + i);
  return floatOut.floatVal;
}

void QuadEEPROM::writeFloat(int address, float value) {
  union floatStore {
    uint8_t floatByte[4];
    float floatVal;
  } 
  floatIn;

  floatIn.floatVal = value;
  for (int i = 0; i < 4; i++) 
    EEPROM.write(address + i, floatIn.floatByte[i]);
}

void QuadEEPROM::updateFloat(int address, float value) {
  if(readFloat(address) != value) {
    writeFloat(address, value);
  }
}





