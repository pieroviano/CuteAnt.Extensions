﻿#if NET40 || NET35 || NET30 || NET20
#pragma warning disable 0420
// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ThreadLocal.cs
//
// <OWNER>[....]</OWNER>
//
// A class that provides a simple, lightweight implementation of thread-local lazy-initialization, where a value is initialized once per accessing 
// thread; this provides an alternative to using a ThreadStatic static variable and having 
// to check the variable prior to every access to see if it's been initialized.
//
// 
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Permissions;
#if !NET35 && !NET30 && !NET20
using System.Diagnostics.Contracts;
#endif

namespace System.Threading
{
	/// <summary>Provides thread-local storage of data.</summary>
	/// <typeparam name="T">Specifies the type of data stored per-thread.</typeparam>
	/// <remarks>
	/// <para>
	/// With the exception of <see cref="Dispose()"/>, all public and protected members of 
	/// <see cref="ThreadLocal{T}"/> are thread-safe and may be used
	/// concurrently from multiple threads.
	/// </para>
	/// </remarks>
	[DebuggerTypeProxy(typeof(SystemThreading_ThreadLocalDebugViewX<>))]
	[DebuggerDisplay("IsValueCreated={IsValueCreated}, Value={ValueForDebugDisplay}, Count={ValuesCountForDebugDisplay}")]
	[HostProtection(Synchronization = true, ExternalThreading = true)]
	public class ThreadLocalX<T> : IDisposable
	{

		// a delegate that returns the created value, if null the created value will be default(T)
		private Func<T> m_valueFactory;

		//
		// ts_slotArray is a table of thread-local values for all ThreadLocal<T> instances
		//
		// So, when a thread reads ts_slotArray, it gets back an array of *all* ThreadLocal<T> values for this thread and this T.
		// The slot relevant to this particular ThreadLocal<T> instance is determined by the m_idComplement instance field stored in
		// the ThreadLocal<T> instance.
		//
		[ThreadStatic]
		static LinkedSlotVolatile[] ts_slotArray;

		[ThreadStatic]
		static FinalizationHelper ts_finalizationHelper;

		// Slot ID of this ThreadLocal<> instance. We store a bitwise complement of the ID (that is ~ID), which allows us to distinguish
		// between the case when ID is 0 and an incompletely initialized object, either due to a thread abort in the constructor, or
		// possibly due to a memory model issue in user code.
		private int m_idComplement;

		// This field is set to true when the constructor completes. That is helpful for recognizing whether a constructor
		// threw an exception - either due to invalid argument or due to a thread abort. Finally, the field is set to false
		// when the instance is disposed.
		private volatile bool m_initialized;

		// IdManager assigns and reuses slot IDs. Additionally, the object is also used as a global lock.
		private static IdManager s_idManager = new IdManager();

		// A linked list of all values associated with this ThreadLocal<T> instance.
		// We create a dummy head node. That allows us to remove any (non-dummy)  node without having to locate the m_linkedSlot field. 
		private LinkedSlot m_linkedSlot = new LinkedSlot(null);

		// Whether the Values property is supported
		private bool m_trackAllValues;

		/// <summary>Initializes the <see cref="System.Threading.ThreadLocal{T}"/> instance.</summary>
		public ThreadLocalX()
		{
			Initialize(null, false);
		}

		/// <summary>Initializes the <see cref="System.Threading.ThreadLocal{T}"/> instance.</summary>
		/// <param name="trackAllValues">Whether to track all values set on the instance and expose them through the Values property.</param>
		public ThreadLocalX(bool trackAllValues)
		{
			Initialize(null, trackAllValues);
		}


