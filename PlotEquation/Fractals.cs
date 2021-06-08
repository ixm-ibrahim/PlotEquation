using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

namespace PlotEquation
{
	public class Mandelbrot
	{
		public int pixelWidth = 0;
		public int pixelHeight = 0;
		public int maxIteration = 0;
		public double rCenter = 0;
		public double iCenter = 0;
		public double radius = 0;
		public double bailoutRadius = 0;
		public double power = 0;
		public bool complexConjugate = false;
		public Color interiorColor = Color.Black;
		public FractalType fractal = FractalType.Mandelbrot;
		public ColoringMethod coloring = ColoringMethod.Exterior;
		public DomainColoring domainColoring = DomainColoring.Normal;
		public List<Color> colors = new List<Color>();
		public OrbitTrap orbitTrap = OrbitTrap.Circle;
		
		internal void Initialize(int pixel_width, int pixel_height, double real_center, double imaginary_center, double graph_radius, int max_iteration, double bailout_radius, double mandelbrot_power, bool complex_conjugate, FractalType frac, ColoringMethod coloring_type, DomainColoring domain_coloring, List<Color> color_list, OrbitTrap ot)
		{
			pixelWidth = pixel_width;
			pixelHeight = pixel_height;
			maxIteration = max_iteration;
			rCenter = real_center;
			iCenter = imaginary_center;
			radius = graph_radius;
			bailoutRadius = bailout_radius;
			power = mandelbrot_power;
			complexConjugate = complex_conjugate;
			fractal = frac;
			coloring = coloring_type;
			domainColoring = domain_coloring;
			colors = color_list;
			orbitTrap = ot;
		}
		
		// Returns string equivalent of a Fractal Type
		internal static string ToString(FractalType fractal)
		{
			switch (fractal)
			{
				case FractalType.BurningShip:
					return "Burning Ship";
				case FractalType.Newton:
					return "Newton";
				default:
					return "Mandelbrot";
			}
		}
        
		internal static List<Point3d> BorderToPoints(Bitmap pic)
		{
			List<Point3d> points = new List<Point3d>();
			pic = ColorPalette.BlackAndWhite(pic);
			
			for(int x = 0; x < pic.Width; x++)
			{
				Color pc = pic.GetPixel(x,0);
				
				for(int y = 1; y < pic.Height; y++)
				{
					if (pc != pic.GetPixel(x,y))
						points.Add(new Point3d(x, y, 0));
					
					pc = pic.GetPixel(x,y);
				}
			}
			
			for(int y = 0; y < pic.Height; y++)
			{
				Color pc = pic.GetPixel(0,y);
				
				for(int x = 1; x < pic.Width; x++)
				{
					if (pc != pic.GetPixel(x,y) && !points.Contains(new Point3d(x, y, 0)))
						points.Add(new Point3d(x, y, 0));
					
					pc = pic.GetPixel(x,y);
				}
			}
			
			for (int i = 0; i < points.Count; i++)
				points[i] = new Point3d(points[i].X / (pic.Width / 2d), -points[i].Y / (pic.Width / 2d), 0);
			
			return points;
		}
		
		internal static List<Curve> BorderToCurves(Bitmap pic, bool pixelated = false)
		{
			var points = InteriorToPoints(pic);
			var pixelSurfaces = new List<Brep>();
			var joinedSurfaces = new List<Brep>();
			var borders = new List<Curve>();
			var rebuiltBorders = new List<Curve>();
			double r = 1 / (double) pic.Width;
			
			Output.ToRhinoConsole("Duplicating pixels into surfaces...");
			
			foreach (Point3d point in points)
			{
				double x = point.X;
				double y = point.Y;
				double z = point.Z;
				Point3d p1 = new Point3d(x-r, y-r, 0);
				Point3d p2 = new Point3d(x+r, y-r, 0);
				Point3d p3 = new Point3d(x+r, y+r, 0);
				Point3d p4 = new Point3d(x-r, y+r, 0);
				
				pixelSurfaces.Add(Brep.CreateFromCornerPoints(p1, p2, p3, p4, 0.0000001));
				RhinoApp.Wait();
			}
			
			Output.ToRhinoConsole("Joining surfaces...");
			RhinoApp.Wait();
			pixelSurfaces = Brep.JoinBreps(pixelSurfaces, 0.0000001).ToList();
			
			Output.ToRhinoConsole("Smoothing pixelated curves...");
			RhinoApp.Wait();
			
			foreach (Brep brep in pixelSurfaces)
			{
				borders.AddRange(Curve.JoinCurves(brep.DuplicateEdgeCurves(true)));
			}
			
			if (!pixelated)
			{
				foreach (Curve curve in borders)
				{
					rebuiltBorders.Add(curve.Rebuild(curve.DuplicateSegments().Length, 3, true));
				}
			}
			else
				rebuiltBorders = borders;
			
			return rebuiltBorders;
		}
		
