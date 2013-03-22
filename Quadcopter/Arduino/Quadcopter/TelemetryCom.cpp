
#include <WProgram.h>
#include <Wire.h>
#include "TelemetryCom.h"
#include "constants.h"

void TelemetryCom::begin() {
}

void TelemetryCom::loop() {
  TelemetryI2CPacket packet;
  byte* packetPtr = (byte*)&packet;
  int i;
  memset(packetPtr, 0, sizeof(TelemetryI2CPacket));

  Wire.requestFrom(TELEMETRY_WIRE_ADDR, sizeof(TelemetryI2CPacket));
  i = 0;
  while(Wire.available()){
    packetPtr[i++] = Wire.receive();
  }
  Yaw = 0; // todo packet.Yaw
  Pitch = packet.Pitch;
  Roll = 0; // todo packet.Roll;
}


