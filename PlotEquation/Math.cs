﻿using NCalc;
using System;
using System.Linq;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

namespace PlotEquation
{
    public enum Direction
	{
		NONE = 0, UP, FORWARD, LEFT, BACKWARD, RIGHT, DOWN
	}
	
	public enum Rotation
	{
		NONE = 0, YAW, PITCH, ROLL
	}
	
    public struct Duple
    {
    	public double First;
    	public double Second;
    	
    	public Duple(double first, double second)
    	{
    		First = first;
    		Second = second;
    	}
    	
		public override string ToString()
	    {
	        return(System.String.Format("({0}, {1})", First, Second));
	    }
		
	    public override bool Equals(object obj)
	    {
	        return this == (Duple) obj;
	    }
    	
	    public override int GetHashCode()
	    {
	        return First.GetHashCode() ^ Second.GetHashCode();
	    }
	    
		public static bool operator==(Duple a, Duple b)
	    {
			return a.First.Equals(b.First) && a.Second.Equals(b.Second);
	    }
		
		public static bool operator!=(Duple a, Duple b)
	    {
	        return !(a == b);
	    }
    }
    
    public struct Vector3D
    {
    	public double X;
		public double Y;
		public double Z;
		
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		
		public static Vector3D Zero
		{
			get { return new Vector3D(0, 0, 0); }
		}
		
		public static Vector3D Right
		{
			get { return new Vector3D(1, 0, 0); }
		}
		public static Vector3D Left
		{
			get { return -Right; }
		}
		public static Vector3D Forward
		{
			get { return new Vector3D(0, 1, 0); }
		}
		public static Vector3D Backward
		{
			get { return -Forward; }
		}
		public static Vector3D Up
		{
			get { return new Vector3D(0, 0, 1); }
		}
		public static Vector3D Down
		{
			get { return -Up; }
		}
		
		public double Magnitude()
		{
			return Math.Sqrt(X*X + Y*Y + Z*Z);
		}
		public static double Magnitude(Vector3D v)
		{
			return Math.Sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z);
		}
		
		public Vector3D Normalize()
		{
			return this / Magnitude();
		}
		public static Vector3D Normalize(Vector3D v)
		{
			return v / Magnitude(v);
		}
		
		public double Angle(Vector3D v)
		{
			return Math.Acos(DotProduct(v) / (Magnitude() * Magnitude(v)));
		}
		
		public override string ToString()
	    {
	        return(System.String.Format("({0}, {1}, {2})", X, Y, Z));
	    }
		
	    public override bool Equals(object obj)
	    {
	        return this == (Vector3D) obj;
	    }
    	
	    public override int GetHashCode()
	    {
	        return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	    }
	    
		public static bool operator==(Vector3D a, Vector3D b)
	    {
			return a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
	    }
		
		public static bool operator!=(Vector3D a, Vector3D b)
	    {
			return !(a==b);
	    }
		
