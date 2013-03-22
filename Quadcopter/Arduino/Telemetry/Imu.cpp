
#include <WProgram.h>
#include "SoftTWI.h"
#include "Imu.h"

#define IMU_AxPin 0
#define IMU_AyPin 1
#define IMU_AzPin 2
#define IMU_GxPin 7
#define IMU_GyPin 6
#define IMU_GzPin 3 
#define IMU_AccelZero 500
#define IMU_AccelAdj(x) (x - IMU_AccelZero)
#define IMU_GyroZero 380
#define IMU_GyroScale 3.4
#define IMU_GyroAdj(x) ((x - IMU_GyroZero) / IMU_GyroScale * (PI / 180.0))

#define HMC5843_W 0x3C
#define HMC5843_R 0x3D
#define HMC5843_MAX 2047
#define HMC5843_MIN -2048

extern SoftTWI g_softTwi;

void Imu::begin() {
  g_softTwi.beginTransmission(HMC5843_W);
  g_softTwi.send(0x02); //Write to Mode register
  g_softTwi.send(0x00); //Clear bit 1, the MD1 bit
  g_softTwi.endTransmission();
  
  _compFilterInitialized = false;
}

void Imu::loop() {
  g_softTwi.beginTransmission(HMC5843_W);
  g_softTwi.send(0x03); //Read from a given address
  g_softTwi.endTransmission();

  g_softTwi.requestFrom(HMC5843_R, 6);
  int16_t rawCompassX = g_softTwi.receiveInt16();
  int16_t rawCompassY = g_softTwi.receiveInt16();
  int16_t rawCompassZ = g_softTwi.receiveInt16();

  int rawAx = IMU_AccelAdj(analogRead(IMU_AxPin));
  int rawAy = IMU_AccelAdj(analogRead(IMU_AyPin));
  int rawAz = IMU_AccelAdj(analogRead(IMU_AzPin));
  _rawGx = IMU_GyroAdj(analogRead(IMU_GxPin));
  _rawGy = IMU_GyroAdj(analogRead(IMU_GyPin));
  _rawGz = IMU_GyroAdj(analogRead(IMU_GzPin));

  _rawCompassYaw = atan2(rawCompassY, rawCompassX);
  _rawAccelPitch = atan2(rawAy, rawAz);
  _rawAccelRoll = -(atan2(rawAx, rawAz));
  
  if(!_compFilterInitialized) {
    _compFilterRoll.init(_rawAccelRoll, _rawGy);
    _compFilterPitch.init(_rawAccelPitch, _rawGx);
    _compFilterYaw.init(_rawCompassYaw, _rawGz);
    _compFilterInitialized = true;
  }
  
  Roll = _compFilterRoll.update(_rawAccelRoll, _rawGy);
  Pitch = _compFilterPitch.update(_rawAccelPitch, _rawGx);
  Yaw = _compFilterYaw.update(_rawCompassYaw, _rawGz);
}







