
#include <math.h>
#include <WProgram.h>
#include "BatteryManager.h"

#define LIPO_Max 12.6
#define LIPO_Min 9.0

void BatteryManager::begin(byte vinPin, byte warningPin, int vMin, int vMax, float warningV) {
  _vinPin = vinPin;
  _warningPin = warningPin;
  pinMode(_warningPin, OUTPUT);
  _y = ((LIPO_Max * vMin) - (LIPO_Min * vMax)) / (LIPO_Max - LIPO_Min);
  _slope = (vMax - vMin) / (LIPO_Max - LIPO_Min);
  _warningV = warningV;
  _warningBlinkState = LOW;
}

void BatteryManager::loop(int time) {
  int x = analogRead(_vinPin);
  _lastReading = (x - _y) / _slope;
  _isWarning = _lastReading < _warningV;
  if(_isWarning){
    if(time - _warningBlinkLast >= 100){
      _warningBlinkLast = time;
      _warningBlinkState = _warningBlinkState == HIGH ? LOW : HIGH;
    }
  }
  else{
    _warningBlinkState = LOW;
  }
  digitalWrite(_warningPin, _warningBlinkState);
}



