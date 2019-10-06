using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FiniteStateMachine<T> where T : Enum
{

    StateAction[] stateActions;
    TransientAction[] openingActions;
    TransientAction[] closingActions;

    FSMHandle handle;

    public FiniteStateMachine(StateAction[] stateActions, TransientAction[] openingActions, TransientAction[] closingActions, T startingState)
    {
        int nrOfStates = Enum.GetValues(typeof(T)).Length;
        if (stateActions.Length != nrOfStates) throw new Exception("Invalid number of state actions, " + nrOfStates + "expected, " + stateActions.Length + " recieved.");
        if (openingActions.Length != nrOfStates) throw new Exception("Invalid number of opening actions, " + nrOfStates + "expected, " + openingActions.Length + " recieved.");
        if (closingActions.Length != nrOfStates) throw new Exception("Invalid number of closing actions, " + nrOfStates + "expected, " + closingActions.Length + " recieved.");

        this.stateActions = stateActions;
        this.openingActions = openingActions;
        this.closingActions = closingActions;

        handle = new FSMHandle(startingState);
    }

    public void Tick(float dt)
    {
        if(handle.IsStateChangeing())
        {
            handle.Lock();
            closingActions[Convert.ToInt32(handle.LastState)].Invoke(handle);
            openingActions[Convert.ToInt32(handle.NextState)].Invoke(handle);
            handle.Unlock();
            handle.ProgressState();
        }
        int i = Convert.ToInt32(handle.CurrentState);
        StateAction sa = stateActions[i];
        sa.Invoke(handle, dt);
    }

    public delegate void StateAction(FSMHandle handle, float dt);
    public delegate void TransientAction(FSMHandle handle);

    public class FSMHandle
    {
        private T currentState;
        private T nextState;

        bool isLocked = false;

        public FSMHandle(T startingState)
        {
            currentState = startingState;
            nextState = startingState;
        }

        public T LastState => currentState;
        public T CurrentState => currentState;
        public T NextState => nextState;

        internal void ChangeState(T nextState)
        {
            if (isLocked)
                throw new Exception("Cant change state while the state machien is locked You are probably trying to change the state during a transition, which is not possible.");
            this.nextState = nextState;
        }

        internal bool IsStateChangeing() => currentState.Equals(nextState) == false;

        internal void Lock() => isLocked = true;

        internal void ProgressState() => currentState = nextState;

        internal void Unlock() => isLocked = false;
    }
}
