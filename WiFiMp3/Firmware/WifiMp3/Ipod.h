
#ifndef _ipod_h_
#define _ipod_h_

#include "AppleAccessoryProtocol.h"

class Ipod : public AppleAccessoryProtocol {
public:
  Ipod(HardwareSerial &serial) : AppleAccessoryProtocol(serial) {}
  
protected:
  virtual void simpleRemoteButtonReleased();
  virtual void simpleRemoteButtonPressed(uint32 buttons);
  virtual void airRemoteCommand(uint8 cmd);
};

#endif
