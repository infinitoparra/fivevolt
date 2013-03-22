
#include <WProgram.h>
#include "NavLights.h"

void NavLights::begin(int leftPin, int rightPin, int beaconPin){
  _leftPin = leftPin;
  _rightPin = rightPin;
  _beaconPin = beaconPin;
  pinMode(_leftPin, OUTPUT);
  pinMode(_rightPin, OUTPUT);
  pinMode(_beaconPin, OUTPUT);
  digitalWrite(_leftPin, HIGH);
  digitalWrite(_rightPin, HIGH);
  _beaconState = HIGH;
}

void NavLights::loop(int time) {
  if(time - _beaconLast >= 500) {
    _beaconLast = time;
    _beaconState = _beaconState == HIGH ? LOW : HIGH;
    digitalWrite(_beaconPin, _beaconState);
  }
}




