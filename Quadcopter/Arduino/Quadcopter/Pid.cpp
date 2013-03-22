
#include "pid.h"

extern float g_pidWindupGuard;

float Pid::update(float targetPosition, float currentPosition) {
  float error;
  float dTerm;

  error = targetPosition - currentPosition;

  _integratedError += error;
  if (_integratedError < -g_pidWindupGuard) {
    _integratedError = -g_pidWindupGuard;
  }
  else if (_integratedError > g_pidWindupGuard) {
    _integratedError = g_pidWindupGuard;
  }

  dTerm = D * (currentPosition - _lastPosition);
  _lastPosition = (P * error) + (I * _integratedError) + dTerm;
  return _lastPosition;
}

void Pid::zeroIntegralError(){
  _integratedError = 0;
}




