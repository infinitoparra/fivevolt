
#ifndef _digiwifi_h_
#define _digiwifi_h_

#include "HardwareSerial.h"
#include <string.h>

#define WIFI_ENCRYPT_OPEN 0
#define WIFI_ENCRYPT_WPA  1
#define WIFI_ENCRYPT_WPA2 2

#define STATUS_HardwareReset      0
#define STATUS_Watchdog_Reset     1
#define STATUS_Joined             2
#define STATUS_NoLongerJoinedToAp 3
#define STATUS_IpConfigError      4

#define ADDR_MODE_Dhcp   0
#define ADDR_MODE_Static 1

#define ASSOC_STATUS_Connected         0x00
#define ASSOC_STATUS_InitInProgress    0x01
#define ASSOC_STATUS_SsidNotFound      0x22
#define ASSOC_STATUS_SsidNotConfigured 0x23
#define ASSOC_STATUS_JoinFailed        0x27
#define ASSOC_STATUS_Scanning          0xff
#define ASSOC_STATUS_WaitingForIp      0x41
#define ASSOC_STATUS_SocketSetup       0x42

#define FRAME_Tx64_Request        0x00
#define FRAME_AT_Command          0x08
#define FRAME_AT_Command_Queue    0x09
#define FRAME_Remote_Cmd_Request  0x07
#define FRAME_TX_IPv4             0x20
#define FRAME_Rx64_Indicator      0x80
#define FRAME_AT_Cmd_Response     0x88
#define FRAME_TX_Status           0x89
#define FRAME_Modem_Status        0x8A
#define FRAME_IO_Data_Sample_Rx_Indicator 0x82
#define FRAME_Remote_Cmd_Response 0x87
#define FRAME_RX_IPv4             0xB0
#define FRAME_NONE                0xFF

#define PROTOCOL_UDP 0
#define PROTOCOL_TCP 1

#define TX_OPTION_TERMINATE_SOCKET 1
#define TX_OPTION_LEAVE_OPEN       0

#define ATCMD_DestAddrLow           "DL"
#define ATCMD_MyIpAddr              "MY"
#define ATCMD_MyAddrMask            "MK"
#define ATCMD_GatewayIpAddr         "GW"
#define ATCMD_SerialNumHigh         "SH"
#define ATCMD_SerialNumLow          "SL"
#define ATCMD_NodeIdentifier        "NI"
#define ATCMD_DestPort              "DE"
#define ATCMD_SerialServicePort     "C0"
#define ATCMD_DeviceTypeIdentifier  "DD"
#define ATCMD_MaxRFPayloadBytes     "NP"
#define ATCMD_SSID                  "ID"
#define ATCMD_NetworkType           "AH"
#define ATCMD_IpProtocol            "IP"
#define ATCMD_IpAddrMode            "MA"
#define ATCMD_TcpTimeout            "TM"
#define ATCMD_EncryptionEnabled     "EE"  /* 0 – No security, 1 – WPA, 2 – WPA2 */
#define ATCMD_SecurityKey           "PK"
#define ATCMD_PowerLevel            "PL"
#define ATCMD_Channel               "CH"
#define ATCMD_BitRate               "BR"
#define ATCMD_ApiEnabled            "AP"
#define ATCMD_InterfaceDataRate     "BD"
#define ATCMD_SerialParity          "NB"
#define ATCMD_StopBits              "SB"
#define ATCMD_PacketizationTimeout  "RO"
#define ATCMD_FlowControlThreshold  "FT"
#define ATCMD_DIO7Config            "D7"
#define ATCMD_DIO6Config            "D6"
#define ATCMD_IOSampleRate          "IR"
#define ATCMD_IODigitalChangeDetect "IC"
#define ATCMD_SampleFromSleepRate   "IF"
#define ATCMD_DIO10Config           "P0"
#define ATCMD_DIO11Config           "P1"
#define ATCMD_DIO12Config           "P2"
#define ATCMD_DIO0Config            "D0"
#define ATCMD_DIO1Config            "D1"
#define ATCMD_DIO2Config            "D2"
#define ATCMD_DIO3Config            "D3"
#define ATCMD_DIO4Config            "D4"
#define ATCMD_DIO5Config            "D5"
#define ATCMD_DIO8Config            "D8"
#define ATCMD_DIO9Config            "D9"
#define ATCMD_AssocLedBlinkTime     "LT"
#define ATCMD_PullUpResisor         "PR"
#define ATCMD_PullDirection         "PD"
#define ATCMD_AnalogVoltageRef      "AV"
#define ATCMD_PMM0DutyCycle         "M0"
#define ATCMD_PMM1DutyCycle         "M1"
#define ATCMD_FirmwareVersion       "VR"
#define ATCMD_HardwareVersion       "HV"
#define ATCMD_AssocIndicator        "AI"
#define ATCMD_ActiveScan            "AS"
#define ATCMD_Temperature           "TP"
#define ATCMD_ConfigCode            "CK"
#define ATCMD_SupplyVoltage         "%V"
#define ATCMD_RSSI                  "DB"
#define ATCMD_CommandModeTimeout    "CT"
#define ATCMD_ExitCommandMode       "CN"
#define ATCMD_GuardTimes            "GT"
#define ATCMD_CommandModeCharacter  "CC"
#define ATCMD_SleepMode             "SM"
#define ATCMD_SleepPeriod           "SP"
#define ATCMD_SleepOptions          "SO"
#define ATCMD_WakeHost              "WH"
#define ATCMD_WakeTime              "ST"
#define ATCMD_ApplyChanges          "AC"
#define ATCMD_Write                 "WR"
#define ATCMD_RestoreDefaults       "RE"
#define ATCMD_SoftwareReset         "FR"
#define ATCMD_NetworkReset          "NR"
#define ATCMD_ForceSample           "IS"

