
#ifndef _imu_h_
#define _imu_h_

#include "CompFilter.h"

class Imu {
public:
  void begin();
  void loop();  
  float Yaw, Pitch, Roll;
  float _rawAccelRoll, _rawAccelPitch, _rawCompassYaw;
  float _rawGx, _rawGy, _rawGz;

private:
  uint8_t _compFilterInitialized;
  CompFilter _compFilterRoll;
  CompFilter _compFilterPitch;
  CompFilter _compFilterYaw;
};

#endif


