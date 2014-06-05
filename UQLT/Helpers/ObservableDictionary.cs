/* Copyright (c) 2007, Dr. WPF
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*
*   * Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*
*   * The name Dr. WPF may not be used to endorse or promote products
*     derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY Dr. WPF ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL Dr. WPF BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace UQLT.Helpers
{
	[Serializable]
	/// <summary>
	/// (Observable)Dictionary that allows binding ItemsControl to a dictionary
	/// By: Dr. WPF
	/// http://drwpf.com/blog/2007/09/16/can-i-bind-my-itemscontrol-to-a-dictionary/
	/// </summary>
	public class ObservableDictionary<TKey, TValue> :
		IDictionary<TKey, TValue>,
		ICollection<KeyValuePair<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>>,
		IDictionary,
		ICollection,
		IEnumerable,
		ISerializable,
		IDeserializationCallback,
		INotifyCollectionChanged,
		INotifyPropertyChanged
	{
		#region constructors

		#region public

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
		/// </summary>
		public ObservableDictionary()
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();

			foreach (KeyValuePair<TKey, TValue> entry in dictionary)
			{
				DoAddEntry((TKey)entry.Key, (TValue)entry.Value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public ObservableDictionary(IEqualityComparer<TKey> comparer)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="comparer">The comparer.</param>
		public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);

			foreach (KeyValuePair<TKey, TValue> entry in dictionary)
			{
				DoAddEntry((TKey)entry.Key, (TValue)entry.Value);
			}
		}

		#endregion public

		#region protected

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		protected ObservableDictionary(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		#endregion protected

		#endregion constructors

		#region properties

		#region public

		/// <summary>
		/// Gets the comparer.
		/// </summary>
		/// <value>
		/// The comparer.
		/// </value>
		public IEqualityComparer<TKey> Comparer
		{
			get
			{
				return _keyedEntryCollection.Comparer;
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get
			{
				return _keyedEntryCollection.Count;
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public Dictionary<TKey, TValue>.KeyCollection Keys
		{
			get
			{
				return TrueDictionary.Keys;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public TValue this[TKey key]
		{
			get
			{
				return (TValue)_keyedEntryCollection[key].Value;
			}
			set
			{
				DoSetEntry(key, value);
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public Dictionary<TKey, TValue>.ValueCollection Values
		{
			get
			{
				return TrueDictionary.Values;
			}
		}

		#endregion public

		#region private

		/// <summary>
		/// Gets the true dictionary.
		/// </summary>
		/// <value>
		/// The true dictionary.
		/// </value>
		private Dictionary<TKey, TValue> TrueDictionary
		{
			get
			{
				if (_dictionaryCacheVersion != _version)
				{
					_dictionaryCache.Clear();
					foreach (DictionaryEntry entry in _keyedEntryCollection)
					{
						_dictionaryCache.Add((TKey)entry.Key, (TValue)entry.Value);
					}
					_dictionaryCacheVersion = _version;
				}
				return _dictionaryCache;
			}
		}

		#endregion private

		#endregion properties

		#region methods

		#region public

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public void Add(TKey key, TValue value)
		{
			DoAddEntry(key, value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			DoClearEntries();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
		/// </returns>
		public bool ContainsKey(TKey key)
		{
			return _keyedEntryCollection.Contains(key);
		}

		/// <summary>
		/// Determines whether the specified value contains value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool ContainsValue(TValue value)
		{
			return TrueDictionary.ContainsValue(value);
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, false);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		public bool Remove(TKey key)
		{
			return DoRemoveEntry(key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			bool result = _keyedEntryCollection.Contains(key);
			value = result ? (TValue)_keyedEntryCollection[key].Value : default(TValue);
			return result;
		}

		#endregion public

		#region protected

		/// <summary>
		/// Adds the entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected virtual bool AddEntry(TKey key, TValue value)
		{
			_keyedEntryCollection.Add(new DictionaryEntry(key, value));
			return true;
		}

		/// <summary>
		/// Clears the entries.
		/// </summary>
		/// <returns></returns>
		protected virtual bool ClearEntries()
		{
			// check whether there are entries to clear
			bool result = (Count > 0);
			if (result)
			{
				// if so, clear the dictionary
				_keyedEntryCollection.Clear();
			}
			return result;
		}

		/// <summary>
		/// Gets the index and entry for key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
		{
			entry = new DictionaryEntry();
			int index = -1;
			if (_keyedEntryCollection.Contains(key))
			{
				entry = _keyedEntryCollection[key];
				index = _keyedEntryCollection.IndexOf(entry);
			}
			return index;
		}

		/// <summary>
		/// Raises the <see cref="E:CollectionChanged" /> event.
		/// </summary>
		/// <param name="args">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			if (CollectionChanged != null)
			{
				CollectionChanged(this, args);
			}
		}

		/// <summary>
		/// Called when the property is changed.
		/// </summary>
		/// <param name="name">The name.</param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		/// <summary>
		/// Removes the entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected virtual bool RemoveEntry(TKey key)
		{
			// remove the entry
			return _keyedEntryCollection.Remove(key);
		}

		/// <summary>
		/// Sets the entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected virtual bool SetEntry(TKey key, TValue value)
		{
			bool keyExists = _keyedEntryCollection.Contains(key);

			// if identical key/value pair already exists, nothing to do
			if (keyExists && value.Equals((TValue)_keyedEntryCollection[key].Value))
			{
				return false;
			}

			// otherwise, remove the existing entry
			if (keyExists)
			{
				_keyedEntryCollection.Remove(key);
			}

			// add the new entry
			_keyedEntryCollection.Add(new DictionaryEntry(key, value));

			return true;
		}

		#endregion protected

		#region private

		/// <summary>
		/// Does the add entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		private void DoAddEntry(TKey key, TValue value)
		{
			if (AddEntry(key, value))
			{
				_version++;

				DictionaryEntry entry;
				int index = GetIndexAndEntryForKey(key, out entry);
				FireEntryAddedNotifications(entry, index);
			}
		}

		/// <summary>
		/// Does the clear entries.
		/// </summary>
		private void DoClearEntries()
		{
			if (ClearEntries())
			{
				_version++;
				FireResetNotifications();
			}
		}

		/// <summary>
		/// Does the remove entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private bool DoRemoveEntry(TKey key)
		{
			DictionaryEntry entry;
			int index = GetIndexAndEntryForKey(key, out entry);

			bool result = RemoveEntry(key);
			if (result)
			{
				_version++;
				if (index > -1)
				{
					FireEntryRemovedNotifications(entry, index);
				}
			}

			return result;
		}

		/// <summary>
		/// Does the set entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		private void DoSetEntry(TKey key, TValue value)
		{
			DictionaryEntry entry;
			int index = GetIndexAndEntryForKey(key, out entry);

			if (SetEntry(key, value))
			{
				_version++;

				// if prior entry existed for this key, fire the removed notifications
				if (index > -1)
				{
					FireEntryRemovedNotifications(entry, index);

					// force the property change notifications to fire for the modified entry
					_countCache--;
				}

				// then fire the added notifications
				index = GetIndexAndEntryForKey(key, out entry);
				FireEntryAddedNotifications(entry, index);
			}
		}

		/// <summary>
		/// Fires the entry added notifications.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="index">The index.</param>
		private void FireEntryAddedNotifications(DictionaryEntry entry, int index)
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			if (index > -1)
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index));
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		/// <summary>
		/// Fires the entry removed notifications.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="index">The index.</param>
		private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			if (index > -1)
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index));
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		/// <summary>
		/// Fires the property changed notifications.
		/// </summary>
		private void FirePropertyChangedNotifications()
		{
			if (Count != _countCache)
			{
				_countCache = Count;
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnPropertyChanged("Keys");
				OnPropertyChanged("Values");
			}
		}

		/// <summary>
		/// Fires the reset notifications.
		/// </summary>
		private void FireResetNotifications()
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion private

		#endregion methods

		#region interfaces

		#region IDictionary<TKey, TValue>

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			DoAddEntry(key, value);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return DoRemoveEntry(key);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
		/// </returns>
		bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
		{
			return _keyedEntryCollection.Contains(key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return TryGetValue(key, out value);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				return Keys;
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				return Values;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get
			{
				return (TValue)_keyedEntryCollection[key].Value;
			}
			set
			{
				DoSetEntry(key, value);
			}
		}

		#endregion IDictionary<TKey, TValue>

		#region IDictionary

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary" /> object.
		/// </summary>
		/// <param name="key">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to use as the value of the element to add.</param>
		void IDictionary.Add(object key, object value)
		{
			DoAddEntry((TKey)key, (TValue)value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		void IDictionary.Clear()
		{
			DoClearEntries();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.IDictionary" /> object contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" /> object.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key; otherwise, false.
		/// </returns>
		bool IDictionary.Contains(object key)
		{
			return _keyedEntryCollection.Contains((TKey)key);
		}

		/// <summary>
		/// Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.IDictionary" /> object.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.IDictionary" /> object.
		/// </returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, true);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> object has a fixed size.
		/// </summary>
		bool IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		bool IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		object IDictionary.this[object key]
		{
			get
			{
				return _keyedEntryCollection[(TKey)key].Value;
			}
			set
			{
				DoSetEntry((TKey)key, (TValue)value);
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		ICollection IDictionary.Keys
		{
			get
			{
				return Keys;
			}
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" /> object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		void IDictionary.Remove(object key)
		{
			DoRemoveEntry((TKey)key);
		}

		ICollection IDictionary.Values
		{
			get
			{
				return Values;
			}
		}

		#endregion IDictionary

		#region ICollection<KeyValuePair<TKey, TValue>>

		/// <summary>
		/// Adds the specified KVP.
		/// </summary>
		/// <param name="kvp">The KVP.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp)
		{
			DoAddEntry(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			DoClearEntries();
		}

		/// <summary>
		/// Determines whether [contains] [the specified KVP].
		/// </summary>
		/// <param name="kvp">The KVP.</param>
		/// <returns></returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> kvp)
		{
			return _keyedEntryCollection.Contains(kvp.Key);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">CopyTo() failed:  array parameter was null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">CopyTo() failed:  index parameter was outside the bounds of the supplied array</exception>
		/// <exception cref="System.ArgumentException">CopyTo() failed:  supplied array was too small</exception>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("CopyTo() failed:  array parameter was null");
			}
			if ((index < 0) || (index > array.Length))
			{
				throw new ArgumentOutOfRangeException("CopyTo() failed:  index parameter was outside the bounds of the supplied array");
			}
			if ((array.Length - index) < _keyedEntryCollection.Count)
			{
				throw new ArgumentException("CopyTo() failed:  supplied array was too small");
			}

			foreach (DictionaryEntry entry in _keyedEntryCollection)
			{
				array[index++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		int ICollection<KeyValuePair<TKey, TValue>>.Count
		{
			get
			{
				return _keyedEntryCollection.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Removes the specified KVP.
		/// </summary>
		/// <param name="kvp">The KVP.</param>
		/// <returns></returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
		{
			return DoRemoveEntry(kvp.Key);
		}

		#endregion ICollection<KeyValuePair<TKey, TValue>>

		#region ICollection

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_keyedEntryCollection).CopyTo(array, index);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		int ICollection.Count
		{
			get
			{
				return _keyedEntryCollection.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
		/// </summary>
		bool ICollection.IsSynchronized
		{
			get
			{
				return ((ICollection)_keyedEntryCollection).IsSynchronized;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
		/// </summary>
		object ICollection.SyncRoot
		{
			get
			{
				return ((ICollection)_keyedEntryCollection).SyncRoot;
			}
		}

		#endregion ICollection

		#region IEnumerable<KeyValuePair<TKey, TValue>>

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, false);
		}

		#endregion IEnumerable<KeyValuePair<TKey, TValue>>

		#region IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion IEnumerable

		#region ISerializable

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
		/// <exception cref="System.ArgumentNullException">info</exception>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			Collection<DictionaryEntry> entries = new Collection<DictionaryEntry>();
			foreach (DictionaryEntry entry in _keyedEntryCollection)
			{
				entries.Add(entry);
			}
			info.AddValue("entries", entries);
		}

		#endregion ISerializable

		#region IDeserializationCallback

		/// <summary>
		/// Runs when the entire object graph has been deserialized.
		/// </summary>
		/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo != null)
			{
				Collection<DictionaryEntry> entries = (Collection<DictionaryEntry>)
													  _siInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
				foreach (DictionaryEntry entry in entries)
				{
					AddEntry((TKey)entry.Key, (TValue)entry.Value);
				}
			}
		}

		#endregion IDeserializationCallback

		#region INotifyCollectionChanged

		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add
			{
				CollectionChanged += value;
			}
			remove
			{
				CollectionChanged -= value;
			}
		}

		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion INotifyCollectionChanged

		#region INotifyPropertyChanged

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				PropertyChanged += value;
			}
			remove
			{
				PropertyChanged -= value;
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		protected virtual event PropertyChangedEventHandler PropertyChanged;

		#endregion INotifyPropertyChanged

		#endregion interfaces

		#region protected classes

		#region KeyedDictionaryEntryCollection<TKey>

		protected class KeyedDictionaryEntryCollection<TKey> : KeyedCollection<TKey, DictionaryEntry>
		{
			#region constructors

			#region public

			/// <summary>
			/// Initializes a new instance of the <see cref="KeyedDictionaryEntryCollection`1"/> class.
			/// </summary>
			public KeyedDictionaryEntryCollection() : base() { }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeyedDictionaryEntryCollection`1"/> class.
			/// </summary>
			/// <param name="comparer">The comparer.</param>
			public KeyedDictionaryEntryCollection(IEqualityComparer<TKey> comparer) : base(comparer) { }

			#endregion public

			#endregion constructors

			#region methods

			#region protected

			/// <summary>
			/// Gets the key for item.
			/// </summary>
			/// <param name="entry">The entry.</param>
			/// <returns></returns>
			protected override TKey GetKeyForItem(DictionaryEntry entry)
			{
				return (TKey)entry.Key;
			}

			#endregion protected

			#endregion methods
		}

		#endregion KeyedDictionaryEntryCollection<TKey>

		#endregion protected classes

		#region public structures

		#region Enumerator

		[Serializable, StructLayout(LayoutKind.Sequential)]
		public struct Enumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			#region constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="Enumerator`2"/> struct.
			/// </summary>
			/// <param name="dictionary">The dictionary.</param>
			/// <param name="isDictionaryEntryEnumerator">if set to <c>true</c> [is dictionary entry enumerator].</param>
			internal Enumerator(ObservableDictionary<TKey, TValue> dictionary, bool isDictionaryEntryEnumerator)
			{
				_dictionary = dictionary;
				_version = dictionary._version;
				_index = -1;
				_isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
				_current = new KeyValuePair<TKey, TValue>();
			}

			#endregion constructors

			#region properties

			#region public

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <value>
			/// The current.
			/// </value>
			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					ValidateCurrent();
					return _current;
				}
			}

			#endregion public

			#endregion properties

			#region methods

			#region public

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				ValidateVersion();
				_index++;
				if (_index < _dictionary._keyedEntryCollection.Count)
				{
					_current = new KeyValuePair<TKey, TValue>((TKey)_dictionary._keyedEntryCollection[_index].Key, (TValue)_dictionary._keyedEntryCollection[_index].Value);
					return true;
				}
				_index = -2;
				_current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			#endregion public

			#region private

			/// <summary>
			/// Validates the current.
			/// </summary>
			/// <exception cref="System.InvalidOperationException">
			/// The enumerator has not been started.
			/// or
			/// The enumerator has reached the end of the collection.
			/// </exception>
			private void ValidateCurrent()
			{
				if (_index == -1)
				{
					throw new InvalidOperationException("The enumerator has not been started.");
				}
				else if (_index == -2)
				{
					throw new InvalidOperationException("The enumerator has reached the end of the collection.");
				}
			}

			/// <summary>
			/// Validates the version.
			/// </summary>
			/// <exception cref="System.InvalidOperationException">The enumerator is not valid because the dictionary changed.</exception>
			private void ValidateVersion()
			{
				if (_version != _dictionary._version)
				{
					throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
				}
			}

			#endregion private

			#endregion methods

			#region IEnumerator implementation

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <value>
			/// The current.
			/// </value>
			object IEnumerator.Current
			{
				get
				{
					ValidateCurrent();
					if (_isDictionaryEntryEnumerator)
					{
						return new DictionaryEntry(_current.Key, _current.Value);
					}
					return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
				}
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			void IEnumerator.Reset()
			{
				ValidateVersion();
				_index = -1;
				_current = new KeyValuePair<TKey, TValue>();
			}

			#endregion IEnumerator implemenation

			#region IDictionaryEnumerator implemenation

			/// <summary>
			/// Gets the entry.
			/// </summary>
			/// <value>
			/// The entry.
			/// </value>
			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					ValidateCurrent();
					return new DictionaryEntry(_current.Key, _current.Value);
				}
			}
			/// <summary>
			/// Gets the key.
			/// </summary>
			/// <value>
			/// The key.
			/// </value>
			object IDictionaryEnumerator.Key
			{
				get
				{
					ValidateCurrent();
					return _current.Key;
				}
			}
			/// <summary>
			/// Gets the value.
			/// </summary>
			/// <value>
			/// The value.
			/// </value>
			object IDictionaryEnumerator.Value
			{
				get
				{
					ValidateCurrent();
					return _current.Value;
				}
			}

			#endregion

			#region fields

			private ObservableDictionary<TKey, TValue> _dictionary;
			private int _version;
			private int _index;
			private KeyValuePair<TKey, TValue> _current;
			private bool _isDictionaryEntryEnumerator;

			#endregion fields
		}

		#endregion Enumerator

		#endregion public structures

		#region fields

		protected KeyedDictionaryEntryCollection<TKey> _keyedEntryCollection;

		private int _countCache = 0;
		private Dictionary<TKey, TValue> _dictionaryCache = new Dictionary<TKey, TValue>();
		private int _dictionaryCacheVersion = 0;
		private int _version = 0;

		[NonSerialized]
		private SerializationInfo _siInfo = null;

		#endregion fields
	}
}