
#include "RingBuffer.h"
#include <malloc.h>

RingBuffer::RingBuffer(uint32 size) {
  m_buffer = (uint8*)malloc(size);
  m_head = 0;
  m_tail = 0;
  m_size = size - 1;
}

RingBuffer::~RingBuffer() {
  free(m_buffer);
}

uint32 RingBuffer::fullCount() {
  int32 size = m_tail - m_head;
  if (m_tail < m_head) {
    size += m_size + 1;
  }
  return (uint16)size;
}

void RingBuffer::insert(uint8 data) {
  m_buffer[m_tail] = data;
  m_tail = (m_tail == m_size) ? 0 : m_tail + 1;
}

void RingBuffer::insert(uint8 *buffer, uint32 count) {
  // todo: this can speed up by using memcopy or something
  for(uint32 i=0; i<count; i++) {
    insert(*buffer);
    buffer++;
  }
}

uint8 RingBuffer::remove() {
  uint8 ch = m_buffer[m_head];
  m_head = (m_head == m_size) ? 0 : m_head + 1;
  return ch;
}

uint32 RingBuffer::remove(uint8 *buffer, uint32 count) {
  // todo: this can speed up by using memcopy or something
  uint32 fc = fullCount();
  uint32 itemsRemoved = fc < count ? fc : count;
  count = itemsRemoved;
  while(count > 0) {
    *buffer++ = remove();
    count--;
  }
  return itemsRemoved;
}

void RingBuffer::clear() {
  m_tail = m_head;
}

