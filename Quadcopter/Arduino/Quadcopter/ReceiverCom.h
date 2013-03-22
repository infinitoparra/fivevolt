
#ifndef _receivercom_h_
#define _receivercom_h_

#include "constants.h"

class ReceiverCom {
public:
  void begin();
  void loop();
  boolean hasSignal;
  float pos[SERVO_INPUT_CHANNELS];
  int isArmed() { return _armed; }

private:
  int _armed;
};

#endif