		/// <summary>
		/// Initializes the <see cref="System.Threading.ThreadLocal{T}"/> instance with the
		/// specified <paramref name="valueFactory"/> function.
		/// </summary>
		/// <param name="valueFactory">
		/// The <see cref="T:System.Func{T}"/> invoked to produce a lazily-initialized value when 
		/// an attempt is made to retrieve <see cref="Value"/> without it having been previously initialized.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="valueFactory"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		public ThreadLocalX(Func<T> valueFactory)
		{
			if (valueFactory == null)
				throw new ArgumentNullException(nameof(valueFactory));

			Initialize(valueFactory, false);
		}

		/// <summary>
		/// Initializes the <see cref="System.Threading.ThreadLocal{T}"/> instance with the
		/// specified <paramref name="valueFactory"/> function.
		/// </summary>
		/// <param name="valueFactory">
		/// The <see cref="T:System.Func{T}"/> invoked to produce a lazily-initialized value when 
		/// an attempt is made to retrieve <see cref="Value"/> without it having been previously initialized.
		/// </param>
		/// <param name="trackAllValues">Whether to track all values set on the instance and expose them via the Values property.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="valueFactory"/> is a null reference (Nothing in Visual Basic).
		/// </exception>
		public ThreadLocalX(Func<T> valueFactory, bool trackAllValues)
		{
			if (valueFactory == null)
				throw new ArgumentNullException(nameof(valueFactory));

			Initialize(valueFactory, trackAllValues);
		}

		private void Initialize(Func<T> valueFactory, bool trackAllValues)
		{
			m_valueFactory = valueFactory;
			m_trackAllValues = trackAllValues;

			// Assign the ID and mark the instance as initialized. To avoid leaking IDs, we assign the ID and set m_initialized
			// in a finally block, to avoid a thread abort in between the two statements.
			try { }
			finally
			{
				m_idComplement = ~s_idManager.GetId();

				// As the last step, mark the instance as fully initialized. (Otherwise, if m_initialized=false, we know that an exception
				// occurred in the constructor.)
				m_initialized = true;
			}
		}

		/// <summary>Releases the resources used by this <see cref="T:System.Threading.ThreadLocal{T}" /> instance.</summary>
		~ThreadLocalX()
		{
			// finalizer to return the type combination index to the pool
			Dispose(false);
		}

		#region IDisposable Members

		/// <summary>Releases the resources used by this <see cref="T:System.Threading.ThreadLocal{T}" /> instance.</summary>
		/// <remarks>
		/// Unlike most of the members of <see cref="T:System.Threading.ThreadLocal{T}"/>, this method is not thread-safe.
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Releases the resources used by this <see cref="T:System.Threading.ThreadLocal{T}" /> instance.</summary>
		/// <param name="disposing">
		/// A Boolean value that indicates whether this method is being called due to a call to <see cref="Dispose()"/>.
		/// </param>
		/// <remarks>
		/// Unlike most of the members of <see cref="T:System.Threading.ThreadLocal{T}"/>, this method is not thread-safe.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			int id;

			lock (s_idManager)
			{
				id = ~m_idComplement;
				m_idComplement = 0;

				if (id < 0 || !m_initialized)
				{
#if !NET35 && !NET30 && !NET20
					Contract.Assert(id >= 0 || !m_initialized, "expected id >= 0 if initialized");
#else
                    Trace.Assert(id >= 0 || !m_initialized, "expected id >= 0 if initialized");
#endif

					// Handle double Dispose calls or disposal of an instance whose constructor threw an exception.
					return;
				}
				m_initialized = false;

				for (LinkedSlot linkedSlot = m_linkedSlot.Next; linkedSlot != null; linkedSlot = linkedSlot.Next)
				{
					LinkedSlotVolatile[] slotArray = linkedSlot.SlotArray;

					if (slotArray == null)
					{
						// The thread that owns this slotArray has already finished.
						continue;
					}

					// Remove the reference from the LinkedSlot to the slot table.
					linkedSlot.SlotArray = null;

					// And clear the references from the slot table to the linked slot and the value so that
					// both can get garbage collected.
					slotArray[id].Value.Value = default(T);
					slotArray[id].Value = null;
				}
			}
			m_linkedSlot = null;
			s_idManager.ReturnId(id);
		}

