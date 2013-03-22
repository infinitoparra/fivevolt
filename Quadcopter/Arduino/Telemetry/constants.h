
#ifndef _global_constants_h_
#define _global_constants_h_

#define RECEIVER_WIRE_ADDR 0x02
#define TELEMETRY_WIRE_ADDR 0x10

#define SERVO_INPUT_CHANNELS 6
#define RECEIVER_Aileron    0
#define RECEIVER_Elevator   1
#define RECEIVER_Throttle   2
#define RECEIVER_Rudder     3
#define RECEIVER_GearEnable 4
#define RECEIVER_Flaps      5

#define PACKET_OK           0x00
#define PACKET_ERROR        0xff
#define PACKET_TELEMETRY    0x01
#define PACKET_SETTINGS     0x02
#define PACKET_RECV         0x11
#define PACKET_IMU          0x21

struct ReceiverI2CPacket {
  byte hasSignal;
  float pos[SERVO_INPUT_CHANNELS];
};

struct TelemetryI2CPacket {
  float Yaw;
  float Pitch;
  float Roll;
};

#endif
