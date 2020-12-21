﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMvvm;
using Xamarin.Essentials;
using zoft.TinyMvvmExtensions.Attributes;
using zoft.TinyMvvmExtensions.Commands;
using zoft.TinyMvvmExtensions.Extensions;
using zoft.TinyMvvmExtensions.Models;
using zoft.TinyMvvmExtensions.WeakSubscription;

namespace zoft.TinyMvvmExtensions.ViewModels
{
    /// <summary>
    /// Core viewmodel, built on top of <see cref="TinyMvvm.ViewModelBase"/>
    /// </summary>
    public abstract class CoreViewModel : ViewModelBase, IDisposable
    {
        #region Base Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        private string _title = string.Empty;

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>The subtitle.</value>
        public string Subtitle
        {
            get => _subtitle;
            set => Set(ref _subtitle, value);
        }
        private string _subtitle = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreViewModel" /> class.
        /// </summary>
        /// <exception cref="NullReferenceException">IMvxLogProvider</exception>
        protected CoreViewModel()
        {
            InitializePropertyDependencies(GetType());

            InitializeMethodDependencies(GetType());

            InitializePropertyChanged();
        }

        #endregion

        #region Dependency Management

        /// <summary>
        /// Helper list used to prevent PropertyCHanged propagation in cases where an action is ExecuteWithoutConditionalDependsOn 
        /// </summary>
        private readonly Dictionary<string, int> _dependsOnConditionalCount = new Dictionary<string, int>();

        /// <summary>
        /// PropertyChanged subscriptipn of this instance
        /// </summary>
        private NotifyPropertyChangedEventSubscription _propertyChangedSubscription;

        /// <summary>
        /// List of all the properties that have DependsOn attribute configured
        /// </summary>
        private readonly Dictionary<string, IList<DependencyInfo>> _propertyDependencies = new Dictionary<string, IList<DependencyInfo>>();

        /// <summary>
        /// List of all the notifiable collections properties that have the PropagateCollectionChange attribute configured
        /// </summary>
        private readonly Dictionary<string, INotifyCollectionChanged> _notifiableCollectionsPropertyDependencies = new Dictionary<string, INotifyCollectionChanged>();

        /// <summary>
        /// List that holds the CollectionChanged subscriptions of NotifiableColelctions that have the PropagateCollectionChange attribute configured
        /// </summary>
        private readonly Dictionary<string, NotifyCollectionChangedEventSubscription> _notifiableCollectionsChangedSubscription = new Dictionary<string, NotifyCollectionChangedEventSubscription>();

        /// <summary>
        /// List of all the methods that have DependsOn attribute configured
        /// </summary>
        private readonly Dictionary<string, IList<MethodInfo>> _methodDependencies = new Dictionary<string, IList<MethodInfo>>();

        /// <summary>
        /// Gets a value indicating whether this instance should react to property changed events
        /// </summary>
        protected bool HasDependencies => _propertyDependencies.Count > 0 ||
                                           _notifiableCollectionsPropertyDependencies.Count > 0 ||
                                           _methodDependencies.Count > 0;

        /// <summary>
        /// Maps all the properties that have DependsOn and/or PropagateCollectionChange attributes configured
        /// </summary>
        private void InitializePropertyDependencies(Type type)
        {
            foreach (var property in type.GetProperties(true))
            {
                var attributes = property.GetCustomAttributes<DependsOnAttribute>(true);
                var dependsOnAttributes = attributes as DependsOnAttribute[] ?? attributes.ToArray();
                if (dependsOnAttributes?.Length > 0)
                {
                    lock (_propertyDependencies)
                    {
                        foreach (var attribute in dependsOnAttributes)
                        {
                            if (!_propertyDependencies.ContainsKey(attribute.Name))
                            {
                                _propertyDependencies.Add(attribute.Name, new List<DependencyInfo>());
                            }

                            _propertyDependencies[attribute.Name].Add(new DependencyInfo(property, attribute.IsConditional));
                        }
                    }
                }

                if (typeof(INotifyCollectionChanged).IsAssignableFrom(property.PropertyType))
                {
                    var attribute = property.GetCustomAttribute<PropagateCollectionChangeAttribute>(true);
                    if (attribute != null)
                    {
                        lock (_notifiableCollectionsPropertyDependencies)
                        {
                            var collection = property.GetValue(this, null) as INotifyCollectionChanged;
                            _notifiableCollectionsPropertyDependencies.Add(property.Name, collection);
                        }
                        lock (_notifiableCollectionsChangedSubscription)
                        {
                            _notifiableCollectionsChangedSubscription.Add(property.Name, null);
                        }
                    }
                }
            }

            if (type != typeof(CoreViewModel))
            {
                var newType = type.GetTypeInfo().BaseType;
                if (newType != null)
                    InitializePropertyDependencies(newType);
            }
        }

