
#include <Servo.h>
#include <Wire.h>
#include <EEPROM.h>
#include "Com.h"
#include "BatteryManager.h"
#include "NavLights.h"
#include "Motor.h"
#include "ReceiverCom.h"
#include "TelemetryCom.h"
#include "Pid.h"
#include "Eeprom.h"

#define COM_BAUD 19200

// analog pin to read battery voltage from
#define BATTERY_VInPin 0
#define BATTERY_WarningPin 2
#define BATTERY_VMin 690
#define BATTERY_VMax 965
#define BATTERY_VWarning 9.9

#define NAVLIGHT_LeftPin 5
#define NAVLIGHT_RightPin 6
#define NAVLIGHT_BeaconPin 7

#define MOTOR_LeftPin 9
#define MOTOR_RightPin 10
#define MOTOR_FrontPin 11
#define MOTOR_BackPin 3

#define HOVER_THRESHOLD 0.2

// global variables
Com g_com;
BatteryManager g_batteryManager;
NavLights g_navLights;
Motor g_motorLeft, g_motorRight, g_motorFront, g_motorBack;
ReceiverCom g_receiverCom;
TelemetryCom g_telemetryCom;
QuadEEPROM g_eeprom;
int g_batteryCheckLast;
int g_comLast;
int g_navLightsLast;
int g_motorLast;
int g_receiverComLast;
int g_telemetryComLast;
Pid g_rollPid, g_pitchPid, g_yawPid;
long temp;
long g_loopCount;
float g_userControlGain;
float g_pidWindupGuard;


void setup(){
  Wire.begin();
  g_com.begin(COM_BAUD);
  g_batteryManager.begin(BATTERY_VInPin, BATTERY_WarningPin, BATTERY_VMin, BATTERY_VMax, BATTERY_VWarning);
  g_navLights.begin(NAVLIGHT_LeftPin, NAVLIGHT_RightPin, NAVLIGHT_BeaconPin);
  g_motorLeft.begin(MOTOR_LeftPin);
  g_motorRight.begin(MOTOR_RightPin);
  g_motorFront.begin(MOTOR_FrontPin);
  g_motorBack.begin(MOTOR_BackPin);
  g_receiverCom.begin();
  g_telemetryCom.begin();
  g_eeprom.begin();

  g_loopCount = 0;
  temp = 0.0f;
}

void loop() {
  // delay 10ms because we don't need to run that fast.
  //  used delayMicroseconds because it doesn't disable interupts during delay
  delayMicroseconds(10000);
  
  int time = millis();
  g_loopCount++;

  // receiver com
  if(time - g_receiverComLast >= 50) {
    g_receiverComLast = time;
    g_receiverCom.loop();
  }

  // telemetry com
  if(time - g_telemetryComLast >= 20) {
    g_telemetryComLast = time;
    g_telemetryCom.loop();
  }

  // motors
  if(time - g_motorLast >= 20) {
    g_motorLast = time;

    if(g_receiverCom.isArmed()) {
      if(abs(g_receiverCom.pos[RECEIVER_Elevator]) > HOVER_THRESHOLD) {
        g_pitchPid.zeroIntegralError();
      }
      float pitchCommand = g_pitchPid.update(g_receiverCom.pos[RECEIVER_Elevator], g_telemetryCom.Pitch) * g_userControlGain;

      if(abs(g_receiverCom.pos[RECEIVER_Aileron]) > HOVER_THRESHOLD) {
        g_rollPid.zeroIntegralError();
      }
      float rollCommand = 0; // todo g_rollPid.update(g_receiverCom.pos[RECEIVER_Aileron], g_telemetryCom.Roll);

      if(abs(g_receiverCom.pos[RECEIVER_Rudder]) > HOVER_THRESHOLD) {
        g_yawPid.zeroIntegralError();
      }
      float yawCommand = 0; // todo g_yawPid.update(g_receiverCom.pos[RECEIVER_Rudder], 0 /*g_telemetryCom.Yaw*/);

      float throttleCommand = g_receiverCom.pos[RECEIVER_Throttle]; // todo: should this be run through a PID, what should the current position be?  g_throttlePid.update(g_receiverCom.pos[RECEIVER_Throttle], g_throttlePid.getLastPosition());
  
      //throttleCommand = temp;
  
      int throttleUp = throttleCommand > 0.2;
  
      g_motorFront.set(throttleUp, throttleCommand + pitchCommand + yawCommand);
      g_motorBack.set(throttleUp, throttleCommand - pitchCommand + yawCommand);
      g_motorRight.set(throttleUp, throttleCommand + rollCommand - yawCommand);
      g_motorLeft.set(throttleUp, throttleCommand - rollCommand - yawCommand);
    } else {
      g_motorFront.set(false, 0.0f);
      g_motorBack.set(false, 0.0f);
      g_motorRight.set(false, 0.0f);
      g_motorLeft.set(false, 0.0f);
    }
  }

  // com
  if(time - g_comLast >= 100){
    g_comLast = time;
    g_com.loop(time);
  }

  // read battery
  if(time - g_batteryCheckLast >= 1000 || g_batteryManager.getIsWarning()) {
    g_batteryCheckLast = time;
    g_batteryManager.loop(time);
  }

  // nav lights
  if(time - g_navLightsLast >= 100) {
    g_navLightsLast = time;
    g_navLights.loop(time);
  }
}










