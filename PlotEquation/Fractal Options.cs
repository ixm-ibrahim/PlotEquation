using System;
using System.Linq;

namespace PlotEquation
{
    public enum FractalType
    {
    	Mandelbrot = 0, BurningShip, Teardrop, Newton, Custom
    }
	
    public enum ColoringMethod
    {
    	Exterior = 0, Exterior_Smooth, Domain, Domain_Exterior, Domain_Interior, Buddha, AntiBuddha
    }
    
    public enum DomainColoring
    {
    	Normal = 0, Brightness, Smooth_Bright_Rings, Bright_Rings, Dark_Rings, Smooth_Dark_Rings
    }
    
    public enum OrbitTrap
    {
    	Circle = 0, Point, Point_Exterior, Point_Interior, Rectangle, Real, Imaginary
    }
}