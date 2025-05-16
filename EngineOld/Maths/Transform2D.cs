using FMOD;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

/// <summary>
/// Describes a 2D transformation.
/// </summary>
public struct Transform2D : IEquatable<Transform2D>, ICloneable
{
    /// <summary>
    /// The position described by this transform
    /// </summary>
    public Vector2 Position;
    /// <summary>
    /// The scale described by this transform
    /// </summary>
    public Vector2 Scale;
    /// <summary>
    /// The rotation described by this transform, measured in radians.
    /// </summary>
    public float Radians;

    private static readonly Transform2D _zero = new(0, 0, 0, 0, 0);
    private static readonly Transform2D _default = new();

    /// <summary>
    /// Default transformation, with the following values:
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Position: {0, 0}</item>
    /// <item>Scale: {1, 1}</item>
    /// <item>Radians: 0</item>
    /// </list>
    /// </remarks>
    public static Transform2D Default => _default;

    /// <summary>
    /// Transformation with all it's values initialized to zero.
    /// </summary>
    public static Transform2D Zero => _zero;

    /// <summary>
    /// Gets the position along the X axis.
    /// </summary>
    public float PositionX
    {
        get => Position.X;
        set => Position.X = value;
    }
    
    /// <summary>
    /// Gets the position along the Y axis.
    /// </summary>
    public float PositionY
    {
        get => Position.Y;
        set => Position.Y = value;
    }

    /// <summary>
    /// Gets the scale along the X axis.
    /// </summary>
    public float ScaleX
    {
        get => Scale.X;
        set => Scale.X = value;
    }

    /// <summary>
    /// Gets the scale along the Y axis.
    /// </summary>
    public float ScaleY
    {
        get => Scale.Y;
        set => Scale.Y = value;
    }

    /// <summary>
    /// Gets the rotation described by this transform, measured in angle degrees.
    /// </summary>
    public float AngleDegrees
    {
        get => GameMath.ToDegrees(Radians);
        set => Radians = GameMath.ToRadians(value);
    }

    #region CONSTRUCTORS
    /// <summary>
    /// Initializes a new <see cref="Transform2D"/> with default values.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Position: {0, 0}</item>
    /// <item>Scale: {1, 1}</item>
    /// <item>Radians: 0</item>
    /// </list>
    /// </remarks>
    public Transform2D()
    {
        Position = Vector2.Zero;
        Scale = Vector2.One;
        Radians = 0f;
    }
    /// <summary>
    /// Initializes a new <see cref="Transform2D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector2"/> instead to initialize only the position.</remarks>
    /// <param name="position">The position to initialize the transform. Can also be passed directly as a <see cref="Vector2"/>.</param>
    public Transform2D(Vector2 position, Vector2 scale, float radians = 0)
    {
        Position = position;
        Scale = scale;
        Radians = radians;
    }
    /// <summary>
    /// Initializes a new <see cref="Transform2D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector2"/> instead to initialize only the position.</remarks>
    /// <param name="position">The position to initialize the transform. Can also be passed directly as a <see cref="Vector2"/>.</param>
    public Transform2D(Vector2 position, float scaleX = 1f, float scaleY = 1f, float radians = 0f) : this(position, new Vector2(scaleX, scaleY), radians) { }
    /// <summary>
    /// Initializes a new <see cref="Transform2D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector2"/> instead to initialize only the position.</remarks>
    public Transform2D(float positionX, float positionY, Vector2 scale, float radians = 0) : this(new Vector2(positionX, positionY), scale, radians) { }
    /// <summary>
    /// Initializes a new <see cref="Transform2D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector2"/> instead to initialize only the position.</remarks>
    public Transform2D(float positionX, float positionY, float scaleX = 1f, float scaleY = 1f, float radians = 0f) : this(new Vector2(positionX, positionY), new Vector2(scaleX, scaleY), radians) { }
    #endregion

    #region CONSTRUCTOR HELPERS
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified position.
    /// </summary>
    /// <remarks>Alternatively, a <see cref="Vector2"></see> can be passed, and it will be converted automatically.</remarks>
    public static Transform2D FromPosition(Vector2 position) => new(position);
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified position.
    /// </summary>
    /// <remarks>Alternatively, a <see cref="Vector2"></see> can be passed, and it will be converted automatically.</remarks>
    public static Transform2D FromPosition(float scaleX, float scaleY) => new(scaleX, scaleY);
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform2D FromScale(Vector2 scale) => new() { Scale = scale };
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform2D FromScale(float scaleX, float scaleY) => new() { Scale = new Vector2(scaleX, scaleY) };
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform2D FromScale(float scale) => new() { Scale = new Vector2(scale)};
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified rotation.
    /// </summary>
    public static Transform2D FromRadians(float radians) => new() { Radians = radians };
    /// <summary>
    /// Creates a <see cref="Transform2D"></see> initialized with a specified rotation.
    /// </summary>
    public static Transform2D FromDegrees(float degrees) => new() { AngleDegrees = degrees };
    #endregion

