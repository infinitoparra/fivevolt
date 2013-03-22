
#include <Wire.h>
#include "ServoInput.h"
#include "Com.h"

#define COM_BAUD 19200
#define SIGNAL_StatusPin 8

Com g_com;
int g_lastCom;
ServoInput g_servoInput;
int g_lastServoInput;

void setup() {
  g_com.begin(COM_BAUD);
  g_servoInput.begin();
  pinMode(SIGNAL_StatusPin, OUTPUT);
}

void loop() {
  // delay 10ms because we don't need to run that fast.
  //  used delayMicroseconds because it doesn't disable interupts during delay
  delayMicroseconds(10000);
  int time = millis();

  if(time - g_lastServoInput > 10){
    g_lastServoInput = time;
    g_servoInput.loop();
    digitalWrite(SIGNAL_StatusPin, g_servoInput.packet.hasSignal ? HIGH : LOW);
  }

  /*
  if(time - g_lastCom > 50){
    g_lastCom = time;
    g_com.loop();
  }
  */
}





