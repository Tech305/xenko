// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    /// <summary>
    /// An abstract class that is the base implementation of the <see cref="ICommandBase"/> interface.
    /// </summary>
    public abstract class CommandBase : DispatcherViewModel, ICommandBase
    {
        /// <summary>
        /// Backing field for the <see cref="IsEnabled"/> property.
        /// </summary>
        private bool isEnabled = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        protected CommandBase([NotNull] IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public bool IsEnabled { get { return isEnabled; } set { SetValue(ref isEnabled, value, InvokeCanExecute); } }

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;
        
        /// <inheritdoc/>
        public virtual bool CanExecute(object parameter)
        {
            return isEnabled;
        }

        /// <inheritdoc/>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Invokes <see cref="CanExecute(object)"/> with a <c>null</c> argument.
        /// </summary>
        /// <returns><c>true</c> if <see cref="CanExecute(object)"/> returned <c>true</c>, <c>false</c> otherwise.</returns>
        public bool CanExecute()
        {
            return CanExecute(null);
        }

        /// <summary>
        /// Invokes the <see cref="Execute(object)"/> command with a <c>null</c> argument.
        /// </summary>
        public void Execute()
        {
            Execute(null);
        }

        private void InvokeCanExecute()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}