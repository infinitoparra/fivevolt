
#include <Wire.h>
#include "Com.h"
#include "Imu.h"
#include "SoftTWI.h"

#define COM_BAUD 19200

SoftTWI g_softTwi;
Com g_com;
int g_lastCom;
Imu g_imu;
int g_lastImu;

void setup() {
  g_com.begin(COM_BAUD);
  g_softTwi.attach(2, 3);
  g_softTwi.begin();
  g_imu.begin();
}

void loop() {
  // delay 10ms because we don't need to run that fast.
  //  used delayMicroseconds because it doesn't disable interupts during delay
  delayMicroseconds(10000);
  
  int time = millis();

  if(time - g_lastImu > 10){
    g_lastImu = time;
    g_imu.loop();
  }  

  if(time - g_lastCom > 50){
    g_lastCom = time;
    g_com.loop();
  }
}
