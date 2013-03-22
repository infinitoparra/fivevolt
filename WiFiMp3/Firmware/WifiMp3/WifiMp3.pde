
#include <WProgram.h>
#include "global.h"
#include "SdCard.h"
#include "Nokia6100Lcd.h"
#include "DigiWiFi.h"
#include "Mp3VS10xx.h"
#include "RingBuffer.h"
#include "Ipod.h"
#include <string.h>

SoftwareSPI g_lcdSpi(LCD_SPI_SCK_PIN, LCD_SPI_CS_PIN, LCD_SPI_MISO_PIN, LCD_SPI_MOSI_PIN);
Nokia6100Lcd g_lcd(g_lcdSpi, LCD_RESET);

Ipod g_ipod(SerialIpod);

DigiWiFi g_wifi(SerialWifi, WIFI_USART_CTS, WIFI_USART_RTS);
RxHeader g_rxHeader;
uint8 g_haveReadHeader = false;
uint8 g_wifiNextFrameId = 1;
uint32 g_wifiLastAssociationStatusCheck = 0;
AtCommandResponseHeader g_lastAtCmdRespHeader;
uint8 g_wifiAssociationStatus = 0xff;

#define g_loopSerialDebugBufferSize 50
char g_loopSerialDebugBuffer[g_loopSerialDebugBufferSize];
char *g_pLoopSerialDebugBufferPos;

HardwareSPI g_mp3spi(MP3_SPI);
Mp3VS10xx g_mp3(g_mp3spi, MP3_XCS, MP3_XDCS, MP3_DREQ, MP3_RESET);

HardwareSPI g_sdspi(SD_SPI);
Sd2Card g_sdcard;
SdVolume g_sdvolume;
SdFile g_sdroot;

SdFile g_currentSongFile;
RingBuffer g_currentSongBuffer(Mp3VS10xx_FifoSize);

WifiMp3Settings g_settings;

void setupLcd();
void setupWifi();
void loopWifi();
void setupSerialDebug();
void loopSerialDebug();
void setupMp3();
void loopMp3();
void serialDebugProcessLine(const char* line);
uint8 getNextWifiFrameId();
void setupSdCard();
void readSettings();
void play(const char* filename);
void setupIpod();
void loopIpod();

void setup() {
  setupSerialDebug();
  //setupLcd();
  setupSdCard();
  readSettings();
  setupMp3();
  setupIpod();
  //setupWifi();
  
  Serial2.begin(AAP_BAUD);
  
  RingBuffer buffer(8192);
  while(true) {
    while(Serial3.available() > 0) {
      uint8 z = Serial3.read();
      //SerialDebug.write(0x01);
      //SerialDebug.write(z);
      buffer.insert(0x01);
      buffer.insert(z);
    }
    
    while(Serial2.available() > 0) {
      uint8 z = Serial2.read();
      //SerialDebug.write(0x02);
      //SerialDebug.write(z);
      buffer.insert(0x02);
      buffer.insert(z);
    }
    
    if(SerialDebug.available() > 0) {
      uint8 z = SerialDebug.read();
      if(z == 's') {
        while(buffer.fullCount() > 0) {
          SerialDebug.write(buffer.remove());
        }
        buffer.clear();
      }
    }
  }
  
  play("00000001.mp3");
  
  SerialDebug.print(">");
}

void loop() {
  //loopWifi();
  loopSerialDebug();
  loopMp3();
  loopIpod();
}

void setupIpod() {
  SerialDebug.print("Init iPod...");
  g_ipod.begin();
  SerialDebug.println("OK");
}

void loopIpod() {
  g_ipod.loop();
}

void setupLcd() {
  SerialDebug.print("Init LCD...");
  g_lcd.begin();
  SerialDebug.println("OK");
}

void setupMp3() {
  SerialDebug.print("Init mp3...");
  g_mp3.begin();
  SerialDebug.println("OK");
}

void loopMp3() {
  if(g_currentSongFile.isOpen()) {
    while(g_currentSongBuffer.emptyCount() >= Mp3VS10xx_MinDreqSize) {
      g_currentSongFile.read(&g_currentSongBuffer, Mp3VS10xx_MinDreqSize);
    }
  }
  
  while(!g_mp3.isBufferFull() && g_currentSongBuffer.fullCount() > 0) {
    g_mp3.send(&g_currentSongBuffer, min(Mp3VS10xx_MinDreqSize, g_currentSongBuffer.fullCount()));
  }
}

