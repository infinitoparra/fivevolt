
#ifndef _AppleAccessoryProtocol_h_
#define _AppleAccessoryProtocol_h_

#include <HardwareSerial.h>

#define AAP_BAUD            19200
#define AAP_MIN_PACKET_SIZE 5       /* 2byte header + 1byte length + 1byte mode + 1byte checksum */

#define AAP_MODE_SWITCH              0
#define AAP_MODE_VOICE_RECORDER      1
#define AAP_MODE_SIMPLE_REMOTE       2
#define AAP_MODE_REQUEST_MODE_STATUS 3
#define AAP_MODE_AiR_MODE            4 /* Advanced iPod Remote (AiR) */
#define AAP_MODE_NIKE_PLUS           9

#define AAP_SIMPLE_REMOTE_PLAY_PAUSE     0x00000001
#define AAP_SIMPLE_REMOTE_VOL_UP         0x00000002
#define AAP_SIMPLE_REMOTE_VOL_DOWN       0x00000004
#define AAP_SIMPLE_REMOTE_SKIP_R         0x00000008
#define AAP_SIMPLE_REMOTE_SKIP_L         0x00000010
#define AAP_SIMPLE_REMOTE_NEXT_ALBUM     0x00000020
#define AAP_SIMPLE_REMOTE_PREV_ALBUM     0x00000040
#define AAP_SIMPLE_REMOTE_STOP           0x00000080
#define AAP_SIMPLE_REMOTE_PLAY           0x00000100
#define AAP_SIMPLE_REMOTE_PAUSE          0x00000200
#define AAP_SIMPLE_REMOTE_MUTE           0x00000400
#define AAP_SIMPLE_REMOTE_NEXT_PLAYLIST  0x00002000
#define AAP_SIMPLE_REMOTE_PREV_PLAYLIST  0x00004000
#define AAP_SIMPLE_REMOTE_TOGGLE_SHUFFLE 0x00008000
#define AAP_SIMPLE_REMOTE_TOGGLE_REPEAT  0x00010000
#define AAP_SIMPLE_REMOTE_IPOD_OFF       0x00040000
#define AAP_SIMPLE_REMOTE_IPOD_ON        0x00080000
#define AAP_SIMPLE_REMOTE_NEXT           0x00100000
#define AAP_SIMPLE_REMOTE_PREV           0x00200000
#define AAP_SIMPLE_REMOTE_MENU           0x00400000
#define AAP_SIMPLE_REMOTE_OK             0x00800000
#define AAP_SIMPLE_REMOTE_SCROLL_UP      0x01000000
#define AAP_SIMPLE_REMOTE_SCROLL_DOWN    0x02000000

#pragma pack(push, 1)

typedef struct AAPReqRespHeader_t {
  uint16 header; // 0xff 0x55
  uint8 length;
  uint8 mode;
  // x byte - command
  // 1 byte - checksum 0x100 - ( (sum of all length/mode/command/parameter bytes) & 0xFF)
} AAPReqRespHeader;

#pragma pack(pop)

class AppleAccessoryProtocol {
public:
  AppleAccessoryProtocol(HardwareSerial &serial);
  void begin();
  void loop();
  
protected:
  void processBuffer();
  void processModeSwitch();
  void processSimpleRemote();
  void processAiR();
  void processRequestModeStatus();
  void writeCurrentMode();

  virtual void setMode(uint8 mode);
  virtual void simpleRemoteButtonReleased() {}
  virtual void simpleRemoteButtonPressed(uint32 buttons) {}
  virtual void airRemoteCommand(uint8 cmd) {}
  
  uint8 calculateChecksum(uint8* pBuffer, uint8 len);

  HardwareSerial *m_pSerial;
  uint8 m_buffer[255];
  uint8 m_bufferIdx;
  uint8 m_currentMode;
};

#endif
