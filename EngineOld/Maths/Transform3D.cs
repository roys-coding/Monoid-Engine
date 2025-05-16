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
/// Describes a 3D transformation.
/// </summary>
public struct Transform3D : IEquatable<Transform3D>, ICloneable
{
    /// <summary>
    /// The position described by this transform
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// The scale described by this transform
    /// </summary>
    public Vector3 Scale;
    /// <summary>
    /// The rotation described by this transform, measured in radians.
    /// </summary>
    public float Radians;

    private static readonly Transform3D _zero = new(0, 0, 0, 0, 0, 0, 0);
    private static readonly Transform3D _default = new();

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
    public static Transform3D Default => _default;

    /// <summary>
    /// Transformation with all it's values initialized to zero.
    /// </summary>
    public static Transform3D Zero => _zero;

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
    /// Initializes a new <see cref="Transform3D"/> with default values.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Position: {0, 0}</item>
    /// <item>Scale: {1, 1}</item>
    /// <item>Radians: 0</item>
    /// </list>
    /// </remarks>
    public Transform3D()
    {
        Position = Vector3.Zero;
        Scale = Vector3.One;
        Radians = 0f;
    }
    /// <summary>
    /// Initializes a new <see cref="Transform3D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector3"/> instead to initialize only the position.</remarks>
    /// <param name="position">The position to initialize the transform. Can also be passed directly as a <see cref="Vector3"/>.</param>
    public Transform3D(Vector3 position, Vector3 scale, float radians = 0)
    {
        Position = position;
        Scale = scale;
        Radians = radians;
    }
    /// <summary>
    /// Initializes a new <see cref="Transform3D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector3"/> instead to initialize only the position.</remarks>
    /// <param name="position">The position to initialize the transform. Can also be passed directly as a <see cref="Vector3"/>.</param>
    public Transform3D(Vector3 position, float scaleX = 1f, float scaleY = 1f, float scaleZ = 1f, float radians = 0f) : this(position, new Vector3(scaleX, scaleY, scaleZ), radians) { }
    /// <summary>
    /// Initializes a new <see cref="Transform3D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector3"/> instead to initialize only the position.</remarks>
    public Transform3D(float positionX, float positionY, float positionZ, Vector3 scale, float radians = 0) : this(new Vector3(positionX, positionY, positionZ), scale, radians) { }
    /// <summary>
    /// Initializes a new <see cref="Transform3D"/>
    /// </summary>
    /// <remarks>Pass a <see cref="Vector3"/> instead to initialize only the position.</remarks>
    public Transform3D(float positionX, float positionY, float positionZ, float scaleX = 1f, float scaleY = 1f, float scaleZ = 1f, float radians = 0f) : this(new Vector3(positionX, positionY, positionZ), new Vector3(scaleX, scaleY, scaleZ), radians) { }
    #endregion

    #region CONSTRUCTOR HELPERS
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified position.
    /// </summary>
    /// <remarks>Alternatively, a <see cref="Vector3"></see> can be passed, and it will be converted automatically.</remarks>
    public static Transform3D FromPosition(Vector3 position) => new(position);
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified position.
    /// </summary>
    /// <remarks>Alternatively, a <see cref="Vector3"></see> can be passed, and it will be converted automatically.</remarks>
    public static Transform3D FromPosition(float scaleX, float scaleY, float scaleZ) => new(scaleX, scaleY, scaleZ);
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform3D FromScale(Vector3 scale) => new() { Scale = scale };
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform3D FromScale(float scaleX, float scaleY, float scaleZ) => new() { Scale = new Vector3(scaleX, scaleY, scaleZ) };
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified scale.
    /// </summary>
    public static Transform3D FromScale(float scale) => new() { Scale = new Vector3(scale)};
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified rotation.
    /// </summary>
    public static Transform3D FromRadians(float radians) => new() { Radians = radians };
    /// <summary>
    /// Creates a <see cref="Transform3D"></see> initialized with a specified rotation.
    /// </summary>
    public static Transform3D FromDegrees(float degrees) => new() { AngleDegrees = degrees };
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
    /// Rotates this <see cref="Transform3D"/>.
    /// </summary>
    /// <param name="radians">Rotation, measured in radians.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D RotateRadians(float radians)
    {
        Radians += radians;
        return this;
    }

