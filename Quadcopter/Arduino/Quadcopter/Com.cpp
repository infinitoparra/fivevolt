
#include "WProgram.h"
#include "Com.h"
#include "constants.h"
#include "BatteryManager.h"
#include "ReceiverCom.h"
#include "TelemetryCom.h"
#include "Motor.h"
#include "Pid.h"
#include "Eeprom.h"

extern BatteryManager g_batteryManager;
extern ReceiverCom g_receiverCom;
extern TelemetryCom g_telemetryCom;
extern Motor g_motorLeft;
extern Motor g_motorRight;
extern Motor g_motorFront;
extern Motor g_motorBack;
extern QuadEEPROM g_eeprom;
extern Pid g_rollPid;
extern Pid g_pitchPid;
extern Pid g_yawPid;
extern long temp;
extern long g_loopCount;
extern float g_userControlGain;
extern float g_pidWindupGuard;

struct Telem {
  float Yaw, Pitch, Roll;
  float MotorL, MotorR, MotorF, MotorB;
  float BatteryLevel;
  float UserElev, UserPitch, UserRoll, UserYaw;
  long loopCount;
};

struct PidPacket {
  uint8_t PidType;
  float P, I, D;
};

void Com::begin(long baud) {
  Serial.begin(baud);
  Serial.println("RESET");
}

void Com::loop(int time) {
  while(Serial.available()>0){
    char cmd = Serial.read();
    switch(cmd) {
    case 't':
      temp = readLong();
      sendOk();
      Serial.print("temp = ");
      Serial.println(temp);
      break;

    case 's': // read settings
      sendSettings();
      break;

    case 'P': // write PID
      updatePid();
      break;
    case 'G': // write Gain
      updateFloat(&g_userControlGain);
      break;
    case 'W': // write Windup Guard
      updateFloat(&g_pidWindupGuard);
      break;
    default:
      sendError();
      break;
    }
  }

  {
    Telem telem;
    sendPacketStart(PACKET_TELEMETRY); 
    telem.Yaw = g_telemetryCom.Yaw;
    telem.Pitch = g_telemetryCom.Pitch;
    telem.Roll = g_telemetryCom.Roll;
    telem.MotorL = g_motorLeft.get();
    telem.MotorR = g_motorRight.get();
    telem.MotorF = g_motorFront.get();
    telem.MotorB = g_motorBack.get();
    telem.BatteryLevel = g_batteryManager.getLastReading();
    telem.UserElev = g_receiverCom.pos[RECEIVER_Throttle];
    telem.UserPitch = g_receiverCom.pos[RECEIVER_Elevator];
    telem.UserRoll = g_receiverCom.pos[RECEIVER_Aileron];
    telem.UserYaw = g_receiverCom.pos[RECEIVER_Rudder];
    telem.loopCount = g_loopCount;
    Serial.write((byte*)&telem, sizeof(Telem));
  }

  if(time - _lastSlowTime > 1000){
    _lastSlowTime = time;

    Serial.print("RECV: ");
    Serial.print(g_receiverCom.hasSignal ? "HAS SIGNAL " : "NO SIGNAL  ");
    for(int i=0; i<SERVO_INPUT_CHANNELS; i++) {
      if(i != 0){
        Serial.print(", ");
      }
      Serial.print(g_receiverCom.pos[i]);
    }

    Serial.print(" TELEM: ");
    Serial.print(g_telemetryCom.Yaw);
    Serial.print(", ");
    Serial.print(g_telemetryCom.Pitch);
    Serial.print(", ");  
    Serial.print(g_telemetryCom.Roll);

    Serial.print(" MOTOR: ");
    Serial.print(g_motorFront.get());
    Serial.print(", ");
    Serial.print(g_motorBack.get());
    Serial.print(", ");  
    Serial.print(g_motorLeft.get());
    Serial.print(", ");  
    Serial.print(g_motorRight.get());

    Serial.print(" PID: ");
    Serial.print(g_rollPid.getLastPosition());
    Serial.print(", ");
    Serial.print(g_pitchPid.getLastPosition());
    Serial.print(", ");  
    Serial.print(g_yawPid.getLastPosition());

    Serial.print(" BAT: ");
    Serial.print(g_batteryManager.getLastReading());
    Serial.println(g_batteryManager.getIsWarning() ? " warning" : " ");
  }
}

