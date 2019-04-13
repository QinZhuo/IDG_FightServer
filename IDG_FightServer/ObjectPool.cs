using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDG
{
    class IndexObjectPool<T> where T : new()
    {

        protected List<T> _objList;
        protected List<bool> _usedList;
        protected int _activeCount;
        public IndexObjectPool(int preCreate)
        {
            _objList = new List<T>();
            _usedList = new List<bool>();

            for (int i = 0; i < preCreate; i++)
            {
                _objList.Add(new T());
                _usedList.Add(false);
            }
        }
        public int Get()
        {
            for (int i = 0; i < _usedList.Count; i++)
            {
                if (!_usedList[i])
                {
                    _usedList[i] = true;
                    _activeCount++;
                    return i;
                }
            }
            return -1;
        }
        public int Count
        {
            get
            {
                return _objList.Count;
            }
        }
        public int ActiveCount
        {
            get
            {
                return _activeCount;
            }
        }
        public void Recover(int index)
        {
            _usedList[index] = false;
            _activeCount--;
        }

        public T this[int index]
        {
            get
            {
                if (_usedList[index])
                {
                    return _objList[index];
                }
                else
                {
                    //Console.WriteLine("被回收的池对象" + _objList[index]);
                    //throw new Exception("被回收的池对象" + _objList[index]);
                    return _objList[index];
                }
            }
        }
        public void Foreach(Action<T> action)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_usedList[i])
                {
                    action(_objList[i]);
                }
            }
        }

    }
}