void play(const char* filename) {
  g_currentSongBuffer.clear();
  if(g_currentSongFile.isOpen()) {
    g_currentSongFile.close();
  }
  if(!g_currentSongFile.open(&g_sdroot, filename, O_READ)) {
    SerialDebug.print("FAILED to open ");
    SerialDebug.println(filename);
  }
}

void setupSdCard() {
  SerialDebug.print("Init sdcard...");
  // todo: g_sdspi.begin(SPI_281_250KHZ, MSBFIRST, SPI_MODE_0);
  g_sdspi.begin(SPI_4_5MHZ, MSBFIRST, SPI_MODE_0);
  if(!g_sdcard.init(&g_sdspi, SD_SPI_CS)) {
    SerialDebug.println("FAILED SDCARD");
    return;
  }
  
  SerialDebug.print(".");
  // todo: g_sdspi.begin(SPI_4_5MHZ, MSBFIRST, SPI_MODE_0);
  delay(100);
  
  SerialDebug.print(".");
  if(!g_sdvolume.init(&g_sdcard)) {
    SerialDebug.println("FAILED SDVOL");
    return;
  }
  
  SerialDebug.print(".");
  if(!g_sdroot.openRoot(&g_sdvolume)) {
    SerialDebug.println("FAILED SDROOT");
    return;
  }
  
  SerialDebug.println("OK");
}

void readSettings() {
  SdFile settingsFile;
  
  SerialDebug.print("ReadSettings...");
  if(!settingsFile.open(&g_sdroot, "settings.bin", O_READ)) {
    SerialDebug.println("FAILED Settings");
    return;
  }
    
  SerialDebug.print(".");
  settingsFile.read(&g_settings, sizeof(WifiMp3Settings));  
  #ifdef SD_DEBUG
    char buffer[20];
  
    SerialDebug.print("ssid: ");
    SerialDebug.println(g_settings.wifi.ssid);

    SerialDebug.print("encryption: ");
    SerialDebug.println(g_settings.wifi.encryption);

    SerialDebug.print("securityKey: ");
    SerialDebug.println(g_settings.wifi.securityKey);

    SerialDebug.print("ipAddrMode: ");
    SerialDebug.println(g_settings.wifi.ipAddrMode);

    SerialDebug.print("ipAddr: ");
    DigiWiFi::ipAddrToStr(g_settings.wifi.ipAddr, buffer);
    SerialDebug.println(buffer);

    SerialDebug.print("subnetMask: ");
    DigiWiFi::ipAddrToStr(g_settings.wifi.subnetMask, buffer);
    SerialDebug.println(buffer);

    SerialDebug.print("gateway: ");
    DigiWiFi::ipAddrToStr(g_settings.wifi.gateway, buffer);
    SerialDebug.println(buffer);
  #endif
  
  SerialDebug.println("OK");
}

void setupWifi() {  
  SerialDebug.print("Init wifi...");
  
  char buffer[16]; // "255.255.255.255" NULL
  
  g_wifi.begin(WIFI_BAUD);
  g_wifi.setSsid(g_settings.wifi.ssid);  // "WifiMonkey"
  g_wifi.setEncryption(g_settings.wifi.encryption); // WIFI_ENCRYPT_WPA2
  g_wifi.setSecurityKey(g_settings.wifi.securityKey); // "7037727884"
  g_wifi.setIpAddressMode(g_settings.wifi.ipAddrMode); // ADDR_MODE_Static
  DigiWiFi::ipAddrToStr(g_settings.wifi.ipAddr, buffer);
  g_wifi.setIp(buffer); // "192.168.0.239"
  DigiWiFi::ipAddrToStr(g_settings.wifi.subnetMask, buffer);
  g_wifi.setSubnetMask(buffer); // "255.255.255.0"
  DigiWiFi::ipAddrToStr(g_settings.wifi.subnetMask, buffer);
  g_wifi.setGateway(buffer); // "192.168.0.1"
  g_wifi.applyChanges();
  SerialDebug.println("OK");
}

