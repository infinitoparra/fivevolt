
#ifndef _com_h_
#define _com_h_

#include "Pid.h"

class Com {
public:
  void begin(long baud);
  void loop(int time);

private:
  long readLong();
  float readFloat();
  uint8_t readBytes(uint8_t* buffer, int len);
  uint8_t computeChecksum(uint8_t* buffer, int len);
  void sendOk();
  void sendError();
  void sendErrorInvalidChecksum(uint8_t recvChecksum, uint8_t expectedChecksum);
  void sendErrorTimeout();
  void sendErrorInvalidPid();
  void sendPacketStart(uint8_t packetType);
  void sendFloat(float f);
  int _lastSlowTime;
  void updatePid();
  void sendSettings();
  void updateFloat(float* f);
  Pid* getPidFromPidType(char pidType);
};

#endif





