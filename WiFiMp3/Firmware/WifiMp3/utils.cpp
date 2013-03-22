
#include "utils.h"

uint32 reverseBits(uint32 value)
{
  asm volatile("rbit r0, r0");
  asm volatile("bx lr");
  return 0;
}

