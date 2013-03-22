using System;
using System.Collections.Generic;
using System.IO;

namespace Quadcopter.Infrastructure
{
    public class MemoryQueue
    {
        private readonly LinkedList<byte> _queue = new LinkedList<byte>();

        public void Write(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                _queue.AddLast(buffer[i]);
            }
        }

        public int Length
        {
            get { return _queue.Count; }
        }

        public byte[] ToArray()
        {
            byte[] result = new byte[Length];
            int offset = 0;
            LinkedListNode<byte> node = _queue.First;
            while (node != null)
            {
                result[offset++] = node.Value;
                node = node.Next;
            }
            return result;
        }

        public byte Dequeue()
        {
            return Dequeue(1)[0];
        }

        public byte[] Dequeue(int length)
        {
            byte[] result = new byte[length];
            int resultOffset = 0;
            while (length > 0 && _queue.First != null)
            {
                result[resultOffset++] = _queue.First.Value;
                _queue.RemoveFirst();
                length--;
            }
            return result;
        }

        public int? IndexOf(byte[] header, out bool partialMatch)
        {
            for (int i = 0; i < Length; i++)
            {
                int h;
                for (h = 0; h < header.Length; h++)
                {
                    if (i + h == Length)
                    {
                        partialMatch = true;
                        return i;
                    }
                    if (Peek(i + h) != header[h])
                    {
                        break;
                    }
                }
                if (h == header.Length)
                {
                    partialMatch = false;
                    return i;
                }
            }
            partialMatch = false;
            return null;
        }

        public byte Peek(int i)
        {
            LinkedListNode<byte> node = _queue.First;
            while (node != null)
            {
                if (i == 0)
                {
                    return node.Value;
                }
                i--;
                node = node.Next;
            }
            throw new IndexOutOfRangeException("Could not peek at '" + i + "' length is only '" + Length + "'");
        }

        public byte[] DequeueUntil(byte[] seq)
        {
            MemoryStream data = new MemoryStream();
            while (Length > seq.Length)
            {
                bool match = true;
                for (int i = 0; i < seq.Length; i++)
                {
                    if (Peek(i) != seq[i])
                    {
                        data.WriteByte(Dequeue());
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    break;
                }
            }
            return data.ToArray();
        }

        public bool StartsWith(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (Peek(i) != data[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
