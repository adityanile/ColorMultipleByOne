// ReSharper disable All

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library.Extensions{
    public static class CollectionsExtension{
        #region Enable disable functions

        public static void EnableAll(this IEnumerable<GameObject> list) => ChangeGameObjectsState(list, true);
        public static void DisableAll(this IEnumerable<GameObject> list) => ChangeGameObjectsState(list, false);

        private static void ChangeGameObjectsState(IEnumerable<GameObject> enumerable, bool value) {
            enumerable.ToList().ForEach(gameObj => gameObj.SetActive(value));
        }

        #endregion

        public static void RemoveAllNullGameObjects(this List<GameObject> list) => list.RemoveAll(gameObj => gameObj == null);

        #region Aggregate functions

        public static string AggregateAll(this IEnumerable<string> list, string seperator = ", ") =>
            AggregateString(list, seperator);

        public static string AggregateAll(this IEnumerable<Sprite> enumerable, string seperator = ", ") =>
            AggregateAllNames(enumerable, seperator);

        public static string AggregateAll(this IEnumerable<GameObject> enumerable, string seperator = ", ") =>
            AggregateAllNames(enumerable, seperator);

        private static string AggregateString(IEnumerable<string> enumerable, string seperator) {
            return string.Join(seperator, enumerable);
        }

        private static string AggregateAllNames<T>(IEnumerable<T> enumerable, string seperator) {
            var allNamesList = new List<string>();
            var list = enumerable.ToList();

            list.ForEach(t => {
                if (typeof(T) == typeof(GameObject)) {
                    var gameObj = t as GameObject;
                    allNamesList.Add(gameObj.name);
                } else if (typeof(T) == typeof(Sprite)) {
                    var sprite = t as Sprite;
                    allNamesList.Add(sprite.name);
                }
            });
            return AggregateString(allNamesList, seperator);
        }

        #endregion


        #region Collider Operations

        public static void EnableAllColliders(this IEnumerable<GameObject> enumerable) => ChangeCollidersState(enumerable.ToList(), true);
        public static void DisableAllColliders(this IEnumerable<GameObject> enumerable) => ChangeCollidersState(enumerable.ToList(), false);

        internal static void ChangeCollidersState(IEnumerable<GameObject> enumerable, bool value) {
            foreach (var gameObj in enumerable) {
                try {
                    gameObj.GetComponent<Collider2D>().enabled = value;
                } catch (Exception e) {
                    Debug.LogError($"CustomExtensions: GameObject = {gameObj.name}\n{e}");
                }
            }
        }

        #endregion


        #region Shuffle Operations

        public static void Shuffle<T>(this IList<T> list) {
            var n = list.Count;
            while (n > 1) {
                var k = (UnityEngine.Random.Range(0, n) % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion

        #region Random Element

        public static T GetRandomElement<T>(this IList<T> list) {
            var randomIndex = UnityEngine.Random.Range(0, list.Count);
            return list[randomIndex];
        }

        #endregion
    }
}