        /// <summary>
        /// Maps all the methods that have DependsOn attribute configured
        /// </summary>
        private void InitializeMethodDependencies(Type type)
        {
            //foreach (var method in type.GetTypeInfo().DeclaredMethods.Where(m => m.ReturnType.Equals(typeof(void)) && m.GetParameters().Length == 0))
            foreach (var method in type.GetTypeInfo().DeclaredMethods.Where(m => m.GetParameters().Length == 0))
            {
                var attributes = method.GetCustomAttributes<DependsOnAttribute>(true);
                var dependsOnAttributes = attributes as DependsOnAttribute[] ?? attributes.ToArray();
                if (dependsOnAttributes?.Length > 0)
                {
                    foreach (var attribute in dependsOnAttributes)
                    {
                        lock (_methodDependencies)
                        {
                            if (!_methodDependencies.ContainsKey(attribute.Name))
                                _methodDependencies.Add(attribute.Name, new List<MethodInfo>());
                            _methodDependencies[attribute.Name].Add(method);
                        }
                    }
                }
            }

            if (type != typeof(CoreViewModel))
            {
                var newType = type.GetTypeInfo().BaseType;
                if (newType != null)
                    InitializeMethodDependencies(newType);
            }
        }

        /// <summary>
        /// Initializes the listeners for property changed events
        /// </summary>
        private void InitializePropertyChanged()
        {
            if (HasDependencies)
            {
                _propertyChangedSubscription = this.WeakSubscribe(OnPropertyChanged);

                foreach (var item in _notifiableCollectionsPropertyDependencies)
                {
                    if (item.Value != null)
                        _notifiableCollectionsChangedSubscription[item.Key] = item.Value.WeakSubscribe(OnCollectionChanged);
                }
            }
        }

        /// <summary>
        /// Called when a property raises the <see cref="PropertyChangedEventHandler"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null)
                return;

            UpdateCollectionPropertyValue(e.PropertyName);
            RaiseDependenciesPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Updates the collection property dependency subscription.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void UpdateCollectionPropertyValue(string propertyName)
        {
            if (_notifiableCollectionsPropertyDependencies.TryGetValue(propertyName, out var collection))
            {
                var senderCollection = this.GetPropertyValue(propertyName) as INotifyCollectionChanged;
                if (!ReferenceEquals(collection, senderCollection))
                {
                    //Remove previous subscription
                    _notifiableCollectionsChangedSubscription[propertyName]?.Dispose();
                    _notifiableCollectionsChangedSubscription[propertyName] = null;

                    //Add new subscription
                    if (senderCollection != null)
                        _notifiableCollectionsChangedSubscription[propertyName] = senderCollection.WeakSubscribe(OnCollectionChanged);

                    _notifiableCollectionsPropertyDependencies[propertyName] = senderCollection;
                }
            }
        }

