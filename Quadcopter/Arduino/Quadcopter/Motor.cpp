
#include <WProgram.h>
#include <Servo.h>
#include "Motor.h"

#define MOTOR_MIN 1200
#define MOTOR_MAX 2000
#define MOTOR_ON 1350

long clamp(long v, long min, long max) {
  if(v > max){
    v = max;
  }
  if(v < min){
    v = min;
  }
  return v;
}

void Motor::begin(int pin) {
  _pin = pin;
  _servo.attach(_pin);
}

void Motor::set(int throttleUp, float power) {
  long t = clamp(MOTOR_MIN + (power * (MOTOR_MAX - MOTOR_MIN)), MOTOR_MIN, MOTOR_MAX);
  if(!throttleUp) {
    t = MOTOR_MIN;
  } else {
    t = max(t, MOTOR_ON);
  }
  //int i = (int)(power * 180.0f);
  //_servo.write(i);
  _servo.writeMicroseconds(t);
  _lastValue = power;
}




