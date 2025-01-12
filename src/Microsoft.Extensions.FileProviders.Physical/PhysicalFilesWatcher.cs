﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders.Physical
{
    /// <summary>
    ///     <para>
    ///     A file watcher that watches a physical filesystem for changes.
    ///     </para>
    ///     <para>
    ///     Triggers events on <see cref="IChangeToken" /> when files are created, change, renamed, or deleted.
    ///     </para>
    /// </summary>
    public partial class PhysicalFilesWatcher : IDisposable
    {
        private static readonly Action<object> _cancelTokenSource = state => ((CancellationTokenSource)state).Cancel();

        public static TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(4);

        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _filePathTokenLookup =
            new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _wildcardTokenLookup =
            new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);

        private readonly FileSystemWatcher _fileWatcher;
        private readonly object _fileWatcherLock = new object();
        private readonly string _root;
        private readonly ExclusionFilters _filters;

        private Timer _timer;
        private bool _timerInitialzed;
        private object _timerLock = new object();
        private Func<Timer> _timerFactory;

        /// <summary>
        /// Initializes an instance of <see cref="PhysicalFilesWatcher" /> that watches files in <paramref name="root" />.
        /// Wraps an instance of <see cref="System.IO.FileSystemWatcher" />
        /// </summary>
        /// <param name="root">Root directory for the watcher</param>
        /// <param name="fileSystemWatcher">The wrapped watcher that is watching <paramref name="root" /></param>
        /// <param name="pollForChanges">
        /// True when the watcher should use polling to trigger instances of
        /// <see cref="IChangeToken" /> created by <see cref="CreateFileChangeToken(string)" />
        /// </param>
        public PhysicalFilesWatcher(
            string root,
            FileSystemWatcher fileSystemWatcher,
            bool pollForChanges)
            : this(root, fileSystemWatcher, pollForChanges, ExclusionFilters.Sensitive)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="PhysicalFilesWatcher" /> that watches files in <paramref name="root" />.
        /// Wraps an instance of <see cref="System.IO.FileSystemWatcher" />
        /// </summary>
        /// <param name="root">Root directory for the watcher</param>
        /// <param name="fileSystemWatcher">The wrapped watcher that is watching <paramref name="root" /></param>
        /// <param name="pollForChanges">
        /// True when the watcher should use polling to trigger instances of
        /// <see cref="IChangeToken" /> created by <see cref="CreateFileChangeToken(string)" />
        /// </param>
        /// <param name="filters">Specifies which files or directories are excluded. Notifications of changes to are not raised to these.</param>
        public PhysicalFilesWatcher(
            string root,
            FileSystemWatcher fileSystemWatcher,
            bool pollForChanges,
            ExclusionFilters filters)
        {
            _root = root;
            _fileWatcher = fileSystemWatcher;
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.Created += OnChanged;
            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Renamed += OnRenamed;
            _fileWatcher.Deleted += OnChanged;
            _fileWatcher.Error += OnError;

            PollForChanges = pollForChanges;
            _filters = filters;

            PollingChangeTokens = new ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>();
            _timerFactory = () => NonCapturingTimer.Create(RaiseChangeEvents, state: PollingChangeTokens, dueTime: TimeSpan.Zero, period: DefaultPollingInterval);
        }

        public bool PollForChanges { get; }

        public bool UseActivePolling { get; set; }

        public ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken> PollingChangeTokens { get; }

        /// <summary>
        ///     <para>
        ///     Creates an instance of <see cref="IChangeToken" /> for all files and directories that match the
        ///     <paramref name="filter" />
        ///     </para>
        ///     <para>
        ///     Globbing patterns are relative to the root directory given in the constructor
        ///     <seealso cref="PhysicalFilesWatcher(string, FileSystemWatcher, bool)" />. Globbing patterns
        ///     are interpreted by <seealso cref="Matcher" />.
        ///     </para>
        /// </summary>
        /// <param name="filter">A globbing pattern for files and directories to watch</param>
        /// <returns>A change token for all files that match the filter</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="filter" /> is null</exception>
        public IChangeToken CreateFileChangeToken(string filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            filter = NormalizePath(filter);

            // Absolute paths and paths traversing above root not permitted.
            if (Path.IsPathRooted(filter) || PathUtils.PathNavigatesAboveRoot(filter))
            {
                return NullChangeToken.Singleton;
            }

            var changeToken = GetOrAddChangeToken(filter);
            TryEnableFileSystemWatcher();

            return changeToken;
        }

        private IChangeToken GetOrAddChangeToken(string pattern)
        {
            if (UseActivePolling)
            {
                LazyInitializer.EnsureInitialized(ref _timer, ref _timerInitialzed, ref _timerLock, _timerFactory);
            }

            IChangeToken changeToken;
            var isWildCard = pattern.IndexOf('*') != -1;
            if (isWildCard || IsDirectoryPath(pattern))
            {
                changeToken = GetOrAddWildcardChangeToken(pattern);
            }
            else
            {
                changeToken = GetOrAddFilePathChangeToken(pattern);
            }

            return changeToken;
        }

        public IChangeToken GetOrAddFilePathChangeToken(string filePath)
        {
            if (!_filePathTokenLookup.TryGetValue(filePath, out var tokenInfo))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                tokenInfo = new ChangeTokenInfo(cancellationTokenSource, cancellationChangeToken);
                tokenInfo = _filePathTokenLookup.GetOrAdd(filePath, tokenInfo);
            }

            IChangeToken changeToken = tokenInfo.ChangeToken;
            if (PollForChanges)
            {
                // The expiry of CancellationChangeToken is controlled by this type and consequently we can cache it.
                // PollingFileChangeToken on the other hand manages its own lifetime and consequently we cannot cache it.
                var pollingChangeToken = new PollingFileChangeToken(new FileInfo(Path.Combine(_root, filePath)));

                if (UseActivePolling)
                {
                    pollingChangeToken.ActiveChangeCallbacks = true;
                    pollingChangeToken.CancellationTokenSource = new CancellationTokenSource();
                    PollingChangeTokens.TryAdd(pollingChangeToken, pollingChangeToken);
                }

                changeToken = new CompositeChangeToken(
                    new[]
                    {
                        changeToken,
                        pollingChangeToken,
                    });
            }

            return changeToken;
        }

        public IChangeToken GetOrAddWildcardChangeToken(string pattern)
        {
            if (!_wildcardTokenLookup.TryGetValue(pattern, out var tokenInfo))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationChangeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
                matcher.AddInclude(pattern);
                tokenInfo = new ChangeTokenInfo(cancellationTokenSource, cancellationChangeToken, matcher);
                tokenInfo = _wildcardTokenLookup.GetOrAdd(pattern, tokenInfo);
            }

            IChangeToken changeToken = tokenInfo.ChangeToken;
            if (PollForChanges)
            {
                // The expiry of CancellationChangeToken is controlled by this type and consequently we can cache it.
                // PollingFileChangeToken on the other hand manages its own lifetime and consequently we cannot cache it.
                var pollingChangeToken = new PollingWildCardChangeToken(_root, pattern);

                if (UseActivePolling)
                {
                    pollingChangeToken.ActiveChangeCallbacks = true;
                    pollingChangeToken.CancellationTokenSource = new CancellationTokenSource();
                    PollingChangeTokens.TryAdd(pollingChangeToken, pollingChangeToken);
                }

                changeToken = new CompositeChangeToken(
                    new[]
                    {
                        changeToken,
                        pollingChangeToken,
                    });
            }

            return changeToken;
        }

        /// <summary>
        /// Disposes the provider. Change tokens may not trigger after the provider is disposed.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Disposes the provider.
        /// </summary>
        /// <param name="disposing"><c>true</c> is invoked from <see cref="IDisposable.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            _fileWatcher.Dispose();
            _timer?.Dispose();
        }

        /// <summary>
        /// Destructor for <see cref="PhysicalFilesWatcher"/>.
        /// </summary>
        ~PhysicalFilesWatcher() => Dispose(false);

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            // For a file name change or a directory's name change notify registered tokens.
            OnFileSystemEntryChange(e.OldFullPath);
            OnFileSystemEntryChange(e.FullPath);

            if (Directory.Exists(e.FullPath))
            {
                try
                {
                    // If the renamed entity is a directory then notify tokens for every sub item.
#if NET35 || NET30 || NET20
                    foreach (
                        var newLocation in
                        Directory.GetFileSystemEntries(e.FullPath, "*"))
#else
                    foreach (
                        var newLocation in
                        Directory.EnumerateFileSystemEntries(e.FullPath, "*", SearchOption.AllDirectories))
#endif
                    {
                        // Calculated previous path of this moved item.
                        var oldLocation = Path.Combine(e.OldFullPath, newLocation.Substring(e.FullPath.Length + 1));
                        OnFileSystemEntryChange(oldLocation);
                        OnFileSystemEntryChange(newLocation);
                    }
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is SecurityException ||
                    ex is DirectoryNotFoundException ||
                    ex is UnauthorizedAccessException)
                {
                    // Swallow the exception.
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            OnFileSystemEntryChange(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            // Notify all cache entries on error.
            foreach (var path in _filePathTokenLookup.Keys)
            {
                ReportChangeForMatchedEntries(path);
            }
        }

        private void OnFileSystemEntryChange(string fullPath)
        {
            try
            {
                var fileSystemInfo = new FileInfo(fullPath);
                if (FileSystemInfoHelper.IsExcluded(fileSystemInfo, _filters))
                {
                    return;
                }

                var relativePath = fullPath.Substring(_root.Length);
                ReportChangeForMatchedEntries(relativePath);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is SecurityException ||
                ex is UnauthorizedAccessException)
            {
                // Swallow the exception.
            }
        }

        private void ReportChangeForMatchedEntries(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // System.IO.FileSystemWatcher may trigger events that are missing the file name,
                // which makes it appear as if the root directory is renamed or deleted. Moving the root directory
                // of the file watcher is not supported, so this type of event is ignored.
                return;
            }

            path = NormalizePath(path);

            var matched = false;
            if (_filePathTokenLookup.TryRemove(path, out var matchInfo))
            {
                CancelToken(matchInfo);
                matched = true;
            }

            foreach (var wildCardEntry in _wildcardTokenLookup)
            {
                var matchResult = wildCardEntry.Value.Matcher.Match(path);
                if (matchResult.HasMatches &&
                    _wildcardTokenLookup.TryRemove(wildCardEntry.Key, out matchInfo))
                {
                    CancelToken(matchInfo);
                    matched = true;
                }
            }

            if (matched)
            {
                TryDisableFileSystemWatcher();
            }
        }

        private void TryDisableFileSystemWatcher()
        {
            lock (_fileWatcherLock)
            {
                if (_filePathTokenLookup.IsEmpty &&
                    _wildcardTokenLookup.IsEmpty &&
                    _fileWatcher.EnableRaisingEvents)
                {
                    // Perf: Turn off the file monitoring if no files to monitor.
                    _fileWatcher.EnableRaisingEvents = false;
                }
            }
        }

        private void TryEnableFileSystemWatcher()
        {
            lock (_fileWatcherLock)
            {
                if ((!_filePathTokenLookup.IsEmpty || !_wildcardTokenLookup.IsEmpty) &&
                    !_fileWatcher.EnableRaisingEvents)
                {
                    // Perf: Turn off the file monitoring if no files to monitor.
                    _fileWatcher.EnableRaisingEvents = true;
                }
            }
        }

        private static string NormalizePath(string filter) => filter = filter.Replace('\\', '/');

        private static bool IsDirectoryPath(string path)
        {
            return path.Length > 0 &&
                (path[path.Length - 1] == Path.DirectorySeparatorChar ||
                path[path.Length - 1] == Path.AltDirectorySeparatorChar);
        }

        private static void CancelToken(ChangeTokenInfo matchInfo)
        {
            if (matchInfo.TokenSource.IsCancellationRequested)
            {
                return;
            }

            Task.Factory.StartNew(
                _cancelTokenSource,
                matchInfo.TokenSource,
                CancellationToken.None,
                GetCreationOptions(), // ## 苦竹 修改 ## TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
        }

        public static void RaiseChangeEvents(object state)
        {
            // Iterating over a concurrent bag gives us a point in time snapshot making it safe
            // to remove items from it.
            var changeTokens = (ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>)state;
            foreach (var item in changeTokens)
            {
                var token = item.Key;

                if (!token.HasChanged)
                {
                    continue;
                }

                if (!changeTokens.TryRemove(token, out _))
                {
                    // Move on if we couldn't remove the item.
                    continue;
                }

                // We're already on a background thread, don't need to spawn a background Task to cancel the CTS
                try
                {
                    token.CancellationTokenSource.Cancel();
                }
                catch
                {

                }
            }
        }

        private readonly struct ChangeTokenInfo
        {
            public ChangeTokenInfo(
                CancellationTokenSource tokenSource,
                CancellationChangeToken changeToken)
                : this(tokenSource, changeToken, matcher: null)
            {
            }

            public ChangeTokenInfo(
                CancellationTokenSource tokenSource,
                CancellationChangeToken changeToken,
                Matcher matcher)
            {
                TokenSource = tokenSource;
                ChangeToken = changeToken;
                Matcher = matcher;
            }

            public CancellationTokenSource TokenSource { get; }

            public CancellationChangeToken ChangeToken { get; }

            public Matcher Matcher { get; }
        }
    }
}
