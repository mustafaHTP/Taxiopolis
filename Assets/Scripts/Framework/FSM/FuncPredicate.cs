using System;

namespace Framework.FSM
{
    public class FuncPredicate : IPredicate
    {
        private readonly Func<bool> _func;

        public FuncPredicate(Func<bool> func)
        {
            _func = func;
        }

        public bool Evaluate()
        {
            return _func?.Invoke() ?? false;
        }
    }
}
