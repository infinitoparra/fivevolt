
#include "XBee.h"
#include <stdlib.h>
#include <util/delay.h>

// todo: make this variable so we can send multiple packets and get different acks
#define FRAMEID 0x01
// todo: make this variable
#define BROADCAST_RADIUS 0
// todo: make this variable
#define OPTIONS 0

#define START_DELIMITER 0x7e
#define FRAMETYPE_TRANSMIT_REQUEST 0x10
#define FRAMETYPE_MODEM_STATUS     0x8A

#define MODEM_STATUS_HARDWARE_RESET      0
#define MODEM_STATUS_WATCHDOG_TIMER      1
#define MODEM_STATUS_JOINED              2
#define MODEM_STATUS_DISASSOCIATED       3
#define MODEM_STATUS_COORDINATOR_STARTED 6
#define MODEM_STATUS_KEY_UPDATED         7

#define sendByte(data) {             \
	uint8_t ___d = (uint8_t)(data);  \
	_serial->write(___d);            \
	checksum += ___d;                \
}

#define sendAddress(address) {        \
	sendByte((address >> 56) & 0xff); \
	sendByte((address >> 48) & 0xff); \
	sendByte((address >> 40) & 0xff); \
	sendByte((address >> 32) & 0xff); \
	sendByte((address >> 24) & 0xff); \
	sendByte((address >> 16) & 0xff); \
	sendByte((address >>  8) & 0xff); \
	sendByte((address >>  0) & 0xff); \
}

#define sendNetworkAddress(networkAddress) { \
	sendByte((networkAddress >> 8) & 0xff);  \
	sendByte((networkAddress >> 0) & 0xff);  \
}

XBee::XBee(Serial &serial) {
	_serial = &serial;
	_timeout = 30 * 1000;
}

void XBee::begin() {
}

void XBee::setSleepPin(OutputPin &sleepPin) {
	_sleepPinEnabled = true;
	_sleepPin = &sleepPin;
}

void XBee::sleep() {
	if(_sleepPinEnabled) {
		_sleepPin->set();
		_joined = false;
	} else {
		// todo: throw error
	}
}

void XBee::wakeUp() {
	if(_sleepPinEnabled) {
		_sleepPin->clear();
	} else {
		// todo: throw error
	}
}

bool XBee::waitForJoined() {
	unsigned long time = 0;
	while(!_joined) {
		if(time > _timeout) {
			break;
		}
		_delay_ms(10);
		time += 10;
	}
	return _joined;
}

XBeeApi::XBeeApi(Serial &serial) : XBee(serial) {
}

void XBeeApi::sendTransmitRequest(uint64_t address, uint16_t networkAddress, uint8_t *data, uint16_t length) {
	uint8_t checksum = 0;
	waitForJoined();
	sendStart(14 + length);
	sendByte(FRAMETYPE_TRANSMIT_REQUEST);
	sendByte(FRAMEID);
	sendAddress(address);
	sendNetworkAddress(networkAddress);
	sendByte(BROADCAST_RADIUS);
	sendByte(OPTIONS);
	for(uint16_t i=0; i<length; i++) {
		sendByte(data[i]);
	}
	sendEnd(checksum);
}

void XBeeApi::sendStart(uint16_t length) {
	_serial->write(START_DELIMITER);
	_serial->write((uint8_t)((length >> 8) & 0xff));
	_serial->write((uint8_t)(length & 0xff));
}

void XBeeApi::sendEnd(uint8_t checksum) {
	_serial->write(0xff - checksum);
}

void XBeeApi::sendPacket(uint8_t *data, uint16_t length) {
	uint8_t checksum = 0;

	sendStart(length);
	for(uint16_t i=0; i<length; i++) {
		sendByte(data[i]);
	}
	sendEnd(checksum);
}

void XBeeApi::rxDataReady() {
	_serial->skipUntil(START_DELIMITER);
	if(_serial->peek() == START_DELIMITER && _serial->available() > 3) {
		uint16_t packetSize = (((uint16_t)_serial->peek(1)) << 8) | ((uint16_t)_serial->peek(2));
		if(_serial->available() >= packetSize + 4) {
			_serial->read(); // start delimiter
			_serial->read(); // packet size (byte1)
			_serial->read(); // packet size (byte2)
			for(uint16_t i=0; i<packetSize; i++) {
				_packetBuffer[i] = _serial->read();
			}
			// todo: validate checksum
			_serial->read(); // checksum

			switch(_packetBuffer[0]) {
				case FRAMETYPE_MODEM_STATUS:
					switch(_packetBuffer[1]) {
						case MODEM_STATUS_JOINED:
							_joined = true;
							break;
						case MODEM_STATUS_DISASSOCIATED:
							_joined = false;
							break;
					}
					break;
			}
		}
	}
}

