using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IDatabase.Engine
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
        public EngineMock(Action<EngineMock<T>> addToEngine) : this()
        {
            _sets = new SetMutator();
            addToEngine(this);
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

        public IEnumerable<string> GetAllNames()
        {
            throw new NotImplementedException();
        }

        public interface ISetMock
        {
            ISetMock AddSet(string name);
            IItemMock AddItem(string name);
            T Settings { get; }
            ISetMock ChangeSettings(T newSettings);
        }
        public interface IItemMock
        {
            ISetMock AddSet(string name);
            IItemMock AddItem(string name);
            IItemMock SetContent(string type, string content);
        }

        private class SetMock : ISetMock, IDbSet<T>
        {
            private ItemMutator _items;
            public SetMutator SetMutator { get; private set; }
            public string Name { get; set; }
            public string SafeName { get { return Name; } }
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

            public ISetMock ChangeSettings(T newSettings)
            {
                Settings = newSettings;
                return this;
            }

            public IEnumerable<string> GetAllNames()
            {
                throw new NotImplementedException();
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
            public string SafeName { get { return Name; } }
            private ItemContentMock _content;

            public IDbItemContent Content {  get { return _content; } }

            public ItemMock(SetMutator sets, ItemMutator items)
            {
                SetMutator = sets;
                ItemMutator = items;
                _content = new ItemContentMock();
            }

            public ISetMock AddSet(string name)
            {
                return SetMutator.AddSet(name);
            }

            public IItemMock AddItem(string name)
            {
                return ItemMutator.AddItem(name);
            }

            public IItemMock SetContent(string type, string content)
            {
                var memStream = new MemoryStream();
                var writer = new StreamWriter(memStream);
                writer.Write(content);
                writer.Flush();
                memStream.Seek(0, SeekOrigin.Begin);
                _content = new ItemContentMock(type, memStream);
                return this;
            }

            private class ItemContentMock : IDbItemContent
            {
                public string Type { get; private set; }
                private readonly Stream _content;

                public string PersistentLink
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public ItemContentMock() { }
                public ItemContentMock(string type, Stream content)
                {
                    Type = type;
                    _content = content;
                }

                public Stream GetContentStream()
                {
                    return _content;
                }

                public IEnumerable<IDbItem> TryAccessSubs()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
