using System;
using System.Collections.Generic;

namespace ZMediaTask.Application.Battle
{
    public sealed class GenericStateMachine<TState>
    {
        private readonly Func<TState, TState, bool> _canTransition;

        public GenericStateMachine(TState initial, Func<TState, TState, bool> canTransition)
        {
            _canTransition = canTransition ?? throw new ArgumentNullException(nameof(canTransition));
            Current = initial;
        }

        public TState Current { get; private set; }

        public bool TryTransition(TState next)
        {
            if (EqualityComparer<TState>.Default.Equals(Current, next))
            {
                return false;
            }

            if (!_canTransition(Current, next))
            {
                return false;
            }

            Current = next;
            return true;
        }
    }
}
