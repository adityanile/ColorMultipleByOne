//Resharper disable all
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Library.Extensions
{
    public static class TransformExtension
    {
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Transform[] FindAllDeepChildren(this Transform aParent, string aName)
        {
            List<Transform> results = new List<Transform>();
            var result = aParent.Find(aName);
            if (result != null)
                results.Add(result);

            foreach (Transform child in aParent)
            {
                var resultss = child.FindAllDeepChildren(aName);
                if (resultss != null)
                    results.AddRange(resultss);
            }

            return results.ToArray();
        }

        public static Transform FindChildIncludingInactive(this Transform aParent, string name)
        {
            foreach (Transform t in aParent)
            {
                if (t.name == name)
                    return t;
            }

            return null;
        }

        public static Transform FindByHierarchy(this Transform aParent, params string[] hierarchy)
        {
            Transform toReturn = aParent;
            foreach (string child in hierarchy)
            {
                if (!toReturn) break;

                toReturn = toReturn.FindChildIncludingInactive(child);
            }

            return toReturn;
        }

        /// <summary>
        /// Finds the child with given name and returns the specified Component
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName">Name of the GameObject</param>
        /// <param name="returnComponent">Component to be returned</param>
        /// <param name="defType">New component will be of this type</param>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <returns>Returns True if new component is attached, False otherwise</returns>
        public static bool FetchComponentInChild<T>(this Transform parent, string childName, out T returnComponent, System.Type defType = null) where T : Component
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                Transform transform;
                (transform = child.transform).SetParent(parent);
                transform.localPosition = Vector3.zero;
            }

            returnComponent = child.GetComponent<T>();

            if (returnComponent) return false;

            if (defType != null)
                returnComponent = child.gameObject.AddComponent(defType) as T;
            else
                returnComponent = child.gameObject.AddComponent<T>();

            return true;
        }
        
        public static bool GetAllComponentsInDeepChildren<T>(this Transform parent, out T[] returnComponents, bool checkParent = true) where T : Component
        {
            List<T> components = new List<T>();
            
            T component;
            if (checkParent)
            {
                component = parent.GetComponent<T>();
                if (component != null)
                    components.Add(component);    
            }
            
            foreach (Transform child in parent)
            {
                component = child.GetComponent<T>();
                if (component != null)
                    components.Add(component);
                T[] childComponents;
                var b = child.GetAllComponentsInDeepChildren<T>(out childComponents);
                if (b)
                    components.AddRange(childComponents);
            }

            returnComponents = components.ToArray();
            return returnComponents.Length > 0;
        }

        public static T GetComponentInParentRecursivly<T>(this Transform thisTransform) where T : Component
        {
            T returnComponent = thisTransform.GetComponent<T>();

            if (returnComponent || thisTransform.parent == null)
                return returnComponent;

            return thisTransform.parent.GetComponentInParentRecursivly<T>();
        }

        public static void GetComponentsInChildren<T>(this Transform thisTransform, Transform[] excludeParents, List<T> list)
        {
            if (list == null) list = new List<T>();

            foreach (Transform child in thisTransform)
            {
                if (excludeParents.Contains(child)) continue;

                T c = child.GetComponent<T>();
                if (c != null) list.Add(c);
                child.GetComponentsInChildren<T>(excludeParents, list);
            }
        }
    }
}