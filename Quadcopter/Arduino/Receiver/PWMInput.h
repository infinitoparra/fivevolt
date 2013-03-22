
#ifndef pwminput_h
#define pwminput_h

#include <WProgram.h>

class PWMInput {
public:
  void attach(byte pin);
  void detach();
  unsigned int read();
  static void measurePulseWidthISR(byte port);
  
  // last fall time in microseconds
  unsigned long getLastSignalTime() { return _fallTime; }

private:
  byte _pin;
  byte _edge;
  unsigned long _riseTime;    
  unsigned long _fallTime; 
  unsigned long _lastGoodWidth;
};

#endif 




