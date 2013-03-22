
#include "PWMInput.h"
#include <pins_arduino.h>

#define RISING_EDGE 1
#define FALLING_EDGE 0
#define MINONWIDTH 950
#define MAXONWIDTH 2075
#define MINOFFWIDTH 12000
#define MAXOFFWIDTH 24000

volatile byte* g_portToPcmask[] = {
  &PCMSK0,
  &PCMSK1,
  &PCMSK2
};

volatile static byte g_PCintLast[3];

volatile static PWMInput* g_pins[24]; 

// gets the position of a single bit within a byte
byte bitToInt(byte b) {
  if(b==0x01) return 0;
  if(b==0x02) return 1;
  if(b==0x04) return 2;
  if(b==0x08) return 3;
  if(b==0x10) return 4;
  if(b==0x20) return 5;
  if(b==0x40) return 6;
  if(b==0x80) return 7;
  return 0;
}

// Attaches PCINT to Arduino Pin
void PWMInput::attach(byte pin) {
  _pin = pin;
  pinMode(_pin, INPUT);
  _edge = FALLING_EDGE;

  byte bit = digitalPinToBitMask(pin);
  byte port = digitalPinToPort(pin);
  byte slot;
  volatile byte *pcmask;

  // map pin to PCIR register
  if (port == NOT_A_PORT) {
    return;
  } 
  else {
    port -= 2;
    pcmask = g_portToPcmask[port];
  }

  // set the mask
  *pcmask |= bit;

  // enable the interrupt
  PCICR |= 0x01 << port;

  g_pins[port * 8 + bitToInt(bit)] = this;
}

// Detaches PCINT from Arduino Pin
void PWMInput::detach() {
  byte bit = digitalPinToBitMask(_pin);
  byte port = digitalPinToPort(_pin);
  volatile byte *pcmask;

  // map pin to PCIR register
  if (port == NOT_A_PORT) {
    return;
  } 
  else {
    port -= 2;
    pcmask = g_portToPcmask[port];
  }

  // disable the mask.
  *pcmask &= ~bit;

  // if that's the last one, disable the interrupt.
  if (*pcmask == 0) {
    PCICR &= ~(0x01 << port);
  }

  g_pins[port * 8 + bitToInt(bit)] = NULL;
}

// Calculate PWM pulse width of receiver data
// If invalid PWM measured, use last known good time
unsigned int PWMInput::read() {
  uint16_t data;
  byte oldSREG;

  oldSREG = SREG;
  cli();
  data = _lastGoodWidth;
  SREG = oldSREG;  

  return data;
}

unsigned long lastPrint = 0;

// ISR which records time of rising or falling edge of signal
void PWMInput::measurePulseWidthISR(byte port) {
  byte bit;
  byte curr;
  byte mask;
  byte pin;
  uint32_t currentTime;
  uint32_t time;

  // get the pin states for the indicated port.
  curr = *portInputRegister(port+2);
  mask = curr ^ g_PCintLast[port];
  g_PCintLast[port] = curr;

  // mask is pins that have changed. screen out non pcint pins.
  if ((mask &= *g_portToPcmask[port]) == 0) {
    return;
  }
  currentTime = micros();

  // mask is pcint pins that have changed.
  for (byte i=0; i < 8; i++) {
    bit = 0x01 << i;
    if (bit & mask) {
      pin = port * 8 + i;
      volatile PWMInput* pinData = g_pins[pin];
      if(pinData == NULL) {
        continue;
      }

      // for each pin changed, record time of change
      if (bit & g_PCintLast[port]) {
        time = currentTime - pinData->_fallTime;
        pinData->_riseTime = currentTime;        
        if ((time >= MINOFFWIDTH) && (time <= MAXOFFWIDTH))
          pinData->_edge = RISING_EDGE;
        else
          pinData->_edge == FALLING_EDGE; // invalid rising edge detected
      }
      else {
        time = currentTime - pinData->_riseTime;
        pinData->_fallTime = currentTime;
        if ((time >= MINONWIDTH) && (time <= MAXONWIDTH) && (pinData->_edge == RISING_EDGE)) {
          pinData->_lastGoodWidth = time;
          pinData->_edge = FALLING_EDGE;
        } 
      }
    }
  }
}

SIGNAL(PCINT0_vect) {
  PWMInput::measurePulseWidthISR(0);
}
SIGNAL(PCINT1_vect) {
  PWMInput::measurePulseWidthISR(1);
}
SIGNAL(PCINT2_vect) {
  PWMInput::measurePulseWidthISR(2);
}