void Com::sendPacketStart(uint8_t packetType) {
  Serial.print("QUAD");
  Serial.write(packetType);
}

long Com::readLong() {
  long l;
  readBytes((byte*)&l, sizeof(long));
}

float Com::readFloat() {
  float l;
  readBytes((byte*)&l, sizeof(float));
}

uint8_t Com::readBytes(uint8_t* buffer, int len) {
  int i = 0;
  while(Serial.available() < len) {
    delay(10);
    i++;
    if(i > 100) {
      return false;
    }
  }

  for(i=0; i<len; i++) {
    buffer[i] = Serial.read();
  }
  return true;
}

Pid* Com::getPidFromPidType(char pidType) {
  switch(pidType) {
  case 'r':
    return &g_rollPid;
  case 'p':
    return &g_pitchPid;
  case 'y':
    return &g_yawPid;
  }
  return NULL;
}

void Com::updateFloat(float* f) {
  float val;
  if(!readBytes((byte*)&val, sizeof(float))) {
    sendErrorTimeout();
    return;
  }

  uint8_t checksum = Serial.read();
  uint8_t computedChecksum = computeChecksum((byte*)&val, sizeof(float));
  if(computedChecksum != checksum) {
    sendErrorInvalidChecksum(checksum, computedChecksum);
    return;
  }

  *f = val;

  g_eeprom.updateValues();

  sendOk();
}

void Com::updatePid() {
  PidPacket pidPacket;
  if(!readBytes((byte*)&pidPacket, sizeof(PidPacket))) {
    sendErrorTimeout();
    return;
  }

  uint8_t checksum = Serial.read();
  uint8_t computedChecksum = computeChecksum((byte*)&pidPacket, sizeof(PidPacket));
  if(computedChecksum != checksum) {
    sendErrorInvalidChecksum(checksum, computedChecksum);
    return;
  }

  Pid *pid = getPidFromPidType(pidPacket.PidType);
  if(pid == NULL){
    sendErrorInvalidPid();
  }

  pid->P = pidPacket.P;
  pid->I = pidPacket.I;
  pid->D = pidPacket.D;

  g_eeprom.updateValues();

  sendOk();
}

void Com::sendFloat(float f) {
  Serial.write((byte*)&f, sizeof(float));
}

void Com::sendSettings() {
  sendPacketStart(PACKET_SETTINGS);
  sendFloat(g_rollPid.P);
  sendFloat(g_rollPid.I);
  sendFloat(g_rollPid.D);
  sendFloat(g_pitchPid.P);
  sendFloat(g_pitchPid.I);
  sendFloat(g_pitchPid.D);
  sendFloat(g_yawPid.P);
  sendFloat(g_yawPid.I);
  sendFloat(g_yawPid.D);
  sendFloat(g_userControlGain);
  sendFloat(g_pidWindupGuard);
  sendOk();
}

uint8_t Com::computeChecksum(uint8_t* buffer, int len) {
  uint8_t result = 0;
  for(int i=0; i<len; i++) {
    result = result ^ buffer[i];
  }
  return result;
}

void Com::sendOk() {
  sendPacketStart(PACKET_OK);
  Serial.println("+OK");
}

void Com::sendError() {
  sendPacketStart(PACKET_ERROR);
  Serial.println("-Error");
}

void Com::sendErrorInvalidPid() {
  sendPacketStart(PACKET_ERROR);
  Serial.println("-Invalid PID");
}

void Com::sendErrorInvalidChecksum(uint8_t recvChecksum, uint8_t expectedChecksum) {
  sendPacketStart(PACKET_ERROR);
  Serial.print("-Invalid Checksum (recv: 0x");
  Serial.print(recvChecksum, HEX);
  Serial.print(", expected: 0x");
  Serial.print(expectedChecksum, HEX);
  Serial.println(")");
}

void Com::sendErrorTimeout() {
  sendPacketStart(PACKET_ERROR);
  Serial.print("-Timeout (available: ");
  int z = Serial.available();
  Serial.print(z);
  Serial.println(")");
}































