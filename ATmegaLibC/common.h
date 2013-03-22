
#ifndef _common_h_
#define _common_h_

#include <inttypes.h>
#include <stdlib.h>

#ifndef bool
	#define bool uint8_t
#endif
#ifndef true
	#define true 1
#endif
#ifndef false
	#define false 0
#endif

#if defined(__AVR_ATmega8U2__) || defined(__AVR_ATmega32U2__) || defined(__AVR_ATmega32U4__)
	#define __AVR_ATmegaXXX__ 1
	#define __AVR_ATmegaXXUX__ 1
#endif

#endif
