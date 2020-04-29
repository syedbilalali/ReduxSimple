﻿using Converto;
using System;
using System.Linq;

namespace ReduxSimple
{
    public static partial class Reducers
    {
        /// <summary>
        /// Create a sub-reducers, for features-like purpose.
        /// </summary>
        /// <typeparam name="TState">Type of the root state.</typeparam>
        /// <typeparam name="TFeatureState">Type of the feature state.</typeparam>
        /// <param name="featureReducers">Reducers that directly use the feature state.</param>
        /// <param name="selectFeature">Select the feature from the root state.</param>
        /// <returns>Returns reducers targeting the root state.</returns>
        [Obsolete("Please use the new version of CreateSubReducers.")]
        public static On<TState>[] CreateSubReducers<TState, TFeatureState>(
            On<TFeatureState>[] featureReducers,
            Func<TState, TFeatureState> selectFeature
        )
            where TState : class, new()
            where TFeatureState : class, new()
        {
            var parentStateProperties = typeof(TState).GetProperties();

            return featureReducers
                .Select(r =>
                {
                    return new On<TState>
                    {
                        Reduce = (state, action) =>
                        {
                            if (r?.Reduce == null)
                                return state;

                            var featureState = selectFeature(state);
                            var reducerResult = r.Reduce(featureState, action);

                            if (featureState.IsDeepEqual(reducerResult))
                            {
                                return state;
                            }

                            var featureProperty = parentStateProperties
                                .SingleOrDefault(p =>
                                {
                                    return p.GetValue(state) == featureState;
                                });

                            if (featureProperty == null)
                            {
                                throw new NotSupportedException(
                                    $"A sub-reducer cannot find the feature reducer of `{typeof(TFeatureState).Name}` inside `{typeof(TState).Name}`."
                                );
                            }

                            var stateCopy = state.Copy();
                            featureProperty.SetValue(stateCopy, reducerResult);

                            return stateCopy;
                        },
                        Types = r.Types
                    };
                })
                .ToArray();
        }
        /// <summary>
        /// Create a sub-reducers, for features-like purpose.
        /// </summary>
        /// <typeparam name="TState">Type of the root state.</typeparam>
        /// <typeparam name="TFeatureState">Type of the feature state.</typeparam>
        /// <param name="featureReducers">Reducers that directly use the feature state.</param>
        /// <param name="selectFeature">Select the feature from the root state.</param>
        /// <returns>Returns reducers targeting the root state.</returns>
        [Obsolete("Please use the new version of CreateSubReducers.")]
        public static On<TState>[] CreateSubReducers<TState, TFeatureState>(
            On<TFeatureState>[] featureReducers,
            ISelectorWithoutProps<TState, TFeatureState?> selectFeature
        )
            where TState : class, new()
            where TFeatureState : class, new()
        {
            var parentStateProperties = typeof(TState).GetProperties();

            return featureReducers
                .Select(r =>
                {
                    return new On<TState>
                    {
                        Reduce = (state, action) =>
                        {
                            if (r?.Reduce == null)
                                return state;

                            var featureState = selectFeature.Apply(state);

                            if (featureState == null)
                                return state;

                            var reducerResult = r.Reduce(featureState, action);

                            if (featureState.IsDeepEqual(reducerResult))
                            {
                                return state;
                            }

                            var featureProperty = parentStateProperties
                                .SingleOrDefault(p =>
                                {
                                    return p.GetValue(state) == featureState;
                                });

                            if (featureProperty == null)
                            {
                                throw new NotSupportedException(
                                    $"A sub-reducer cannot find the feature reducer of `{typeof(TFeatureState).Name}` inside `{typeof(TState).Name}`."
                                );
                            }

                            var stateCopy = state.Copy();
                            featureProperty.SetValue(stateCopy, reducerResult);

                            return stateCopy;
                        },
                        Types = r.Types
                    };
                })
                .ToArray();
        }

        public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(Func<TState, TFeatureState> featureSelector)
            where TState : class, new()
            where TFeatureState : class, new()
        {
            return new ImplicitStateLens<TState, TFeatureState>(featureSelector);
        }
        public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(ISelectorWithoutProps<TState, TFeatureState> featureSelector)
            where TState : class, new()
            where TFeatureState : class, new()
        {
            var selector = new Func<TState, TFeatureState>(state => featureSelector.Apply(state));
            return new ImplicitStateLens<TState, TFeatureState>(selector);
        }
        public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(
            Func<TState, TFeatureState> featureSelector,
            Func<TState, TFeatureState, TState> stateReducer
        )
            where TState : class, new()
            where TFeatureState : class, new()
        {
            return new ExplicitStateLens<TState, TFeatureState>(featureSelector, stateReducer);
        }
        public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(
            ISelectorWithoutProps<TState, TFeatureState> featureSelector,
            Func<TState, TFeatureState, TState> stateReducer
        )
            where TState : class, new()
            where TFeatureState : class, new()
        {
            var selector = new Func<TState, TFeatureState>(state => featureSelector.Apply(state));
            return new ExplicitStateLens<TState, TFeatureState>(selector, stateReducer);
        }
    }
}