		public static Vector3D operator+(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.X+b.X, a.Y+b.Y, a.Z+b.Z);
		}
		public static Vector3D operator+(Vector3D a, Quaternion b)
		{
			return new Vector3D(a.X+b.X, a.Y+b.Y, a.Z+b.Z);
		}
		
		public static Vector3D operator-(Vector3D v)
		{
			return v * -1;
		}
		public static Vector3D operator-(Vector3D a, Vector3D b)
		{
			return a + -b;
		}
		
		public static Vector3D operator*(Vector3D v, double d)
		{
			return new Vector3D(v.X*d, v.Y*d, v.Z*d);
		}
		public static Vector3D operator*(double d, Vector3D v)
		{
			return v * d;
		}
		public static Vector3D operator*(Quaternion q, Vector3D v)
		{
			double xx = q.X * q.X;
			double yy = q.Y * q.Y;
			double zz = q.Z * q.Z;
			double xy = q.X * q.Y;
			double xz = q.X * q.Z;
			double yz = q.Y * q.Z;
			double wx = q.W * q.X;
			double wy = q.W * q.Y;
			double wz = q.W * q.Z;
			
			Vector3D result;
			
			result.X = v.X * (1 - 2 * (yy + zz)) + v.Y * 2 * (xy - wz) + v.Z * 2 * (xz + wy);
			result.Y = v.X * 2 * (xy + wz) + v.Y * (1 - 2 * (xx + zz)) + v.Z * 2 * (yz - wx);
			result.Z = v.X * 2 * (xz - wy) + v.Y * 2 * (yz + wx) + v.Z * (1 - 2 * (xx + yy));
			
			return result;
		}
		public static Vector3D operator*(Vector3D v, Quaternion q)
		{
			double xx = q.X * q.X;
			double yy = q.Y * q.Y;
			double zz = q.Z * q.Z;
			double xy = q.X * q.Y;
			double xz = q.X * q.Z;
			double yz = q.Y * q.Z;
			double wx = q.W * q.X;
			double wy = q.W * q.Y;
			double wz = q.W * q.Z;
			
			Vector3D result;
			
			result.X = v.X * (1 - 2 * (yy + zz)) + v.Y * 2 * (xy - wz) + v.Z * 2 * (xz + wy);
			result.Y = v.X * 2 * (xy + wz) + v.Y * (1 - 2 * (xx + zz)) + v.Z * 2 * (yz - wx);
			result.Z = v.X * 2 * (xz - wy) + v.Y * 2 * (yz + wx) + v.Z * (1 - 2 * (xx + yy));
			
			return result;
		}
		
		public static Vector3D operator/(Vector3D v, double d)
		{
			return v * (1/d);
		}
		
		public double DotProduct(Vector3D v)
		{
			return (X * v.X) + (Y * v.Y) + (Z * v.Z);
		}
		public static double DotProduct(Vector3D a, Vector3D b)
		{
			return a.DotProduct(b);
		}
		
		public Vector3D CrossProduct(Vector3D v)
		{
			return new Vector3D(Y*v.Z - Z*v.Y, Z*v.X - X*v.Z, X*v.Y - Y*v.X);
		}
		public static Vector3D CrossProduct(Vector3D a, Vector3D b)
		{
			return a.CrossProduct(b);
		}
    }
    
	public struct Complex
	{
		public double R;
		public double I;
		
		public Complex(double real, double imaginary)
	    {
	        R = real;
	        I = imaginary;
	    }
		public Complex(Quaternion q)
	    {
	        R = q.W;
	        I = q.X;
	    }
		
		public static Complex NaN
	    {
			get { return new Complex(Double.NaN, Double.NaN); }
	    }
		
		public static Complex Zero
	    {
			get { return new Complex(0,0); }
	    }
	    
		public static Complex i
	    {
			get { return new Complex(0,1); }
	    }
	    
		public double Real
	    {
			get { return R; }
	        
	        set { R = 1; }
	    }
	    
		public double Imaginary
	    {
			get { return I; }
	        
	        set { I = 1; }
	    }
	    
		public double Radius
	    {
			get { return Abs(new Complex(R,I)); }
	    }
	    
		public double Angle
	    {
			get { return Arg(new Complex(R,I)); }
	    }
	    
		public override string ToString()
	    {
	        return(System.String.Format("({0}, {1}i)", R, I));
	    }
		
	    public override bool Equals(object obj)
	    {
	        return this == (Complex) obj;
	    }
    	
	    public override int GetHashCode()
	    {
	        return R.GetHashCode() ^ I.GetHashCode();
	    }
	    
		public static bool operator==(Complex z, Complex d)
	    {
			return z.R.Equals(d.R) && z.I.Equals(d.I);
	    }
		
		public static bool operator!=(Complex z, Complex d)
	    {
	        return !(z == d);
	    }
		
	    public static Complex operator+(Complex z, int d)
	    {
	    	return z + Convert.ToDouble(d);
	    }
	    public static Complex operator+(int d, Complex z)
	    {
	    	return z + Convert.ToDouble(d);
	    }
	    public static Complex operator+(Complex z, double d)
	    {
	        return d + z;
	    }
	    public static Complex operator+(double d, Complex z)
	    {
	        return new Complex(z.R + d, z.I);
	    }
	    public static Complex operator+(Complex z, Complex d)
	    {
	        return new Complex(z.R + d.R, z.I + d.I);
	    }
	    
	    public static Complex operator-(Complex z)
	    {
	        return z * -1;
	    }
	    public static Complex operator-(Complex z, int d)
	    {
	        return new Complex(z.R - d, z.I);
	    }
	    public static Complex operator-(int d, Complex z)
	    {
	        return new Complex(d - z.R, -z.I);
	    }
	    public static Complex operator-(Complex z, double d)
	    {
	        return new Complex(z.R - d, z.I);
	    }
	    public static Complex operator-(double d, Complex z)
	    {
	        return new Complex(d - z.R, -z.I);
	    }
	    public static Complex operator-(Complex z, Complex d)
	    {
	        return new Complex(z.R - d.R, z.I - d.I);
	    }
	    
	    public static Complex operator*(Complex z, int d)
	    {
	    	return z * Convert.ToDouble(d);
	    }
	    public static Complex operator*(int d, Complex z)
	    {
	    	return z * Convert.ToDouble(d);
	    }
	    public static Complex operator*(Complex z, double d)
	    {
	        return new Complex(z.R * d, z.I * d);
	    }
	    public static Complex operator*(double d, Complex z)
	    {
	        return new Complex(z.R * d, z.I * d);
	    }
	    public static Complex operator*(Complex z, Complex d)
	    {
	        return new Complex(z.R * d.R - z.I * d.I, z.R * d.I + d.R * z.I);
	    }
	    
	    public static Complex operator/(Complex z, int d)
	    {
	    	return new Complex(z.R / d, z.I / d);
	    }
	    public static Complex operator/(int d, Complex z)
	    {
	    	double x = z.R*z.R + z.I*z.I;
	    	
	        return new Complex(d * z.R / x, -d * z.I / x);
	    }
	    public static Complex operator/(Complex z, double d)
	    {
	    	return new Complex(z.R / d, z.I / d);
	    }
	    public static Complex operator/(double d, Complex z)
	    {
	    	double x = z.R*z.R + z.I*z.I;
	    	
	        return new Complex(d * z.R / x, -d * z.I / x);
	    }
	    public static Complex operator/(Complex z, Complex d)
	    {
	    	double x = d.R*d.R + d.I*d.I;
	    	
	        return new Complex((z.R*d.R + z.I*d.I) / x, (d.R*z.I - z.R*d.I) / x);
	    }
	    
	    public static Complex operator^(Complex z, int p)
	    {
	    	Complex nz = z;
	    	
	    	if (p.Equals(0))
	    		return new Complex(1,0);
	    	if (p.Equals(1))
	    		return z;
	    	if (p > 0)
	    	{
	    		for (int i = 0; i < p - 1; i++)
	    			nz = nz*z;
	    		
                return nz;
	    	}
	    	
	    	// p < 0
    		for (int i = 0; i < -p - 1; i++)
    			nz = nz*z;
    		
    		if (Complex.IsZero(nz))
    			return Complex.Zero;
    		
            return 1 / nz;
	    }
	    public static Complex operator^(int d, Complex z)
	    {
	    	double r = Math.Pow(d, z.R),
	    		   theta = z.I * Math.Log(d);
	    	
	    	return Double.IsNaN(theta) || Double.IsNaN(r) ? z : new Complex(r * Math.Cos(theta), r * Math.Sin(theta));
	    }
	    public static Complex operator^(Complex z, double p)
	    {
	    	if (p.Equals(0))
	    		return new Complex(1,0);
	    	if (p.Equals(1))
	    		return z;
	    	if (p > 0 && p.Equals((int) p))
	    	{
	    		Complex nz = z;
	    		
	    		for (int i = 0; i < p - 1; i++)
	    			nz = nz*z;
	    		
                return nz;
	    	}
	    	if (p < 0 && p.Equals((int) p))
	    	{
	    		Complex nz = z;
	    		
	    		for (int i = 0; i < -p - 1; i++)
	    			nz = nz*z;
	    		
	    		if (Complex.IsZero(nz))
	    			return Complex.Zero;
	    		
                return 1 / nz;
	    	}
	    	
	    	double r = Math.Pow(Abs(z), p),
	    		   theta = Arg(z);
	    	
	    	return Double.IsNaN(theta) || Double.IsNaN(r) ? z : new Complex(r * Math.Cos(theta * p), r * Math.Sin(theta * p));
	    }
	    public static Complex operator^(double d, Complex z)
	    {
	    	double r = Math.Pow(d, z.R),
	    		   theta = z.I * Math.Log(d);
	    	
	    	return Double.IsNaN(theta) || Double.IsNaN(r) ? z : new Complex(r * Math.Cos(theta), r * Math.Sin(theta));
	    }
	    public static Complex operator^(Complex z, Complex p)
	    {
	    	return ((z.R*z.R + z.I*z.I) ^ (p / 2)) * Exp((new Complex(0,1)) * p * Arg(z));
	    }
	    
	    public Complex Pow(int d)
	    {
	    	return this ^ d;
	    }
	    public Complex Pow(double d)
	    {
	    	return this ^ d;
	    }
	    public Complex Pow(Complex c)
	    {
	    	return this ^ c;
	    }
	    
	    public static bool IsZero(Complex z)
	    {
	    	return z.R.Equals(0) && z.I.Equals(0);
	    }
	    
	    public static bool IsNaN(Complex z)
	    {
	    	return Double.IsNaN(z.R) || Double.IsNaN(z.I);
	    }
	    
	    public static bool IsInfinity(Complex z)
	    {
	    	return Double.IsInfinity(z.R) || Double.IsInfinity(z.I);
	    }
	    
	    public static bool IsPositiveInfinity(Complex z)
	    {
	    	return Double.IsPositiveInfinity(z.R) || Double.IsPositiveInfinity(z.I);
	    }
	    
	    public static bool IsNegativeInfinity(Complex z)
	    {
	    	return Double.IsNegativeInfinity(z.R) || Double.IsNegativeInfinity(z.I);
	    }
	    
	    public static int Sign(Complex z)
	    {
	    	return z.R.Equals(0) ? Math.Sign(z.I) : Math.Sign(z.R);
	    }
	    
	    public static double Magnitude(Complex z)
	    {
	    	return z.Radius;
	    }
	    
	    public static double Arg(Complex z)
	    {
	    	double arg = 0;
	    	
	    	/* 
 			 * 	Atan2 (four-quadrant arctangent):
	    	 * 		
	    	 * 		double atan = Math.Atan(z.I/z.R);
	    	 * 		
	    	 * 		if (z.R > 0)
	    	 * 			arg = atan;
	    	 * 		if (z.R < 0 && z.I >= 0)
	    	 * 			arg = atan + Math.PI;
	    	 * 		if (z.R < 0 && z.I < 0)
	    	 * 			arg = atan - Math.PI;
	    	 * 		if (z.R.Equals(0) && z.I > 0)
	    	 * 			arg = Math.PI / 2;
	    	 * 		if (z.R.Equals(0) && z.I < 0)
	    	 * 			arg = -Math.PI / 2;
	    	 * 		if (z == new Complex (0,0))
	    	 * 			arg = Double.NaN;
	    	 * 
	    	 */
	    	
	    	if (z.R.Equals(0))
	    	{
	    		if (z.I < 0 || z.I.Equals(-1))
	    			arg = -Math.PI / 2;
	    		else if (z.I.Equals(0))
	    			return Double.NaN;
	    		else if (z.I > 0 || z.I.Equals(1))
	    			arg = Math.PI / 2;
	    	}
	    	else if (z.I.Equals(0))
	    	{
    			if (z.R.Equals(1))
    				arg = 0;
    			else if (z.R.Equals(-1))
    				arg = Math.PI;
	    	}
    		else if (z == new Complex(1,1))
    			arg = Math.PI / 4;
    		else
    			arg = Math.Atan2(z.I, z.R);
    		
    		return arg;
	    }
	    
	    public static double Abs(Complex z)
	    {
	    	if (Double.IsPositiveInfinity(z.R))
	    		z.R = 10e99;
	    	else if (Double.IsNegativeInfinity(z.R))
	    		z.R = -10e99;
	    	if (Double.IsPositiveInfinity(z.I))
	    		z.I = 10e99;
	    	else if (Double.IsNegativeInfinity(z.I))
	    		z.I = -10e99;
	    	
	    	return Math.Sqrt(z.R*z.R + z.I*z.I);
	    }
	    
	    public static Complex Exp(Complex z)
	    {
	    	double r = Math.Exp(z.R);
	    	
	    	if (z.R.Equals(0))
	    		return new Complex(Math.Cos(z.I), Math.Sin(z.I));
	    	if (z.I.Equals(0))
	    		return new Complex(r, 0);
	    	
	    	return IsNaN(z) ? z : new Complex(r * Math.Cos(z.I), r * Math.Sin(z.I));
	    }
	    
	    public static Complex Ln(Complex z)
	    {
	    	if (z.R.Equals(0))
				return new Complex(Math.Log(z.I), Math.PI / 2);
	    	if (z.I.Equals(0))
				return new Complex(Math.Log(z.R), 0);
			
	    	return new Complex(.5 * Math.Log(z.R*z.R + z.I*z.I), Math.Atan2(z.I, z.R));
	    }
	    
	    public static Complex Log10(Complex z)
	    {
	    	double ln10 = Math.Log(10);
			
	    	return new Complex(0.5 * Math.Log(z.R*z.R + z.I*z.I) / ln10, Arg(z) / ln10);
	    }
	    
	    public static Complex Log(double b, Complex a)
	    {
	    	return Ln(a) / Math.Log(b);
	    }
	    public static Complex Log(Complex b, Complex a)
	    {
	    	return Ln(a) / Ln(b);
	    }
	    
	    public static Complex Sqrt(Complex z)
	    {
	    	double r = Abs(z);
	    	
	    	return new Complex(.5 * Math.Sqrt(2 * (r + z.R)), .5 * Sign(new Complex(z.I, -r)) * Math.Sqrt(2 * (r - z.R)));
	    }
	    
	    public static Complex Inverse(Complex z)
	    {
	    	double x = z.R*z.R + z.I*z.I;
	    	
	    	return new Complex(z.R / x, -z.I / x);
	    }
	    
	    public static Complex Sin(Complex z)
	    {
			if (z.R.Equals(0))
				return new Complex(0, Math.Sinh(z.I));
	    	if (z.I.Equals(0))
	    		return new Complex(Math.Sin(z.R), 0);
			
	    	return new Complex(Math.Sin(z.R % (2 * Math.PI))*Math.Cosh(z.I), Math.Cos(z.R % (2 * Math.PI))*Math.Sinh(z.I));
	    }
	    
	    public static Complex Cos(Complex z)
	    {
	    	if (z.R.Equals(0))
				return new Complex(Math.Cosh(z.I), 0);
	    	if (z.I.Equals(0))
	    		return new Complex(Math.Cos(z.R), 0);
	    	
	    	return new Complex(Math.Cos(z.R % (2 * Math.PI))*Math.Cosh(z.I), -Math.Sin(z.R % (2 * Math.PI))*Math.Sinh(z.I));
	    }
	    
	    public static Complex Tan(Complex z)
	    {
			if (z.R.Equals(0))
				return new Complex(0, Math.Tanh(z.I));
	    	if (z.I.Equals(0))
				return new Complex(Math.Tan(z.R % (2 * Math.PI)), 0);
	    	
			double cosr =  Math.Cos(z.R % (2 * Math.PI));
			double sinhi = Math.Sinh(z.I % (2 * Math.PI));
	
			double denom = cosr*cosr + sinhi*sinhi;
	
			return new Complex(Math.Sin(z.R % (2 * Math.PI)) * cosr / denom, sinhi * Math.Cosh(z.I % (2 * Math.PI)) / denom);
	    }
	    
	    public static Complex Sinh(Complex z)
	    {
	    	if (z.R.Equals(0))
				return new Complex(0, Math.Sin(z.I % (2 * Math.PI)));
	    	if (z.I.Equals(0))
	    		return new Complex(Math.Sinh(z.R), 0);
			
	    	return new Complex(Math.Sinh(z.R)*Math.Cos(z.I % (2 * Math.PI)), Math.Cosh(z.R)*Math.Sin(z.I % (2 * Math.PI)));
	    }
	    
	    public static Complex Cosh(Complex z)
	    {
	    	if (z.R.Equals(0))
				return new Complex(Math.Cos(z.I % (2 * Math.PI)), 0);
	    	if (z.I.Equals(0))
	    		return new Complex(Math.Cosh(z.R), 0);
			
	    	return new Complex(Math.Cosh(z.R)*Math.Cos(z.I % (2 * Math.PI)), Math.Sinh(z.R)*Math.Sin(z.I % (2 * Math.PI)));
	    }
	    
	    public static Complex Tanh(Complex z)
	    {
			if (z.R.Equals(0))
				return new Complex(z.R, Math.Tan(z.I % (2 * Math.PI)) );
	    	if (z.I.Equals(0))
				return new Complex(Math.Tanh(z.R), z.I);
	    	
			double sinhr = Math.Sinh(z.R);
			double cosi =  Math.Cos(z.I % (2 * Math.PI));
			double denom = sinhr*sinhr + cosi*cosi;
			
	    	return Sinh(z) / Cosh(z);
	    }
	    
	    public static Complex Asin(Complex z)
	    {
	    	if (z.R.Equals(0))
				return new Complex(0, Math.Log(z.I + Math.Sqrt(z.I*z.I + 1)));
	    	if (z.I.Equals(0))
	    	{
				if (z.R >= -1 && z.R <= 1)
					return new Complex(Math.Asin(z.R), 0);
				
				return new Complex(Double.NaN, 0);
			}
	    	
			double ss = z.R*z.R + z.I*z.I + 1;
			double ssp2r = Math.Sqrt(ss + 2 * z.R);
			double ssm2r = Math.Sqrt(ss - 2 * z.R);
			double sum = .5*( ssp2r + ssm2r );
	
			return new Complex(Math.Asin(.5 * (ssp2r - ssm2r)), Complex.Sign(new Complex(z.I, -z.R)) * Math.Log(sum + Math.Sqrt(sum * sum - 1)));
	    }
	    
	    public static Complex Acos(Complex z)
	    {
//	    	if (z.R.Equals(0)) // Doesn't work
//				return new Complex(Math.PI / 2, Math.Sign(z.I) * Math.Log(Math.Sqrt(z.I*z.I + 1) + Math.Sqrt(z.I*z.I)));
	    	if (z.I.Equals(0))
	    	{
				if (z.R >= -1 && z.R <= 1)
					return new Complex(Math.Acos(z.R), 0);
				
				return new Complex(Double.NaN, 0);
			}
	    	
			double ss = z.R*z.R + z.I*z.I + 1;
			double ssp2r = Math.Sqrt(ss + 2 * z.R);
			double ssm2r = Math.Sqrt(ss - 2 * z.R);
			double sum = .5*( ssp2r + ssm2r );
	
			return new Complex(Math.Acos(ssp2r / 2 - ssm2r / 2), -Complex.Sign(new Complex(z.I, -z.R)) * Math.Log(sum + Math.Sqrt(sum * sum - 1)));
	    }
	    
	    public static Complex Atan(Complex z)
	    {
	    	if (z.I.Equals(0))
				return new Complex(Math.Atan(z.R), z.I);
	    	
	      	double opi = 1 + z.I;
	      	double omi = 1 - z.I;
			double rr = z.R * z.R;
			
	    	return new Complex(.5 * (Math.Atan2(z.R, omi) - Math.Atan2(-z.R, opi)), .25 * Math.Log((rr + opi * opi) / (rr + omi * omi)));
	    }
	    
	    public static Complex Asinh(Complex z)
	    {
	    	return Ln(z + Sqrt(z*z + 1));
	    }
	    
	    public static Complex Acosh(Complex z)
	    {
	    	return Ln(z + Sqrt(z*z - 1));
	    }
	    
	    public static Complex Atanh(Complex z)
	    {
	    	return .5 * Ln((1 + z) / (1 - z));
	    }
	}
	
	public struct Quaternion
	{
		public double W;
		public double X;
		public double Y;
		public double Z;
		
		public Quaternion(double w, double x, double y, double z)
		{
			W = w;
			X = x;
			Y = y;
			Z = z;
		}
		public Quaternion(double w, Vector3D v)
		{
			W = w;
			X = v.X;
			Y = v.Y;
			Z = v.Z;
		}
		public Quaternion(Complex c, Complex d)
		{
			W = c.R;
			X = c.I;
			Y = d.R;
			Z = d.I;
		}
		public Quaternion(Complex c)
		{
			W = c.R;
			X = c.I;
			Y = 0;
			Z = 0;
		}
		
		public static Quaternion FromAxisAngle(double angle, Vector3D axis)
		{
			return new Quaternion(Math.Cos(angle/2), axis * Math.Sin(angle/2));
		}
		
		public static Quaternion From2Vectors(Vector3D u, Vector3D v)
		{
//			double m = Math.Sqrt(2 + 2 * Vector3D.DotProduct(u, v));
//			
//			return new Quaternion(.5 * m, Vector3D.CrossProduct(u, v) / m);
			
			
			Quaternion q = new Quaternion(0,Vector3D.CrossProduct(u,v));
			
			if (Equals(q.ImaginaryComponent().Magnitude(), 1))
				q.W = 1 + Vector3D.DotProduct(u, v);
			else
				q.W = Math.Sqrt(Math.Pow(u.Magnitude(),2) * Math.Pow(v.Magnitude(), 2)) + Vector3D.DotProduct(u, v);
			
			q.Normalize();
			
			return q;
		}
		
		public static Quaternion Identity
	    {
			get { return new Quaternion(1, 0, 0, 0); }
	    }
	    
		public double Magnitude()
		{
			return Math.Sqrt(W*W + X*X + Y*Y + Z*Z);
		}
		public static double Magnitude(Quaternion q)
		{
			return Math.Sqrt(q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z);
		}
	    
		public Quaternion Conjugate()
		{
			return new Quaternion(W, -X, -Y, -Z);
		}
		public static Quaternion Conjugate(Quaternion q)
		{
			return new Quaternion(q.W, -q.X, -q.Y, -q.Z);
		}
		
		public Quaternion Inverse()
		{
			return this / (W*W + X*X + Y*Y + Z*Z);
		}
		public static Quaternion Inverse(Quaternion q)
		{
			return q / (q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z);
		}
		
		public Quaternion Normalize()
		{
			return this / Magnitude();
		}
		public static Quaternion Normalize(Quaternion q)
		{
			return q / Magnitude(q);
		}
		
		public Vector3D ImaginaryComponent()
		{
			return new Vector3D(X, Y, Z);
		}
		public static Vector3D ImaginaryComponent(Quaternion q)
		{
			return new Vector3D(q.X, q.Y, q.Z);
		}
		
		public double ImaginaryMagnitude()
		{
			return ImaginaryComponent().Magnitude();
		}
		public static double ImaginaryMagnitude(Quaternion q)
		{
			return q.ImaginaryComponent().Magnitude();
		}
	    
		public Vector3D ImaginaryNormalize()
		{
			return ImaginaryComponent().Normalize();
		}
		public static Vector3D ImaginaryNormalize(Quaternion q)
		{
			return q.ImaginaryComponent().Normalize();
		}
		
		public Vector3D Axis()
		{
			return ImaginaryComponent().Normalize();
		}
		public static Vector3D Axis(Quaternion q)
		{
			return q.ImaginaryComponent().Normalize();
		}
		
		public double Angle()
		{
			return 2 * Math.Acos(Normalize().W);
		}
		public static double Angle(Quaternion q)
		{
			return 2 * Math.Acos(q.Normalize().W);
		}
		
		public void ToEuler(ref double yaw, ref double pitch, ref double roll)
		{
			double ysqr = Y * Y;
			
			// pitch (x-axis rotation)
			double t0 = 2 * (W * X + Y * Z);
			double t1 = 1 - 2 * (X * X + ysqr);
			roll = Math.Atan2(t0, t1);
			
			// roll (y-axis rotation)
			t0 = 2 * (W * Y - Z * X);
			t0 = t0 > 1 ? 1 : t0;
			t0 = t0 < -1 ? -1 : t0;
			pitch = Math.Asin(t0);
			
			// yaw (z-axis rotation)
			t0 = 2 * (W * Z + X * Y);
			t1 = 1 - 2 * (ysqr + Z * Z);  
			yaw = Math.Atan2(t0, t1);
		}
		public static void ToEuler(Quaternion q, ref double yaw, ref double pitch, ref double roll)
		{
			double ysqr = q.Y * q.Y;
			
			// pitch (x-axis rotation)
			double t0 = 2 * (q.W * q.X + q.Y * q.Z);
			double t1 = 1 - 2 * (q.X * q.X + ysqr);
			roll = Math.Atan2(t0, t1);
			
			// roll (y-axis rotation)
			t0 = 2 * (q.W * q.Y - q.Z * q.X);
			t0 = t0 > 1 ? 1 : t0;
			t0 = t0 < -1 ? -1 : t0;
			pitch = Math.Asin(t0);
			
			// yaw (z-axis rotation)
			t0 = 2 * (q.W * q.Z + q.X * q.Y);
			t1 = 1 - 2 * (ysqr + q.Z * q.Z);  
			yaw = Math.Atan2(t0, t1);
		}
		
		public override string ToString()
	    {
	        return(System.String.Format("({0}, {1}i, {2}j, {3}k)", W, X, Y, Z));
	    }
		
	    public override bool Equals(object obj)
	    {
	        return this == (Quaternion) obj;
	    }
    	
	    public override int GetHashCode()
	    {
	        return W.GetHashCode() ^ X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	    }
	    
		public static bool operator==(Quaternion a, Quaternion b)
	    {
			return a.W.Equals(b.W) && a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
	    }
		
		public static bool operator!=(Quaternion a, Quaternion b)
	    {
			return !(a==b);
	    }
		
		public static Quaternion operator+(Quaternion a, int b)
		{
			return b + a;
		}
		public static Quaternion operator+(int a, Quaternion b)
		{
			return new Quaternion(a + b.W, b.X, b.Y, b.Z);
		}
		public static Quaternion operator+(Quaternion a, double b)
		{
			return b + a;
		}
		public static Quaternion operator+(double a, Quaternion b)
		{
			return new Quaternion(a + b.W, b.X, b.Y, b.Z);
		}
		public static Quaternion operator+(Quaternion a, Complex b)
		{
			return b + a;
		}
		public static Quaternion operator+(Complex a, Quaternion b)
		{
			return new Quaternion(a.R + b.W, a.I + b.X, b.Y, b.Z);
		}
		public static Quaternion operator+(Quaternion a, Vector3D b)
		{
			return new Quaternion(a.W, a.X+b.X, a.Y+b.Y, a.Z+b.Z);
		}
		public static Quaternion operator+(Vector3D a, Quaternion b)
		{
			return b + a;
		}
		public static Quaternion operator+(Quaternion a, Quaternion b)
		{
			return new Quaternion(a.W+b.W, a.X+b.X, a.Y+b.Y, a.Z+b.Z);
		}
		
		public static Quaternion operator-(Quaternion q)
		{
			return q * -1;
		}
		public static Quaternion operator-(Quaternion a, int b)
		{
			return new Quaternion(a.W - b, a.X, a.Y, a.Z);
		}
		public static Quaternion operator-(int a, Quaternion b)
		{
			return new Quaternion(a - b.W, -b.X, -b.Y, -b.Z);
		}
		public static Quaternion operator-(Quaternion a, double b)
		{
			return new Quaternion(a.W - b, a.X, a.Y, a.Z);
		}
		public static Quaternion operator-(double a, Quaternion b)
		{
			return new Quaternion(a - b.W, -b.X, -b.Y, -b.Z);
		}
		public static Quaternion operator-(Quaternion a, Complex b)
		{
			return new Quaternion(a.W - b.R, a.X - b.I, a.Y, a.Z);
		}
		public static Quaternion operator-(Complex a, Quaternion b)
		{
			return new Quaternion(a.R - b.W, a.I - b.X, -b.Y, -b.Z);
		}
		public static Quaternion operator-(Quaternion a, Vector3D b)
		{
			return new Quaternion(a.W, a.X-b.X, a.Y-b.Y, a.Z-b.Z);
		}
		public static Quaternion operator-(Vector3D a, Quaternion b)
		{
			return new Quaternion(-b.W, a.X-b.X, a.Y-b.Y, a.Z-b.Z);
		}
		public static Quaternion operator-(Quaternion a, Quaternion b)
		{
			return new Quaternion(a.W-b.W, a.X-b.X, a.Y-b.Y, a.Z-b.Z);
		}
		
		public static Quaternion operator*(Quaternion q, int d)
		{
			return new Quaternion(q.W*d, q.X*d, q.Y*d, q.Z*d);
		}
		public static Quaternion operator*(int d, Quaternion q)
		{
			return q * d;
		}
		public static Quaternion operator*(Quaternion q, double d)
		{
			return new Quaternion(q.W*d, q.X*d, q.Y*d, q.Z*d);
		}
		public static Quaternion operator*(double d, Quaternion q)
		{
			return q * d;
		}
		public static Quaternion operator*(Quaternion a, Quaternion b)
		{
			return new Quaternion(a.W*b.W - a.X*b.X - a.Y*b.Y - a.Z*b.Z,
			                      a.W*b.X + a.X*b.W + a.Y*b.Z - a.Z*b.Y,
			                      a.W*b.Y - a.X*b.Z + a.Y*b.W + a.Z*b.X,
			                      a.W*b.Z + a.X*b.Y - a.Y*b.X + a.Z*b.W);
		}
		
		public static Quaternion operator/(Quaternion q, int d)
		{
			return new Quaternion(q.W/d, q.X/d, q.Y/d, q.Z/d);
		}
		public static Quaternion operator/(int d, Quaternion q)
		{
			return d * q.Inverse();
		}
		public static Quaternion operator/(Quaternion q, double d)
		{
			return new Quaternion(q.W/d, q.X/d, q.Y/d, q.Z/d);
		}
		public static Quaternion operator/(double d, Quaternion q)
		{
			return d * q.Inverse();
		}
		public static Quaternion operator/(Quaternion a, Quaternion b)
		{
			return new Quaternion( a.W*b.W + a.X*b.X + a.Y*b.Y + a.Z*b.Z,
			                      -a.W*b.X + a.X*b.W - a.Y*b.Z + a.Z*b.Y,
			                      -a.W*b.Y + a.X*b.Z + a.Y*b.W - a.Z*b.X,
			                      -a.W*b.Z - a.X*b.Y + a.Y*b.X + a.Z*b.W)
						/ (b.W*b.W + b.X*b.X + b.Y*b.Y + b.Z*b.Z);
		}
		
		public static Quaternion operator^(Quaternion q, int power)
		{
			return Exp((Ln(q)*power));
		}
		public static Quaternion operator^(int n, Quaternion power)
		{
			return Exp((Math.Log(n)*power));
		}
		public static Quaternion operator^(Quaternion q, double power)
		{
			return Exp((Ln(q)*power));
		}
		public static Quaternion operator^(double d, Quaternion power)
		{
			return Exp((Math.Log(d)*power));
		}
		public static Quaternion operator^(Quaternion q, Quaternion power)
		{
			return Exp((Ln(q)*power));
		}
	 	
	    public Quaternion Pow(int d)
	    {
	    	return this ^ d;
	    }
	    public Quaternion Pow(double d)
	    {
	    	return this ^ d;
	    }
	    public Quaternion Pow(Quaternion c)
	    {
	    	return this ^ c;
	    }
	    
	    public static bool IsZero(Quaternion q)
	    {
	    	return q.W.Equals(0) && q.X.Equals(0) && q.Y.Equals(0) && q.Z.Equals(0);
	    }
	    
	    public static bool IsNaN(Quaternion q)
	    {
	    	return Double.IsNaN(q.W) || Double.IsNaN(q.X) || Double.IsNaN(q.Y) || Double.IsNaN(q.Z);
	    }
	    
	    public static bool IsInfinity(Quaternion q)
	    {
	    	return Double.IsInfinity(q.W) || Double.IsInfinity(q.X) || Double.IsInfinity(q.Y) || Double.IsInfinity(q.Z);
	    }
	    
	    public static bool IsPositiveInfinity(Quaternion q)
	    {
	    	return Double.IsPositiveInfinity(q.W) || Double.IsPositiveInfinity(q.X) || Double.IsPositiveInfinity(q.Y) || Double.IsPositiveInfinity(q.Z);
	    }
	    
	    public static bool IsNegativeInfinity(Quaternion q)
	    {
	    	return Double.IsPositiveInfinity(q.W) || Double.IsPositiveInfinity(q.X) || Double.IsPositiveInfinity(q.Y) || Double.IsPositiveInfinity(q.Z);
	    }
	    
	    public static double Abs(Quaternion q)
	    {
	    	return Math.Sqrt(q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z);
	    }
	    
		public static Quaternion Exp(Quaternion q)
		{
			double r  = Math.Sqrt(q.X*q.X+q.Y*q.Y+q.Z*q.Z);
			double et = Math.Exp(q.W);
			double s  = r.Equals(0) ? 0 : et * Math.Sin(r) / r;
			
			q.W = et * Math.Cos(r);
			q.X *= s;
			q.Y *= s;
			q.Z *= s;
			
			return q;
		}
		
		public static Quaternion Ln(Quaternion q)
		{
			double r  = Math.Sqrt(q.X*q.X+q.Y*q.Y+q.Z*q.Z);
			double t  = r.Equals(0) ? 0 : Math.Atan2(r, q.W) / r ;
			
			q.W = 0.5 * Math.Log(q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z);
			q.X *= t;
			q.Y *= t;
			q.Z *= t;
			
			return q;
		}
	    
	    public static Quaternion Log(Quaternion q, double d)
	    {
	    	return Ln(q) / Math.Log(d);
	    }
	    public static Quaternion Log(Quaternion q, Quaternion d)
	    {
	    	return Ln(q) / Ln(d);
	    }
	    
		public static Quaternion Sqrt(Quaternion q)
		{
			double m = 0;
			double absIm = Math.Sqrt(q.X*q.X + q.Y*q.Y + q.Z*q.Z);
			Complex z = Complex.Sqrt(new Complex(q.W, absIm));
			
			if (absIm.Equals(0))
				m = z.I;
			else
				m = z.I / absIm;
			
			return new Quaternion(z.R, m * q.X, m * q.Y, m * q.Z);
		}
	    
	    public static Quaternion Sin(Quaternion q)
	    {
	    	double r = q.ImaginaryComponent().Magnitude();
	    	
	    	return new Quaternion(Math.Sin(q.W)*Math.Cosh(r), Math.Cos(q.W)*Math.Sinh(r) * (q.ImaginaryComponent() / r));
	    }
	    
	    public static Quaternion Cos(Quaternion q)
	    {
	    	double r = q.ImaginaryComponent().Magnitude();
	    	
	    	return new Quaternion(Math.Cos(q.W)*Math.Cosh(r), -Math.Sin(q.W)*Math.Sinh(r) * (q.ImaginaryComponent() / r));
	    }
	    
	    public static Quaternion Tan(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Tan(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Sinh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Sinh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Cosh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Cosh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Tanh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Tanh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Asin(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Asin(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Acos(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Acos(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Atan(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Atan(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Asinh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Asinh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Acosh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Acosh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	    
	    public static Quaternion Atanh(Quaternion q)
	    {
			double absIm = q.ImaginaryComponent().Magnitude();
			Complex z = Complex.Atanh(new Complex(q.W, absIm));
			
			return new Quaternion(z.R, (absIm.Equals(0) ? z.I : z.I / absIm) * q.ImaginaryComponent());
	    }
	}
	
	public class Turtle
	{
		public Vector3D forward = Vector3D.Right;
		public Vector3D position = Vector3D.Zero;
		
		public Vector3D rightAxis = Vector3D.Right;
		public Vector3D forwardAxis = Vector3D.Forward;
		public Vector3D upAxis = Vector3D.Up;
		
		public void Initialize(Vector3D positionVector, Vector3D forwardVector)
		{
			position = positionVector;
			forward = forwardVector;
			rightAxis = Vector3D.Right;
			forwardAxis = Vector3D.Forward;
			upAxis = Vector3D.Up;
			
			if (forward == Vector3D.Right)
			{
//				upAxis = Quaternion.VectorComponent(Quaternion.From2Vectors(Vector3D.Up, forward)).Normalize();
//		    	upAxis = Vector3D.CrossProduct(upAxis, forward);
//		    	
//			    rightAxis = Quaternion.VectorComponent(Quaternion.From2Vectors(Vector3D.Forward, forward)).Normalize();
//			    rightAxis = Vector3D.CrossProduct(rightAxis, forward);
//		    	
//			    forwardAxis = Vector3D.CrossProduct(upAxis, rightAxis);
				
				rightAxis = Vector3D.Backward;
				forwardAxis = Vector3D.Right;
			}
			else if (forward == Vector3D.Up)
			{
				forwardAxis = Vector3D.Up;
				upAxis = Vector3D.Backward;
			}
		}
		public void Initialize(Vector3D positionVector, Vector3D forwardVector, Vector3D yawAxis, Vector3D pitchAxis, Vector3D rollAxis)
		{
			position = positionVector;
			forward = forwardVector;
			rightAxis = pitchAxis.Normalize();
			forwardAxis = rollAxis.Normalize();
			upAxis = yawAxis.Normalize();
		}
		
		public override string ToString()
	    {
			return(System.String.Format("Position: ({0},{1},{2})  -  ", position.X, position.Y, position.Z) + System.String.Format("Heading: ({0},{1},{2})", forward.X, forward.Y, forward.Z));
	    }
		
		public void MoveForward(double scalar)
		{
			position += forward * scalar;
		}
		
		Vector3D PointAtRotationAxis(Quaternion rotation, Vector3D axis)
		{
			return rotation.Inverse() * (rotation * axis);
		}
		
		void TurnForward(Quaternion rotation)
		{
			forward = rotation.Inverse() * (rotation * forward);
		}
		
		public void Yaw(double angle, ref Quaternion localRotation)
		{
			angle *= Math.PI / 180;
			
			localRotation = Quaternion.FromAxisAngle(angle / 2, upAxis);
			
			TurnForward(localRotation);
			rightAxis = PointAtRotationAxis(localRotation, rightAxis);
			forwardAxis = PointAtRotationAxis(localRotation, forwardAxis);
		}
		public void Yaw(double angle)
		{
			angle *= Math.PI / 180;
			
			Quaternion localRotation = Quaternion.FromAxisAngle(angle / 2, upAxis);
			
			TurnForward(localRotation);
			rightAxis = PointAtRotationAxis(localRotation, rightAxis);
			forwardAxis = PointAtRotationAxis(localRotation, forwardAxis);
		}
		
		public void Pitch(double angle, ref Quaternion localRotation)
		{
		    angle *= Math.PI / 180;
			
		    localRotation = Quaternion.FromAxisAngle(angle / 2, rightAxis);
			
			TurnForward(localRotation);
			upAxis = PointAtRotationAxis(localRotation, upAxis);
			forwardAxis = PointAtRotationAxis(localRotation, forwardAxis);
		}
		public void Pitch(double angle)
		{
		    angle *= Math.PI / 180;
			
		    Quaternion localRotation = Quaternion.FromAxisAngle(angle / 2, rightAxis);
			
			TurnForward(localRotation);
			upAxis = PointAtRotationAxis(localRotation, upAxis);
			forwardAxis = PointAtRotationAxis(localRotation, forwardAxis);
		}
		
		public void Roll(double angle, ref Quaternion localRotation)
		{
		    angle *= Math.PI / 180;
			
		    localRotation = Quaternion.FromAxisAngle(angle / 2, forwardAxis);
			
			TurnForward(localRotation);
			rightAxis = PointAtRotationAxis(localRotation, rightAxis);
			upAxis = PointAtRotationAxis(localRotation, upAxis);
		}
		public void Roll(double angle)
		{
		    angle *= Math.PI / 180;
			
		    Quaternion localRotation = Quaternion.FromAxisAngle(angle / 2, forwardAxis);
			
			TurnForward(localRotation);
			rightAxis = PointAtRotationAxis(localRotation, rightAxis);
			upAxis = PointAtRotationAxis(localRotation, upAxis);
		}
	}
	
	public static class Calculate
	{
		internal static double Fpart(double d)
		{
			return d - (int) d;
		}
		
        // Gets decimal count of a value, used for rounding doubles in graph generation
        internal static int DecimalCount(double val)
        {
            int i = 0;

            while (!Math.Round(val, i).Equals(val) && i < 14)
                i++;
			
            return i;
        }
		
        internal static double Distance(Point3d a, Point3d b)
        {
        	return Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
        }
        
        internal static double Inverse(double d)
        {
        	return -1 / d;
        }
        
        internal static double Slope(Point3d a, Point3d b)
        {
        	return (b.Y - a.Y) / (b.X - a.X);
        }
        
        // Returns the midpoint
        // (Curve)
        internal static Point3d Midpoint(Curve crv)
        {
        	return crv.PointAtNormalizedLength(.5);
        }
        // (Two Points)
        internal static Point3d Midpoint(Point3d a, Point3d b)
        {
        	return new Point3d((a.X + b.X) / 2, (a.Y + b.Y) / 2, (a.Z + b.Z) / 2);
        }
        
        // Returns the area of a shape
        internal static double Area(IList<Point3d> verticies)
        {
        	double sum = 0;
        	
        	for(int i = 0; i < verticies.Count - 2; i++)
        		sum += verticies[i].X * verticies[i + 1].Y - verticies[i+1].X * verticies[i].Y;
        	
        	return -(.5 * sum + verticies[verticies.Count - 1].X * verticies[0].Y - verticies[0].X * verticies[verticies.Count - 1].Y);
        }
        
        // Returns the center point of a shape
        internal static Point3d Centroid(Brep shape)
        {
        	List<Point3d> verticies = shape.DuplicateVertices().ToList();
        	
        	switch (verticies.Count)
        	{
        		case 2:
        			return Midpoint(verticies[0], verticies[1]);
        		case 3:
        			return Rhino.Geometry.Intersect.Intersection.CurveCurve(Generate.Line(verticies[2], Midpoint(verticies[0], verticies[1])), Generate.Line(verticies[1], Midpoint(verticies[0], verticies[2])), 0, 0)[0].PointA;
        		case 4:
        			var centroids = new List<Point3d>();
        			
        			centroids.Add(Centroid(Generate.Triangle(verticies[0], verticies[1], verticies[2])));
        			centroids.Add(Centroid(Generate.Triangle(verticies[1], verticies[2], verticies[3])));
        			centroids.Add(Centroid(Generate.Triangle(verticies[2], verticies[3], verticies[0])));
        			centroids.Add(Centroid(Generate.Triangle(verticies[3], verticies[0], verticies[1])));
        			
        			return Rhino.Geometry.Intersect.Intersection.CurveCurve(Generate.Line(centroids[0], centroids[2]), Generate.Line(centroids[1], centroids[3]), 0, 0)[0].PointA;
        		default:
//        			var centroid = new Point3d(0,0,0);
//        			double area = Area(verticies);
//        			
//        			for(int i = 0; i < verticies.Count - 2; i++)
//        				centroid.X += (verticies[i].X + verticies[i+1].X) * (verticies[i].X * verticies[i + 1].Y - verticies[i+1].X * verticies[i].Y);
//        			
//        			for(int i = 0; i < verticies.Count - 2; i++)
//        				centroid.Y += (verticies[i].Y + verticies[i+1].Y) * (verticies[i].X * verticies[i + 1].Y - verticies[i+1].X * verticies[i].Y);
//        			
//        			centroid.X += 2*(verticies[verticies.Count - 1].X + verticies[0].X) * (verticies[verticies.Count - 1].X * verticies[0].Y - verticies[0].X * verticies[verticies.Count - 1].Y);
//        			centroid.Y += 2*(verticies[verticies.Count - 1].Y + verticies[0].Y) * (verticies[verticies.Count - 1].X * verticies[0].Y - verticies[0].X * verticies[verticies.Count - 1].Y);
//        			
//        			return centroid / (6 * area);
					Point3d sum = new Point3d();
					
					foreach (Point3d vertex in verticies)
						sum += vertex;
					
					//sum += verticies[0];
					
					return sum / verticies.Count;
        	}
        }
        
        // Returns the scale factor for a Sierpinski polygon
        internal static double ScaleFactor(int sides)
        {
        	double sum = 0;
        	
        	for(int k = 1; k <= sides / 4; k++)
        		sum += Math.Cos(2 * Math.PI * k / sides);
        	
        	return 1 / (2 * (1 + sum));
        }
        
        internal static int BoolCount(List<bool> list, bool o)
        {
        	int count = 0;
        	
        	foreach (bool b in list)
        		if (b == o)
        			count++;
        	
        	return count;
        }
        
        // Differentiate equation at a certain point
		internal static double Differentiation(RhinoDoc doc, Expression eq, double x, string var)
		{//Output.ToRhinoConsole("Derivative at 2", Calculate.Derivative(doc, new Expression(simplifiedEq + "+0*" + one.ToLower()), 2));
			double a = 0,
				   b = 0,
				   c = 0,
				   d = 0,
				   h = 1e-6,
				   h2 = 2 * h;
			
//			eq.Parameters[var] = x + h;
//			a = Convert.ToDouble(eq.Evaluate());
//			
//			eq.Parameters[var] = x;
//			b = Convert.ToDouble(eq.Evaluate());
//			
//			return (a - b) / h;
			eq.Parameters[var] = x-h2;
			a = (double)eq.Evaluate();
			
			eq.Parameters[var] = x - h;
			b = 8 * Convert.ToDouble(eq.Evaluate());
			
			eq.Parameters[var] = x + h;
			c = 8 * Convert.ToDouble(eq.Evaluate());
			
			eq.Parameters[var] = x + h2;
			d = Convert.ToDouble(eq.Evaluate());
			
			return (a - b + c - d) / (6 * h2);
		}
		
		internal static double NewtonsMethod(RhinoDoc doc, Expression eq, double independent, double xN, string iVar, string dVar, int pointIterations = 20)
		{
			eq.Parameters[iVar] = independent;
			
			for (int i = 0; i < pointIterations; i++)
			{
				eq.Parameters[dVar] = xN;
				xN = xN - Convert.ToDouble(eq.Evaluate()) / Differentiation(doc, eq, xN, dVar);
			}
			
			return xN;
		}
		
		// Point calculation for cartesian graphs
        internal static Point3d CartesianPoint(double x, double y, double z = 0)
        {
        	return new Point3d(x, y, z);
        }
        
        // Point calculation for polar or spherical graphs
        internal static Point3d PolarPoint(double theta, double r)
        {
        	return new Point3d(r * Math.Cos(theta), r * Math.Sin(theta), 0);
        }
        internal static Point3d PolarPoint(double theta, double phi, double r)
        {
        	return new Point3d(r * Math.Cos(theta) * Math.Sin(phi), r * Math.Sin(theta) * Math.Sin(phi), r * Math.Cos(phi));
        }
        
        // Point calculation for cylindrical graphs
        internal static Point3d CylindricalPoint(double theta, double z, double r)
        {
        	return new Point3d(r * Math.Cos(theta), r * Math.Sin(theta), z);
        }
        
        internal static Complex Mandelbrot(Complex z, Complex c, double power, bool complexConjugate = false)
        {
        	if (complexConjugate)
        		z.I *= -1;
        	
        	return (z^power) + c;
        }
        
        internal static Complex BurningShip(Complex z, Complex c, double power, bool complexConjugate = false)
        {
        	if (complexConjugate)
        		z.I *= -1;
        	
        	return ((new Complex(Math.Abs(z.R), Math.Abs(z.I)))^power) + c;
        }
        
        internal static Complex Teardrop(Complex z, Complex c, double power, bool complexConjugate = false)
        {
        	if (complexConjugate)
        		z.I *= -1;
        	
        	return (z^power) + 1 / c;
        }
        
        internal static Complex Custom(Complex z, Complex c, double power, bool complexConjugate = false)
        {
        	if (complexConjugate)
        		z.I *= -1;
        	
        	return Complex.Cos(z) + c;
        }
	}
	
	public static class Transform
	{
        internal static Point3d MovePointX(ref Point3d point, double distance)
        {
        	point.X += distance;
        	
        	return point;
        }
        internal static Point3d MovePointX(Point3d point, double distance)
        {
        	point.X += distance;
        	
        	return point;
        }
        
        internal static Point3d MovePointY(ref Point3d point, double distance)
        {
        	point.Y += distance;
        	
        	return point;
        }
        internal static Point3d MovePointY(Point3d point, double distance)
        {
        	point.Y += distance;
        	
        	return point;
        }
        
        internal static Point3d MovePointZ(ref Point3d point, double distance)
        {
        	point.Z += distance;
        	
        	return point;
        }
        internal static Point3d MovePointZ(Point3d point, double distance)
        {
        	point.Z += distance;
        	
        	return point;
        }
        
        internal static void MoveSquareX(ref Point3d p1, ref Point3d p2, ref Point3d p3, ref Point3d p4, double distance)
        {
        	MovePointX(ref p1, distance);
        	MovePointX(ref p2, distance);
        	MovePointX(ref p3, distance);
        	MovePointX(ref p4, distance);
        }
        
        internal static void MoveSquareY(ref Point3d p1, ref Point3d p2, ref Point3d p3, ref Point3d p4, double distance)
        {
        	MovePointY(ref p1, distance);
        	MovePointY(ref p2, distance);
        	MovePointY(ref p3, distance);
        	MovePointY(ref p4, distance);
        }
        
        internal static void MoveSquareZ(ref Point3d p1, ref Point3d p2, ref Point3d p3, ref Point3d p4, double distance)
        {
        	MovePointZ(ref p1, distance);
        	MovePointZ(ref p2, distance);
        	MovePointZ(ref p3, distance);
        	MovePointZ(ref p4, distance);
        }
        
        internal static Curve RotateLine2D(Curve line, double angle)
        {
        	var points = new List<Point3d>();
        	Point3d start = line.PointAtStart,
        			end = line.PointAtEnd;
			
        	points.Add(start);
        	points.Add(new Point3d(start.X + (end.X - start.X) * Math.Cos(angle * Math.PI / 180) - (end.Y - start.Y) * Math.Sin(angle * Math.PI / 180), start.Y + (end.X - start.X) * Math.Sin(angle * Math.PI / 180) + (end.Y - start.Y) * Math.Cos(angle * Math.PI / 180), 0));
        	
        	return Rhino.Geometry.Curve.CreateInterpolatedCurve(points, 3);
        }
        
        internal static Point3d RotateZYX(double yaw, double roll, double pitch, double length = 1)
        {
        	Point3d newPoint = new Point3d(1,0,0);
			
//			// Yaw
//			newPoint = new Point3d(newPoint.X * Math.Cos((currentYaw * Math.PI) / 180) - newPoint.Y * Math.Sin((currentYaw * Math.PI) / 180), newPoint.X * Math.Sin((currentYaw * Math.PI) / 180) + newPoint.Y * Math.Cos((currentYaw * Math.PI) / 180), newPoint.Z);
//			// Pitch
//			newPoint = new Point3d(newPoint.X, newPoint.Y * Math.Cos((currentPitch * Math.PI) / 180) - newPoint.Z * Math.Sin((currentPitch * Math.PI) / 180), newPoint.Y * Math.Sin((currentPitch * Math.PI) / 180) + newPoint.Z * Math.Cos((currentPitch * Math.PI) / 180));
//			// Roll
//			newPoint = new Point3d(newPoint.Z * Math.Sin((currentRoll * Math.PI) / 180) + newPoint.X * Math.Cos((currentRoll * Math.PI) / 180), newPoint.Y, newPoint.Z * Math.Cos((currentRoll * Math.PI) / 180) - newPoint.X * Math.Sin((currentRoll * Math.PI) / 180));
//			Output.ToRhinoConsole("Yaw", currentYaw);Output.ToRhinoConsole("Pitch", currentPitch);Output.ToRhinoConsole("Roll", currentRoll);
        	double sinYaw = Math.Sin(yaw * Math.PI / 180),
				   sinRoll = Math.Sin(roll * Math.PI / 180),
				   sinPitch = Math.Sin(pitch * Math.PI / 180),
				   cosYaw = Math.Cos(yaw * Math.PI / 180),
				   cosRoll = Math.Cos(roll * Math.PI / 180),
				   cosPitch = Math.Cos(pitch * Math.PI / 180),
				   x = newPoint.X,
				   y = newPoint.Y,
				   z = newPoint.Z;
			
			newPoint.X = (x * cosYaw * cosRoll) + (y * (cosYaw * sinRoll * sinPitch - cosPitch * sinYaw)) + (z * (sinYaw * sinPitch - cosYaw * cosPitch * sinRoll));
			newPoint.Y = (x * cosRoll * sinYaw) + (y * (cosYaw * cosPitch - sinYaw * sinRoll * sinPitch)) + (z * (cosPitch * sinYaw * sinRoll - cosYaw * sinPitch));
			newPoint.Z = (-x * sinRoll) - (y * cosRoll * sinPitch) + (z * cosRoll * cosPitch);
//			Output.ToRhinoConsole("(x * Math.Sin(roll))", (x * Math.Sin(roll)));Output.ToRhinoConsole("(-y * sinPitch * cosRoll)", (y * sinPitch * cosRoll));Output.ToRhinoConsole("(z * cosPitch * cosRoll)", (z * cosPitch * cosRoll));
//			Output.ToRhinoConsole("x", newPoint.X);Output.ToRhinoConsole("y", newPoint.Y);Output.ToRhinoConsole("z", newPoint.Z);
			
			return newPoint * length;
        }
	}
}