		#endregion

		/// <summary>Creates and returns a string representation of this instance for the current thread.</summary>
		/// <returns>The result of calling <see cref="System.Object.ToString"/> on the <see cref="Value"/>.</returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <see cref="Value"/> for the current thread is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <exception cref="T:System.InvalidOperationException">
		/// The initialization function referenced <see cref="Value"/> in an improper manner.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="ThreadLocal{T}"/> instance has been disposed.
		/// </exception>
		/// <remarks>
		/// Calling this method forces initialization for the current thread, as is the
		/// case with accessing <see cref="Value"/> directly.
		/// </remarks>
		public override string ToString()
		{
			return Value.ToString();
		}

		/// <summary>Gets or sets the value of this instance for the current thread.</summary>
		/// <exception cref="T:System.InvalidOperationException">
		/// The initialization function referenced <see cref="Value"/> in an improper manner.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="ThreadLocal{T}"/> instance has been disposed.
		/// </exception>
		/// <remarks>
		/// If this instance was not previously initialized for the current thread,
		/// accessing <see cref="Value"/> will attempt to initialize it. If an initialization function was 
		/// supplied during the construction, that initialization will happen by invoking the function 
		/// to retrieve the initial value for <see cref="Value"/>.  Otherwise, the default value of 
		/// <typeparamref name="T"/> will be used.
		/// </remarks>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public T Value
		{
			get
			{
				LinkedSlotVolatile[] slotArray = ts_slotArray;
				LinkedSlot slot;
				int id = ~m_idComplement;

				//
				// Attempt to get the value using the fast path
				//
				if (slotArray != null   // Has the slot array been initialized?
						&& id >= 0   // Is the ID non-negative (i.e., instance is not disposed)?
						&& id < slotArray.Length   // Is the table large enough?
						&& (slot = slotArray[id].Value) != null   // Has a LinkedSlot object has been allocated for this ID?
						&& m_initialized // Has the instance *still* not been disposed (important for ----s with Dispose)?
				)
				{
					// We verified that the instance has not been disposed *after* we got a reference to the slot.
					// This guarantees that we have a reference to the right slot.
					// 
					// Volatile read of the LinkedSlotVolatile.Value property ensures that the m_initialized read
					// will not be reordered before the read of slotArray[id].
					return slot.Value;
				}

				return GetValueSlow();
			}
			set
			{
				LinkedSlotVolatile[] slotArray = ts_slotArray;
				LinkedSlot slot;
				int id = ~m_idComplement;

				//
				// Attempt to set the value using the fast path
				//
				if (slotArray != null   // Has the slot array been initialized?
						&& id >= 0   // Is the ID non-negative (i.e., instance is not disposed)?
						&& id < slotArray.Length   // Is the table large enough?
						&& (slot = slotArray[id].Value) != null   // Has a LinkedSlot object has been allocated for this ID?
						&& m_initialized // Has the instance *still* not been disposed (important for ----s with Dispose)?
						)
				{
					// We verified that the instance has not been disposed *after* we got a reference to the slot.
					// This guarantees that we have a reference to the right slot.
					// 
					// Volatile read of the LinkedSlotVolatile.Value property ensures that the m_initialized read
					// will not be reordered before the read of slotArray[id].
					slot.Value = value;
				}
				else
				{
					SetValueSlow(value, slotArray);
				}
			}
		}

