using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

/// <summary>
/// A collection of weighted elements.
/// </summary>
/// <remarks><see cref="SelectRandom"/> can be used to retrieve a random element from this collection.</remarks>
public class WeightedCollection<T> : IEnumerable<T>
{
    private readonly RNG _randomGenerator;
    private readonly Dictionary<T, int> _weights;
    private int _totalWeight = 0;

    /// <summary>
    /// A collection containing all the elements.
    /// </summary>
    public IEnumerable<T> Elements => _weights.Keys;
    /// <summary>
    /// A collection containing all the elements' weights.
    /// </summary>
    public IEnumerable<int> Weights => _weights.Values;
    /// <summary>
    /// Gets the sum of all the elements' weights.
    /// </summary>
    public int TotalWeight => _totalWeight;
    /// <summary>
    /// Gets the number of elements in this collection.
    /// </summary>
    public int Count => _weights.Count;

    /// <summary>
    /// Instantiates a collection of weighted elements.
    /// </summary>
    /// <remarks><see cref="SelectRandom"/> can be used to retrieve a random element from this collection.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="numberGenerator"/> is null.</exception>
    public WeightedCollection(RNG numberGenerator)
    {
        ArgumentNullException.ThrowIfNull(numberGenerator);

        _weights = new();
        _randomGenerator = numberGenerator;
    }

    /// <returns>
    /// The non-negative weight of the specified <paramref name="element"/>, or <c>-1</c> if the element is not in the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    public int GetWeight(T element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (!_weights.TryGetValue(element, out int value)) return -1;

        return value;
    }

    /// <summary>
    /// Adds the specified <paramref name="element"/>, and sets it's weight.
    /// </summary>
    /// <remarks>This will not override the element's weight if it's already in the collection.<br/>
    /// <paramref name="weight"/> must be positive.</remarks>
    /// <returns>
    /// <c>true</c> if the element was successfully added to the collection;<br/>
    /// <c>false</c> if it was already in the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified <paramref name="weight"/> is negative.</exception>
    public bool Add(T element, int weight)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        if (_weights.ContainsKey(element))
        {
            return false;
        }

        _weights.Add(element, weight);

        return true;
    }

    /// <summary>
    /// Sets the weight of the specified <paramref name="element"/>.
    /// </summary>
    /// <remarks>If the element is not in the collection, it will <c>not</c> be added.<br/>
    /// <paramref name="weight"/> must be positive.</remarks>
    /// <returns>
    /// <c>true</c> if the element's weight was set successfully;<br/>
    /// <c>false</c> if the element was not in the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified <paramref name="weight"/> is negative.</exception>
    public bool SetWeight(T element, int weight)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        if (!_weights.ContainsKey(element))
        {
            return false;
        }

        _weights[element] = weight;

        return true;
    }

    /// <summary>
    /// Sets the weight of the specified <paramref name="element"/>.
    /// </summary>
    /// <remarks>If the element is not in the collection, it <c>will</c> be added.<br/>
    /// <paramref name="weight"/> must be positive.</remarks>
    /// <returns>
    /// <c>true</c> if the element was already in the collection;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified <paramref name="weight"/> is negative.</exception>
    public bool AddOrSetWeight(T element, int weight)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        int previousWeight = 0;
        bool result;

        if (_weights.ContainsKey(element))
        {
            previousWeight = _weights[element];
            _weights[element] = weight;
            result = true;
        }
        else
        {
            _weights.Add(element, weight);
            result = false;
        }

        _totalWeight -= previousWeight;
        _totalWeight += weight;

        return result;
    }

    /// <summary>
    /// Removes the specified <paramref name="element"/> from the collection.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the element was successfully removed from the collection;<br/>
    /// <c>false</c> if the element was not in the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    public bool Remove(T element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (!_weights.TryGetValue(element, out int weight)) return false;
        _totalWeight -= weight;

        _weights.Remove(element);

        return true;
    }

    /// <summary>Checks if the specified <paramref name="element"/> is in the collection.</summary>
    /// <returns>
    /// <c>true</c> if the element is in the collection;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is null.</exception>
    public bool Contains(T element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return _weights.ContainsKey(element);
    }

    /// <summary>
    /// Selects a random element from the collection using their weights.
    /// </summary>
    /// <remarks>If the collection is empty, an <see cref="InvalidOperationException"/> will be thrown.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
    public T SelectRandom()
    {
        if (_weights.Count == 0)
        {
            throw new InvalidOperationException("The collection was empty. There must be at least one element to select from.");
        }

        int randomWeight = _randomGenerator.Int(0, _totalWeight);
        T selectedElement = default;

        foreach (var kvp in _weights)
        {
            if (randomWeight < kvp.Value)
            {
                selectedElement = kvp.Key;
                break;
            }

            randomWeight -= kvp.Value;
        }

        return selectedElement;
    }

    public IEnumerator<T> GetEnumerator() => Elements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();

    /// <summary>
    /// Gets or sets the weight of the specified <paramref name="element"/>.
    /// </summary>
    /// <remarks>When setting the weight, if the element is not in the collection, it will be added.</remarks>
    /// <returns>
    /// The non-negative weight of the specified <paramref name="element"/>, or <c>-1</c> if the element is not in the collection.
    /// </returns>
    public int this[T element]
    {
        get => GetWeight(element);
        set => AddOrSetWeight(element, value);
    }
}
