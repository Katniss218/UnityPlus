using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Provides built-in descriptors for standard .NET types via the Provider system.
    /// This allows these types to be overridden by user providers if registered in a specific context.
    /// </summary>
    public static class BuiltInDescriptorProviders
    {
        [MapsInheritingFrom( typeof( char ) )]
        private static IDescriptor ProvideChar() => new PrimitiveConfigurableDescriptor<char>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)(v.ToString()),
            ( d, c ) => { string strData = (string)d; return string.IsNullOrEmpty( strData ) ? '\0' : strData[0]; }
        );

        [MapsInheritingFrom( typeof( string ) )]
        private static IDescriptor ProvideString() => new PrimitiveConfigurableDescriptor<string>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (string)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( bool ) )]
        private static IDescriptor ProvideBool() => new PrimitiveConfigurableDescriptor<bool>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (bool)(SerializedPrimitive)d );

        // --- Numeric Types (Explicit) ---

        [MapsInheritingFrom( typeof( byte ) )]
        private static IDescriptor ProvideByte() => new PrimitiveConfigurableDescriptor<byte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (byte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( sbyte ) )]
        private static IDescriptor ProvideSByte() => new PrimitiveConfigurableDescriptor<sbyte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (sbyte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( short ) )]
        private static IDescriptor ProvideInt16() => new PrimitiveConfigurableDescriptor<short>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (short)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ushort ) )]
        private static IDescriptor ProvideUInt16() => new PrimitiveConfigurableDescriptor<ushort>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ushort)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( int ) )]
        private static IDescriptor ProvideInt32() => new PrimitiveConfigurableDescriptor<int>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (int)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( uint ) )]
        private static IDescriptor ProvideUInt32() => new PrimitiveConfigurableDescriptor<uint>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (uint)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( long ) )]
        private static IDescriptor ProvideInt64() => new PrimitiveConfigurableDescriptor<long>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (long)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ulong ) )]
        private static IDescriptor ProvideUInt64() => new PrimitiveConfigurableDescriptor<ulong>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ulong)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( float ) )]
        private static IDescriptor ProvideSingle() => new PrimitiveConfigurableDescriptor<float>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (float)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( double ) )]
        private static IDescriptor ProvideDouble() => new PrimitiveConfigurableDescriptor<double>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (double)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( decimal ) )]
        private static IDescriptor ProvideDecimal() => new PrimitiveConfigurableDescriptor<decimal>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (decimal)(SerializedPrimitive)d );

        // --- Extended System Types ---

        [MapsInheritingFrom( typeof( Guid ) )]
        private static IDescriptor ProvideGuid() => new PrimitiveConfigurableDescriptor<Guid>(
            ( v, w, c ) => w.Data = v.SerializeGuid(),
            ( d, c ) => d.DeserializeGuid()
        );

        [MapsInheritingFrom( typeof( Type ) )]
        private static IDescriptor ProvideType() => new PrimitiveConfigurableDescriptor<Type>(
            ( v, w, c ) => w.Data = v.SerializeType(),
            ( d, c ) => d.DeserializeType()
        );

        [MapsInheritingFrom( typeof( DateTime ) )]
        private static IDescriptor ProvideDateTime() => new PrimitiveConfigurableDescriptor<DateTime>(
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTime.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( DateTimeOffset ) )]
        private static IDescriptor ProvideDateTimeOffset() => new PrimitiveConfigurableDescriptor<DateTimeOffset>(
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTimeOffset.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( TimeSpan ) )]
        private static IDescriptor ProvideTimeSpan() => new PrimitiveConfigurableDescriptor<TimeSpan>(
            // TimeSpan is saved as `[-][d'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "c", CultureInfo.InvariantCulture ),
            ( d, c ) => TimeSpan.ParseExact( (string)d, "c", CultureInfo.InvariantCulture )
        );


        // --- Inner Types ---

        [MapsInheritingFrom( typeof( Delegate ) )]
        public static IDescriptor DelegateMapping() => new PrimitiveConfigurableDescriptor<Delegate>(
            ( v, w, c ) =>
            {
                var data = Persistent_Delegate.GetData( v, c.ReverseMap );
                w.Data = data;
            },
            ( d, c ) =>
            {
                if( d is SerializedObject obj && obj.TryGetValue( KeyNames.VALUE, out var val ) )
                    d = val;
                return Persistent_Delegate.ToDelegate( d, c.ForwardMap );
            }
        );

        [MapsInheritingFrom( typeof( KeyValuePair<,> ) )]
        public static IDescriptor GetDescriptor<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var keyContext = selector.Select( new ContextSelectionArgs( 0, typeof( TKey ), typeof( TKey ), 2 ) );
            var valueContext = selector.Select( new ContextSelectionArgs( 1, typeof( TValue ), typeof( TValue ), 2 ) );

            return new MemberwiseDescriptor<KeyValuePair<TKey, TValue>>()
                .WithConstructor(
                    args => new KeyValuePair<TKey, TValue>( args[0] != null ? (TKey)args[0] : default, args[1] != null ? (TValue)args[1] : default ),
                    ("key", typeof( TKey )),
                    ("value", typeof( TValue ))
                )
                .WithReadonlyMember( "key", keyContext, kvp => kvp.Key )
                .WithReadonlyMember( "value", valueContext, kvp => kvp.Value );
        }

        // --- Value Tuples ---

        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        private static IDescriptor ProvideValueTuple2<T1, T2>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2> t, T2 v ) => t.Item2 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        private static IDescriptor ProvideValueTuple3<T1, T2, T3>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3> t, T3 v ) => t.Item3 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        private static IDescriptor ProvideValueTuple4<T1, T2, T3, T4>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4> t, T4 v ) => t.Item4 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        private static IDescriptor ProvideValueTuple5<T1, T2, T3, T4, T5>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T5 v ) => t.Item5 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        private static IDescriptor ProvideValueTuple6<T1, T2, T3, T4, T5, T6>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T6 v ) => t.Item6 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        private static IDescriptor ProvideValueTuple7<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T6 v ) => t.Item6 = v )
                .WithMember( "7", t => t.Item7, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T7 v ) => t.Item7 = v );
        }

        [MapsInheritingFrom( typeof( Array ) )]
        private static IDescriptor ProvideArray<T>( ContextKey context, Type targetType ) // context needs to be here because of the 'pass through' feature where one provider can handle families of contexts.
        {
            IDescriptor desc;
            if( targetType.IsArray && targetType.GetArrayRank() > 1 )
            {
                desc = new MultiDimensionalDescriptor<Array, T>()
                {
                    Factory = lengths => Array.CreateInstance( typeof( T ), lengths ),
                    GetLengths = arr =>
                    {
                        int[] lengths = new int[arr.Rank];
                        for( int i = 0; i < arr.Rank; i++ )
                            lengths[i] = arr.GetLength( i );
                        return lengths;
                    },
                    GetFlatValues = arr =>
                    {
                        T[] flat = new T[arr.Length];

                        int i = 0;
                        foreach( T value in arr )
                            flat[i++] = value;

                        return flat;
                    },
                    SetFlatValues = ( arr, flat ) =>
                    {
                        int count = Math.Min( arr.Length, flat.Length );

                        int i = 0;
                        foreach( var _ in arr )
                        {
                            if( i >= count )
                                break;

                            arr.SetValue( flat[i], i );
                            i++;
                        }
                    }
                };
                return desc;
            }

            desc = new IndexedCollectionDescriptor<T[], T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new T[capacity],
                ResizeMethod = ( arr, newSize ) =>
                {
                    if( arr == null || arr.Length != newSize )
                    {
                        if( arr != null )
                            Array.Resize( ref arr, newSize );
                        else
                            arr = new T[newSize];
                    }
                    return arr;
                },
                GetCountMethod = arr => arr.Length,
                GetElementMethod = ( arr, index ) => arr[index],
                SetElementMethod = ( arr, index, item ) => arr[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( List<> ) )]
        private static IDescriptor ProvideList<T>( ContextKey context )
        {
            var desc = new IndexedCollectionDescriptor<List<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new List<T>( capacity ),
                ResizeMethod = ( list, newSize ) =>
                {
                    list.Clear();
                    if( list.Capacity < newSize )
                        list.Capacity = newSize;
                    for( int i = 0; i < newSize; i++ )
                        list.Add( default );
                    return list;
                },
                GetCountMethod = list => list.Count,
                GetElementMethod = ( list, index ) => list[index],
                SetElementMethod = ( list, index, item ) => list[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ) )]
        private static IDescriptor ProvideDictionary<TKey, TValue>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( context ),
                Factory = capacity => new Dictionary<TKey, TValue>( capacity ),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( HashSet<> ) )]
        private static IDescriptor ProvideHashSet<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<HashSet<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new HashSet<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Queue<> ) )]
        private static IDescriptor ProvideQueue<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Queue<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new Queue<T>( capacity ),
                AddMethod = ( coll, item ) => coll.Enqueue( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Stack<> ) )]
        private static IDescriptor ProvideStack<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<Stack<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new Stack<T>( capacity ),
                AddMethod = ( coll, item ) => coll.Push( item ),
                GetEnumerable = coll => { var arr = coll.ToArray(); Array.Reverse( arr ); return arr; },
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( LinkedList<> ) )]
        private static IDescriptor ProvideLinkedList<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<LinkedList<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new LinkedList<T>(),
                AddMethod = ( coll, item ) => coll.AddLast( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentQueue<> ) )]
        private static IDescriptor ProvideConcurrentQueue<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentQueue<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentQueue<T>(),
                AddMethod = ( coll, item ) => coll.Enqueue( item ),
                ClearMethod = coll => { while( coll.TryDequeue( out _ ) ) ; }
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentStack<> ) )]
        private static IDescriptor ProvideConcurrentStack<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentStack<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentStack<T>(),
                AddMethod = ( coll, item ) => coll.Push( item ),
                GetEnumerable = coll => { var arr = coll.ToArray(); Array.Reverse( arr ); return arr; },
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentBag<> ) )]
        private static IDescriptor ProvideConcurrentBag<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentBag<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentBag<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => { while( coll.TryTake( out _ ) ) ; }
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.Concurrent.ConcurrentDictionary<,> ) )]
        private static IDescriptor ProvideConcurrentDictionary<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>(),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( SortedDictionary<,> ) )]
        private static IDescriptor ProvideSortedDictionary<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<SortedDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new SortedDictionary<TKey, TValue>(),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( SortedList<,> ) )]
        private static IDescriptor ProvideSortedList<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );
            var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), 0 );

            ContextKey keyCtx = selector?.Select( args1 ) ?? ContextKey.Default;
            ContextKey valCtx = selector?.Select( args2 ) ?? ContextKey.Default;
            ContextKey kvpCtx = ContextKey.Default;
            if( keyCtx != ContextKey.Default || valCtx != ContextKey.Default )
            {
                kvpCtx = ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            var desc = new EnumeratedCollectionDescriptor<SortedList<TKey, TValue>, KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = new UniformSelector( kvpCtx ),
                Factory = capacity => new SortedList<TKey, TValue>( capacity ),
                AddMethod = ( coll, item ) => coll[item.Key] = item.Value,
                ClearMethod = coll => coll.Clear()
            };

            return desc;
        }

        [MapsInheritingFrom( typeof( SortedSet<> ) )]
        private static IDescriptor ProvideSortedSet<T>( ContextKey context )
        {
            var desc = new EnumeratedCollectionDescriptor<SortedSet<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new SortedSet<T>(),
                AddMethod = ( coll, item ) => coll.Add( item ),
                ClearMethod = coll => coll.Clear()
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( System.Collections.ObjectModel.ObservableCollection<> ) )]
        private static IDescriptor ProvideObservableCollection<T>( ContextKey context )
        {
            var desc = new IndexedCollectionDescriptor<System.Collections.ObjectModel.ObservableCollection<T>, T>()
            {
                ElementSelector = ContextRegistry.GetSelector( context ),
                Factory = capacity => new System.Collections.ObjectModel.ObservableCollection<T>(),
                ResizeMethod = ( coll, newSize ) =>
                {
                    coll.Clear();
                    for( int i = 0; i < newSize; i++ )
                        coll.Add( default );
                    return coll;
                },
                GetCountMethod = coll => coll.Count,
                GetElementMethod = ( coll, index ) => coll[index],
                SetElementMethod = ( coll, index, item ) => coll[index] = item
            };
            return desc;
        }

        [MapsInheritingFrom( typeof( Enum ) )]
        private static IDescriptor ProvideEnum<T>() where T : struct, Enum
        {
            return new EnumDescriptor<T>();
        }

        [MapsAnyInterface( ContextType = typeof( Ctx.Ref ) )]
        [MapsAnyClass( ContextType = typeof( Ctx.Ref ) )]
        private static IDescriptor ProvideReference<T>() where T : class
        {
            return new ReferenceDescriptor<T>();
        }
    }
}