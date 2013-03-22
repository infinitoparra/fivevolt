
#include <Servo.h>

#ifndef _motor_h_
#define _motor_h_

class Motor {
public:
  void begin(int pin);
  void set(int throttleUp, float power);
  float get() { return _lastValue; }

private:
  int _pin;
  Servo _servo;
  float _lastValue;
};

#endif

