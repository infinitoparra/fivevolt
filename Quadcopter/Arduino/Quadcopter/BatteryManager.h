
#ifndef _batterymanager_h_
#define _batterymanager_h_

#include <WProgram.h>

class BatteryManager {
public:
  void begin(byte vinPin, byte warningPin, int vMin, int vMax, float warningV);
  void loop(int time);
  float getLastReading() { 
    return _lastReading; 
  }
  boolean getIsWarning() { 
    return _isWarning; 
  }

private:
  byte _vinPin, _warningPin;
  float _lastReading;
  float _slope, _y;
  float _warningV;
  boolean _isWarning;
  int _warningBlinkState;
  int _warningBlinkLast;
};

#endif