    /// <summary>
    /// Rotates this <see cref="Transform3D"/>.
    /// </summary>
    /// <param name="degrees">Rotation, measured in angle degrees.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D RotateDegrees(float degrees)
    {
        float radians = GameMath.ToRadians(degrees);
        Radians += radians;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <param name="degrees">Rotation, measured in angle degrees.</param>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleBy(float scale)
    {
        Scale *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleBy(Vector3 scale)
    {
        Scale *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleBy(float scaleX, float scaleY, float scaleZ)
    {
        Scale.X *= scaleX;
        Scale.Y *= scaleY;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleXBy(float scale)
    {
        Scale.X *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleYBy(float scale)
    {
        Scale.Y *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D ScaleZBy(float scale)
    {
        Scale.Z *= scale;
        return this;
    }

    /// <summary>
    /// Scales this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D Translate(Vector3 translation)
    {
        Position += translation;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D Translate(float translationX, float translationY, float translateZ)
    {
        Position.X += translationX;
        Position.Y += translationY;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D TranslateX(float translation)
    {
        Position.X += translation;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D TranslateY(float translation)
    {
        Position.Y += translation;
        return this;
    }

    /// <summary>
    /// Translates this <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D TranslateZ(float translation)
    {
        Position.Z += translation;
        return this;
    }

    /// <summary>
    /// Adds the position, scale and rotation of two <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D Compose(Transform3D other)
    {
        Position += other.Position;
        Scale += other.Scale;
        Radians += other.Radians;

        return this;
    }

    /// <summary>
    /// Subtracts the position, scale and rotation of two <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public Transform3D Reverse(Transform3D other)
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
    public Transform3D Reset()
    {
        Position = Vector3.Zero;
        Scale = Vector3.One;
        Radians = 0f;
        return this;
    }
    #endregion

    #region MODIFY STATIC
    /// <summary>
    /// Adds the position, scale and rotation of two <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public static Transform3D Compose(Transform3D left, Transform3D right) => new(left.Position + right.Position,
                                                                              left.Scale + right.Scale,
                                                                              left.Radians + right.Radians);

    /// <summary>
    /// Subtracts the position, scale and rotation of two <see cref="Transform3D"/>.
    /// </summary>
    /// <remarks>Effects are applied to this instance.<br/>
    /// Use <see cref="CloneTransform"/> if a copy of this transformation is needed.
    /// </remarks>
    public static Transform3D Reverse(Transform3D left, Transform3D right) => new(left.Position - right.Position,
                                                                                   left.Scale - right.Scale,
                                                                                   left.Radians - right.Radians);
    #endregion

    #region OPERATORS
    // Arithmetic
    public static Transform3D operator +(Transform3D left, Transform3D right) => new(left.Position + right.Position,
                                                                                     left.Scale + right.Scale,
                                                                                     left.Radians + right.Radians);
    public static Transform3D operator -(Transform3D left, Transform3D right) => new(left.Position - right.Position,
                                                                                     left.Scale - right.Scale,
                                                                                     left.Radians - right.Radians);

    // Logical
    public static bool operator ==(Transform3D left, Transform3D right) => left.Equals(right);
    public static bool operator !=(Transform3D left, Transform3D right) => !left.Equals(right);

    // Cast
    public static implicit operator Matrix(Transform3D transform) => transform.GetMatrix();
    public static implicit operator Transform3D(Vector3 position) => new(position);
    public static explicit operator Vector3(Transform3D transform) => transform.Position;
    #endregion

    #region OVERRIDE / INTERFACES
    public readonly bool Equals(Transform3D other) => Position == other.Position
                                                      && Scale == other.Scale
                                                      && Radians == other.Radians;
    public override readonly bool Equals(object obj) => obj is Transform3D other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Position, Scale, Radians);
    public override readonly string ToString() => $"position: {Position}, scale: {Scale}, radians: {Radians}";

    public readonly object Clone() => new Transform3D(Position, Scale, Radians);
    /// <summary>
    /// Creates an identical copy of this <see cref="Transform3D"/>.
    /// </summary>
    public readonly Transform3D CloneTransform() => new(Position, Scale, Radians);
    #endregion
}