		private T GetValueSlow()
		{
			// If the object has been disposed, the id will be -1.
			int id = ~m_idComplement;
			if (id < 0)
			{
				//throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
				throw new ObjectDisposedException("ThreadLocal_Disposed");
			}

#if !NET35 && !NET30 && !NET20
            Debugger.NotifyOfCrossThreadDependency();
#endif

			// Determine the initial value
			T value;
			if (m_valueFactory == null)
			{
				value = default(T);
			}
			else
			{
				value = m_valueFactory();

				if (IsValueCreated)
				{
					//throw new InvalidOperationException(Environment.GetResourceString("ThreadLocal_Value_RecursiveCallsToValue"));
					throw new InvalidOperationException("ThreadLocal_Value_RecursiveCallsToValue");
				}
			}

			// Since the value has been previously uninitialized, we also need to set it (according to the ThreadLocal semantics).
			Value = value;
			return value;
		}

		private void SetValueSlow(T value, LinkedSlotVolatile[] slotArray)
		{
			int id = ~m_idComplement;

			// If the object has been disposed, id will be -1.
			if (id < 0)
			{
				//throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
				throw new ObjectDisposedException("ThreadLocal_Disposed");
			}

			// If a slot array has not been created on this thread yet, create it.
			if (slotArray == null)
			{
				slotArray = new LinkedSlotVolatile[GetNewTableSize(id + 1)];
				ts_finalizationHelper = new FinalizationHelper(slotArray, m_trackAllValues);
				ts_slotArray = slotArray;
			}

			// If the slot array is not big enough to hold this ID, increase the table size.
			if (id >= slotArray.Length)
			{
				GrowTable(ref slotArray, id + 1);
				ts_finalizationHelper.SlotArray = slotArray;
				ts_slotArray = slotArray;
			}

			// If we are using the slot in this table for the first time, create a new LinkedSlot and add it into
			// the linked list for this ThreadLocal instance.
			if (slotArray[id].Value == null)
			{
				CreateLinkedSlot(slotArray, id, value);
			}
			else
			{
				// Volatile read of the LinkedSlotVolatile.Value property ensures that the m_initialized read
				// that follows will not be reordered before the read of slotArray[id].
				LinkedSlot slot = slotArray[id].Value;

				// It is important to verify that the ThreadLocal instance has not been disposed. The check must come
				// after capturing slotArray[id], but before assigning the value into the slot. This ensures that
				// if this ThreadLocal instance was disposed on another thread and another ThreadLocal instance was
				// created, we definitely won't assign the value into the wrong instance.

				if (!m_initialized)
				{
					//throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
					throw new ObjectDisposedException("ThreadLocal_Disposed");
				}

				slot.Value = value;
			}
		}

		/// <summary>Creates a LinkedSlot and inserts it into the linked list for this ThreadLocal instance.</summary>
		private void CreateLinkedSlot(LinkedSlotVolatile[] slotArray, int id, T value)
		{
			// Create a LinkedSlot
			var linkedSlot = new LinkedSlot(slotArray);

			// Insert the LinkedSlot into the linked list maintained by this ThreadLocal<> instance and into the slot array
			lock (s_idManager)
			{
				// Check that the instance has not been disposed. It is important to check this under a lock, since
				// Dispose also executes under a lock.
				if (!m_initialized)
				{
					//throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
					throw new ObjectDisposedException("ThreadLocal_Disposed");
				}

				LinkedSlot firstRealNode = m_linkedSlot.Next;

				//
				// Insert linkedSlot between nodes m_linkedSlot and firstRealNode. 
				// (m_linkedSlot is the dummy head node that should always be in the front.)
				//
				linkedSlot.Next = firstRealNode;
				linkedSlot.Previous = m_linkedSlot;
				linkedSlot.Value = value;

				if (firstRealNode != null)
				{
					firstRealNode.Previous = linkedSlot;
				}
				m_linkedSlot.Next = linkedSlot;

				// Assigning the slot under a lock prevents a ---- with Dispose (dispose also acquires the lock).
				// Otherwise, it would be possible that the ThreadLocal instance is disposed, another one gets created
				// with the same ID, and the write would go to the wrong instance.
				slotArray[id].Value = linkedSlot;
			}
		}

