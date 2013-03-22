
#ifndef _xbeefivevoltprotocol_h_
#define _xbeefivevoltprotocol_h_

#include "XBee.h"
#include <stdlib.h>

#define XBEEFV_USER_UNKNOWN 0x00

#define XBEEFV_PACKETTYPE_CAPABILITY     0x01
#define XBEEFV_PACKETTYPE_LOWBATTERY     0x02
#define XBEEFV_PACKETTYPE_CAPABILITYDATA 0x03
#define XBEEFV_PACKETTYPE_MESSAGE        0x04

#define XBEEFV_CAPABILITY_SCALE  0x0100

#define XBEEFV_MESSAGELEVEL_DEBUG 0x01
#define XBEEFV_MESSAGELEVEL_INFO  0x02
#define XBEEFV_MESSAGELEVEL_WARN  0x03
#define XBEEFV_MESSAGELEVEL_ERROR 0x04

class XBeeFV : public XBeeApi {
public:
	XBeeFV(Serial &serial);
	void sendCapabilities(uint16_t capability);
	void sendLowBattery();
	void sendCapabilityData(uint64_t address, uint16_t networkAddress, uint16_t capability, uint8_t *data, uint16_t dataLength);
	void sendMessage(uint64_t address, uint16_t networkAddress, uint8_t level, const char* message);
	void sendMessage(uint8_t level, const char* message);
};

struct XBeeFV_Packet {
	uint8_t packetType;
};

struct XBeeFV_CapabilityPacket {
	XBeeFV_Packet packetHeader;
	uint16_t      capability;
};

struct XBeeFV_LowBatterPacket {
	XBeeFV_Packet packetHeader;
};

struct XBeeFV_CapabilityDataPacket {
	XBeeFV_Packet packetHeader;
	uint16_t      capability;
	uint16_t      dataLength;
	// uint8_t[]  data;
};

struct XBeeFV_MessagePacket {
	XBeeFV_Packet packetHeader;
	uint8_t level;
	// const char* message
};

#endif