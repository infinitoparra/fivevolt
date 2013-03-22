
#ifndef _servoinput_h_
#define _servoinput_h_

#include <WProgram.h>
#include "constants.h"
#include "PWMInput.h"

class ServoInput {
public:
  void begin();
  void loop();
  ReceiverI2CPacket packet;
  
private:
  PWMInput _ch[SERVO_INPUT_CHANNELS];
  int _gearEnableCounter;
};

#endif


