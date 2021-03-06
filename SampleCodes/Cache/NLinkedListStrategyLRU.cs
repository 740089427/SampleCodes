﻿using System;
using System.Collections.Generic;
using System.Text;
using SampleCodes.Collections;

namespace SampleCodes.Cache
{
    /// <summary>
    /// 实现最近最少算法的双向链表策略
    /// </summary>
    public class NLinkedListStrategyLRU : INLinkedListStrategy
    {
        public void Add<T>(T value, NLinkedList<T> link)
        {
            link.AddFirst(value);
        }

        public void Hit<T>(LinkedListNode<T> node, NLinkedList<T> link)
        {
            link.AddFirst(node);
        }

        public void Hit<T>(T value, NLinkedList<T> link, Func<T, T, bool> compare)
        {
            lock (link)
            {
                var next = link.First;
                LinkedListNode<T> hitNode = null;
                while (next != null)
                {
                    if (compare(value, next.Value))
                    {
                        hitNode = next;
                    }
                    next = next.Next;
                }

                if (hitNode == null)
                {
                    link.AddFirst(value);
                }
                else
                {
                    link.AddFirst(hitNode);
                }
            }
        }
    }
}
