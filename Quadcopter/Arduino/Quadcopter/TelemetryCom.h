
#ifndef _TelemetryCom_h_
#define _TelemetryCom_h_

#include "constants.h"

class TelemetryCom {
public:
  void begin();
  void loop();
  float Roll, Pitch, Yaw;
};

#endif

