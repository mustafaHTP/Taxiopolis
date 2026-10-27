using System;
using System.Collections.Generic;

namespace Framework.FSM
{
    public class StateMachine
    {
        private StateNode _current;
        private readonly Dictionary<Type, StateNode> _nodes = new();
        private readonly HashSet<ITransition> _anyTransitions = new();

        public IState Current => _current?.State;

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);

            _current.State?.OnUpdate();
        }

        public void FixedUpdate()
        {
            _current.State?.OnFixedUpdate();
        }

        public void Init()
        {
            _current.State?.OnEnter();
        }

        public void SetState(IState state)
        {
            _current = _nodes[state.GetType()];
        }

        public void ChangeState(IState state)
        {
            if (_current.State == state) return;

            var previousState = _current.State;
            var nextState = _nodes[state.GetType()].State;

            previousState?.OnExit();
            nextState?.OnEnter();

            _current = _nodes[state.GetType()];
        }

        public void AddTransition(IState from, IState to, IPredicate predicate)
        {
            var fromNode = GetOrAddNode(from);
            fromNode.AddTransition(GetOrAddNode(to).State, predicate);
        }

        public void AddAnyTransition(IState to, IPredicate predicate)
        {
            var toNode = GetOrAddNode(to);
            _anyTransitions.Add(new Transition(toNode.State, predicate));
        }

        private StateNode GetOrAddNode(IState state)
        {
            var node = _nodes.GetValueOrDefault(state.GetType());

            if (node == null)
            {
                node = new StateNode(state);
                _nodes.Add(state.GetType(), node);
            }

            return node;
        }

        private ITransition GetTransition()
        {
            foreach (var transition in _anyTransitions)
            {
                if (transition.Predicate.Evaluate())
                    return transition;
            }

            foreach (var transition in _current.Transitions)
            {
                if (transition.Predicate.Evaluate())
                    return transition;
            }

            return null;
        }


        private class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new();
            }

            public void AddTransition(IState to, IPredicate predicate)
            {
                Transitions.Add(new Transition(to, predicate));
            }
        }
    }
}