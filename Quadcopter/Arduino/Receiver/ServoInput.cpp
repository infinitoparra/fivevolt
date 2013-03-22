
#include <avr/io.h>
#include "ServoInput.h"

#define DT_MIN 960.0f
#define DT_MAX 2072.0f

// 2 seconds
#define SIGNAL_LOSS_THRESHOLD 2000000

int ServoInputPinMapping[] = { 2, 3, 4, 5, 6, 7 };

void ServoInput::begin() {
  for(int i=0; i<SERVO_INPUT_CHANNELS; i++) {
    _ch[i].attach(ServoInputPinMapping[i]);
    if(i == RECEIVER_Throttle || i == RECEIVER_GearEnable) {
      packet.pos[i] = 0.0f;
    }
    else {
      packet.pos[i] = 0.5f;
    }
  }
}

void ServoInput::loop() {
  byte oldSREG;

  if(micros() - _ch[0].getLastSignalTime() > SIGNAL_LOSS_THRESHOLD) {
    packet.hasSignal = false;
  }
  else{
    packet.hasSignal = true;
  }

  for(int i=0; i<SERVO_INPUT_CHANNELS; i++) {
    float p = ((float)_ch[i].read() - DT_MIN) / (DT_MAX - DT_MIN);
    p = min(1.0f, max(0.0f, p));
    if(i == RECEIVER_Elevator || i == RECEIVER_Rudder) {
      p = 1.0f - p;
    }
    if(i == RECEIVER_GearEnable) {
      if(p > 0.5) {
        p = 1.0f;
      } 
      else {
        p = 0.0f;
      }
    }
    if(i == RECEIVER_Aileron || i == RECEIVER_Elevator || i == RECEIVER_Rudder) {
      p -= 0.5;
    }

    // smooth the value using last value
    if(i == RECEIVER_GearEnable) {
      if(abs(packet.pos[RECEIVER_GearEnable] - p) < 0.1) {
        _gearEnableCounter = 0;
      } else {
        _gearEnableCounter++;
        if(_gearEnableCounter >= 5) {
          _gearEnableCounter = 0;
          // packet.pos[i] is set below to p
        } else {
          p = packet.pos[RECEIVER_GearEnable];
        }
      }
    }
    else {
      p = (packet.pos[i] * 0.25) + (p * 0.75);
    }

    oldSREG = SREG;
    cli();
    packet.pos[i] = p;
    SREG = oldSREG;
  }
}











