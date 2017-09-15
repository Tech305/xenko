// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    internal class NodeViewModelDynamicMetaObject : DynamicMetaObject
    {
        private readonly NodeViewModel node;

        public NodeViewModelDynamicMetaObject([NotNull] Expression parameter, NodeViewModel node)
            : base(parameter, BindingRestrictions.Empty, node)
        {
            this.node = node;
        }

        [NotNull]
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression.Convert(Expression, LimitType);

            Expression expression;
            var propertyName = binder.Name;
            var args = new Expression[1];

            if (binder.Name.StartsWith(GraphViewModel.HasChildPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasChildPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModel).GetMethod(nameof(NodeViewModel.GetChild), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(GraphViewModel.HasCommandPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasCommandPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModel).GetMethod(nameof(NodeViewModel.GetCommand), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(GraphViewModel.HasAssociatedDataPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasAssociatedDataPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModel).GetMethod(nameof(NodeViewModel.GetAssociatedData), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else
            {
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModel).GetMethod(nameof(NodeViewModel.GetDynamicObject), BindingFlags.Public | BindingFlags.Instance), args);
            }

            var getMember = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            return getMember;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return node.Children.Select(x => x.Name).Concat(node.Commands.Select(x => x.Name)).Concat(node.AssociatedData.Select(x => x.Key));
        }
    }
}