    #region MATRIX
    /// <summary>
    /// Gets a matrix that describes this transformation.
    /// </summary>
    public readonly Matrix GetMatrix()
    {
        float rotationCos = MathF.Cos(Radians);
        float rotationSin = MathF.Sin(Radians);

        return new()
        {
            M11 = Scale.X * rotationCos, // Scale in X and rotate
            M12 = Scale.X * rotationSin,  // Scale in X and rotate
            M21 = -Scale.Y * rotationSin, // Scale in Y and rotate
            M22 = Scale.Y * rotationCos,   // Scale in Y and rotate
            M33 = 1f,
            M41 = Position.X, // Translate
            M42 = Position.Y, // Translate
            M44 = 1f
        };
    }

    /// <summary>
    /// Gets a matrix that describes the inverse of this transformation.
    /// </summary>
    public readonly Matrix GetInverseMatrix() => Matrix.Invert(GetMatrix());
    #endregion

    #region MODIFY INSTANCE
    /// <summary>
    /// Rotates this <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="radians">Rotation, measured in radians.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D RotateRadians(float radians)
    {
        Radians += radians;
        return this;
    }

    /// <summary>
    /// Rotates this <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="degrees">Rotation, measured in angle degrees.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D RotateDegrees(float degrees)
    {
        float radians = GameMath.ToRadians(degrees);
        Radians += radians;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="degrees">Rotation, measured in angle degrees.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D ScaleBy(float scale)
    {
        Scale *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D ScaleBy(Vector2 scale)
    {
        Scale *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D ScaleBy(float scaleX, float scaleY)
    {
        Scale.X *= scaleX;
        Scale.Y *= scaleY;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D ScaleXBy(float scale)
    {
        Scale.X *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D ScaleYBy(float scale)
    {
        Scale.Y *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D Translate(Vector2 translation)
    {
        Position += translation;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D Translate(float translationX, float translationY)
    {
        Position.X += translationX;
        Position.Y += translationY;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D TranslateX(float translation)
    {
        Position.X += translation;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D TranslateY(float translation)
    {
        Position.Y += translation;
        return this;
    }

    /// <summary>
    /// Adds the position, scale and rotation of two <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D Compose(Transform2D other)
    {
        Position += other.Position;
        Scale += other.Scale;
        Radians += other.Radians;

        return this;
    }

    /// <summary>
    /// Subtracts the position, scale and rotation of two <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D Reverse(Transform2D other)
    {
        Position -= other.Position;
        Scale -= other.Scale;
        Radians -= other.Radians;
        return this;
    }

    /// <summary>
    /// Resets the position and rotation to zero, and scale to one.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform2D Reset()
    {
        Position = Vector2.Zero;
        Scale = Vector2.One;
        Radians = 0f;
        return this;
    }
    #endregion

    #region MODIFY STATIC
    /// <summary>
    /// Adds the position, scale and rotation of two <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public static Transform2D Compose(Transform2D left, Transform2D right) => new(left.Position + right.Position,
                                                                              left.Scale + right.Scale,
                                                                              left.Radians + right.Radians);

    /// <summary>
    /// Subtracts the position, scale and rotation of two <see cref="Transform2D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public static Transform2D Reverse(Transform2D left, Transform2D right) => new(left.Position - right.Position,
                                                                                   left.Scale - right.Scale,
                                                                                   left.Radians - right.Radians);
    #endregion

    #region OPERATORS
    // Arithmetic
    public static Transform2D operator +(Transform2D left, Transform2D right) => new(left.Position + right.Position,
                                                                                     left.Scale + right.Scale,
                                                                                     left.Radians + right.Radians);
    public static Transform2D operator -(Transform2D left, Transform2D right) => new(left.Position - right.Position,
                                                                                     left.Scale - right.Scale,
                                                                                     left.Radians - right.Radians);

    // Logical
    public static bool operator ==(Transform2D left, Transform2D right) => left.Equals(right);
    public static bool operator !=(Transform2D left, Transform2D right) => !left.Equals(right);

    // Cast
    public static implicit operator Matrix(Transform2D transform) => transform.GetMatrix();
    public static implicit operator Transform2D(Vector2 position) => new(position);
    public static explicit operator Vector2(Transform2D transform) => transform.Position;
    #endregion

    #region OVERRIDE / INTERFACES
    public readonly bool Equals(Transform2D other) => Position == other.Position
                                                      && Scale == other.Scale
                                                      && Radians == other.Radians;
    public override readonly bool Equals(object obj) => obj is Transform2D other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Position, Scale, Radians);
    public override readonly string ToString() => $"position: {Position}, scale: {Scale}, radians: {Radians}";

    public readonly object Clone() => new Transform2D(Position, Scale, Radians);
    /// <summary>
    /// Creates an identical copy of this <see cref="Transform2D"/>.
    /// </summary>
    public readonly Transform2D CloneTransform() => new(Position, Scale, Radians);
    #endregion
}
