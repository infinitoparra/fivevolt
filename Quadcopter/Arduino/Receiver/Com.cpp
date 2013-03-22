
#include <WProgram.h>
#include <Wire.h>
#include "constants.h"
#include "Com.h"
#include "ServoInput.h"

extern ServoInput g_servoInput;
void requestEvent();

void Com::begin(long baud){
  Serial.begin(baud);
  Serial.println("RESET");
  Wire.begin(RECEIVER_WIRE_ADDR);
  Wire.onRequest(requestEvent);
}

void Com::loop(){
  /*
  if(g_servoInput.packet.hasSignal) {
    for(int i=0; i<SERVO_INPUT_CHANNELS; i++) {
      if(i!=0) {
        Serial.print(", ");
      }
      Serial.print(g_servoInput.packet.pos[i]);
    }
  }
  else{
    Serial.print("NO SIGNAL");
  }
  Serial.println();
  */
  
  Serial.print("QUAD");
  Serial.write(PACKET_RECV);
  unsigned long l = micros();
  Serial.write((byte*)&l, sizeof(unsigned long));
  Serial.write((byte*)&g_servoInput.packet, sizeof(ReceiverI2CPacket));
}

void requestEvent() {
  Wire.send((byte*)&g_servoInput.packet, sizeof(ReceiverI2CPacket));
}