		internal static List<Point3d> InteriorToPoints(Bitmap pic)
		{
			List<Point3d> points = new List<Point3d>();
			pic = ColorPalette.BlackAndWhite(pic);
			
			for(int x = 0; x < pic.Width; x++)
				for(int y = 0; y < pic.Height; y++)
					if (pic.GetPixel(x,y) == Color.FromArgb(0,0,0))
						points.Add(new Point3d(x / (double) (pic.Width / 2d), -y / (pic.Width / 2d), 0));
			
			return points;
		}
		
		internal static List<Point3d> ExteriorToPoints(Bitmap pic)
		{
			List<Point3d> points = new List<Point3d>();
			pic = ColorPalette.BlackAndWhite(pic);
			
			for(int x = 0; x < pic.Width; x++)
				for(int y = 0; y < pic.Height; y++)
					if (pic.GetPixel(x,y) == Color.FromArgb(255,255,255))
						points.Add(new Point3d(x / (double) (pic.Width / 2d), -y / (pic.Width / 2d), 0));
			
			return points;
		}
		
		internal static List<Brep> InteriorToSurfaces(Bitmap pic, bool pixelated = false)
		{
			var surfaces = new List<Brep>();
			if (!pixelated)
			{
				var borders = BorderToCurves(pic);
			
				foreach (Curve curve in borders)
				{
					List<Curve> c = new List<Curve>();
					c.Add(curve);
					surfaces.AddRange(Brep.CreatePlanarBreps(c).ToList());
				}
			}
			else
			{
				var points = InteriorToPoints(pic);
				var joinedSurfaces = new List<Brep>();
				var borders = new List<Curve>();
				var rebuiltBorders = new List<Curve>();
				double r = 1 / (double) pic.Width;
				
				Output.ToRhinoConsole("Duplicating pixels into surfaces...");
				
				foreach (Point3d point in points)
				{
					double x = point.X;
					double y = point.Y;
					double z = point.Z;
					Point3d p1 = new Point3d(x-r, y-r, 0);
					Point3d p2 = new Point3d(x+r, y-r, 0);
					Point3d p3 = new Point3d(x+r, y+r, 0);
					Point3d p4 = new Point3d(x-r, y+r, 0);
					
					surfaces.Add(Brep.CreateFromCornerPoints(p1, p2, p3, p4, 0.0000001));
					RhinoApp.Wait();
				}
				
				Output.ToRhinoConsole("Joining surfaces...");
				RhinoApp.Wait();
				surfaces = Brep.JoinBreps(surfaces, 0.0000001).ToList();
			}
			
			return surfaces;
		}
		
		internal bool IsBailoutOrbit()
		{
			return orbitTrap == OrbitTrap.Circle || orbitTrap == OrbitTrap.Rectangle || orbitTrap == OrbitTrap.Real || orbitTrap == OrbitTrap.Imaginary;
		}
		
