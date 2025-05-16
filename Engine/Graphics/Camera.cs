
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MonoidEngine;

public class Camera
{
    /// <summary>
    /// Camera's position, scale and rotation.
    /// </summary>
    public Transform2D Transform;
    /// <summary>
    /// Camera's origin, relative to it's top-right position.
    /// </summary>
    /// <remarks>Set to half the camera's viewport size to center the camera.</remarks>
    public Vector2 Origin;
    /// <summary>
    /// Camera's viewport.
    /// </summary>
    /// <remarks>Area of the screen that this camera will draw to.</remarks>
    public Viewport Viewport;

    protected Transform2D _previousTransform;
    protected Vector2 _previousOrigin;

    protected Matrix _matrix;
    protected Matrix _matrixInverse;

    /// <summary>
    /// Whether this camera moved during this frame.
    /// </summary>
    public bool ChangedThisFrame => _previousOrigin != Origin || _previousTransform != Transform;
    /// <summary>
    /// This camera's transformation matrix, which describes it's position, scale and rotation.
    /// </summary>
    public Matrix Matrix => _matrix;
    /// <summary>
    /// The inverse of this camera's transformation matrix, which describes it's position, scale and rotation.
    /// </summary>
    public Matrix MatrixInverse => _matrixInverse;

    /// <summary>
    /// Instantiates a new camera.
    /// </summary>
    public Camera(Transform2D transform, Point viewportPosition, Point viewportSize)
    {
        Transform = transform;
        Viewport = new Viewport(new Rectangle(viewportPosition, viewportSize));
    }
    /// <summary>
    /// Instantiates a new camera.
    /// </summary>
    public Camera(Transform2D transform, Point viewportPosition, int viewportWidth, int viewportHeight) : this(transform, viewportPosition, new Point(viewportWidth, viewportHeight)) { }
    /// <summary>
    /// Instantiates a new camera.
    /// </summary>
    public Camera(Transform2D transform, int viewportX, int viewportY, Point viewportSize) : this(transform, new Point(viewportX, viewportY), viewportSize) { }
    /// <summary>
    /// Instantiates a new camera.
    /// </summary>
    public Camera(Transform2D transform, int viewportX, int viewportY, int viewportWidth, int viewportHeight) : this(transform, new Point(viewportX, viewportY), new Point(viewportWidth, viewportHeight)) { }

    public void Update()
    {
        // Recalculate matrices when the origin or transform change.
        if (ChangedThisFrame)
        {
            _matrix = GetMatrix();
            _matrixInverse = Matrix.Invert(_matrix);
        }

        _previousTransform = Transform;
        _previousOrigin = Origin;
    }

    /// <summary>
    /// Copies and transforms a vector to this camera's coordinates.
    /// </summary>
    /// <param name="vector">Vector that will be transformed.</param>
    /// <returns>Transformed vector.</returns>
    public Vector2 TransformVector(Vector2 vector)
    {
        return Vector2.Transform(vector, GetMatrix());
    }

    /// <summary>
    /// Copies and transforms a vector from this camera's coordinates to global coordinates.
    /// </summary>
    /// <param name="vector">Vector that will be transformed.</param>
    /// <returns>Transformed vector.</returns>
    public void TransformVector(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, GetMatrix());
    }

    /// <summary>
    /// Transforms a vector from this camera's coordinates to global coordinates.
    /// </summary>
    /// <param name="vector">Vector that will be transformed.</param>
    /// <returns>Transformed vector.</returns>
    public Vector2 TransformVectorInverse(Vector2 vector)
    {
        return Vector2.Transform(vector, _matrixInverse);
    }

    /// <summary>
    /// Transforms a vector to this camera's coordinates.
    /// </summary>
    /// <param name="vector">Vector that will be transformed.</param>
    /// <returns>Transformed vector.</returns>
    public void TransformVectorInverse(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, _matrixInverse);
    }

    /// <summary>
    /// Centers the camera's origin to its center.
    /// </summary>
    public void CenterOrigin()
    {
        Origin = Viewport.Bounds.Size.ToVector2() * 0.5f;
    }

    protected Matrix GetMatrix()
    {
        float rotationCos = MathF.Cos(Transform.Radians);
        float rotationSin = MathF.Sin(Transform.Radians);

        return new()
        {
            M11 = Transform.Scale.X * rotationCos, // Scale in X and rotate
            M12 = Transform.Scale.X * rotationSin,  // Scale in X and rotate
            M21 = -Transform.Scale.Y * rotationSin, // Scale in Y and rotate
            M22 = Transform.Scale.Y * rotationCos,   // Scale in Y and rotate
            M33 = 1f,
            M41 = -Transform.PositionX + Origin.X, // Translate
            M42 = -Transform.PositionY + Origin.Y, // Translate
            M44 = 1f
        };
    }
}
