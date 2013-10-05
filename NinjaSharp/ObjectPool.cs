using System;
using System.Collections.Generic;

namespace ThirdPartyNinjas.NinjaSharp
{
	public class ObjectPool<T>
	{
		public ObjectPool(Func<T> allocator, int initialCapacity)
        {
			if (allocator == null)
				throw new ArgumentNullException("allocator");

			this.allocator = allocator;
			pool = new Queue<T>(initialCapacity);
			for (int i = 0; i < initialCapacity; i++)
				pool.Enqueue(allocator());
        }

		public T GetItem()
        {
			if (pool.Count > 1)
				return pool.Dequeue();
			return allocator();
        }

        public void ReturnItem(T item)
        {
			pool.Enqueue(item);
        }

		Func<T> allocator;
		Queue<T> pool;
    }
}