		internal Color ReturnDomainColor(Complex z)
		{
			double theta = z.Angle;
			double r = z.Radius;
        	
			if (Complex.IsZero(z))
			    theta = 0;
			else if (Complex.IsNaN(z))
			{
			    z = Complex.Zero;
			    theta = 0;
			}
			//Output.Here(z, theta, r);
			Color color = colors[(int) ((theta < 0 ? theta + Math.PI * 2 : theta) * (colors.Count - 1) / (2 * Math.PI))];
			
			double s = Math.Abs(Math.Sin((r * 2*Math.PI) % (2 * Math.PI)));
			double b = Math.Sqrt(Math.Sqrt(Math.Abs(Math.Sin((z.I * 2*Math.PI) % (2 * Math.PI)) * Math.Sin((z.R * 2*Math.PI) % (2 * Math.PI)))));
			double b2 = .5 * ((1 - s) + b + Math.Sqrt(Math.Pow(1 - s - b, 2) + 0.01));
			//Output.Here(s, b, b2);
			switch (domainColoring)
			{
				case DomainColoring.Brightness:
					return ColorPalette.ColorFromHSV(color.GetHue(), 1, b2 > 1 ? 1 : b2);
				case DomainColoring.Smooth_Bright_Rings:
					return ColorPalette.ColorFromHSV(color.GetHue(), s, 1);
				case DomainColoring.Bright_Rings:
					return ColorPalette.ColorFromHSV(color.GetHue(), r % 1, 1);
				case DomainColoring.Smooth_Dark_Rings:
					return ColorPalette.ColorFromHSV(color.GetHue(), 1, s);
				case DomainColoring.Dark_Rings:
					return ColorPalette.ColorFromHSV(color.GetHue(), 1, r % 1);
				default:
					return ColorPalette.ColorFromHSV(color.GetHue(), s, b2 > 1 ? 1 : b2);
			}
		}
		
		// Tests to see if orbit is within desired Orbit Trap
		internal bool WithinOrbitTrap(Complex z)
		{
			switch (orbitTrap)
			{
				case OrbitTrap.Rectangle:
					return Math.Abs(z.R) < bailoutRadius && Math.Abs(z.I) < bailoutRadius;
				case OrbitTrap.Real:
					return Math.Abs(z.R) < bailoutRadius;
				case OrbitTrap.Imaginary:
					return Math.Abs(z.I) < bailoutRadius;
				default:
					return z.Radius < bailoutRadius;
			}
		}
		
		// Returns the 'z' value based on the calculation used
		internal Complex FindZ(Complex pz, Complex z, Complex c)
		{
			switch (fractal)
			{
				case FractalType.Mandelbrot:
					return Calculate.Mandelbrot(z, c, power, complexConjugate);
				case FractalType.BurningShip:
					return Calculate.BurningShip(z, c, power, complexConjugate);
				case FractalType.Teardrop:
					return Calculate.Teardrop(z, c, power, complexConjugate);
				case FractalType.Newton:
				default:
					return Calculate.Custom(z, c, power, complexConjugate);
			}
		}
		
		// Returns a color for a certain coordinate on an image corresponding to fractal type
		Color GetPixelColor(Complex coordinate, Complex juliaC)
		{// Setting z equal to coordinate reduces iterations by 1...? Test
			Complex z = Complex.Zero;
			Complex c = Complex.Zero;
			
			if (juliaC != Complex.Zero)
			{
				z = fractal == FractalType.Teardrop ? Complex.Inverse(coordinate): coordinate;
				c = juliaC;
			}
			else
			{
				z = Complex.Zero;
				c = coordinate;
			}
			
			Complex pz = z;
			int currentIteration = 0;
			double dist = 1e99;
			double max = 0;
			
			while (WithinOrbitTrap(z) && currentIteration < maxIteration)
			{
				z = FindZ(pz, z, c);
            	
            	if (orbitTrap == OrbitTrap.Point)
            	{
            		dist = Math.Min(dist, Math.Pow((z == Complex.Zero ? 0 : z.Radius), 2));//Output.Here(1);
            		max = Math.Max(max, dist);//Output.Here(2, z);
            	}
            	
				if (pz == z)
					currentIteration = maxIteration;
            	
				if (Complex.IsInfinity(z) || Double.IsInfinity(z.Radius))
				{
					currentIteration = maxIteration;
					z = pz;
				}
            	
				pz = z;
            	
				currentIteration++;
				RhinoApp.Wait();
			}
			
			if (!IsBailoutOrbit())
        		currentIteration = (int) (Math.Sqrt(dist) * maxIteration / (max.Equals(0) ? 1 : max));
			
			switch (coloring)
			{
				case ColoringMethod.Domain:
					return ReturnDomainColor(z);
				case ColoringMethod.Domain_Exterior:
					return currentIteration < maxIteration ? ReturnDomainColor(z) : Color.Black;
				case ColoringMethod.Domain_Interior:
					return currentIteration <= maxIteration ? Color.Black : ReturnDomainColor(z);
				case ColoringMethod.Exterior_Smooth:
				{
					for (int i = 0; i < 3; i++)
					{
						z = FindZ(pz, z, c);
		            	
		            	if (orbitTrap == OrbitTrap.Point)
		            	{
		            		dist = Math.Min(dist, Math.Pow((z == Complex.Zero ? 0 : z.Radius), 2));
		            		max = Math.Max(max, dist);
		            	}
		            	
						if (Complex.IsInfinity(z) || Double.IsInfinity(z.Radius))
							z = pz;
						
						pz = z;
		            	
						currentIteration++;
						RhinoApp.Wait();
					}
					
					if (!IsBailoutOrbit())
		        		currentIteration = (int) (Math.Sqrt(dist) * maxIteration / (max.Equals(0) ? 1 : max));
					
					if (currentIteration < maxIteration)
					{
						double mu = currentIteration - (Math.Log(Math.Log((z).Radius)) / Math.Log(bailoutRadius));
						return colors[(int) (mu * (colors.Count - 1) / maxIteration)];
					}
					
					return Color.Black;
				}
				default:
				{
                	if (currentIteration < maxIteration || orbitTrap == OrbitTrap.Point)
						return colors[(int) ((currentIteration >= maxIteration ? maxIteration - 1: currentIteration) * (colors.Count - 1) / maxIteration)];
					
					return Color.Black;
				}
			}
		}
		
