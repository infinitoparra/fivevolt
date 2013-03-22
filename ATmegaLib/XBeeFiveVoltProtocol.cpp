
#include "XBeeFiveVoltProtocol.h"
#include <string.h>

XBeeFV::XBeeFV(Serial &serial) : XBeeApi(serial) {
}

void XBeeFV::sendCapabilities(uint16_t capability) {
	XBeeFV_CapabilityPacket packet;
	packet.packetHeader.packetType = XBEEFV_PACKETTYPE_CAPABILITY;
	packet.capability = capability;
	sendTransmitRequest(XBEE_ADDR_BROADCAST, XBEE_NETADDR_UNKNOWN, (uint8_t*)&packet, sizeof(XBeeFV_CapabilityPacket));
}

void XBeeFV::sendLowBattery() {
	XBeeFV_LowBatterPacket packet;
	packet.packetHeader.packetType = XBEEFV_PACKETTYPE_LOWBATTERY;
	sendTransmitRequest(XBEE_ADDR_COORDINATOR, XBEE_NETADDR_UNKNOWN, (uint8_t*)&packet, sizeof(XBeeFV_LowBatterPacket));
}

void XBeeFV::sendCapabilityData(uint64_t address, uint16_t networkAddress, uint16_t capability, uint8_t *data, uint16_t dataLength) {
	uint16_t packetLength = sizeof(XBeeFV_CapabilityDataPacket) + dataLength;
	XBeeFV_CapabilityDataPacket *pPacket = (XBeeFV_CapabilityDataPacket*)malloc(packetLength);
	pPacket->packetHeader.packetType = XBEEFV_PACKETTYPE_CAPABILITYDATA;
	pPacket->capability = capability;
	pPacket->dataLength = dataLength;
	uint8_t *p = ((uint8_t*)pPacket) + sizeof(XBeeFV_CapabilityDataPacket);
	for(uint16_t i=0; i<dataLength; i++) {
		*(p++) = data[i];
	}
	sendTransmitRequest(address, networkAddress, (uint8_t*)pPacket, packetLength);
	free(pPacket);
}

void XBeeFV::sendMessage(uint64_t address, uint16_t networkAddress, uint8_t level, const char* message) {
	uint16_t packetLength = sizeof(XBeeFV_MessagePacket) + strlen(message);
	XBeeFV_MessagePacket *pPacket = (XBeeFV_MessagePacket*)malloc(packetLength);
	pPacket->packetHeader.packetType = XBEEFV_PACKETTYPE_MESSAGE;
	pPacket->level = level;
	char *p = ((char*)pPacket) + sizeof(XBeeFV_MessagePacket);
	strcpy(p, message);
	sendTransmitRequest(address, networkAddress, (uint8_t*)pPacket, packetLength);
	free(pPacket);
}

void XBeeFV::sendMessage(uint8_t level, const char* message) {
	sendMessage(XBEE_ADDR_COORDINATOR, XBEE_NETADDR_UNKNOWN, level, message);
}