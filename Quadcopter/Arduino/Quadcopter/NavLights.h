
#ifndef _navlights_h_
#define _navlights_h_

class NavLights {
public:
  void begin(int leftPin, int rightPin, int beaconPin);
  void loop(int time);
private:
  int _leftPin, _rightPin;
  int _beaconPin;
  int _beaconState;
  int _beaconLast;
};

#endif