		Color[,] ReturnColors(Complex juliaC)
		{
			var pixelArray = new Color[pixelWidth,pixelHeight];
			
			double xMin = rCenter - radius,
            	   xMax = rCenter + radius,
            	   yMin = iCenter - radius,
            	   yMax = iCenter + radius;
            double xScale = (xMax - xMin) / Convert.ToDouble(pixelWidth);  // Amount to move each pixel in the real numbers
            double yScale = (yMax - yMin) / Convert.ToDouble(pixelHeight);  // Amount to move each pixel in the imaginary numbers
            
            for (int x = 0; x < pixelWidth; x++)
            {//Output.Here(x);
            	for (int y = 0; y < pixelHeight; y++)
	            {//Output.Here(y);
                	// sets the coordinate of 'c'
					pixelArray[x,y] = GetPixelColor(new Complex((xScale * x) - Math.Abs(xMin), (yScale * y) - Math.Abs(yMin)), juliaC);
	            	RhinoApp.Wait();
				}
            }
			return pixelArray;
		}
		
		// Returns a list of colors that corresponds to the image of a Buddhabrot
		Color[,] ReturnBuddhaColors(Complex juliaC)
		{
        	var counterArray = new int[pixelWidth, pixelHeight];
        	var pixelArray = new Color[pixelWidth, pixelHeight];
        	var pointsOfInterest = new List<Complex>();
            List<Color> monochrome = new List<Color>();
            monochrome.Add(Color.Black);
			monochrome.Add(Color.White);
			monochrome = ColorPalette.Custom(monochrome);
       		int max = 0;
       		
       		double xMin = rCenter - radius,
            	   xMax = rCenter + radius,
            	   yMin = iCenter - radius,
            	   yMax = iCenter + radius;
            double xScale = (xMax - xMin) / Convert.ToDouble(pixelWidth);  // Amount to move each pixel in the real numbers
            double yScale = (yMax - yMin) / Convert.ToDouble(pixelHeight);  // Amount to move each pixel in the imaginary numbers
            
        	for(int x = 0; x < pixelWidth; x++)
        		for (int y = 0; y < pixelHeight; y++)
        		{
        			Complex z = new Complex (0,0);
					Complex c = new Complex (0,0);
        			Complex coordinate = new Complex((xScale * x) + xMin, (yScale * y) + yMin);
//					Complex c = new Complex((new Random(Guid.NewGuid().GetHashCode())).NextDouble() * (rCenter + 2 * radius) - radius, (new Random(Guid.NewGuid().GetHashCode())).NextDouble() * (iCenter + 2 * radius) - radius);
					
					if (juliaC != new Complex(0,0))
					{
						z = coordinate;
						c = juliaC;
					}
					else
					{
						z = coordinate;
						c = coordinate;
					}
					
					Complex pz = z;
        			int currentIteration = 0;
        			
        			for (currentIteration = 0; WithinOrbitTrap(z) && currentIteration < maxIteration; currentIteration++)
        			{
        				z = FindZ(pz, z, c);
                	
						if (pz == z)
							currentIteration = maxIteration;
                    	
						pz = z;
						
            			RhinoApp.Wait();
        			}
        			
        			if ((coloring == ColoringMethod.Buddha && currentIteration != maxIteration) || (coloring == ColoringMethod.AntiBuddha && currentIteration == maxIteration))
        				pointsOfInterest.Add(c);
            	}
        	
        	Output.ToRhinoConsole("Starting Buddhabrot mapping...");
        	
    		foreach (Complex c in pointsOfInterest)
    		{
    			Complex z = c;
    			Complex pz = z;
    			
    			for (int currentIteration = 0; WithinOrbitTrap(z) && currentIteration < maxIteration; currentIteration++)
    			{
    				int arrayX = (int) (pixelWidth * (z.R - xMin) / (xMax - xMin));
    				int arrayY = (int) (pixelHeight * (z.I - yMin) / (yMax - yMin));
    				
    				counterArray[arrayX, arrayY]++;
    				
    				if (counterArray[arrayX, arrayY] > max)
    					max = counterArray[arrayX, arrayY];
    				
    				z = FindZ(pz, z, c);
                	
					if (pz == z)
						currentIteration = maxIteration;
                	
					pz = z;
    				
    				RhinoApp.Wait();
    			}
        	}
            
    		for (int x = 0; x < pixelWidth; x++)
        		for (int y = 0; y < pixelHeight; y++)
    				pixelArray[x,y] = monochrome[counterArray[x,y] * (monochrome.Count - 1) / max];
    		
            return pixelArray;
		}
		
