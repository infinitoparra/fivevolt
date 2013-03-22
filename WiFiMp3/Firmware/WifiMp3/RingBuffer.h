
#ifndef _RingBuffer_h_
#define _RingBuffer_h_

#include <libmaple.h>

class RingBuffer {
public:
  RingBuffer(uint32 size);
  ~RingBuffer();
  
  /**
   * @brief Return the number of elements stored in the ring buffer.
   */
  uint32 fullCount();
  uint32 emptyCount() { return m_size - fullCount(); }
  
  /**
   * Append element onto the end of a ring buffer.
   */
  void insert(uint8 data);
  void insert(uint8 *buffer, uint32 count);
  
  /**
   * @brief Remove and return the first item from a ring buffer.
   */
  uint8 remove();
  
  /**
   * @brief Remove and return the count of items actually removed.
   */
  uint32 remove(uint8 *buffer, uint32 count);
  
  void clear();
  
private:
  uint16 m_head; /**< Index of the next item to remove */
  uint16 m_tail; /**< Index where the next item will get inserted */
  uint16 m_size;
  uint8 *m_buffer;
};

#endif

