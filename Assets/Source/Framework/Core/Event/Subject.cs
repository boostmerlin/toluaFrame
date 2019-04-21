using System;
using System.Collections.Generic;
using System.Text;

namespace Ginkgo
{
    public static class EventExtensions
    {
        public static void Subscribe<T>(this IObservable<T> observable, Action<T> action)
        {
            observable.Subscribe(new SimpleObserver<T>(action));
        }
    }
    public class SimpleObserver<T> : IObserver<T>
    {
        public Action<T> onNext;
        public SimpleObserver(Action<T> action)
        {
            this.onNext = action;
        }
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            if(onNext != null)
            {
                onNext(value);
            }
        }
    }
    public abstract class Observer<T> : IObserver<T>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public abstract void OnNext(T value);
    }

    public class ListObserver<T> : IObserver<T>
    {
        private readonly List<IObserver<T>> _observers;

        public ListObserver()
        {
            _observers = new List<IObserver<T>>();
        }

        public void OnCompleted()
        {
            var targetObservers = _observers;
            for (int i = 0; i < targetObservers.Count; i++)
            {
                targetObservers[i].OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            var targetObservers = _observers;
            for (int i = 0; i < targetObservers.Count; i++)
            {
                targetObservers[i].OnError(error);
            }
        }

        public void OnNext(T value)
        {
            var targetObservers = _observers;
            for (int i = 0; i < targetObservers.Count; i++)
            {
                targetObservers[i].OnNext(value);
            }
        }

        public int Length
        {
            get
            {
                return _observers.Count;
            }
        }

        internal void Add(IObserver<T> observer)
        {
            _observers.Add(observer);
        }

        internal void Remove(IObserver<T> observer)
        {
            var i = _observers.IndexOf(observer);
            if (i < 0)
                return;
            _observers.RemoveAt(i);
        }
    }

    public sealed class Subject<T> : ISubject<T>
    {
        object observerLock = new object();
        ListObserver<T> outObserver = new ListObserver<T>();
        public bool HasObservers
        {
            get
            {
                return outObserver.Length > 0;
            }
        }

        public void OnCompleted()
        {
            lock (observerLock)
            {
                outObserver.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");
            lock (observerLock)
            {
                outObserver.OnError(error);
            }
        }

        public void OnNext(T value)
        {
            outObserver.OnNext(value);
        }

        public void Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException("observer");
            var ex = default(Exception);

            lock (observerLock)
            {
                outObserver.Add(observer);
            }
            if (ex != null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }
        }
    }
}