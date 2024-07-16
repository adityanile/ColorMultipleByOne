//Resharper disable all 
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Library.Extensions{
    internal static class CustomExtensions{
        #region GameObject Operations

        public static List<GameObject> GetChildGameObjects(this GameObject gameObj) => FetchChildGameObjects(gameObj.transform);
        public static List<GameObject> GetChildGameObjects(this Transform parentTransform) => FetchChildGameObjects(parentTransform);
        public static List<T> GetChildComponents<T>(this GameObject gameObj) => FetchChildComponents<T>(gameObj.transform);
        public static List<T> GetChildComponents<T>(this Transform parentTransform) => FetchChildComponents<T>(parentTransform);
        
        public static T GetOrAddComponent<T>(this GameObject gameObject, Action<T> initializer = null) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                if (initializer != null)
                {
                    initializer.Invoke(component);
                }
            }

            return component;
        }
        
        private static List<GameObject> FetchChildGameObjects(Transform parentTransform) {
            var childGameObjs = new List<GameObject>();
            for (var i = 0; i < parentTransform.childCount; i++) {
                childGameObjs.Add(parentTransform.transform.GetChild(i).gameObject);
            }

            return childGameObjs;
        }

        private static List<T> FetchChildComponents<T>(Transform parentTransform) {
            var childComponents = new List<T>();
            for (var i = 0; i < parentTransform.childCount; i++) {
                childComponents.Add(parentTransform.transform.GetChild(i).GetComponent<T>());
            }

            return childComponents;
        }

        #endregion
    }
}