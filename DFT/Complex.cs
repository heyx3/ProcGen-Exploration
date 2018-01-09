using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// A complex number.
/// </summary>
public struct Complex
{
    /// <summary>
    /// The real and imaginary parts of the number.
    /// </summary>
    public float R, I;


    public Complex(float r, float i) { R = r; I = i; }


    public Complex Conjugate { get { return new Complex(R, -I); } }

    public static Complex operator +(Complex a, Complex b) { return new Complex(a.R + b.R, a.I + b.I); }
    public static Complex operator -(Complex a, Complex b) { return new Complex(a.R - b.R, a.I - b.I); }

    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex((a.R * b.R) - (a.I * b.I),
                           (a.R * b.I) + (a.I * b.R));
    }
    public static Complex operator *(Complex a, float f) { return new Complex(a.R * f, a.I * f); }

    public static Complex operator -(Complex a) { return new Complex(-a.R, -a.I); }
}