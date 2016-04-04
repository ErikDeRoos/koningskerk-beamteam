using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IDatabase
{
    public class EngineMock<T> : IEngine<T> where T : class, ISetSettings, new()
    {
        private SetMutator _sets;

        public IEnumerable<IDbSet<T>> Where(Func<IDbSet<T>, bool> query)
        {
            return _sets.Where(query);
        }

        public EngineMock()
        {
            _sets = new SetMutator();
        }

        private class SetMutator : IEnumerable<SetMock>
        {
            private List<SetMock> _sets = new List<SetMock>();
            public SetMutator()
            {
            }

            public ISetMock AddSet(string name)
            {
                var newSet = new SetMock(this)
                {
                    Name = name,
                };
                _sets.Add(newSet);
                return newSet;
            }

            public IEnumerator<SetMock> GetEnumerator()
            {
                return ((IEnumerable<SetMock>)_sets).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<SetMock>)_sets).GetEnumerator();
            }
        }


        public ISetMock AddSet(string name)
        {
            return _sets.AddSet(name);
        }

        public interface ISetMock
        {
            ISetMock AddSet(string name);
            IItemMock AddItem(string name);
            T Settings { get; }
        }
        public interface IItemMock
        {
            ISetMock AddSet(string name);
            IItemMock AddItem(string name);
        }

        private class SetMock : ISetMock, IDbSet<T>
        {
            private ItemMutator _items;
            public SetMutator SetMutator { get; private set; }
            public string Name { get; set; }
            public T Settings { get; set; }
            public IEnumerable<IDbItem> Where(Func<IDbItem, bool> query)
            {
                return _items.Where(query);
            }

            public SetMock(SetMutator sets)
            {
                Settings = new T();
                SetMutator = sets;
                _items = new ItemMutator(sets);
            }

            public ISetMock AddSet(string name)
            {
                return SetMutator.AddSet(name);
            }

            public IItemMock AddItem(string name)
            {
                return _items.AddItem(name);
            }
        }

        private class ItemMutator : IEnumerable<IDbItem>
        {
            private List<IDbItem> _items = new List<IDbItem>();
            private SetMutator _set;
            public ItemMutator(SetMutator set)
            {
                _set = set;
            }

            public IItemMock AddItem(string name)
            {
                var newItem = new ItemMock(_set, this)
                {
                    Name = name
                };
                _items.Add(newItem);
                return newItem;
            }

            public IEnumerator<IDbItem> GetEnumerator()
            {
                return ((IEnumerable<IDbItem>)_items).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<IDbItem>)_items).GetEnumerator();
            }
        }
        private class ItemMock : IItemMock, IDbItem
        {
            public SetMutator SetMutator { get; private set; }
            public ItemMutator ItemMutator { get; private set; }

            public string Name { get; set; }

            public IDbItemContent Content
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public ItemMock(SetMutator sets, ItemMutator items)
            {
                SetMutator = sets;
                ItemMutator = items;
            }

            public ISetMock AddSet(string name)
            {
                return SetMutator.AddSet(name);
            }

            public IItemMock AddItem(string name)
            {
                return ItemMutator.AddItem(name);
            }
        }
    }
}
