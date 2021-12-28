using System.Collections.Concurrent;

namespace OOProgramming.DesignPattern
{
    class  ObjectPool<T> where T : new()
    {
        private readonly ConcurrentBag<T> items = new ConcurrentBag<T>();
        private int counter = 0;
        private int MAX = 10;

        public void Release(T item)
        {
            //TO DO   
            if (counter < MAX)
            {
                items.Add(item);
                counter++;
            }
        }

        public T Get()
        {
            //TO DO
            T item;
            if (items.TryTake(out item))
            {
                counter--;
                return item;
            }
            else
            {
                T obj = new T();
                items.Add(obj);
                counter++;
                return obj;
            }
        }
    }
}
