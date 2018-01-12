﻿using System;
using Bonsai.Code.DomainModel.Facts.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// Blueprint of a fact's template and editor.
    /// </summary>
    public class FactDefinition<T> : IFactDefinition
        where T: FactModelBase
    {
        public FactDefinition(string id, string title, string shortTitle = null)
        {
            Id = id;
            Title = title;
            ShortTitle = shortTitle ?? title;
            Kind = typeof(T);
        }

        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Short title for displaying in the info block.
        /// </summary>
        public string ShortTitle { get; }

        /// <summary>
        /// Type of the fact's kind.
        /// </summary>
        public Type Kind { get; }
    }

    /// <summary>
    /// Shared interface for untyped fact definitions.
    /// </summary>
    public interface IFactDefinition
    {
        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Short title for displaying in the info block.
        /// </summary>
        string ShortTitle { get; }

        /// <summary>
        /// Type of the fact's kind.
        /// </summary>
        Type Kind { get; }
    }
}
