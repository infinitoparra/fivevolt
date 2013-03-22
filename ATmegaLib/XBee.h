
#ifndef _xbee_h_
#define _xbee_h_

#include "Serial.h"
#include "IO.h"

#define XBEE_ADDR_COORDINATOR    0x0000000000000000
#define XBEE_ADDR_BROADCAST      0x000000000000FFFF
#define XBEE_NETADDR_UNKNOWN     0xFFFE

class XBee {
public:
	virtual void begin();
	void setSleepPin(OutputPin &sleepPin);
	void sleep();
	void wakeUp();
	virtual void sendTransmitRequest(uint64_t address, uint16_t networkAddress, uint8_t *data, uint16_t length) = 0;
	virtual void rxDataReady() = 0;
	bool isJoined() { return _joined; }
	bool waitForJoined();
	void setTimeout(unsigned long ms) { _timeout = ms; }

protected:
	XBee(Serial &serial);

	Serial *_serial;
	volatile bool _joined;

private:
	bool _sleepPinEnabled;
	OutputPin *_sleepPin;
	unsigned long _timeout;
};

class XBeeApi : public XBee {
public:
	XBeeApi(Serial &serial);
	virtual void sendTransmitRequest(uint64_t address, uint16_t networkAddress, uint8_t *data, uint16_t length);
	virtual void rxDataReady();

private:
	void sendStart(uint16_t length);
	void sendEnd(uint8_t checksum);
	void sendByte(uint8_t data, uint8_t *pChecksum);
	void sendPacket(uint8_t *data, uint16_t length);

	uint8_t _packetBuffer[128];
};

#endif