        /// <summary>
        /// Handles the collection changed events for the notifiable collections marked with the PropagateCollectionChange attribute
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var collection in _notifiableCollectionsPropertyDependencies.Where(nc => ReferenceEquals(sender, nc.Value)).OfType<KeyValuePair<string, INotifyCollectionChanged>?>())
            {
                RaiseDependenciesPropertyChanged(collection.Value.Key);
            }
        }

        /// <summary>
        /// Raises the dependencies property changed.
        /// </summary>
        /// <param name="dependencyName">Name of the dependency.</param>
        public void RaiseDependenciesPropertyChanged(string dependencyName)
        {
            // Ensure this method runs in the main thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => RaiseDependenciesPropertyChanged(dependencyName));
                return;
            }

            //Prevents the conditional DependsOn from firing, if the execution was made
            //to prevent propagation (ExecuteWithoutConditionalDependsOn)
            if (_dependsOnConditionalCount.ContainsKey(dependencyName) &&
                _dependsOnConditionalCount[dependencyName] > 0)
            {
                return;
            }

            lock (_propertyDependencies)
            {
                if (_propertyDependencies.TryGetValue(dependencyName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (typeof(ICommand).IsAssignableFrom(property.Info.PropertyType))
                        {
                            var command = property.Info.GetValue(this, null) as SyncCommand;
                            command?.RaiseCanExecuteChanged();
                        }
                        else if (typeof(IAsyncCommand).IsAssignableFrom(property.Info.PropertyType))
                        {
                            var command = property.Info.GetValue(this, null) as AsyncCommand;
                            command?.RaiseCanExecuteChanged();
                        }
                        else
                        {
                            RaisePropertyChanged(property.Info.Name);
                        }
                    }
                }
            }

            lock (_methodDependencies)
            {
                if (_methodDependencies.TryGetValue(dependencyName, out var methods))
                {
                    foreach (var method in methods)
                    {
                        method.Invoke(this, null);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the property changed handlers.
        /// </summary>
        private void RemovePropertyChangedHandlers()
        {
            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.Dispose();
                _propertyChangedSubscription = null;
            }
        }

        /// <summary>
        /// Removes the collection changed handlers.
        /// </summary>
        private void RemoveCollectionChangedHandlers()
        {
            foreach (var item in _notifiableCollectionsChangedSubscription)
            {
                if (item.Value == null)
                    continue;
                try
                {
                    item.Value.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // This error might occur during dispose.
                }
            }
            _notifiableCollectionsChangedSubscription.Clear();
        }

        /// <summary>
        /// Executes the action, preventing the propagation of DependsOn
        /// that are marked with the 'IsConditional' flag for the specified property
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="action">The action.</param>
        protected void ExecuteWithoutConditionalDependsOn(string propertyName, Action action)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (!_dependsOnConditionalCount.ContainsKey(propertyName))
                        _dependsOnConditionalCount.Add(propertyName, 1);
                    else
                        _dependsOnConditionalCount[propertyName]++;

                    action.Invoke();

                    _dependsOnConditionalCount[propertyName] = Math.Max(0, _dependsOnConditionalCount[propertyName] - 1);
                }
                catch
                {
                    _dependsOnConditionalCount.Remove(propertyName);
                }
            });
        }

        #endregion

        #region Busy Notification Management

        private int _busyCount;

        /// <summary>
        /// Message to be shown in the busy indicator
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            set => Set(ref _busyMessage, value);
        }
        private string _busyMessage;

        /// <summary>
        /// Executes work asynchronously.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="workMessage">The work message.</param>
        /// <param name="isSilent">if set to <c>true</c> [is silent].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        protected virtual async Task DoWorkAsync(Func<Task> action, string workMessage = null, bool isSilent = false)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            StartWork(workMessage, isSilent);

            try
            {
                await action.Invoke().ConfigureAwait(false);

                FinishWork(isSilent);
            }
            catch
            {
                FinishWork(isSilent);
                throw;
            }
        }

        /// <summary>
        /// Executes work asynchronously and returns the result of the action invocation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="workMessage">The work message.</param>
        /// <param name="isSilent">if set to <c>true</c> [is silent].</param>
        /// <returns></returns>
        protected virtual async Task<T> DoWorkAsync<T>(Func<Task<T>> action, string workMessage = null, bool isSilent = false)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            StartWork(workMessage, isSilent);

            try
            {
                var result = await action.Invoke().ConfigureAwait(false);

                FinishWork(isSilent);

                return result;
            }
            catch
            {
                FinishWork(isSilent);
                throw;
            }
        }

        /// <summary>
        /// Signals the IsBusy to indicate that a new work has started
        /// </summary>
        /// <param name="isSilent">if set to <c>true</c> the IsBusy will no be signaled.</param>
        protected virtual void StartWork(bool isSilent = false)
        {
            if (!isSilent)
            {
                Interlocked.Increment(ref _busyCount);
                IsBusy = _busyCount > 0;
            }
        }

        /// <summary>
        /// Signals the IsBusy to indicate that a new work has started and sets busy message.
        /// </summary>
        /// <param name="message">The busy message.</param>
        /// <param name="isSilent">if set to <c>true</c> the IsBusy will no be signaled.</param>
        public virtual void StartWork(string message, bool isSilent = false)
        {
            if (!isSilent && !message.IsNullOrWhiteSpace())
            {
                BusyMessage = message;
            }

            StartWork(isSilent);
        }

        /// <summary>
        /// Signals the IsBusy to indicate that work is finished.
        /// </summary>
        /// <param name="isSilent">if set to <c>true</c> the IsBusy will no be signaled.</param>
        public virtual void FinishWork(bool isSilent = false)
        {
            if (!isSilent)
            {
                Interlocked.Decrement(ref _busyCount);
                IsBusy = _busyCount > 0;

                if (_busyCount <= 0)
                {
                    BusyMessage = null;
                }
            }
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    DisposeManagedResources();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                DisposeUnmanagedResources();

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            RemovePropertyChangedHandlers();
            RemoveCollectionChangedHandlers();
        }

        /// <summary>
        /// Disposes the unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanagedResources() { }

        #endregion
    }
}