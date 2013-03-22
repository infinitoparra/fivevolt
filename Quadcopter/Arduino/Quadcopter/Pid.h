
#ifndef _pid_h_
#define _pid_h_

class Pid
{
public:
  float update(float targetPosition, float currentPosition);
  void zeroIntegralError();
  float getLastPosition() { return _lastPosition; }
  float P, I, D;
private:
  float _lastPosition;
  float _integratedError;
};

#endif

