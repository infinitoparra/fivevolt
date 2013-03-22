
#ifndef _spi_h_
#define _spi_h_

#include "IoPin.h"
#include <spi.h>

/**
 * @brief Defines the possible SPI communication speeds.
 */
typedef enum SPIFrequency {
    SPI_18MHZ       = 0, /**< 18 MHz */
    SPI_9MHZ        = 1, /**< 9 MHz */
    SPI_4_5MHZ      = 2, /**< 4.5 MHz */
    SPI_2_25MHZ     = 3, /**< 2.25 MHz */
    SPI_1_125MHZ    = 4, /**< 1.125 MHz */
    SPI_562_500KHZ  = 5, /**< 562.500 KHz */
    SPI_281_250KHZ  = 6, /**< 281.250 KHz */
    SPI_140_625KHZ  = 7, /**< 140.625 KHz */
} SPIFrequency;

#define MAX_SPI_FREQS 8

#define LSBFIRST 0
#define MSBFIRST 1

class Spi {
public:
	Spi(spi_dev *pDev);
	void setMode(SPIFrequency frequency, uint32 bitOrder, uint32 mode);
	void enable();

private:
	spi_dev *m_dev;
	SPIFrequency m_frequency;
	uint32 m_bitOrder;
	uint32 m_mode;
};

#endif