		/// <summary>Gets a list for all of the values currently stored by all of the threads that have accessed this instance.</summary>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="ThreadLocal{T}"/> instance has been disposed.
		/// </exception>
		public IList<T> Values
		{
			get
			{
				if (!m_trackAllValues)
				{
					//throw new InvalidOperationException(Environment.GetResourceString("ThreadLocal_ValuesNotAvailable"));
					throw new InvalidOperationException("ThreadLocal_ValuesNotAvailable");
				}

				var list = GetValuesAsList(); // returns null if disposed
				//if (list == null) throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
				if (list == null) throw new ObjectDisposedException("ThreadLocal_Disposed");
				return list;
			}
		}

		/// <summary>Gets all of the threads' values in a list.</summary>
		private List<T> GetValuesAsList()
		{
			List<T> valueList = new List<T>();
			int id = ~m_idComplement;
			if (id == -1)
			{
				return null;
			}

			// Walk over the linked list of slots and gather the values associated with this ThreadLocal instance.
			for (LinkedSlot linkedSlot = m_linkedSlot.Next; linkedSlot != null; linkedSlot = linkedSlot.Next)
			{
				// We can safely read linkedSlot.Value. Even if this ThreadLocal has been disposed in the meantime, the LinkedSlot
				// objects will never be assigned to another ThreadLocal instance.
				valueList.Add(linkedSlot.Value);
			}

			return valueList;
		}

		/// <summary>Gets the number of threads that have data in this instance.</summary>
		private int ValuesCountForDebugDisplay
		{
			get
			{
				int count = 0;
				for (LinkedSlot linkedSlot = m_linkedSlot.Next; linkedSlot != null; linkedSlot = linkedSlot.Next)
				{
					count++;
				}
				return count;
			}
		}

		/// <summary>Gets whether <see cref="Value"/> is initialized on the current thread.</summary>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The <see cref="ThreadLocal{T}"/> instance has been disposed.
		/// </exception>
		public bool IsValueCreated
		{
			get
			{
				int id = ~m_idComplement;
				if (id < 0)
				{
					//throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
					throw new ObjectDisposedException("ThreadLocal_Disposed");
				}

				LinkedSlotVolatile[] slotArray = ts_slotArray;
				return slotArray != null && id < slotArray.Length && slotArray[id].Value != null;
			}
		}


		/// <summary>Gets the value of the ThreadLocal&lt;T&gt; for debugging display purposes. It takes care of getting
		/// the value for the current thread in the ThreadLocal mode.</summary>
		internal T ValueForDebugDisplay
		{
			get
			{
				LinkedSlotVolatile[] slotArray = ts_slotArray;
				int id = ~m_idComplement;

				LinkedSlot slot;
				if (slotArray == null || id >= slotArray.Length || (slot = slotArray[id].Value) == null || !m_initialized)
					return default(T);
				return slot.Value;
			}
		}

		/// <summary>Gets the values of all threads that accessed the ThreadLocal&lt;T&gt;.</summary>
		internal List<T> ValuesForDebugDisplay // same as Values property, but doesn't throw if disposed
		{
			get { return GetValuesAsList(); }
		}

		/// <summary>Resizes a table to a certain length (or larger).</summary>
		private void GrowTable(ref LinkedSlotVolatile[] table, int minLength)
		{
#if !NET35 && !NET30 && !NET20
			Contract.Assert(table.Length < minLength);
#else
			Trace.Assert(table.Length < minLength);
#endif

			// Determine the size of the new table and allocate it.
			int newLen = GetNewTableSize(minLength);
			LinkedSlotVolatile[] newTable = new LinkedSlotVolatile[newLen];

			//
			// The lock is necessary to avoid a race with ThreadLocal.Dispose. GrowTable has to point all 
			// LinkedSlot instances referenced in the old table to reference the new table. Without locking, 
			// Dispose could use a stale SlotArray reference and clear out a slot in the old array only, while 
			// the value continues to be referenced from the new (larger) array.
			//
			lock (s_idManager)
			{
				for (int i = 0; i < table.Length; i++)
				{
					LinkedSlot linkedSlot = table[i].Value;
					if (linkedSlot != null && linkedSlot.SlotArray != null)
					{
						linkedSlot.SlotArray = newTable;
						newTable[i] = table[i];
					}
				}
			}

			table = newTable;
		}

