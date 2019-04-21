using System;
using System.Collections.Generic;
using System.Text;

namespace Ginkgo
{
    public interface IObserver<in T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }

    public interface IObservable<T>
    {
        void Subscribe(IObserver<T> observer);
    }

    public interface ISubject<T> : IObserver<T>, IObservable<T>
    {
    }
}