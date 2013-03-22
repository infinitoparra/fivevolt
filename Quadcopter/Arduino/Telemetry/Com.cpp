
#include <WProgram.h>
#include <Wire.h>
#include "constants.h"
#include "Com.h"
#include "Imu.h"

extern Imu g_imu;
void requestEvent();

void Com::begin(long baud){
  Serial.begin(baud);
  Serial.println("RESET");
  Wire.begin(TELEMETRY_WIRE_ADDR);
  Wire.onRequest(requestEvent);
}

void Com::loop(){
  /*
  Serial.print(g_imu.Yaw);
   Serial.print(",");
   Serial.print(g_imu.Pitch);
   Serial.print(",");
   Serial.print(g_imu.Roll);
   Serial.println();
   */

  Serial.print("QUAD");
  Serial.write(PACKET_IMU);
  unsigned long l = micros();
  Serial.write((byte*)&l, sizeof(unsigned long));
  Serial.write((byte*)&g_imu._rawAccelRoll, sizeof(float));
  Serial.write((byte*)&g_imu._rawAccelPitch, sizeof(float));
  Serial.write((byte*)&g_imu._rawCompassYaw, sizeof(float));
  Serial.write((byte*)&g_imu._rawGx, sizeof(float));
  Serial.write((byte*)&g_imu._rawGy, sizeof(float));
  Serial.write((byte*)&g_imu._rawGz, sizeof(float));
  Serial.write((byte*)&g_imu.Roll, sizeof(float));
  Serial.write((byte*)&g_imu.Pitch, sizeof(float));
  Serial.write((byte*)&g_imu.Yaw, sizeof(float));
}

void requestEvent() {
  TelemetryI2CPacket packet;
  packet.Yaw = g_imu.Yaw;
  packet.Pitch = g_imu.Pitch;
  packet.Roll = g_imu.Roll;
  Wire.send((byte*)&packet, sizeof(TelemetryI2CPacket));
}