		/// <summary>Chooses the next larger table size</summary>
		private static int GetNewTableSize(int minSize)
		{
			// Array.MaxArrayLength
			const int MaxArrayLength = 0X7FEFFFFF;
			if ((uint)minSize > MaxArrayLength)
			{
				// Intentionally return a value that will result in an OutOfMemoryException
				return int.MaxValue;
			}
#if !NET35 && !NET30 && !NET20
            Contract.Assert(minSize > 0);
#else
			Trace.Assert(minSize > 0);
#endif

			//
			// Round up the size to the next power of 2
			//
			// The algorithm takes three steps: 
			//   input -> subtract one -> propagate 1-bits to the right -> add one
			//
			// Let's take a look at the 3 steps in both interesting cases: where the input 
			// is (Example 1) and isn't (Example 2) a power of 2.
			//
			//   Example 1: 100000 -> 011111 -> 011111 -> 100000
			//   Example 2: 011010 -> 011001 -> 011111 -> 100000
			// 
			int newSize = minSize;

			// Step 1: Decrement
			newSize--;

			// Step 2: Propagate 1-bits to the right.
			newSize |= newSize >> 1;
			newSize |= newSize >> 2;
			newSize |= newSize >> 4;
			newSize |= newSize >> 8;
			newSize |= newSize >> 16;

			// Step 3: Increment
			newSize++;

			// Don't set newSize to more than Array.MaxArrayLength
			if ((uint)newSize > MaxArrayLength)
			{
				newSize = MaxArrayLength;
			}

			return newSize;
		}

		/// <summary>
		/// A wrapper struct used as LinkedSlotVolatile[] - an array of LinkedSlot instances, but with volatile semantics
		/// on array accesses.
		/// </summary>
		private struct LinkedSlotVolatile
		{
			internal volatile LinkedSlot Value;
		}

		/// <summary>
		/// A node in the doubly-linked list stored in the ThreadLocal instance.
		/// 
		/// The value is stored in one of two places:
		/// 
		///     1. If SlotArray is not null, the value is in SlotArray.Table[id]
		///     2. If SlotArray is null, the value is in FinalValue.
		/// </summary>
		private sealed class LinkedSlot
		{
			internal LinkedSlot(LinkedSlotVolatile[] slotArray)
			{
				SlotArray = slotArray;
			}

			// The next LinkedSlot for this ThreadLocal<> instance
			internal volatile LinkedSlot Next;

			// The previous LinkedSlot for this ThreadLocal<> instance
			internal volatile LinkedSlot Previous;

			// The SlotArray that stores this LinkedSlot at SlotArray.Table[id].
			internal volatile LinkedSlotVolatile[] SlotArray;

			// The value for this slot.
			internal T Value;
		}

		/// <summary>A manager class that assigns IDs to ThreadLocal instances</summary>
		private class IdManager
		{
			// The next ID to try
			private int m_nextIdToTry = 0;

			// Stores whether each ID is free or not. Additionally, the object is also used as a lock for the IdManager.
			private List<bool> m_freeIds = new List<bool>();

			internal int GetId()
			{
				lock (m_freeIds)
				{
					int availableId = m_nextIdToTry;
					while (availableId < m_freeIds.Count)
					{
						if (m_freeIds[availableId]) { break; }
						availableId++;
					}

					if (availableId == m_freeIds.Count)
					{
						m_freeIds.Add(false);
					}
					else
					{
						m_freeIds[availableId] = false;
					}

					m_nextIdToTry = availableId + 1;

					return availableId;
				}
			}