void loopWifi() {
  if(millis() - g_wifiLastAssociationStatusCheck > 5000) {
    g_wifi.queryAssociationStatus();
    g_wifiLastAssociationStatusCheck = millis();
  }
  
  uint16 avail = g_wifi.available();
  if(avail == 0) {
    if(g_haveReadHeader && g_wifi.getFrameIdentifier() == FRAME_NONE) {
      g_haveReadHeader = false;
    }
    return;
  }
  #ifdef WIFI_DEBUG
    SerialDebug.print("frameid: ");
    SerialDebug.println(g_wifi.getFrameIdentifier());
  #endif          
  switch(g_wifi.getFrameIdentifier()) {
    case FRAME_Modem_Status:
      #ifdef WIFI_DEBUG
        SerialDebug.println("FRAME_Modem_Status");
      #endif          
      if(avail >= sizeof(ModemStatus)) {
        ModemStatus status;
        g_wifi.read((uint8*)&status, sizeof(ModemStatus));
        #ifdef WIFI_DEBUG
          SerialDebug.print("wifi modem status: ");
          SerialDebug.println(status.status);
        #endif
        g_wifi.skipFrame();
      }
      break;
    
    case FRAME_TX_Status:
      #ifdef WIFI_DEBUG
        SerialDebug.println("FRAME_TX_Status");
      #endif
      if(avail >= sizeof(TxStatus)) {
        TxStatus status;
        g_wifi.read((uint8*)&status, sizeof(TxStatus));
        #ifdef WIFI_DEBUG
          SerialDebug.print("wifi tx status: ");
          SerialDebug.print(status.frameId);
          SerialDebug.print(", ");
          SerialDebug.println(status.status);
        #endif
        g_wifi.skipFrame();
      }
      break;
      
    case FRAME_AT_Cmd_Response:
      #ifdef WIFI_DEBUG
        SerialDebug.println("FRAME_AT_Cmd_Response");
      #endif  
      if(!g_haveReadHeader) {
        if(avail >= sizeof(AtCommandResponseHeader)) {
          g_wifi.read((uint8*)&g_lastAtCmdRespHeader, sizeof(AtCommandResponseHeader));
          g_haveReadHeader = true;
          #ifdef WIFI_DEBUG
            SerialDebug.print("wifi AT resp: ");
            SerialDebug.print(g_lastAtCmdRespHeader.frameId);
            SerialDebug.print(", ");
            SerialDebug.print(g_lastAtCmdRespHeader.atCommand[0]);
            SerialDebug.print(g_lastAtCmdRespHeader.atCommand[1]);
            SerialDebug.print(", ");
            SerialDebug.println(g_lastAtCmdRespHeader.status);
          #endif
        }
      } else {
        if(strncmp(g_lastAtCmdRespHeader.atCommand, ATCMD_AssocIndicator, 2) == 0) {
          if(g_wifi.available() > 0) {
            g_wifiAssociationStatus = g_wifi.read();
            #ifdef WIFI_DEBUG
              SerialDebug.print("Association Status: ");
              SerialDebug.println(g_wifiAssociationStatus, 16);
            #endif
          }
        } else {        
          #ifdef WIFI_DEBUG
            while(g_wifi.available() > 0) {
              uint8 ch = g_wifi.read();
              if(isPrintable(ch)) {
                SerialDebug.print((char)ch);
              } else {
                SerialDebug.print('(');
                SerialDebug.print(ch, 16);
                SerialDebug.print(')');
              }
            }
            if(g_wifi.available() == 0) {
              SerialDebug.println();
            }
          #else  
            g_wifi.skipFrame();
          #endif
        }
      }
      break;
      
    case FRAME_RX_IPv4:
      #ifdef WIFI_DEBUG
        SerialDebug.println("FRAME_RX_IPv4");
      #endif
      if(!g_haveReadHeader) {
        if(avail >= sizeof(RxHeader)) {
          char ipAddress[17];
          g_wifi.read((uint8*)&g_rxHeader, sizeof(RxHeader));
          g_wifi.ipAddrToStr(g_rxHeader.srcIp, ipAddress);
          g_haveReadHeader = true;
          #ifdef WIFI_DEBUG
            SerialDebug.print("wifi rx: ");
            SerialDebug.print(g_rxHeader.frameId);
            SerialDebug.print(", ");
            SerialDebug.print(g_rxHeader.srcIp);
            SerialDebug.print(", ");
            SerialDebug.print(g_rxHeader.destPort);
            SerialDebug.print(", ");
            SerialDebug.print(g_rxHeader.srcPort);
            SerialDebug.print(", ");
            SerialDebug.print(g_rxHeader.protocol);
            SerialDebug.print(", ");
            SerialDebug.println(g_rxHeader.status);
          #endif
        }
      } else {
        #ifdef WIFI_DEBUG
          while(g_wifi.available() > 0) {
            uint8 ch = g_wifi.read();
            if(isPrintable(ch)) {
              SerialDebug.print((char)ch);
            } else {
              SerialDebug.print('(');
              SerialDebug.print(ch, 16);
              SerialDebug.print(')');
            }
          }
          if(g_wifi.available() == 0) {
            SerialDebug.println();
          }
        #else  
          g_wifi.skipFrame();
        #endif
      }
      break;
    
    default:
      #ifdef WIFI_DEBUG
        SerialDebug.print("wifi unhandled: ");
        SerialDebug.println(g_wifi.getFrameIdentifier());
      #endif
      g_wifi.skipFrame();
      break;
  }
}

