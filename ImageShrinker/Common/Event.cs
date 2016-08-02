using System;
using System.Collections.Generic;

namespace ImageShrinker.Common
{
    public class Event<T>
    {
        private HashSet<Action<T>> _actions = new HashSet<Action<T>>();

        public void Subscribe(Action<T> action)
        {
            _actions.Add(action);
        }

        public void Raise(T parameter)
        {
            foreach (var action in _actions)
            {
                action(parameter);
            }
        }
    }

    public class Event
    {
        private HashSet<Action> _actions = new HashSet<Action>();
        public void Subscribe(Action action)
        {
            _actions.Add(action);
        }

        public void Raise()
        {
            foreach (var action in _actions)
            {
                action();
            }
        }
    }
}