			// Return an ID to the pool
			internal void ReturnId(int id)
			{
				lock (m_freeIds)
				{
					m_freeIds[id] = true;
					if (id < m_nextIdToTry) m_nextIdToTry = id;
				}
			}
		}

		/// <summary>
		/// A class that facilitates ThreadLocal cleanup after a thread exits.
		/// 
		/// After a thread with an associated thread-local table has exited, the FinalizationHelper 
		/// is reponsible for removing back-references to the table. Since an instance of FinalizationHelper 
		/// is only referenced from a single thread-local slot, the FinalizationHelper will be GC'd once
		/// the thread has exited.
		/// 
		/// The FinalizationHelper then locates all LinkedSlot instances with back-references to the table
		/// (all those LinkedSlot instances can be found by following references from the table slots) and
		/// releases the table so that it can get GC'd.
		/// </summary>
		private class FinalizationHelper
		{
			internal LinkedSlotVolatile[] SlotArray;
			private bool m_trackAllValues;

			internal FinalizationHelper(LinkedSlotVolatile[] slotArray, bool trackAllValues)
			{
				SlotArray = slotArray;
				m_trackAllValues = trackAllValues;
			}

			~FinalizationHelper()
			{
				LinkedSlotVolatile[] slotArray = SlotArray;
#if !NET35 && !NET30 && !NET20
				Contract.Assert(slotArray != null);
#else
                Trace.Assert(slotArray != null);
#endif

				for (int i = 0; i < slotArray.Length; i++)
				{
					LinkedSlot linkedSlot = slotArray[i].Value;
					if (linkedSlot == null)
					{
						// This slot in the table is empty
						continue;
					}

					if (m_trackAllValues)
					{
						// Set the SlotArray field to null to release the slot array.
						linkedSlot.SlotArray = null;
					}
					else
					{
						// Remove the LinkedSlot from the linked list. Once the FinalizationHelper is done, all back-references to
						// the table will be have been removed, and so the table can get GC'd.
						lock (s_idManager)
						{
							if (linkedSlot.Next != null)
							{
								linkedSlot.Next.Previous = linkedSlot.Previous;
							}

							// Since the list uses a dummy head node, the Previous reference should never be null.
#if !NET35 && !NET30 && !NET20
							Contract.Assert(linkedSlot.Previous != null);
#else
							Trace.Assert(linkedSlot.Previous != null);
#endif
							linkedSlot.Previous.Next = linkedSlot.Next;
						}
					}
				}
			}
		}
	}

	/// <summary>A debugger view of the ThreadLocal&lt;T&gt; to surface additional debugging properties and 
	/// to ensure that the ThreadLocal&lt;T&gt; does not become initialized if it was not already.</summary>
	internal sealed class SystemThreading_ThreadLocalDebugViewX<T>
	{
		//The ThreadLocal object being viewed.
		private readonly ThreadLocalX<T> m_tlocal;

		/// <summary>Constructs a new debugger view object for the provided ThreadLocal object.</summary>
		/// <param name="tlocal">A ThreadLocal object to browse in the debugger.</param>
		public SystemThreading_ThreadLocalDebugViewX(ThreadLocalX<T> tlocal)
		{
			m_tlocal = tlocal;
		}

		/// <summary>Returns whether the ThreadLocal object is initialized or not.</summary>
		public bool IsValueCreated
		{
			get { return m_tlocal.IsValueCreated; }
		}

		/// <summary>Returns the value of the ThreadLocal object.</summary>
		public T Value
		{
			get
			{
				return m_tlocal.ValueForDebugDisplay;
			}
		}

		/// <summary>Return all values for all threads that have accessed this instance.</summary>
		public List<T> Values
		{
			get
			{
				return m_tlocal.ValuesForDebugDisplay;
			}
		}
	}
}
#endif