        // Fractal initialization
        internal Bitmap Generate(Complex juliaC)
		{
			var pic = new Bitmap(pixelWidth, pixelHeight);
			var pixelArray = new Color[pixelWidth,pixelHeight];
			
            Output.ToRhinoConsole("Beginning fractal calculations...");
            
            if (coloring == ColoringMethod.Buddha || coloring == ColoringMethod.AntiBuddha)
            	pixelArray = ReturnBuddhaColors(juliaC);
            else
            	pixelArray = ReturnColors(juliaC);
            
        	Output.ToRhinoConsole("Mapping pixels to image...");
        	
        	for (int x = 0; x < pic.Width; x++)
        		for (int y = 0; y < pic.Height; y++)
        			pic.SetPixel(x, y, pixelArray[x,y]);
            
            return pic;
		}
        
        // Creates an animation of the unit revolution of a Julia Set
        internal List<Bitmap> JuliaSetUnitCircle(int frameNumber, double unitCircleRadius)
		{
			List<Bitmap> frames = new List<Bitmap>();
            
			const double min = 0;
			const double max = 2*Math.PI;
			double iteration = (double) (max / frameNumber);
			
            for (double i = min; i < max; i += iteration)
            {
            	frames.Add(Generate(new Complex(unitCircleRadius*Math.Cos(i), unitCircleRadius*Math.Sin(i))));
            	RhinoApp.Wait();
            	Output.Here(i);
            }
            
            return frames;
		}
        
        // Creates and animation of the power change of a Fractal
        internal List<Bitmap> PowerChange(int frameNumber, double minPower, double maxPower, Complex juliaC)
        {
        	List<Bitmap> frames = new List<Bitmap>();
        	double iteration = (double) ((maxPower - minPower) / frameNumber);
        	Output.Here(maxPower,minPower);
        	Output.Here(iteration);
            for (double i = minPower; i <= maxPower; i += iteration)
            {
            	power = i;
            	frames.Add(Generate(juliaC));
            	RhinoApp.Wait();
            	Output.Here(i);
            }
            
            return frames;
        }
        
        // Creates and animation of the change in maximum number of iterations of a Fractal
        internal List<Bitmap> IterationChange(int frameNumber, double minIterations, double maxIterations, Complex juliaC)
        {
        	List<Bitmap> frames = new List<Bitmap>();
        	double iteration = (double) ((minIterations - maxIterations) / frameNumber);
        	Output.Here(minIterations, maxIteration);
        	Output.Here(iteration);
            for (double i = minIterations; i <= maxIterations; i += iteration)
            {
            	maxIteration = (int) Math.Round(i);
            	frames.Add(Generate(juliaC));
            	RhinoApp.Wait();
            	Output.Here(i);
            }
            
            return frames;
        }
	}
}