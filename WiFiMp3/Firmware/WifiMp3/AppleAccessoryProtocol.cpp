
#include "global.h"
#include "AppleAccessoryProtocol.h"

AppleAccessoryProtocol::AppleAccessoryProtocol(HardwareSerial &serial) {
  m_pSerial = &serial;
}

void AppleAccessoryProtocol::begin() {
  m_pSerial->begin(AAP_BAUD);
  m_bufferIdx = 0;
  m_currentMode = AAP_MODE_AiR_MODE;
}

void AppleAccessoryProtocol::loop() {
  while(m_pSerial->available() > 0) {
    if(m_bufferIdx == 0) {
      m_buffer[m_bufferIdx++] = m_pSerial->read();
      if(m_buffer[0] != 0xff) {
        m_bufferIdx = 0;
        continue;
      }
    }
    
    if(m_bufferIdx == 1) {
      m_buffer[m_bufferIdx++] = m_pSerial->read();
      if(m_buffer[1] == 0xff) { // might be the actual start of the header
        m_bufferIdx = 1; // backup the index up 1 to re-read byte 2
        continue;
      }
      if(m_buffer[1] != 0x55) {
        m_bufferIdx = 0;
        continue;
      }
    }
    
    if(m_bufferIdx == 2 || m_bufferIdx == 3) { // length || mode
      m_buffer[m_bufferIdx++] = m_pSerial->read();
    }
    
    if(m_bufferIdx >= 4) {
      m_buffer[m_bufferIdx++] = m_pSerial->read();
      if(m_bufferIdx == m_buffer[2] + 4 /* header(2) + length(1) + checksum(1) */) {
        processBuffer();
        m_bufferIdx = 0;
      }
    }
  }
}

void AppleAccessoryProtocol::processBuffer() {
  #ifdef IPOD_DEBUG
    SerialDebug.print("ipod: ");
    for(uint8 i=0; i<m_bufferIdx; i++) {
      SerialDebug.print(m_buffer[i], 16);
      SerialDebug.print(" ");
    }
    SerialDebug.println();
  #endif
  
  uint8 mode = m_buffer[3];
  switch(mode) { // mode
    case AAP_MODE_SWITCH:
      processModeSwitch();
      break;
    case AAP_MODE_SIMPLE_REMOTE:
      processSimpleRemote();
      break;
    case AAP_MODE_AiR_MODE:
      processAiR();
      break;
    case AAP_MODE_REQUEST_MODE_STATUS:
      processRequestModeStatus();
      break;
    default:
      SerialDebug.print("iPod Unhandled mode: ");
      SerialDebug.println(mode);
      break;
  }
}

void AppleAccessoryProtocol::processRequestModeStatus() {
  // todo: what should we do?
  SerialDebug.println("processRequestModeStatus");
}

void AppleAccessoryProtocol::processSimpleRemote() {
  uint8 len = m_buffer[2] - 1;
  if(len < 2) {
    SerialDebug.println("iPod invalid simple remote command (too short)");
    return;
  }
  
  if(m_buffer[4] != 0x00) {
    SerialDebug.println("iPod invalid simple remote command");
    return;
  }
  
  if(len == 2 && m_buffer[5] == 0x00) {
    simpleRemoteButtonReleased();
    return;
  }
  
  if(len == 2) {
    simpleRemoteButtonPressed(m_buffer[5]);
    return;
  }
  
  if(len == 3) {
    simpleRemoteButtonPressed(((uint32)m_buffer[6] << 8) | m_buffer[5]);
    return;
  }
  
  if(len == 4) {
    simpleRemoteButtonPressed(((uint32)m_buffer[7] << 16) | ((uint32)m_buffer[6] << 8) | m_buffer[5]);
    return;
  }
  
  if(len == 5) {
    simpleRemoteButtonPressed(((uint32)m_buffer[8] << 24) | ((uint32)m_buffer[7] << 16) | ((uint32)m_buffer[6] << 8) | m_buffer[5]);
    return;
  }

  SerialDebug.println("iPod invalid simple remote command (too long)");
}

void AppleAccessoryProtocol::processAiR() {
  uint8 len = m_buffer[2] - 1;
  if(len != 2) {
    SerialDebug.println("iPod invalid AiR command (bad length)");
    return;
  }
  
  if(m_buffer[4] != 0x00) {
    SerialDebug.println("iPod invalid AiR command");
    return;
  }

  airRemoteCommand(m_buffer[5]);
}

void AppleAccessoryProtocol::processModeSwitch() {
  if(m_buffer[4] == 0x01) {
    setMode(m_buffer[5]);
  }
  else if(m_buffer[4] == 0x05) {
    setMode(AAP_MODE_AiR_MODE);
  }
  else if(m_buffer[4] == 0x06) {
    setMode(AAP_MODE_SIMPLE_REMOTE);
  }
    
  // get current mode
  else if(m_buffer[4] == 0x03) {
    writeCurrentMode();
  }
}

void AppleAccessoryProtocol::writeCurrentMode() {
  m_buffer[0] = 0xff;
  m_buffer[1] = 0x55;
  m_buffer[2] = 0x03; // length
  m_buffer[3] = AAP_MODE_SWITCH;
  m_buffer[4] = 0x04;
  m_buffer[5] = m_currentMode;
  m_buffer[6] = calculateChecksum(m_buffer, 6);
  m_pSerial->write(m_buffer, 7);
  #ifdef IPOD_DEBUG
    SerialDebug.print("sending current mode: ");
    SerialDebug.println(m_currentMode);
  #endif
}

// 0x100 - ( (sum of all length/mode/command/parameter bytes) & 0xFF)
uint8 AppleAccessoryProtocol::calculateChecksum(uint8* pBuffer, uint8 len) {
  uint8 checksum = 0;
  for(uint8 i=0; i<len; i++) {
    checksum += pBuffer[i];
  }
  return 0x100 - checksum;
}

void AppleAccessoryProtocol::setMode(uint8 mode) {
  m_currentMode = mode;
  writeCurrentMode();
}

