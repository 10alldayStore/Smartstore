﻿using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileProviders;
using Smartstore.IO;

namespace Smartstore.Utilities
{
    /// <summary>
    /// Deterministic hash code combiner
    /// </summary>
    [DebuggerDisplay("{CombinedHashString}")]
    public struct HashCodeCombiner
    {
        private long _combinedHash64;

        public int CombinedHash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _combinedHash64.GetHashCode(); }
        }

        public string CombinedHashString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _combinedHash64.GetHashCode().ToString("x", CultureInfo.InvariantCulture); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HashCodeCombiner(long seed)
        {
            _combinedHash64 = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(IEnumerable e)
        {
            if (e == null)
            {
                Add(0);
            }
            else
            {
                var count = 0;
                foreach (object o in e)
                {
                    Add(o);
                    count++;
                }
                Add(count);
            }

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(HashCodeCombiner self)
        {
            return self.CombinedHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(int i)
        {
            _combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ i;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(string s)
        {
            var hashCode = (s != null) ? XxHashUnsafe.ComputeHash(s) : 0;
            return Add((int)hashCode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(object o)
        {
            int hashCode = (o != null) 
                ? (o is string s ? (int)XxHashUnsafe.ComputeHash(s) : o.GetHashCode())
                : 0;

            return Add(hashCode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(DateTime d)
        {
            return Add(d.GetHashCode());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add(DateTimeOffset d)
        {
            return Add(d.GetHashCode());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashCodeCombiner Add<TValue>(TValue value, IEqualityComparer<TValue> comparer)
        {
            var hashCode = value != null ? comparer.GetHashCode(value) : 0;
            return Add(hashCode);
        }

        public HashCodeCombiner Add(IFileEntry entry, bool deep = true)
        {
            Guard.NotNull(entry, nameof(entry));

            if (!entry.Exists)
                return this;

            Add(entry.PhysicalPath.ToLower());
            Add(entry.LastModified);

            if (entry is IFile file)
            {
                Add(file.Length.GetHashCode());
            }

            if (entry is IDirectory dir)
            {
                foreach (var e in dir.EnumerateFiles(deep: deep))
                {
                    Add(e);
                }
            }

            return this;
        }

        public HashCodeCombiner Add(FileSystemInfo fi, bool deep = true)
        {
            Guard.NotNull(fi, nameof(fi));

            if (!fi.Exists)
                return this;

            Add(fi.FullName.ToLower());
            Add(fi.CreationTimeUtc);
            Add(fi.LastWriteTimeUtc);

            if (fi is FileInfo file)
            {
                Add(file.Length.GetHashCode());
            }

            if (fi is DirectoryInfo dir)
            {
                foreach (var f in dir.GetFiles())
                {
                    Add(f);
                }
                if (deep)
                {
                    foreach (var s in dir.GetDirectories())
                    {
                        Add(s);
                    }
                }
            }

            return this;
        }

        public HashCodeCombiner Add(IFileInfo entry)
        {
            Guard.NotNull(entry, nameof(entry));

            if (!entry.Exists)
                return this;

            Add(entry.PhysicalPath.ToLower());
            Add(entry.LastModified);

            if (!entry.IsDirectory)
            {
                Add(entry.Length.GetHashCode());
            }

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashCodeCombiner Start()
        {
            return new HashCodeCombiner(0x1505L);
        }
    }
}
