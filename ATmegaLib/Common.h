
#ifndef _common_h_
#define _common_h_

#if defined(__AVR_ATmega8U2__)
	#define __AVR_ATmegaXXU2__ 1
	#define __AVR_ATmegaXXX__ 1
#endif

#if defined(__AVR_ATmega32U4__)
	#define __AVR_ATmegaXXU4__ 1
	#define __AVR_ATmegaXXX__ 1
#endif

#if defined(__AVR_ATmega328P__)
	#define __AVR_ATmegaXX8__ 1
	#define __AVR_ATmegaXXX__ 1
#endif

#if defined(__AVR_ATmega644P__)
	#define __AVR_ATmegaXXX__ 1
#endif

#if defined(__AVR_ATmega1284P__)
	#define __AVR_ATmegaXXX__ 1
#endif

#endif /* _common_h_ */