#pragma pack(push, 1)

typedef struct ModemStatus_t {
  uint8 status;
} ModemStatus;

typedef struct TxStatus_t {
  uint8 frameId;
  uint8 status;
} TxStatus;

typedef struct TxHeader_t {
  uint8 frameId;
  uint32 destIp;
  uint16 destPort;
  uint16 srcPort;
  uint8 protocol;
  uint8 options;
} TxHeader;

typedef struct RxHeader_t {
  uint8 frameId;
  uint32 srcIp;
  uint16 destPort;
  uint16 srcPort;
  uint8 protocol;
  uint8 status;
} RxHeader;

typedef struct AtCommandHeader_t {
  uint8 frameId;
  char atCommand[2];
} AtCommandHeader;

typedef struct AtCommandResponseHeader_t {
  uint8 frameId;
  char atCommand[2];
  uint8 status;
} AtCommandResponseHeader;

#pragma pack(pop)

class DigiWiFi {
public:
  DigiWiFi(HardwareSerial &serial, uint8 cts, uint8 rts);
  void begin(uint32 baud);
  uint16 available();
  uint8 read();
  void read(uint8 *pBuffer, uint16 len);
  uint8 getFrameIdentifier() { return m_loopFrameIdentifier; }
  void skipFrame();
  
  // pBuffer - should be at least 17 bytes (111.111.111.111\0)
  static uint8 ipAddrToStr(uint32 ip, char *pBuffer);
  
  // frameId - Set to a value that will be passed back in the Tx Status frame. 0 disables the Tx Status frame.
  // protocol - PROTOCOL_UDP or PROTOCOL_TCP
  // options - TX_OPTION_TERMINATE_SOCKET or TX_OPTION_LEAVE_OPEN
  void beginTx(uint8 frameId, uint32 destIp, uint16 destPort, uint16 srcPort, uint8 protocol, uint8 options, uint16 dataLen);
  void tx(uint8 b);
  void endTx();

  void setSsid(const char* ssid) { writeAtCommand(0, ATCMD_SSID, (uint8*)ssid, strlen(ssid)); }
  void setEncryption(uint8 encryption) { writeAtCommand(0, ATCMD_EncryptionEnabled, &encryption, 1); }
  void setSecurityKey(const char* key) { writeAtCommand(0, ATCMD_SecurityKey, (uint8*)key, strlen(key)); }
  void setIpAddressMode(uint8 mode) { writeAtCommand(0, ATCMD_IpAddrMode, &mode, 1); }
  void setIp(const char* ip) { writeAtCommand(0, ATCMD_MyIpAddr, (uint8*)ip, strlen(ip)); }
  void setSubnetMask(const char* subnetMask) { writeAtCommand(0, ATCMD_MyAddrMask, (uint8*)subnetMask, strlen(subnetMask)); }
  void setGateway(const char* gateway) { writeAtCommand(0, ATCMD_GatewayIpAddr, (uint8*)gateway, strlen(gateway)); }
  void applyChanges() { writeAtCommand(0, ATCMD_ApplyChanges, NULL, 0); }
  void queryAssociationStatus() { writeAtCommand(0, ATCMD_AssocIndicator, NULL, 0); }

  void writeAtQuery(uint8 frameId, const char* atCmd) { writeAtCommand(frameId, atCmd, NULL, 0); }
  void writeAtCommand(uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen);
  void writeAtCommandQueue(uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen);
  
  void debug(HardwareSerial &serial);
  
protected:
  static uint8 itoa(uint8 n, char *p, uint8 base);

  void writeAtCommandInternal(uint8 apiFrameId, uint8 frameId, const char* atCmd, uint8 *pParameterValue, uint16 parameterValueLen);

  // len - length of payload not including api frame id
  void writeBeginDataFrame(uint8 apiFrameId, uint16 len);
  void writeToDataFrame(uint8 ch);
  void writeToDataFrame(uint8 *pBuffer, uint16 len);
  void writeEndDataFrame();

  HardwareSerial *m_pSerial;
  uint8 m_cts;
  uint8 m_rts;
  uint8 m_writeChecksum;
  uint8 m_loopState;
  uint16 m_loopLength; // length of frame data
  uint16 m_loopLengthRemaining; // data remaining in current frame
  uint8 m_loopFrameIdentifier;
};

#endif

