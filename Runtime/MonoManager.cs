using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Becs
{
    public abstract class MonoManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] protected T Prefab;
        private ObjectPool<T> _pool;
        private readonly List<T> _units = new();

        private ObjectPool<T> Pool
        {
            get
            {
                if (_pool == null) throw new InvalidOperationException("You need to call InitPool before using it.");
                return _pool;
            }
            set => _pool = value;
        }

        protected virtual void Awake()
        {
            InitPool();
        }

        protected void InitPool(int initial = 10, int max = 100, bool collectionChecks = false)
        {
            Pool = new ObjectPool<T>(
                CreateSetup,
                GetSetup,
                ReleaseSetup,
                DestroySetup,
                collectionChecks,
                initial,
                max);
        }

        #region Overrides

        protected virtual T CreateSetup() => Instantiate(Prefab, transform);
        protected virtual void GetSetup(T mono) => mono.gameObject.SetActive(true);
        protected virtual void ReleaseSetup(T mono) => mono.gameObject.SetActive(false);
        protected virtual void DestroySetup(T mono) => Destroy(mono.gameObject);

        #endregion

        public T AddUnit()
        {
            T unit = Pool.Get();
            _units.Add(unit);
            return unit;
        }

        public void RemoveUnit(T unit)
        {
            _units.Remove(unit);
            Pool.Release(unit);
        }

        public T AddSingle()
        {
            int unitCount = GetCount();
            if (unitCount != 0)
            {
                Debug.LogError($"{typeof(T)} is not single, unit count {unitCount}");
                ClearUnits();
            }

            return AddUnit();
        }

        public void RemoveIndex(int index)
        {
            var unit = _units[index];
            RemoveUnit(unit);
        }

        public void RemoveIndices(List<int> indices)
        {
            indices.Sort();
            indices.Reverse();
            foreach (var index in indices)
            {
                if (index >= GetCount())
                {
                    continue;
                }

                RemoveUnit(_units[index]);
            }
        }

        public void ClearUnits()
        {
            int unitCount = GetCount();
            for (int i = unitCount-1; i >= 0; i--)
            {
                RemoveUnit(_units[i]);
            }
        }

        public IReadOnlyCollection<T> GetUnits() => _units;

        public IEnumerable<(int index, T unit)> EnumerateUnits()
        {
            int index = 0;
            foreach (var unit in _units)
            {
                yield return (index, unit);
                index++;
            }
        }

        public T GetSingle()
        {
            int unitCount = GetCount();
            if (unitCount != 1)
            {
                Debug.LogError($"{typeof(T)} is not single, unit count {unitCount}");
            }

            return _units[0];
        }

        public int GetCount() => _units.Count;
        public bool IsEmpty() => GetCount() == 0;
    }
}