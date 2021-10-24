using System;
using System.Collections.Generic;
using UnityEngine;

// State Machine class from Jason Weimann's "Unity Bots with State Machines" video https://www.youtube.com/watch?v=V75hgcsCGOM&t=493s
public class StateMachine
{
   private IState _currentState;
   
   private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type,List<Transition>>();   // A dictionary that returns a list of transitions for a specified state (represented by a Type object)
   private List<Transition> _currentTransitions = new List<Transition>();       // A list of the transitions from the current state to a specified state
   private List<Transition> _anyTransitions = new List<Transition>();           // A list of "any state" transitions (meaning you can complete the transition from any state) AKA (From: anywhere, To: specified state)
   
   private static List<Transition> EmptyTransitions = new List<Transition>(0);  // A placeholder list representing no transitions (allows us to not have to worry about dealing with the nullptr case)

   public void Tick()
   {
      var transition = GetTransition();     // Gets the first transition from the current state whose conditions have been met
      if (transition != null)               // If such a transition was found, update the current state to be the state to which the selected transition takes us
         SetState(transition.To);
      
      _currentState?.Tick();                // Activate the main body of the current state
   }

   public void SetState(IState state)       // Performs necessary processes to complete a full transition to a new state
   {
      if (state == _currentState)           // Prevents us from wasting processing power transitioning to the state we're already in
         return;
      
      _currentState?.OnExit();              // Activate the exit processes for the about-to-be-transitioned-from state (_currentState is null before we set the default state)
      _currentState = state;                // Update our variable for the current state
      
      _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);   // Essentially, we're passing a reference to _currentTransitions, which will be updated with the transitions for the specified state (note that even if no transitions are found, _currentTransitions will still be modified to now be a nullptr)
      if (_currentTransitions == null)              // If we didn't find any transitions for the state we provided, set our current transitions to be an empty list object
         _currentTransitions = EmptyTransitions;    // We want _currentTransitions to point to SOME object (even if there's no useful data) so that it doesn't cause any weird errors in the rest of our code (which is expecting a pointer to an list object)
      
      _currentState.OnEnter();              // Activate the enter processes for the new state we have just transitioned into
   }

   public void AddTransition(IState from, IState to, Func<bool> predicate)      // 
   {
      if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)   // Check to see if a list of transitions has already been started for the specified state (meaning there is at least one transition from the state)
      {
         transitions = new List<Transition>();
         _transitions[from.GetType()] = transitions;
      }
      
      transitions.Add(new Transition(to, predicate));
   }

   public void AddAnyTransition(IState state, Func<bool> predicate)
   {
      _anyTransitions.Add(new Transition(state, predicate));
   }

   private class Transition     // The container representing information about a given transition. Note that we don't have a "from" data member -- this is because our dictionary stores transitions relative to a given state (which represents the "from" state)
   {
      public Func<bool> Condition {get; }
      public IState To { get; }

      public Transition(IState to, Func<bool> condition)
      {
         To = to;
         Condition = condition;
      }
   }

   private Transition GetTransition()           // Searches through all possible transitions FROM the current state; the first transition whose conditions are met is returned
   {
      foreach(var transition in _anyTransitions)
         if (transition.Condition())
            return transition;
      
      foreach (var transition in _currentTransitions)
         if (transition.Condition())
            return transition;

      return null;
   }
}
