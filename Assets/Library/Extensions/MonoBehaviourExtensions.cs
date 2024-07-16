using System;
using System.Collections;
using UnityEngine;

namespace Library.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static void Invoke(this MonoBehaviour mb, Action action, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(action, delay));
        }

        private static IEnumerator InvokeRoutine(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        /// <summary>
        /// Waits until passed func returns true and then executes passed action.
        /// </summary>
        /// <param name="func">Condition to wait until it becomes true</param>
        /// <param name="action">Code to be executed after waiting</param>
        /// <returns>Coroutine that is started for waiting.</returns>
        public static Coroutine WaitUntilAndInvoke(this MonoBehaviour behaviour, Func<bool> func, Action action)
        {
            return behaviour.StartCoroutine(_WaitForAndInvoke(func, action));
        }

        private static IEnumerator _WaitForAndInvoke(Func<bool> func, Action action)
        {
            yield return new WaitUntil(func);
            action.Invoke();
        }
        
        public static void InvokeContinuous(this MonoBehaviour behaviour, Action action, float delay, int times) {
            Debug.Log($"Times: {times}");
            behaviour.Invoke(() =>
            {
                switch (times)
                {
                    case > 0:
                        times--;
                        action.Invoke();
                        behaviour.InvokeContinuous(action, delay, times);
                        break;
                    case 0:
                        action.Invoke();
                        break;
                    case < 0:
                        action.Invoke();
                        behaviour.InvokeContinuous(action, delay, times);
                        break;
                }
            }, delay);
        }
    }
}