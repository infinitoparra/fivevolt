
#include <WProgram.h>
#include <Wire.h>
#include "ReceiverCom.h"
#include "constants.h"

#define ENABLED_THRESHOLD 0.8f
#define ARM_THROTTLE_THRESHOLD 0.2f

void ReceiverCom::begin() {
}

void ReceiverCom::loop() {
  ReceiverI2CPacket packet;
  byte* packetPtr = (byte*)&packet;
  int i;
  memset(packetPtr, 0, sizeof(ReceiverI2CPacket));
  
  Wire.requestFrom(RECEIVER_WIRE_ADDR, sizeof(ReceiverI2CPacket));
  i = 0;
  while(Wire.available()){
    packetPtr[i++] = Wire.receive();
  }
  hasSignal = packet.hasSignal;
  for(i=0; i<SERVO_INPUT_CHANNELS; i++) {
    pos[i] = packet.pos[i];
  }
  
  // check if we are armed
  if(!hasSignal || pos[RECEIVER_GearEnable] < ENABLED_THRESHOLD) {
    _armed = false;
  } 
  else if(_armed == false) {
    if(hasSignal 
      && pos[RECEIVER_GearEnable] > ENABLED_THRESHOLD 
      && pos[RECEIVER_Throttle] < ARM_THROTTLE_THRESHOLD) {
        _armed = true;
    }
  }
}



