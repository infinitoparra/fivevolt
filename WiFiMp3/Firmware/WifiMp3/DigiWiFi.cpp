
#include <io.h>
#include "DigiWiFi.h"
#include "global.h"

#define WIFI_DEBUG

#ifndef min
#define min(a,b) ( ((a)<(b)) ? (a) : (b) )
#endif

#define DIGI_START_DELIMITER 0x7e

#define LOOP_NONE       0
#define LOOP_FRAME_DATA 1
#define LOOP_CHECKSUM   2

DigiWiFi::DigiWiFi(HardwareSerial &serial, uint8 cts, uint8 rts) {
  m_pSerial = &serial;
  m_cts = cts;
  m_rts = rts;
}

void DigiWiFi::begin(uint32 baud) {
  m_pSerial->begin(baud);
  m_loopState = LOOP_NONE;
  m_loopFrameIdentifier = FRAME_NONE;
  pinMode(m_cts, INPUT);
  pinMode(m_rts, OUTPUT);
}

void DigiWiFi::debug(HardwareSerial &serial) {
  serial.print("m_loopState: ");
  serial.println(m_loopState);
  serial.print("m_loopLength: ");
  serial.println(m_loopLength);
  serial.print("m_loopLengthRemaining: ");
  serial.println(m_loopLengthRemaining);
  serial.print("m_loopFrameIdentifier: ");
  serial.println(m_loopFrameIdentifier);
}

void DigiWiFi::writeBeginDataFrame(uint8 apiFrameId, uint16 len) {
  len++;
  m_pSerial->write(DIGI_START_DELIMITER);
  m_pSerial->write((len>>8) & 0xff);
  m_pSerial->write(len & 0xff);
  m_writeChecksum = 0;
  writeToDataFrame(apiFrameId);
}

void DigiWiFi::writeToDataFrame(uint8 ch) {
  m_pSerial->write(ch);
  m_writeChecksum += ch;
}

void DigiWiFi::writeToDataFrame(uint8 *pBuffer, uint16 len) {
  // todo: this could be spead up
  while(len > 0) {
    writeToDataFrame(*pBuffer);
    pBuffer++;
    len--;
  }
}

void DigiWiFi::writeEndDataFrame() {
  m_pSerial->write(0xff - m_writeChecksum);
}

void DigiWiFi::skipFrame() {
  uint16 avail;
  while((avail = available()) > 0) {
    for(uint16 i=0; i<avail; i++) {
      read();
    }
  }
}

uint16 DigiWiFi::available() {
  digitalWrite(m_rts, 1);
  switch(m_loopState) {
    case LOOP_NONE:
      // min packet => start delimiter (1 byte) + length (2 bytes) + api frame identifier (1 byte) + checksum (1 byte)
      if(m_pSerial->available() >= 5) {
        //DEBUG: SerialDebug.println("LOOP_NONE");
        if(m_pSerial->read() != DIGI_START_DELIMITER) break; // ignore data until we get a start
        m_loopLength = m_pSerial->read();
        m_loopLength = m_loopLength << 8;
        m_loopLength |= m_pSerial->read();
        m_loopLengthRemaining = m_loopLength;
        m_loopFrameIdentifier = read(); // frame identifier is part of data
        m_loopState = LOOP_FRAME_DATA;
        //DEBUG: SerialDebug.println("LOOP_NONE Done");
        break;
      }
      break;
            
    case LOOP_FRAME_DATA:
      //DEBUG: SerialDebug.println("LOOP_FRAME_DATA");
      break;
            
    case LOOP_CHECKSUM:
      if(m_pSerial->available() > 0) {
        //DEBUG: SerialDebug.println("LOOP_CHECKSUM");
        m_pSerial->read();
        m_loopState = LOOP_NONE;
        m_loopFrameIdentifier = FRAME_NONE;
        m_loopLength = 0;
      }
      break;
  }
  return min(m_loopLengthRemaining, m_pSerial->available());
}

uint8 DigiWiFi::read() {
  uint8 result = m_pSerial->read();
  m_loopLengthRemaining--;
  if(m_loopLengthRemaining == 0) {
    m_loopState = LOOP_CHECKSUM;
  }
  return result;
}

void DigiWiFi::read(uint8 *pBuffer, uint16 len) {
  // todo: this could be spead up
  while(len > 0) {
    *pBuffer = read();
    pBuffer++;
    len--;
  }
}

void DigiWiFi::beginTx(uint8 frameId, uint32 destIp, uint16 destPort, uint16 srcPort, uint8 protocol, uint8 options, uint16 dataLen) {
  writeBeginDataFrame(FRAME_TX_IPv4, sizeof(TxHeader) + dataLen);
  
  TxHeader header;
  header.frameId = frameId;
  header.destIp = destIp;
  header.destPort = destPort;
  header.srcPort = srcPort;
  header.protocol = protocol;
  header.options = options;
  writeToDataFrame((uint8*)&header, sizeof(TxHeader));
}

void DigiWiFi::tx(uint8 b) {
  writeToDataFrame(b);
}

void DigiWiFi::endTx() {
  writeEndDataFrame();
}

uint8 DigiWiFi::itoa(uint8 n, char *pBuffer, uint8 base) {
  char *p = pBuffer;
  char buf[4]; // max size of n is 255 which is 3 digits + null
  uint8 i = 0;
  
  if(n == 0) {
    *p++ = '0';
    *p++ = '\0';
    return 1;
  }
  
  while (n > 0) {
    buf[i++] = n % base;
    n /= base;
  }
  
  for (; i > 0; i--) {
    *p++ = (char)(buf[i - 1] < 10 
      ? '0' + buf[i - 1] 
      : 'A' + buf[i - 1] - 10);
  }
  *p++ = '\0';
  return p - pBuffer;
}

uint8 DigiWiFi::ipAddrToStr(uint32 ip, char *pBuffer) {
  char *p = pBuffer;
  itoa((ip >> 24) & 0xff, p, 10);
  p += strlen(p);
  *p++ = '.';
  itoa((ip >> 16) & 0xff, p, 10);
  p += strlen(p);
  *p++ = '.';
  itoa((ip >> 8) & 0xff, p, 10);
  p += strlen(p);
  *p++ = '.';
  itoa((ip >> 0) & 0xff, p, 10);
  p += strlen(p);
  return p - pBuffer;
}

void DigiWiFi::writeAtCommandInternal(uint8 apiFrameId, uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen) {
  writeBeginDataFrame(apiFrameId, sizeof(AtCommandHeader) + parameterValueLen);
  
  AtCommandHeader header;
  header.frameId = frameId;
  header.atCommand[0] = atCmd[0];
  header.atCommand[1] = atCmd[1];
  writeToDataFrame((uint8*)&header, sizeof(AtCommandHeader));  
  writeToDataFrame(pParameterValue, parameterValueLen);
  writeEndDataFrame();
}

void DigiWiFi::writeAtCommand(uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen) {
  writeAtCommandInternal(FRAME_AT_Command, frameId, atCmd, pParameterValue, parameterValueLen);
}

void DigiWiFi::writeAtCommandQueue(uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen) {
  writeAtCommandInternal(FRAME_AT_Command_Queue, frameId, atCmd, pParameterValue, parameterValueLen);
}