void setupSerialDebug() {
  SerialDebug.begin(DEBUG_BAUD);
  SerialDebug.print("Init SerialDebug... ");

  g_pLoopSerialDebugBufferPos = g_loopSerialDebugBuffer;
  *g_pLoopSerialDebugBufferPos = '\0';
  
  SerialDebug.println("OK");
}

uint8 getNextWifiFrameId() {
  uint8 result = g_wifiNextFrameId;
  g_wifiNextFrameId++;
  // skip 0 because that tells the wifi to ignore the frame id (see transmit request)
  if(g_wifiNextFrameId == 0) {
    g_wifiNextFrameId++;
  }
  return result;
}

void loopSerialDebug() {
  while(SerialDebug.available() > 0) {
    char ch = SerialDebug.read();
    SerialDebug.print(ch);
    if(ch == '\n') {
      // ignore
    }
    else if(ch == '\r' || (g_pLoopSerialDebugBufferPos - g_loopSerialDebugBuffer >= g_loopSerialDebugBufferSize - 2)) {
      *g_pLoopSerialDebugBufferPos++ = '\0';
      serialDebugProcessLine(g_loopSerialDebugBuffer);
      g_pLoopSerialDebugBufferPos = g_loopSerialDebugBuffer;
    } else {
      *g_pLoopSerialDebugBufferPos++ = ch;
    }
  }
}

void serialDebugProcessLine(const char* line) {
  if(!strcmp(line, "wifiip")) {
    g_wifi.writeAtQuery(getNextWifiFrameId(), ATCMD_MyIpAddr);
    SerialDebug.println("OK");
  } else if(!strncmp(line, "wifiat ", 7)) {
    char atCmd[3];
    atCmd[0] = line[7];
    atCmd[1] = line[8];
    atCmd[2] = '\0';
    g_wifi.writeAtQuery(getNextWifiFrameId(), atCmd);
    SerialDebug.println("OK");
  } else if(!strncmp(line, "wifissid ", 9)) {
    char ssid[20];
    strcpy(ssid, line+9);
    g_wifi.writeAtCommand(getNextWifiFrameId(), ATCMD_SSID, (uint8*)ssid, strlen(ssid));
    SerialDebug.print("OK ");
    SerialDebug.println(ssid);
  } else if(!strncmp(line, "wifienc ", 8)) {
    uint8 enc = *(line+8) - '0';
    g_wifi.writeAtCommand(getNextWifiFrameId(), ATCMD_EncryptionEnabled, &enc, 1);
    SerialDebug.print("OK ");
    SerialDebug.println(enc);
  } else if(!strncmp(line, "wifikey ", 8)) {
    char key[20];
    strcpy(key, line+8);
    g_wifi.writeAtCommand(getNextWifiFrameId(), ATCMD_SecurityKey, (uint8*)key, strlen(key));
    SerialDebug.print("OK ");
    SerialDebug.println(key);
  } else if(!strncmp(line, "play ", 5)) {
    char filename[20];
    strcpy(filename, line+5);
    play(filename);
    SerialDebug.print("OK ");
    SerialDebug.println(filename);
  } else if(!strcmp(line, "mp3debug")) {
    SerialDebug.println("OK");
    g_mp3.debug();
  } else {
    if(line[0] != '\0') {
      SerialDebug.println("Unknown Command");
    }
  }
  SerialDebug.print(">");
}

void Ipod::simpleRemoteButtonReleased() {
  SerialDebug.println("simpleRemoteButtonReleased");
}

void Ipod::simpleRemoteButtonPressed(uint32 buttons) {
  SerialDebug.print("simpleRemoteButtonPressed: 0x");
  SerialDebug.println(buttons, 16);
}

void Ipod::airRemoteCommand(uint8 cmd) {
  SerialDebug.print("airRemoteCommand: 0x");
  SerialDebug.println(cmd, 16);
}


