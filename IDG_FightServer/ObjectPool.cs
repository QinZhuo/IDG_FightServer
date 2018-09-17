using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDG
{
    class IndexObjectPool<T> where T:new()
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

    class ObjectPool<T>:IDisposable where T:new()
    {

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                    {
                        foreach (var item in _objects)
                        {
                            (item as IDisposable).Dispose();
                        }
                    }
                }
                
                _objects = null;
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ObjectPool() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion


        protected Queue<T> _objects;
        //private Func<T> _objectGenerator;
        private bool _staticNum;
        public ObjectPool(int preCreate, bool staticNum = false)
        {
           // if (objectGenerator == null) throw new ArgumentNullException(nameof(objectGenerator));
            //_objectGenerator = objectGenerator;
            _objects = new Queue<T>(preCreate);
            _staticNum = staticNum;
            for(int i = 0; i < preCreate; i++)
            {
                _objects.Enqueue(new T());
            }
        }


        public int Count
        {
            get
            {
                lock (_objects)
                {
                    return _objects.Count;
                }
            }
        }

        public virtual T Get()
        {
            lock (_objects)
            {
                if (_objects.Count > 0)
                {
                    return _objects.Dequeue();
                }
            }
            if (_staticNum) {
                return default(T);
            }else
            {
                return new T();
            }
            
        }

        public virtual void Recover(T item)
        {
            lock (_objects)
            {
                _objects.Enqueue(item);
            }
        }
       
    }

    
}
