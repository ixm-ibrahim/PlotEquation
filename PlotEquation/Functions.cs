using NCalc;
using NReco.VideoConverter;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Input;
using Rhino.Input.Custom;
using SharpAvi.Codecs;
using SharpAvi.Output;
// LABEL 4D STEPS // FIND UNNESSECARY REDRAWS// CHANGE WHEN CURVES/POINTS ARE DISPLAYED// FIX LAST ITERATION FOR LOOPS// MAKE 4D SOLID FROM POINTS// ERROR 4D: FAILURE IN CALCULATING THE RIGHT NUMBER OF ERRORS/FAILURE TO GET ALL SUCCESSFUL SURFACES// ADJUST CONICAL COORDINATES// ADJUST CURVE GROUPING// ADD 4D VARIABLES WHEN BAD VARS ENTERED// FIX ALL BREAK STATEMENTS// MAKE ERROR CATCH METHOD FOR 3D// ERROR WITH "xx"// FIX s ERROR
namespace PlotEquation
{
	public static class Output
	{
		// Functions for checking values and logic errors
        public static void ToRhinoConsole(object o)
        {
            RhinoApp.WriteLine("{0}", o);
        }
    	public static void ToRhinoConsole(string s, object o)
        {
    		RhinoApp.WriteLine("{0}: {1}", s, o);
        }
		
        internal static void Here(object o = null)
        {
        	RhinoApp.WriteLine("HERE {0}", o);
        }
        internal static void Here(object o, object p)
        {
        	RhinoApp.WriteLine("HERE {0}---{1}", o, p);
        }
        internal static void Here(object o, object p, object q)
        {
        	RhinoApp.WriteLine("HERE {0}---{1}---{2}", o, p, q);
        }
        
        internal static void ShowPoints(List<Point3d> points)
        {
        	for(int i = 0; i < points.Count; i++)
        	{
        		ToRhinoConsole("i", i);
        		ToRhinoConsole("\tx", points[i].X);
        		ToRhinoConsole("\ty", points[i].Y);
        		ToRhinoConsole("\tx", points[i].X);
        	}
        }
        internal static void ShowPoints(RhinoDoc doc, List<Point3d> points)
        {
        	for(int i = 0; i < points.Count; i++)
        		doc.Objects.AddPoint(points[i]);
        }
	}
	
	public static class String
	{
        internal static double GetDouble(string s)
        {
        	var e = new Expression(s);
        	Equation.SetParameters(ref e);
        	
        	return Convert.ToDouble(e.Evaluate());
        }
        
        internal static int IndexOfClosingBracket(IList<string> s, int start, int end, string closingBracket = "]")
        {
        	string openingBracket = null;
			int nestedBrackets = 1,
				e = 0;
			
			switch (closingBracket)
			{
				case "}":
					openingBracket = "{";
					break;
				case ")":
					openingBracket = "(";
					break;
				default:
					openingBracket = "[";
					break;
			}
			
			for(e = start; e < end && nestedBrackets != 0; e++)
				if (s[e].Equals(openingBracket))
					nestedBrackets++;
				else if (s[e].Equals(closingBracket))
					nestedBrackets--;
			
			return e < end ? e : -1;
        }
        internal static int IndexOfClosingBracket(IList<string> s, int start, string closingBracket = "]")
        {
        	string openingBracket = null;
			int nestedBrackets = 1,
				e = 0;
			
			switch (closingBracket)
			{
				case "}":
					openingBracket = "{";
					break;
				case ")":
					openingBracket = "(";
					break;
				default:
					openingBracket = "[";
					break;
			}
			
			for(e = start; e < s.Count && nestedBrackets != 0; e++)
				if (s[e].Equals(openingBracket))
					nestedBrackets++;
				else if (s[e].Equals(closingBracket))
					nestedBrackets--;
			
			return e < s.Count ? e : -1;
        }
        internal static int IndexOfClosingBracket(IList<string> s, string closingBracket = "]")
        {
        	string openingBracket = null;
			int nestedBrackets = 1,
				e = 0;
			
			switch (closingBracket)
			{
				case "}":
					openingBracket = "{";
					break;
				case ")":
					openingBracket = "(";
					break;
				default:
					openingBracket = "[";
					break;
			}
			
			for(e = 0; e < s.Count && nestedBrackets != 0; e++)
				if (s[e].Equals(openingBracket))
					nestedBrackets++;
				else if (s[e].Equals(closingBracket))
					nestedBrackets--;
			
			return e < s.Count ? e : -1;
        }
        
        internal static int GetArgumentNumber(string s)
        {
        	int nestedLoops = 0,
        		count = 1;
        	
        	foreach (char c in s)
        		if (c.Equals('{') || c.Equals('('))
        			nestedLoops++;
        		else if (c.Equals('}') || c.Equals(')'))
        			nestedLoops--;
        		else if (c.Equals(',') && nestedLoops == 0)
        			count++;
        	
    		return count;
        }
        
        internal static List<Duple> ArgumentBounds(string s)
        {
        	int nestedLoops = 0,
        		start = 0;
        	var list = new List<Duple>();
        	
        	for (int i = 0; i < s.Length; i++)
        		if (s[i].Equals('{') || s[i].Equals('('))
        			nestedLoops++;
        		else if (s[i].Equals('}') || s[i].Equals(')'))
        			nestedLoops--;
        		else if (s[i].Equals(',') && nestedLoops == 0)
        		{
        			list.Add(new Duple(start, i - 1));
        			start = i + 1;
        		}
        	
        	list.Add(new Duple(start, s.Length - 1));
        	
        	return list;
        }
        
        internal static string[] ToArray(string str)
		{
			return str.Split();
		}
		internal static string[] ToArray(string str, char[] separators)
		{
			return str.Split(separators, StringSplitOptions.None);
		}
		internal static string[] ToArray(string str, string[] separators)
		{
			return str.Split(separators, StringSplitOptions.None);
		}
		
		internal static string[] WordsToArray(string str)
		{
			char[] separators = {' ', ',', '.', ':', ';'};
			return str.Split(separators,StringSplitOptions.RemoveEmptyEntries);
		}
		
		internal static List<string> SplitLindenmayerRule(string rule, IList<string> drawVars, IList<string> moveVars, IList<string> controlVars)
		{
			var splitRule = new List<string>();
			
			for (int i = 0; i < rule.Length; i++)
			{
				int varLength = 0;
				
				for (int e = 0; e < drawVars.Count; e++)
				{
					if (rule.IndexOf(drawVars[e], StringComparison.Ordinal) == i)
					{
						varLength = drawVars[e].Length;
						break;
					}
				}
				for (int e = 0; e < moveVars.Count; e++)
				{
					if (rule.IndexOf(moveVars[e], StringComparison.Ordinal) == i)
					{
						varLength = moveVars[e].Length;
						break;
					}
				}
				for (int e = 0; e < controlVars.Count; e++)
				{
					if (rule.IndexOf(controlVars[e], StringComparison.Ordinal) == i)
					{
						varLength = controlVars[e].Length;
						break;
					}
				}
				
				varLength = (varLength == 0) ? 1 : varLength;
				splitRule.Add(rule.Substring(i, varLength));
				rule = rule.Substring(i + varLength);
				i = -1;
			}
			
			return splitRule;
		}
        
        internal static List<string> SeparateArguments(string s)
        {
        	var arguments = new List<string>();
        	var argumentBounds = ArgumentBounds(s);
        	
        	for (int i = 0; i < argumentBounds.Count; i++)
        		arguments.Add(s.Substring((int) argumentBounds[i].First, (int) (argumentBounds[i].Second - argumentBounds[i].First) + 1));
        	
        	return arguments;
        }
        
        internal static bool WordExistsAt(string equation, int index, string word)
        {
        	bool good = true;
        	
			for (int i = 0; i < word.Length && good; i++)
				good &= equation[index + i] == word[i];
        	
        	return good;
        }
        internal static bool WordExistsAt(string equation, int index, IList<string> words)
        {
        	for (int i = 0; i < words.Count; i++)
        		if (WordExistsAt(equation, index, words[i]))
        			return true;
        	
        	return false;
        }
        
        internal static string WordAt(string equation, int index, IList<string> words)
        {
        	for (int i = 0; i < words.Count; i++)
				if (WordExistsAt(equation, index, words[i]))
					return words[i];
        	
        	return null;
        }
	}
	
	public static class Generate
    {
		// Stereographic Projection
		// (Curve)
        internal static void StereographicProjection(RhinoDoc doc, IList<Point3d> points, double sphereRadius, int pointIteration, bool showPoints)
        {
            List<Point3d> newPoints = new List<Point3d>(),
            			  pts = new List<Point3d>();
            List<Curve> curves = new List<Curve>();
			double z = 0;
			
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].Z.Equals(sphereRadius))
					z = sphereRadius - pointIteration;
				else
					z = points[i].Z;
				
				newPoints.Add(Calculate.CartesianPoint(points[i].X / (sphereRadius - z), points[i].Y / (sphereRadius - z), 0));
			}
			
//			Output.ShowPoints(newPoints);
			curves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(newPoints, 3, CurveKnotStyle.ChordSquareRoot));
			
            Graph.CleanCurves(ref curves);
            
            if (!showPoints)
            	newPoints = null;
            
            Graph.EndAction(doc, "SteregraphicProjection", curves, newPoints);
        }
        // (Surface)
        internal static void StereographicProjection(RhinoDoc doc, IList<List<Point3d>> points, double sphereRadius, int pointIteration, bool showPoints)
        {
            List<Point3d> newPoints = new List<Point3d>(),
            			  pts = new List<Point3d>();
            List<Curve> curves = new List<Curve>();
			double z = 0;
			
			for (int i = 0; i < points.Count; i++)
			{
				for (int e = 0; e < points[i].Count; e++)
				{
					if (points[i][e].Z.Equals(1))
						z = sphereRadius - pointIteration;
					else
						z = points[i][e].Z;
					
					pts.Add(Calculate.CartesianPoint(points[i][e].X / (sphereRadius - z), points[i][e].Y / (sphereRadius - z), 0));
				}
				
				newPoints.AddRange(pts);
				curves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
				pts = new List<Point3d>();
			}
			
            Graph.CleanCurves(ref curves);
            
            if (!showPoints)
            	newPoints = null;
            
            Graph.EndAction(doc, "SteregraphicProjection", curves, newPoints);
        }
		
		// Inverse Stereographic Projection
		// (Curve)
        internal static void InverseStereographicProjection(RhinoDoc doc, IList<Point3d> points, double sphereRadius, bool showPoints)
        {
            List<Point3d> newPoints = new List<Point3d>(),
            			  pts = new List<Point3d>();
            List<Curve> curves = new List<Curve>();
            double x = 0,
            	   y = 0,
            	   z = 0,
            	   r2 = 0;
			
			for (int i = 0; i < points.Count; i++)
			{
				x = points[i].X;
				y = points[i].Y;
				z = points[i].Z;
				r2 = x*x + y*y;
				
				newPoints.Add(Calculate.CartesianPoint(2 * x / (r2 + sphereRadius), 2 * y / (r2 + sphereRadius), ((r2 - sphereRadius)) / (r2 + sphereRadius)));
			}
			
//			Output.ShowPoints(newPoints);
			curves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(newPoints, 3, CurveKnotStyle.ChordSquareRoot));
			
            Graph.CleanCurves(ref curves);
            
            if (!showPoints)
            	newPoints = null;
            
            Graph.EndAction(doc, "InverseSteregraphicProjection", curves, newPoints);
        }
        // (Surface)
        internal static void InverseStereographicProjection(RhinoDoc doc, IList<List<Point3d>> points, double sphereRadius, bool showPoints)
        {
            List<Point3d> newPoints = new List<Point3d>(),
            			  pts = new List<Point3d>();
            List<Curve> curves = new List<Curve>();
			double x = 0,
				   y = 0,
				   z = 0,
				   d = 0;
			
			for (int i = 0; i < points.Count; i++)
			{
				for (int e = 0; e < points[i].Count; e++)
				{
					x=  points[i][e].X;
					y = points[i][e].Y;
					z = points[i][e].Z;
					d = sphereRadius + x*x + y*y;
					
					newPoints.Add(Calculate.CartesianPoint(2 * x / d, 2 * y / d, (-sphereRadius + x*x + y*y) / d));
				}
				
				newPoints.AddRange(pts);
				curves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
				pts = new List<Point3d>();
			}
			
            Graph.CleanCurves(ref curves);
            
            if (!showPoints)
            	newPoints = null;
            
            Graph.EndAction(doc, "SteregraphicProjection", curves, newPoints);
        }
		
        // Constructs a line based off of two points
        internal static Curve Line(Point3d a, Point3d b)
        {
        	var pts = new List<Point3d>();
        	pts.Add(a);
        	pts.Add(b);
        	
        	return Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3);
        }
        
        // Constructs a triangle based off of three points
        internal static Brep Triangle(Point3d left, Point3d right, Point3d top)
        {
        	return Brep.CreateFromCornerPoints(left, right, top, .0000000000001);
        }
        
        // Constructs a square based off of four points
        internal static Brep Quadrilateral(Point3d bottomLeft, Point3d bottomRight, Point3d topLeft, Point3d topRight)
        {
        	return Brep.CreateFromCornerPoints(bottomLeft, bottomRight, topRight, topLeft, .0000000000001);
        }
        
        // Constructs a polygon border based off a list of ordered points
        internal static Curve PolygonBorder(List<Point3d> verticies)
        {
        	verticies.Add(verticies[0]);
        	return new Polyline(verticies).ToNurbsCurve();
        }
        
        // Constructs a polygon based off a list of ordered points
        internal static Brep Polygon(List<Point3d> verticies)
        {
        	var boundary = new List<Curve>();
        	boundary.Add(new Polyline(verticies).ToNurbsCurve());
        	
        	return Brep.CreatePlanarBreps(boundary)[0];
        }
        
        internal static Curve RegularPolygonBorder(Point3d origin, double radius, int sides, bool mirrored = false)
        {
        	var verticies = new List<Point3d>();
        	double start = 90,
        	end = 450;
        	
        	if (mirrored)
        	{
        		start = 270;
        		end = 630;
        	}
        	
        	for(double degree = start; degree < end; degree += (double) 360 / sides)
        		verticies.Add(new Point3d(radius * Math.Cos(degree * Math.PI / 180) + origin.X, radius * Math.Sin(degree * Math.PI / 180) + origin.Y, 0));
        	
        	return PolygonBorder(verticies);
        }
        
        internal static Brep RegularPolygon(Point3d origin, double radius, int sides, bool mirrored = false)
        {
        	var verticies = new List<Point3d>();
        	double start = 90,
        	end = 450;
        	
        	if (mirrored)
        	{
        		start = 270;
        		end = 630;
        	}
        	
        	for(double degree = start; degree < end; degree += (double) 360 / sides)
        		verticies.Add(new Point3d(radius * Math.Cos(degree * Math.PI / 180) + origin.X, radius * Math.Sin(degree * Math.PI / 180) + origin.Y, 0));
        	
        	verticies.Add(verticies[0]);
        	return Polygon(verticies);
        }
        
        // Constructs a tetrahedron based off of four points
        internal static Brep Tetrahedron(Point3d left, Point3d right, Point3d top, Point3d up)
        {
        	var faces = new List<Brep>();
        	faces.Add(Triangle(left, top, up));
        	faces.Add(Triangle(top, right, up));
        	faces.Add(Triangle(right, left, up));
        	faces.Add(Triangle(left, top, right));
        	
        	return Brep.JoinBreps(faces, .00000000001)[0];
        }
        
        // Constructs a tetrahedron based off of four points
        internal static Brep SquarePyramid(Point3d bottomLeft, Point3d bottomRight, Point3d topLeft, Point3d topRight, Point3d up)
        {
        	var faces = new List<Brep>();
        	faces.Add(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
        	faces.Add(Triangle(bottomLeft, bottomRight, up));
        	faces.Add(Triangle(bottomRight, topRight, up));
        	faces.Add(Triangle(topRight, topLeft, up));
        	faces.Add(Triangle(topLeft, bottomLeft, up));
        	
        	return Brep.JoinBreps(faces, .00000000001)[0];
        }
        
        // Constructs a hexahedron based off of four points and a vertical height
        internal static Brep Hexahedron(Point3d bottomLeft, Point3d bottomRight, Point3d topLeft, Point3d topRight, double height)
        {
        	var faces = new List<Brep>();
        	
        	Point3d upBottomLeft = bottomLeft,
        			upBottomRight = bottomRight,
        			upTopLeft = topLeft,
        			upTopRight = topRight;
        	
        	upBottomLeft.Z = bottomLeft.Z + height;
        	upBottomRight.Z = bottomRight.Z + height;
        	upTopLeft.Z = topLeft.Z + height;
        	upTopRight.Z = topRight.Z + height;
        	
        	faces.Add(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
        	faces.Add(Quadrilateral(bottomLeft, bottomRight, upBottomLeft, upBottomRight));
        	faces.Add(Quadrilateral(bottomRight, topRight, upBottomRight, upTopRight));
        	faces.Add(Quadrilateral(topLeft, topRight, upTopLeft, upTopRight));
        	faces.Add(Quadrilateral(topLeft, bottomLeft, upTopLeft, upBottomLeft));
        	faces.Add(Quadrilateral(upBottomLeft, upBottomRight, upTopLeft, upTopRight));
        	
        	return Brep.JoinBreps(faces, .00000000001)[0];
        }
        
        // Constructs a octahedron based off of six points
        internal static Brep Octahedron(Point3d downLeft, Point3d downRight, Point3d downBottom, Point3d upLeft, Point3d upRight, Point3d upTop)
        {
        	var faces = new List<Brep>();
        	
        	faces.Add(Triangle(downLeft, downRight, downBottom));
        	faces.Add(Triangle(downLeft, downBottom, upLeft));
        	faces.Add(Triangle(upLeft, downBottom, upRight));
        	faces.Add(Triangle(downBottom, downRight, upRight));
        	faces.Add(Triangle(upRight, upTop, downRight));
        	faces.Add(Triangle(downLeft, downRight, upTop));
        	faces.Add(Triangle(upTop, upLeft, downLeft));
        	faces.Add(Triangle(upLeft, upRight, upTop));
        	
        	return Brep.JoinBreps(faces, .00000000001)[0];
        }
        
        // Generates the Sierpinski Triangle
        internal static List<Brep> SierpinskiTriangle(int iterations, double side1, double side2, double side3, bool inverse = false)
        {
        	List<Brep> newTriangles = new List<Brep>();
    		Point3d left = new Point3d(0, 0, 0),
    				right = new Point3d(side1, 0, 0),
    				top = new Point3d();
    		double x = (Math.Pow(side1, 2) + Math.Pow(side2, 2) - Math.Pow(side3, 2)) / (2 * side1);
        	double scale = 1;
        	top = new Point3d(x, Math.Sqrt(Math.Pow(side2, 2) - Math.Pow(x, 2)),  0);
    		List<Brep> previouslyMade = new List<Brep>();
        	
        	for(int i = 1; i <= iterations; i++)
        	{
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			if (!inverse)
    			{
	        		if (newTriangles.Count != 0)
		        	{
		    			List<Brep> oldTriangles = newTriangles;
		    			newTriangles = new List<Brep>();
		    			
	        			foreach (Brep triangle in oldTriangles)
	        			{
	        				List<Point3d> corners = triangle.DuplicateVertices().ToList();
	        				Point3d leftPoint = corners[0],
	        						rightPoint = corners[1],
	        						topPoint = corners[2],
	        						leftMidpoint = Calculate.Midpoint(Line(leftPoint, topPoint)),
	        						rightMidpoint = Calculate.Midpoint(Line(rightPoint, topPoint)),
	        						newLeft = leftPoint,
	        						newRight = new Point3d(leftPoint.X + (right.X * scale), leftPoint.Y, 0),
	        						newUp = leftMidpoint;
	        				
	        				newTriangles.Add(Triangle(newLeft, newRight, newUp));
	        				
	        				newLeft = newRight;
	        				newRight = rightPoint;
	        				newUp = rightMidpoint;
	        				
	        				newTriangles.Add(Triangle(newLeft, newRight, newUp));
	        				
	        				newLeft = leftMidpoint;
	        				newRight = rightMidpoint;
	        				newUp = topPoint;
	        				
	        				newTriangles.Add(Triangle(newLeft, newRight, newUp));
	        			}
	        		}
	        		else
	        			newTriangles.Add(Triangle(left, right, top));
        		}
    			else
    			{
    				if (i > 2)
    				{
    					List<Brep> newlyMade = new List<Brep>();
    					
    					foreach (Brep triangle in previouslyMade)
	        			{
	        				List<Point3d> corners = triangle.DuplicateVertices().ToList();
	        				Point3d leftPoint = corners[0],
	        						rightPoint = corners[1],
	        						downPoint = corners[2],
	        						leftMidpoint = Calculate.Midpoint(Line(leftPoint, downPoint)),
	        						rightMidpoint = Calculate.Midpoint(Line(rightPoint, downPoint)),
	        						topMidpoint = Calculate.Midpoint(Line(leftPoint, rightPoint)),
	        						newLeft = new Point3d(leftMidpoint.X - side1 * scale, leftMidpoint.Y, 0),
	        						newRight = leftMidpoint,
	        						newDown = new Point3d(downPoint.X - side1 * scale, downPoint.Y, 0);
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
	        				
	        				newLeft = rightMidpoint;
	        				newRight = new Point3d(rightMidpoint.X + side1 * scale, rightMidpoint.Y, 0);
	        				newDown = new Point3d(downPoint.X + side1 * scale, downPoint.Y, 0);
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
	        				
	        				newLeft = new Point3d(topMidpoint.X - side1 * scale / 2, topMidpoint.Y + .5 * (leftPoint.Y - downPoint.Y), 0);
	        				newRight = new Point3d(topMidpoint.X + side1 * scale / 2, topMidpoint.Y + .5 * (leftPoint.Y - downPoint.Y), 0);
	        				newDown = topMidpoint;
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
    					}
    					
    					newTriangles.AddRange(newlyMade);
    					previouslyMade = newlyMade;
    				}
    				else if (i == 2)
    				{
    					newTriangles.Add(Triangle(Calculate.Midpoint(Line(left, top)), Calculate.Midpoint(Line(right, top)), Calculate.Midpoint(Line(left, right))));
    					previouslyMade = newTriangles;
    				}
    			}
    			
    			scale /= 2;
        	}
        	
        	return newTriangles;
        }
        
        // Generates the N-Flake
        internal static List<Curve> KochSnowflake(int iterations, int sides, double sideLength, double angle, int curveType, bool polygon = true)
        {
        	var newLines = new List<Curve>();
        	double radius = sideLength / (2 * Math.Sin((180 / sides) * Math.PI / 180));
        	var circle = new Arc(Point3d.Origin, radius, 2 * Math.PI);
        	var curveParameters = circle.ToNurbsCurve().DivideByCount(sides, true);
        	
        	if (polygon)
        		newLines.AddRange(RegularPolygonBorder(Point3d.Origin, radius, sides).DuplicateSegments().ToList());
        	else
        		newLines.Add(Line(new Point3d(-sideLength / 2, 0, 0), new Point3d(sideLength / 2, 0, 0)));
        	
			Output.ToRhinoConsole("Iteration: 1");
			
        	for(int i = 2; i <= iterations; i++)
        	{
    			Output.ToRhinoConsole("Iteration: " + i);
    			
    			List<Curve> oldLines = newLines;
    			newLines = new List<Curve>();
    			RhinoApp.Wait();
    			double flakeAngle = angle.Equals(90) ? 60 : angle;
    			
    			foreach (Curve line in oldLines)
    			{
    				double flatLength = line.GetLength() / (2 + (Math.Sin((180 - 2 * flakeAngle) * Math.PI / 180) / Math.Sin(flakeAngle * Math.PI / 180))),
    					   middleLength = line.GetLength() - 2 * flatLength,
    					   slope = Calculate.Inverse(Calculate.Slope(line.PointAtStart, line.PointAtEnd));
    				Curve firstFlat = Line(line.PointAtStart, line.PointAtNormalizedLength(flatLength / line.GetLength())),
    					  secondFlat = Line(line.PointAtNormalizedLength((flatLength + middleLength) / line.GetLength()), line.PointAtEnd),
    					  firstSide = firstFlat,//inaccessible?
    					  secondSide = firstFlat;
    				
					if (angle.Equals(90) && curveType == 2)
					{
						flatLength = line.GetLength() / 4;
						firstFlat = Line(line.PointAtStart, line.PointAtNormalizedLength(.25));
						secondFlat = Line(line.PointAtNormalizedLength(.75), line.PointAtEnd);
					}
    				
    				double height = Math.Sqrt(Math.Pow(flatLength, 2) - Math.Pow(middleLength / 2, 2));
    				Point3d midpoint = Calculate.Midpoint(line.PointAtStart, line.PointAtEnd),
    						corner = new Point3d(0,0,0);
    				
    				corner.X = height / -Math.Sqrt(1 + Math.Pow(slope, 2));
    				corner.Y = slope * height / -Math.Sqrt(1 + Math.Pow(slope, 2));
    				
    				if (Double.IsInfinity(slope))
    				{
    					corner.X = 0;
    					corner.Y = height;
    				}
    				
    				Point3d negativePoint = corner;
    				
    				negativePoint.X = midpoint.X - corner.X;
    				negativePoint.Y = midpoint.Y - corner.Y;
    				
    				corner.X += midpoint.X;
    				corner.Y += midpoint.Y;
    				
    				Point3d rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
    				
    				if (!polygon)
    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
    				
    				if (curveType != 3)
    					corner = Calculate.Distance(corner, rotatedEndpoint) < Calculate.Distance(negativePoint, rotatedEndpoint) ? corner : negativePoint;
    				
					if (curveType != 2)
					{
	    				if (angle.Equals(90))
	    				{
    						corner.X = firstFlat.GetLength() / -Math.Sqrt(1 + Math.Pow(slope, 2));
		    				corner.Y = slope * firstFlat.GetLength() / -Math.Sqrt(1 + Math.Pow(slope, 2));
		    				
							Point3d corner2 = corner;
		    				
		    				if (Double.IsInfinity(slope))
		    				{
		    					corner.X = 0;
		    					corner.Y = firstFlat.GetLength();
		    				}
		    				
		    				negativePoint = corner;
		    				
		    				negativePoint.X = firstFlat.PointAtEnd.X - corner.X;
		    				negativePoint.Y = firstFlat.PointAtEnd.Y - corner.Y;
		    				corner.X += firstFlat.PointAtEnd.X;
		    				corner.Y += firstFlat.PointAtEnd.Y;
		    				
		    				rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
		    				
		    				if (!polygon)
		    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
		    				
		    				if (curveType != 3)
		    					corner = Calculate.Distance(corner, rotatedEndpoint) < Calculate.Distance(negativePoint, rotatedEndpoint) ? corner : negativePoint;
		    				
		    				if (curveType == 3)
		    					newLines.Add(Line(firstFlat.PointAtEnd, negativePoint));
		    				
	    					if (Double.IsInfinity(slope))
		    				{
		    					corner2.X = 0;
		    					corner2.Y = firstFlat.GetLength();
		    				}
		    				
		    				negativePoint = corner2;
		    				
		    				negativePoint.X = secondFlat.PointAtStart.X - corner2.X;
		    				negativePoint.Y = secondFlat.PointAtStart.Y - corner2.Y;
		    				
		    				corner2.X += secondFlat.PointAtStart.X;
		    				corner2.Y += secondFlat.PointAtStart.Y;
		    				
		    				rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
		    				
		    				if (!polygon)
		    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
		    				
		    				if (curveType != 3)
		    					corner2 = Calculate.Distance(corner2, rotatedEndpoint) < Calculate.Distance(negativePoint, rotatedEndpoint) ? corner2 : negativePoint;
	    					
	    					if (curveType == 3)
	    					{
	    						newLines.Add(Line(negativePoint, secondFlat.PointAtStart));
	    						newLines.Add(Line(newLines[newLines.Count - 2].PointAtEnd, newLines[newLines.Count - 1].PointAtStart));
	    					}
		    				
	    					firstSide = Line(firstFlat.PointAtEnd, corner);
	    					secondSide = Line(corner2, secondFlat.PointAtStart);
	    					newLines.Add(Line(corner, corner2));
	    				}
	    				else
	    				{
	    					firstSide = Line(firstFlat.PointAtEnd, corner);
	    					secondSide = Line(corner, secondFlat.PointAtStart);
	    					
	    					if (curveType == 3)
	    					{
	    						newLines.Add(Line(firstFlat.PointAtEnd, negativePoint));
	    						newLines.Add(Line(negativePoint, secondFlat.PointAtStart));
	    					}
	    				}
					}
					else
					{
						if (!angle.Equals(90))
						{
							if (corner == negativePoint)
							{
								var tempCorner = new Point3d();
			    				
								tempCorner.X = height / -Math.Sqrt(1 + Math.Pow(slope, 2));
								tempCorner.Y = slope * height / -Math.Sqrt(1 + Math.Pow(slope, 2));
								
								if (Double.IsInfinity(slope))
								{
									tempCorner.X = 0;
									tempCorner.Y = height;
								}
								
								negativePoint = tempCorner;
								
								negativePoint.X = midpoint.X - tempCorner.X;
								negativePoint.Y = midpoint.Y - tempCorner.Y;
								
								tempCorner.X += midpoint.X;
								tempCorner.Y += midpoint.Y;
								
			    				rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
			    				
			    				if (!polygon)
			    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
			    				
								negativePoint = tempCorner;
							}
							
							firstSide = Line(firstFlat.PointAtEnd, corner);
		    				secondSide = Line(negativePoint, secondFlat.PointAtStart);
		    				newLines.Add(Line(firstSide.PointAtEnd, secondSide.PointAtStart));
						}
						else
						{
    						corner.X = firstFlat.GetLength() / -Math.Sqrt(1 + Math.Pow(slope, 2));
		    				corner.Y = slope * firstFlat.GetLength() / -Math.Sqrt(1 + Math.Pow(slope, 2));
		    				
							Point3d corner2 = corner,
									point1 = new Point3d(),
									point2 = new Point3d();
		    				
		    				if (Double.IsInfinity(slope))
		    				{
		    					corner.X = 0;
		    					corner.Y = firstFlat.GetLength();
		    				}
		    				
		    				negativePoint = corner;
		    				
		    				negativePoint.X = firstFlat.PointAtEnd.X - corner.X;
		    				negativePoint.Y = firstFlat.PointAtEnd.Y - corner.Y;
		    				
		    				corner.X += firstFlat.PointAtEnd.X;
		    				corner.Y += firstFlat.PointAtEnd.Y;
		    				
		    				rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
		    				
		    				if (!polygon)
		    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
		    				
		    				point2 = corner;
		    				corner = Calculate.Distance(corner, rotatedEndpoint) < Calculate.Distance(negativePoint, rotatedEndpoint) ? corner : negativePoint;
		    				point2 = corner != negativePoint ? negativePoint : point2;
		    				
	    					if (Double.IsInfinity(slope))
		    				{
		    					corner2.X = 0;
		    					corner2.Y = firstFlat.GetLength();
		    				}
		    				
		    				negativePoint = corner2;
		    				
		    				negativePoint.X = secondFlat.PointAtStart.X - corner2.X;
		    				negativePoint.Y = secondFlat.PointAtStart.Y - corner2.Y;
		    				
		    				corner2.X += secondFlat.PointAtStart.X;
		    				corner2.Y += secondFlat.PointAtStart.Y;
		    				
		    				rotatedEndpoint = Transform.RotateLine2D(line, -flakeAngle).PointAtEnd;
		    				
		    				if (!polygon)
		    					rotatedEndpoint = Transform.RotateLine2D(line, flakeAngle).PointAtEnd;
		    				
		    				point1 = corner2;
		    				corner2 = Calculate.Distance(corner2, rotatedEndpoint) < Calculate.Distance(negativePoint, rotatedEndpoint) ? negativePoint : corner2;
		    				point1 = corner2 != negativePoint ? negativePoint : point1;
		    				
		    				point1 = Line(corner, point1).PointAtNormalizedLength(.5);
		    				point2 = Line(corner2, point2).PointAtNormalizedLength(.5);
		    				
	    					firstSide = Line(firstFlat.PointAtEnd, corner);
	    					secondSide = Line(corner2, secondFlat.PointAtStart);
	    					newLines.Add(Line(corner, point1));
	    					newLines.Add(Line(point1, Calculate.Midpoint(line)));
	    					newLines.Add(Line(Calculate.Midpoint(line), point2));
	    					newLines.Add(Line(point2, corner2));
						}
					}
    				
    				newLines.Add(firstFlat);
    				newLines.Add(firstSide);
    				newLines.Add(secondSide);
    				newLines.Add(secondFlat);
    			}
    			
        	}
        	
        	return newLines;
        }
        
        // Generates the N-Flake
        internal static List<Brep> SierpinskiNGon(int iterations, int sides, double radius, bool centralPolygon = false)
        {
        	var newPolygons = new List<Brep>();
        	var circle = new Arc(Point3d.Origin, radius, 2 * Math.PI);
        	var curveParameters = circle.ToNurbsCurve().DivideByCount(sides, true);
        	double scale = 1,
        		   scaleFactor = Calculate.ScaleFactor(sides);
        	
        	newPolygons.Add(RegularPolygon(Point3d.Origin, radius, sides));
			Output.ToRhinoConsole("Iteration: 1");
			Output.ToRhinoConsole("Scale: 1");
			
        	for(int i = 2; i <= iterations; i++)
        	{
    			scale *= scaleFactor;
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			List<Brep> oldPolygons = newPolygons;
    			newPolygons = new List<Brep>();
    			
    			RhinoApp.Wait();
    			
    			foreach (Brep polygon in oldPolygons)
    			{
        			var verticies = polygon.DuplicateVertices().ToList();
    				Point3d centroid = Calculate.Centroid(polygon);
					bool mirrored = false || verticies[0].Y < verticies[verticies.Count / 2].Y;
    				
    				foreach (Point3d vertex in verticies)
    					newPolygons.Add(RegularPolygon(Line(centroid, vertex).PointAtNormalizedLength(1 - scaleFactor), radius * scale, sides, mirrored));
    				
    				if (centralPolygon)
    					newPolygons.Add(RegularPolygon(centroid, radius * scale, sides, !mirrored));
    			}
    			
        	}
        	
        	return newPolygons;
        }
        
        // Generates the Sierpinski Tetrahedron, the 3D equivalent of the Sierpinski Triangle
        internal static List<Brep> SierpinskiTetrahedron(int iterations, double side1, double side2, double side3, double height, bool inverse = false)
        {
        	var newTetrahedrons = new List<Brep>();
    		Point3d left = new Point3d(0, 0, 0),
    				right = new Point3d(side1, 0, 0),
    				top = new Point3d(side1, 0, 0),
    				up = new Point3d();
    		double x = (Math.Pow(side1, 2) + Math.Pow(side2, 2) - Math.Pow(side3, 2)) / (2 * side1);
        	top = new Point3d(x, Math.Sqrt(Math.Pow(side2, 2) - Math.Pow(x, 2)),  0);
        	Point3d center = Calculate.Centroid(Triangle(left, right, top));
        	up = new Point3d(center.X, center.Y, height);
    		var previouslyMade = new List<Brep>();
    		double scale = 1;
        	
        	for(int i = 1; i <= iterations; i++)
        	{
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			if (!inverse)
    			{
	        		if (newTetrahedrons.Count != 0)
		        	{
		    			List<Brep> oldTetrahedrons = newTetrahedrons;
		    			newTetrahedrons = new List<Brep>();
		    			
		    			RhinoApp.Wait();
		    			
	        			foreach (Brep tetrahedron in oldTetrahedrons)
	        			{
	        				List<Point3d> corners = tetrahedron.DuplicateVertices().ToList();
	        				Point3d leftPoint = corners[0],
	        						rightPoint = corners[3],
	        						topPoint = corners[1],
	        						upPoint = corners[2],
	        						leftMidpoint = Calculate.Midpoint(Line(leftPoint, topPoint)),
	        						rightMidpoint = Calculate.Midpoint(Line(rightPoint, topPoint)),
	        						newLeft = leftPoint,
	        						newRight = new Point3d(leftPoint.X + (right.X * scale), leftPoint.Y, leftPoint.Z),
	        						newTop = leftMidpoint,
	        						newUp = new Point3d(),
	        						newLeft1 = new Point3d(),
	        						newRight1 = new Point3d(),
	        						newTop1 = new Point3d();
	        				center = Calculate.Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newLeft1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				newLeft = newRight;
	        				newRight = rightPoint;
	        				newTop = rightMidpoint;
	        				center = Calculate.Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newRight1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				newLeft = leftMidpoint;
	        				newRight = rightMidpoint;
	        				newTop = topPoint;
	        				center = Calculate.Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newTop1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				center = Calculate.Centroid(Triangle(newLeft1, newRight1, newTop1));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft1, newRight1, newTop1, newUp));
	        			}
	        		}
	        		else
	        			newTetrahedrons.Add(Tetrahedron(left, right, top, up));
        		}
    			else
    			{
    				if (i > 2)
    				{
    					var newlyMade = new List<Brep>();
    					
    					foreach (Brep octahedron in previouslyMade)
	        			{
	        				List<Point3d> corners = octahedron.DuplicateVertices().ToList();
	        				Point3d downLeft = corners[0],
	        						downRight = corners[1],
	        						downBottom = corners[2],
	        						upLeft = corners[3],
	        						upRight = corners[4],
	        						upTop = corners[5],
	        						newDownRight = Calculate.Midpoint(downBottom, downLeft),
	        						newDownLeft = new Point3d(newDownRight.X - (side1 * scale), newDownRight.Y, newDownRight.Z),
	        						newDownBottom = new Point3d(downBottom.X - (side1 * scale), downBottom.Y, downBottom.Z),
	        						newUpRight = Calculate.Midpoint(downBottom, upLeft),
	        						newUpLeft = new Point3d(newUpRight.X - (side1 * scale), newUpRight.Y, newUpRight.Z),
	        						newUpTop = Calculate.Midpoint(downLeft, upLeft);
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownLeft = Calculate.Midpoint(downBottom, downRight);
	        				newDownRight = new Point3d(newDownLeft.X + (side1 * scale), newDownLeft.Y, newDownLeft.Z);
	        				newDownBottom = new Point3d(downBottom.X + (side1 * scale), downBottom.Y, downBottom.Z);
    						newUpLeft = Calculate.Midpoint(downBottom, upRight);
    						newUpRight = new Point3d(newUpLeft.X + (side1 * scale), newUpLeft.Y, newUpLeft.Z);
    						newUpTop = Calculate.Midpoint(downRight, upRight);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownBottom = Calculate.Midpoint(downLeft, downRight);
	        				newDownLeft = new Point3d(newDownBottom.X - .5 * (downBottom.X - downLeft.X), newDownBottom.Y + .5 * (downLeft.Y - downBottom.Y), newDownBottom.Z);
	        				newDownRight = new Point3d(newDownBottom.X + .5 * (downRight.X - downBottom.X), newDownBottom.Y + .5 * (downRight.Y - downBottom.Y), newDownBottom.Z);
    						newUpLeft = Calculate.Midpoint(downLeft, upTop);
    						newUpRight = Calculate.Midpoint(downRight, upTop);
    						newUpTop = new Point3d(newUpLeft.X + .5 * (upTop.X - upLeft.X), newUpLeft.Y + .5 * (upTop.Y - upLeft.Y), newUpLeft.Z);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownLeft = Calculate.Midpoint(upLeft, upTop);
	        				newDownRight = Calculate.Midpoint(upRight, upTop);
	        				newDownBottom = Calculate.Midpoint(upLeft, upRight);
    						center = Calculate.Centroid(Triangle(upLeft, upRight, upTop));
    						newUpLeft = Calculate.Midpoint(center, upLeft);
    						newUpLeft.Z = newUpLeft.Z + (height * scale);
    						newUpRight = Calculate.Midpoint(center, upRight);
    						newUpRight.Z = newUpRight.Z + (height * scale);
    						newUpTop = Calculate.Midpoint(center, upTop);
    						newUpTop.Z = newUpTop.Z + (height * scale);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
    					}
    					
    					newTetrahedrons.AddRange(newlyMade);
    					previouslyMade = newlyMade;
    				}
    				else if (i == 2)
    				{
    					newTetrahedrons.Add(Octahedron(Calculate.Midpoint(Line(left, top)), Calculate.Midpoint(Line(right, top)), Calculate.Midpoint(Line(left, right)), Calculate.Midpoint(Line(left, up)), Calculate.Midpoint(Line(right, up)), Calculate.Midpoint(Line(top, up))));
    					previouslyMade = newTetrahedrons;
    				}
    			}
    			
    			scale /= 2;
        	}
        	
        	return newTetrahedrons;
        }
        
        // (NEEDS INVERSE )Generates the Sierpinski Tetrahedron, the 3D equivalent of the Sierpinski Triangle
        internal static List<Brep> SierpinskiPyramid(int iterations, double side1, double side2, double angle, double height, bool inverse = false)
        {
        	var newPyramids = new List<Brep>();
    		Point3d bottomLeft = new Point3d(0, 0, 0),
    				bottomRight = new Point3d(side1, 0, 0),
    				topRight = new Point3d(side1 + side2 * Math.Cos((180 - angle) * Math.PI / 180), side2 * Math.Sin((180 - angle) * Math.PI / 180), 0),
    				topLeft = new Point3d(topRight.X - side1, topRight.Y, 0),
    				up = new Point3d(),
        			center = Calculate.Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
        	up = new Point3d(center.X, center.Y, height);
    		var previouslyMade = new List<Brep>();
    		double scale = 1;
        	
        	for(int i = 1; i <= iterations; i++)
        	{
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			if (!inverse)
    			{
	        		if (newPyramids.Count != 0)
		        	{
		    			List<Brep> oldPyramids = newPyramids;
		    			newPyramids = new List<Brep>();
		    			
		    			RhinoApp.Wait();
		    			
	        			foreach (Brep pyramid in oldPyramids)
	        			{
	        				List<Point3d> corners = pyramid.DuplicateVertices().ToList();
	        				Point3d bottomLeftPoint = corners[0],
	        						bottomRightPoint = corners[1],
	        						topLeftPoint = corners[3],
	        						topRightPoint = corners[2],
	        						upPoint = corners[4],
	        						newBottomLeft = bottomLeftPoint,
	        						newBottomRight = Calculate.Midpoint(bottomRightPoint, bottomLeftPoint),
	        						newTopLeft = Calculate.Midpoint(bottomLeftPoint, topLeftPoint),
	        						newTopRight = Calculate.Centroid(Quadrilateral(bottomLeftPoint, bottomRightPoint, topLeftPoint, topRightPoint)),
	        						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight)),
	        						newBottomLeft1 = new Point3d(),
	        						newBottomRight1 = new Point3d(),
	        						newTopLeft1 = new Point3d(),
	        						newTopRight1 = new Point3d();
	        				
	        				newUp.Z = newUp.Z + (height * scale);
	        				newBottomLeft1 = newUp;
	        						
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
	        				newBottomLeft = newBottomRight;
	        				newBottomRight = bottomRightPoint;
	        				newTopLeft = newTopRight;
	        				newTopRight = Calculate.Midpoint(bottomRightPoint, topRightPoint);
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newBottomRight1 = newUp;
    						
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
	        				newBottomLeft = newTopLeft;
	        				newBottomRight = newTopRight;
	        				newTopLeft = Calculate.Midpoint(topLeftPoint, topRightPoint);
	        				newTopRight = topRightPoint;
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newTopRight1 = newUp;
	        				
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
	        				newBottomRight = newBottomLeft;
	        				newBottomLeft = Calculate.Midpoint(topLeftPoint, bottomLeftPoint);
	        				newTopRight = newTopLeft;
	        				newTopLeft = topLeftPoint;
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newTopLeft1 = newUp;
	        				
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft1, newBottomRight1, newTopLeft1, newTopRight1));
    						newUp.Z = newUp.Z + (height * scale);
	        				
	        				newPyramids.Add(SquarePyramid(newBottomLeft1, newBottomRight1, newTopLeft1, newTopRight1, newUp));
	        				
	        			}
	        		}
	        		else
	        			newPyramids.Add(SquarePyramid(bottomLeft, bottomRight, topLeft, topRight, up));
        		}
    			
    			scale /= 2;
        	}
        	
        	return newPyramids;
        }
        
        // (NEEDS INVERSE) Generates the Sierpinski Octahedron, the 3D equivalent of the Sierpinski Triangle
        internal static List<Brep> SierpinskiOctahedron(int iterations, double side1, double side2, double angle, double upHeight, double downHeight, bool inverse = false)
        {
        	var newOctahedrons = new List<Brep>();
    		Point3d bottomLeft = new Point3d(0, 0, 0),
    				bottomRight = new Point3d(side1, 0, 0),
    				topRight = new Point3d(side1 + side2 * Math.Cos((180 - angle) * Math.PI / 180), side2 * Math.Sin((180 - angle) * Math.PI / 180), 0),
    				topLeft = new Point3d(topRight.X - side1, topRight.Y, 0),
    				up = new Point3d(),
    				down = new Point3d(),
        			center = Calculate.Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
        	up = new Point3d(center.X, center.Y, upHeight);
        	down = new Point3d(center.X, center.Y, -downHeight);
    		var previouslyMade = new List<Brep>();
    		double scale = 1;
    		
        	for(int i = 1; i <= iterations; i++)
        	{
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			if (!inverse)
    			{
	        		if (newOctahedrons.Count != 0)
		        	{
		    			List<Brep> oldOctahedrons = newOctahedrons;
		    			newOctahedrons = new List<Brep>();
		    			
		    			RhinoApp.Wait();
		    			
	        			foreach (Brep octahedron in oldOctahedrons)
	        			{
	        				List<Point3d> corners = octahedron.DuplicateVertices().ToList();
	        				Point3d bottomLeftPoint = corners[3],
	        						bottomRightPoint = corners[4],
	        						topRightPoint = corners[1],
	        						topLeftPoint = corners[0],
	        						upPoint = corners[5],
	        						downPoint = corners[2],
	        						newBottomLeft = bottomLeftPoint,
	        						newBottomRight = Calculate.Midpoint(bottomRightPoint, bottomLeftPoint),
	        						newTopLeft = Calculate.Midpoint(bottomLeftPoint, topLeftPoint),
	        						newTopRight = Calculate.Centroid(Quadrilateral(bottomLeftPoint, bottomRightPoint, topLeftPoint, topRightPoint)),
	        						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight)),
	        						newDown = newUp,
	        						newBottomLeft1 = new Point3d(),
	        						newBottomRight1 = new Point3d(),
	        						newTopLeft1 = new Point3d(),
	        						newTopRight1 = new Point3d(),
	        						newBottomLeft2 = new Point3d(),
	        						newBottomRight2 = new Point3d(),
	        						newTopLeft2 = new Point3d(),
	        						newTopRight2 = new Point3d();
	        				
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newBottomLeft1 = newUp;
	        				newBottomLeft2 = newDown;
	        						
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
	        				newBottomLeft = newBottomRight;
	        				newBottomRight = bottomRightPoint;
	        				newTopLeft = newTopRight;
	        				newTopRight = Calculate.Midpoint(bottomRightPoint, topRightPoint);
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newBottomRight1 = newUp;
	        				newBottomRight2 = newDown;
    						
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
	        				newBottomLeft = newTopLeft;
	        				newBottomRight = newTopRight;
	        				newTopLeft = Calculate.Midpoint(topLeftPoint, topRightPoint);
	        				newTopRight = topRightPoint;
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newTopRight1 = newUp;
	        				newTopRight2 = newDown;
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
	        				newBottomRight = newBottomLeft;
	        				newBottomLeft = Calculate.Midpoint(topLeftPoint, bottomLeftPoint);
	        				newTopRight = newTopLeft;
	        				newTopLeft = topLeftPoint;
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newTopLeft1 = newUp;
	        				newTopLeft2 = newDown;
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft1, newBottomRight1, newTopLeft1, newTopRight1));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft1, newTopRight1, newDown, newBottomLeft1, newBottomRight1, newUp));
	        				
    						newUp = Calculate.Centroid(Quadrilateral(newBottomLeft2, newBottomRight2, newTopLeft2, newTopRight2));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft2, newTopRight2, newDown, newBottomLeft2, newBottomRight2, newUp));
	        			}
	        		}
	        		else
	        			newOctahedrons.Add(Octahedron(topLeft, topRight, down, bottomLeft, bottomRight, up));
        		}
    			
    			scale /= 2;
        	}
        	
        	return newOctahedrons;
        }
        
        internal static List<Brep> KochSnowflake3D_CubeMethod(int iterations, double length, double width, double height, double angle, bool centralPolygon, bool inverse = false)
        {
        	var newCubes = new List<Brep>();
    		Point3d bottomLeft = new Point3d(0, 0, 0),
    				bottomRight = new Point3d(length, 0, 0),
    				topRight = new Point3d(length + width * Math.Cos((180 - angle) * Math.PI / 180), width * Math.Sin((180 - angle) * Math.PI / 180), 0),
    				topLeft = new Point3d(topRight.X - length, topRight.Y, 0),
        			center = Calculate.Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
    		var previouslyMade = new List<Brep>();
    		double scale = 1,
    			   actualWidth = topLeft.Y;
        	
    		newCubes.Add(Hexahedron(bottomLeft, bottomRight, topLeft, topRight, height));
    		Output.ToRhinoConsole("Iteration: 1");
    		Output.ToRhinoConsole("Scale: 1");
    		
    		for(int i = 2; i <= iterations; i++)
    		{
    			scale /= 3;
    			Output.ToRhinoConsole("Iteration: " + i);
    			Output.ToRhinoConsole("Scale: " + scale);
    			
    			if (!inverse)
    			{
	    			List<Brep> oldCubes = newCubes;
	    			newCubes = new List<Brep>();
	    			
	    			RhinoApp.Wait();
	    			//skips some?
        			foreach (Brep cube in oldCubes)
        			{
        				List<Point3d> corners = cube.DuplicateVertices().ToList();
        				Point3d downBottomLeftPoint = corners[0],
	        					downBottomRightPoint = corners[1],
	        					downTopRightPoint = corners[2],
	        					downTopLeftPoint = corners[3],
	        					upBottomLeftPoint = corners[5],
	        					upBottomRightPoint = corners[4],
	        					upTopRightPoint = corners[6],
	        					upTopLeftPoint = corners[7],
	        					newBottomLeft = Line(downBottomLeftPoint, downBottomRightPoint).PointAtNormalizedLength(.3333333333),
	    						newBottomRight = Line(downBottomLeftPoint, downBottomRightPoint).PointAtNormalizedLength(.6666666666),
	    						newTopRight = Line(newBottomRight, Line(downTopLeftPoint, downTopRightPoint).PointAtNormalizedLength(.6666666666)).PointAtNormalizedLength(.3333333333),
	    						newTopLeft = new Point3d(newTopRight.X - length * scale, newTopRight.Y, newTopRight.Z);
	    				
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -2 * length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				// Front Complete
	    				
	    				Transform.MoveSquareY(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, 2 * actualWidth * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -2 * length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				// Back Complete
	    				
	    				Transform.MoveSquareY(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -actualWidth * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				// Left Complete
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, length * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				// Top Complete
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -height * scale);
	    				newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
	    				
	    				// All Complete
	    				
	    				Transform.MoveSquareX(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, -length * scale);
	    				Transform.MoveSquareZ(ref newBottomLeft, ref newBottomRight, ref newTopLeft, ref newTopRight, height * scale);
	    				
	    				if (centralPolygon)
	    					newCubes.Add(Hexahedron(newBottomLeft, newBottomRight, newTopLeft, newTopRight, height * scale));
        			}
    			}
    		}
    		
        	return newCubes;
        }
        
        // Returns the list of Guids for every object added
        internal static List<Guid> Guids(RhinoDoc doc, List<Point3d> points)
        {
        	List<Guid> guids = new List<Guid>();
        	
        	foreach (Point3d point in points)
            {
            	guids.Add(doc.Objects.AddPoint(point));
            	doc.Objects.Hide(guids[guids.Count - 1], true);
            }
        	
        	return guids;
        }
        internal static List<Guid> Guids(RhinoDoc doc, List<Curve> curves)
        {
        	List<Guid> guids = new List<Guid>();
        	
        	foreach (Curve curve in curves)
            {
            	guids.Add(doc.Objects.AddCurve(curve));
            	doc.Objects.Hide(guids[guids.Count - 1], true);
            }
        	
        	return guids;
        }
        internal static List<Guid> Guids(RhinoDoc doc, List<Surface> surfaces)
        {
        	List<Guid> guids = new List<Guid>();
        	
        	foreach (Surface surface in surfaces)
            {
            	guids.Add(doc.Objects.AddSurface(surface));
            	doc.Objects.Hide(guids[guids.Count - 1], true);
            }
        	
        	return guids;
        }
        
        // Creates a box that tightly conforms to a surface
        internal static List<Point3d> BoundingBox(RhinoDoc doc, GeometryBase surface, bool accurate = false)
		{
            BoundingBox boundingBox = new BoundingBox(),
            			box = surface.GetBoundingBox(accurate);
			var corners = new List<Point3d>(8);
            
			if (box.IsValid)
				boundingBox = box;
			else
				boundingBox.Union(box);
			
			if (!boundingBox.IsValid)
				RhinoApp.WriteLine("BoundingBox failed. Unable to calculate bounding box.");
			else
			{
				corners.Add(boundingBox.Corner(false, false, false));
				corners.Add(boundingBox.Corner(true, false, false));
				corners.Add(boundingBox.Corner(true, true, false));
				corners.Add(boundingBox.Corner(false, true, false));
				corners.Add(boundingBox.Corner(false, false, true));
				corners.Add(boundingBox.Corner(true, false, true));
				corners.Add(boundingBox.Corner(true, true, true));
				corners.Add(boundingBox.Corner(false, true, true));
			}
			
			return corners;
		}
		
        // Generates surface meshes before hiding and showing in order to make 4-dimensional visualizations faster
        internal static List<Guid> Meshes(RhinoDoc doc, List<Surface> surfaces)
        {
            List<Guid> guids = new List<Guid>();
            
            for (int i = 0; i < surfaces.Count; i++)
            	guids.Add(doc.Objects.AddSurface((Surface)surfaces[i]));
            
            for (int i = 0; i < surfaces.Count; i++)
            {
        		RhinoApp.Wait();
            	var obj = doc.Objects.Find(guids[i]);
            	
            	if (obj != null)
            	{
            		RhinoApp.WriteLine("Generating Meshes ({0}/{1})", i + 1, surfaces.Count);
            	
					obj.CreateMeshes(MeshType.Render, doc.GetMeshingParameters(MeshingParameterStyle.None), false);
					obj.CommitChanges();
            	}
            	else
            		RhinoApp.WriteLine("Mesh Generation Failed at ({0}/{1})", i + 1, surfaces.Count);
				
            	doc.Objects.Hide(doc.Objects.Find(guids[i]), true);
            }
            
            return guids;
        }
        
        // Generates surface with the Rhino Command NetworkSurface
        internal static Surface SurfaceThroughPoints(RhinoDoc doc, List<List<Point3d>> points, string desc = null)
        {
            Output.ToRhinoConsole("SurfaceFromPoints", desc);
            List<Point3d> p = new List<Point3d>();
            
            foreach (List<Point3d> list in points)
            	p.AddRange(list);
            
            return NurbsSurface.CreateThroughPoints(p, points.Count, points[0].Count, 3, 3, false, false);
        }
		
        // Generates surface with the Rhino Command NetworkSurface
        internal static Surface SurfaceFromPoints(RhinoDoc doc, List<List<Point3d>> points, string desc = null)
        {
            Output.ToRhinoConsole("SurfaceFromPoints", desc);
            List<Point3d> p = new List<Point3d>();
            doc.Views.Redraw();
            
            foreach (List<Point3d> list in points)
            	p.AddRange(list);
            
            return NurbsSurface.CreateFromPoints(p, points.Count, points[0].Count, 3, 3);
        }
		
        internal static Surface Surface(RhinoDoc doc, List<List<Point3d>> points, IList<List<Curve>> curves, bool attemptNetworkSurface)
        {
        	List<Curve> uEndCurves = new List<Curve>(),
        				vEndCurves = new List<Curve>();
			Surface surface = null;
        	
        	if (curves[0].Count > 1)
            {
        		uEndCurves.Add(curves[0][0]);
	            uEndCurves.Add(curves[0][curves[0].Count - 1]);
            }
        	else
        		uEndCurves = curves[0];
        	
        	if (curves[1].Count > 1)
            {
        		vEndCurves.Add(curves[1][0]);
	            vEndCurves.Add(curves[1][curves[1].Count - 1]);
            }
        	else
        		vEndCurves = curves[1];
        	
        	if (attemptNetworkSurface)
        	{
	            if (surface == null)
	        		surface = Generate.NetworkSurface(doc, curves[0], curves[1], RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
	        		surface = Generate.NetworkSurface(doc, curves[1], curves[0], RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
	            	surface = Generate.NetworkSurface(doc, uEndCurves, curves[1], RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
	            	surface = Generate.NetworkSurface(doc, curves[1], uEndCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
	        		surface = Generate.NetworkSurface(doc, curves[0], vEndCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
	            	surface = Generate.NetworkSurface(doc, vEndCurves, curves[0], RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
	            if (surface == null)
        			Output.ToRhinoConsole("NetworkSurface failed. Try decreasing the interation values.");
        	}
        	
            if (surface == null)
	        	surface = Generate.SurfaceThroughPoints(doc, points);
            if (surface == null)
				surface = Generate.SurfaceFromPoints(doc, points);
            
            return surface;
        }
        
        // Generates surface with the Rhino Command NetworkSurface
        internal static Surface NetworkSurface(RhinoDoc doc, List<Curve> uCurves, List<Curve> vCurves, double edgeTolerance, double interiorTolerance, double angleTolerance, string desc = null)
        {
            Output.ToRhinoConsole("NetworkSurface", desc);
            List<Point3d> pts = new List<Point3d>();
            Surface surface;
            int error;
            
            surface = NurbsSurface.CreateNetworkSurface(uCurves, 1, 1, vCurves, 1, 1, edgeTolerance, interiorTolerance, angleTolerance, out error);
			
            switch (error)
            {
                case 1:
                    RhinoApp.WriteLine("Curve sorter failed");
                    break;
                case 2:
                    RhinoApp.WriteLine("Network initializing failed");
                    break;
                case 3:
                    RhinoApp.WriteLine("Failed to build surface");
                    break;
                case 4:
                    RhinoApp.WriteLine("Network surface is not valid");
                    break;
                default:
                    break;
            }

			if (error != 0)
				surface = null;

            return surface;
        }
        
        static List<Curve> DrawLindenmayerSystem(RhinoDoc doc, List<Curve> lines, ref List<Brep> pipes, string step, ref Turtle turtle, double lineLength, double lineIncrement, double lineScale, double lineThickness, double nextThickness, double thicknessIncrement, double thicknessScale, double turnAngle, double turnAngleIncrement, List<string> drawVars, List<string> moveVars, List<string> controlVars, bool drawFractal, ref List<Guid> objects)
        {
        	var splitSteps = String.SplitLindenmayerRule(step, drawVars, moveVars, controlVars);
    		var endpointParameters = new List<double>();
    		
			splitSteps.Add(" ");
			endpointParameters.Add(0);
			endpointParameters.Add(1);
			
			// Takes a rule and draws it in Rhino
    		for (int i = 0; i < splitSteps.Count - 1; i++)
    		{
    			string currentVar = splitSteps[i];
    			int parenthesisIndex = 0;
    			double tempAngle = 0;
    			double tempLineScale = 0;
    			double tempThicknessScale = 0;
    			double tempThicknessIncrement = 0;
    			bool keepGoing = true;
    			
    			if (splitSteps[i+1].Equals("("))
    			{
    				parenthesisIndex = String.IndexOfClosingBracket(splitSteps, i + 2, ")");
					string n = "";
					
					for (int e = i + 2; e < parenthesisIndex - 1; e++)
						n += splitSteps[e];
					
					if (String.GetArgumentNumber(n) != 1)
					{
						keepGoing = false;
						break;
					}
					
					n = "";
					
					for (int e = i + 2; e < parenthesisIndex - 1; e++)
						n += splitSteps[e];
					
					tempAngle = turnAngle;
					tempLineScale = lineScale;
					tempThicknessScale = thicknessScale;
					tempThicknessIncrement = thicknessIncrement;
					turnAngle = String.GetDouble(n);
					lineScale = turnAngle;
					thicknessIncrement = turnAngle;
					thicknessScale = turnAngle;
    			}

                // Drawing forward
                if (drawVars.Contains(currentVar))
                {
                    Vector3D p = turtle.position;
                    Vector3D v = p + turtle.forward * lineLength;
                    var radii = new List<double>();

                    radii.Add(lineThickness / 2);
                    radii.Add(nextThickness / 2);

                    lines.Add(Line(new Point3d(p.X, p.Y, p.Z), new Point3d(v.X, v.Y, v.Z)));
                    turtle.position = v;

                    if (!lineThickness.Equals(0) && !nextThickness.Equals(0))
                    {
                        pipes.AddRange(Brep.CreatePipe(lines.Last(), endpointParameters, radii, false, PipeCapMode.Flat, true, doc.ModelAbsoluteTolerance, doc.ModelAngleToleranceRadians));
                        lineThickness = nextThickness;
                    }

                    if (drawFractal)
                    {
                        objects.Add(doc.Objects.AddCurve(lines.Last()));

                        //if (!lineThickness.Equals(0) && !nextThickness.Equals(0))
                            //objects.Add(doc.Objects.AddBrep(pipes.Last()));

                        doc.Views.Redraw();

                        Graph.Wait(10);
                    }
                }
                //B *[^A] *{ RollArray, 360, 3, [^(30)A]}
                // Moving forward
                else if (moveVars.Contains(currentVar))
                    turtle.position += turtle.forward * lineLength;
                // Turning right and left (yaw)
                else if (splitSteps[i].Equals("+"))
                    turtle.Yaw(turnAngle);
                else if (splitSteps[i].Equals("-"))
                    turtle.Yaw(-turnAngle);
                // Pitching up and down
                else if (splitSteps[i].Equals("^"))
                    turtle.Pitch(turnAngle);
                else if (splitSteps[i].Equals("&"))
                    turtle.Pitch(-turnAngle);
                // Rolling right and left
                else if (splitSteps[i].Equals("@"))
                    turtle.Roll(turnAngle);
                else if (splitSteps[i].Equals("#"))
                    turtle.Roll(-turnAngle);
                // Reversing the meaning of all the rotation commands
                else if (splitSteps[i].Equals("!"))
                    turnAngle *= -1;
                // Turning to look the exact opposite direction
                else if (splitSteps[i].Equals("|"))
                {
                    //turtle.forward *= -1;
                    //turtle.rightAxis *= -1;
                    //turtle.upAxis *= -1;
                    turtle.Yaw(180);
                }
                // Scaling line length
                else if (splitSteps[i].Equals("*"))
                    lineLength *= lineScale;
                else if (splitSteps[i].Equals("/"))
                    lineLength /= lineScale;
                // Incrementing turning angle
                else if (splitSteps[i].Equals(":"))
                    turnAngle += turnAngleIncrement;
                else if (splitSteps[i].Equals(";"))
                    turnAngle -= turnAngleIncrement;
                // Incrementing line thickness
                else if (splitSteps[i].Equals("<"))
                    nextThickness += thicknessIncrement;
                else if (splitSteps[i].Equals(">"))
                    nextThickness -= thicknessIncrement;
                // Scaling line thickness
                else if (splitSteps[i].Equals("$"))
                    nextThickness *= thicknessScale;
                else if (splitSteps[i].Equals("%"))
                    nextThickness /= thicknessScale;
                // Branching
                else if (splitSteps[i].Equals("["))
                {
                    string newStep = "";
                    var temp = new Turtle();
                    temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                    int index = String.IndexOfClosingBracket(splitSteps, i + 1);

                    for (int o = i + 1; o < index - 1; o++)
                        newStep += splitSteps[o];

                    i = index - 1;

                    lines = DrawLindenmayerSystem(doc, lines, ref pipes, newStep + " ", ref temp, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                }// B*[^A]*{RollArray, 360, 3, [^(30)A]}
                 // Custom commands
                else if (splitSteps[i].Equals("{"))
                {
                    int index = String.IndexOfClosingBracket(splitSteps, i + 2, "}");
                    string command = "";
                    var arguments = new List<string>();

                    for (int e = i + 1; e < index - 1; e++)
                        command += splitSteps[e];

                    arguments = String.SeparateArguments(command);

                    if (arguments.Count == 2)
                    {
                        double probability = 1;

                        if (Double.TryParse(arguments[0], out probability) && probability >= 0 && probability <= 1)
                        {
                            var temp = new Turtle();
                            temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                            if (new Random((int)DateTime.Now.Ticks & 0x0000FFFF).NextDouble() <= probability)
                                lines = DrawLindenmayerSystem(doc, lines, ref pipes, arguments[1] + " ", ref turtle, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                        }

                    }
                    else if (arguments.Count == 3)
                    {
                        double probability = 1;

                        if (Double.TryParse(arguments[0], out probability) && probability >= 0 && probability <= 1)
                        {
                            var temp = new Turtle();
                            temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                            lines = DrawLindenmayerSystem(doc, lines, ref pipes, (new Random((int)DateTime.Now.Ticks & 0x0000FFFF).NextDouble() <= probability ? arguments[1] : arguments[2]) + " ", ref turtle, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                        }

                    }
                    else if (arguments.Count == 4)
                        if (arguments[0].Equals("YawArray"))
                        {
                            double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
                            command = "";

                            for (int e = 0; e < String.GetDouble(arguments[2]); e++)
                                command += "+(" + angle + ")" + arguments[3];

                            var temp = new Turtle();
                            temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                            lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", ref temp, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                        }
                        else if (arguments[0].Equals("PitchArray"))
                        {
                            double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
                            command = "";

                            for (int e = 0; e < String.GetDouble(arguments[2]); e++)
                                command += "^(" + angle + ")" + arguments[3];

                            var temp = new Turtle();
                            temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                            lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", ref temp, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                        }
                        else if (arguments[0].Equals("RollArray"))
                        {
                            double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
                            command = "";

                            for (int e = 0; e < String.GetDouble(arguments[2]); e++)
                                command += "@(" + angle + ")" + arguments[3];

                            var temp = new Turtle();
                            temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);

                            lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", ref temp, lineLength, lineIncrement, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, turnAngleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
                        }

                    i = index - 1;
                }// B*${.8, [^(randint(-6,6))A]}*${RollArray, randint(270, 360), randint(2,3), [^(randint(20,45))A]}
    			
    			if (splitSteps[i+1].Equals("(") && keepGoing)
    			{
					turnAngle = tempAngle;
					lineScale = tempLineScale;
					thicknessScale = tempThicknessScale;
					thicknessIncrement = tempThicknessIncrement;
					
					i = parenthesisIndex - 1;
    			}
    			
    			RhinoApp.Wait();
    		}
    		
        	return lines;
        }
        
        internal static List<Curve> LindenmayerSystem(RhinoDoc doc, ref List<Brep> pipes, int iterations, double lineLength, double lineIncrement, double lineScale, double lineThickness, double thicknessIncerement, double thicknessScale, double angle, double angleIncrement, List<string> drawVars, List<string> moveVars, List<string> controlVars, IList<string> drawRules, IList<string> moveRules, IList<string> controlRules, string axiom, Direction drawDirection, bool drawFractal)
        {
        	var steps = new List<string>();
        	var words = new List<string>();
        	string currentStep = axiom;
        	
        	/*
        	 * Yaw: rotation around the z-axis
        	 * Pitch: rotation around the x-axis
        	 * Roll: rotation around the y-axis
        	 * 
        	 */
        	
        	steps.Add(axiom);
        	words.Add("YawArray");
        	words.Add("PitchArray");
        	words.Add("RollArray");
        	
        	// Writing Loop
        	for (int i = 0; i < iterations; i++)
        	{
        		string newStep = "";
        		
        		Output.ToRhinoConsole("Writing Iteration (" + (i + 1) + "/" + iterations + ")");
        		
        		for (int e = 0; e < currentStep.Length; e++)
        		{
        			if (String.WordExistsAt(currentStep, e, words))
        			{
        				string word = String.WordAt(currentStep, e, words);
        				
        				e += word.Length - 1;
        				newStep += word;
        			}
        			else if (drawVars.Contains(currentStep[e].ToString()) && !drawRules[drawVars.IndexOf(currentStep[e].ToString())].ToLower().Equals("none"))
        				newStep += drawRules[drawVars.IndexOf(currentStep[e].ToString())];
        			else if (moveVars.Contains(currentStep[e].ToString()) && !moveRules[moveVars.IndexOf(currentStep[e].ToString())].ToLower().Equals("none"))
        				newStep += moveRules[moveVars.IndexOf(currentStep[e].ToString())];
        			else if (controlVars.Contains(currentStep[e].ToString()) && !controlRules[controlVars.IndexOf(currentStep[e].ToString())].ToLower().Equals("none"))
        				newStep += controlRules[controlVars.IndexOf(currentStep[e].ToString())];
        			else
        				newStep += currentStep[e].ToString();
        		}
        		
        		steps.Add(newStep);
        		currentStep = newStep;
        	}
        	Output.Here(steps.Last());
        	
        	var turtle = new Turtle();
        	turtle.Initialize(Vector3D.Zero, new Vector3D((drawDirection == Direction.RIGHT ? 1 : 0), (drawDirection == Direction.FORWARD ? 1 : 0), (drawDirection == Direction.UP ? 1 : 0)));
        	
        	var objects = new List<Guid>();
        	var lines = DrawLindenmayerSystem(doc, new List<Curve>(), ref pipes, steps.Last(), ref turtle, lineLength, lineIncrement, lineScale, lineThickness, lineThickness, thicknessIncerement, thicknessScale, angle, angleIncrement, drawVars, moveVars, controlVars, drawFractal, ref objects);
        	
        	Graph.DeleteObjects(doc, objects);
        	
        	return lines;
        }
	}
	
	public static class Graph
	{
		internal static void Wait(int milliSeconds = 1000)
		{
			DateTime startTime = DateTime.Now;
			var t = new TimeSpan();
			
			while (t.TotalMilliseconds < milliSeconds)
				t = DateTime.Now.Subtract(startTime);
		}
		
		// Converts a NurbsSurfacePointList to List<Point3d>
		internal static List<Point3d> ToPoint3dList(NurbsSurfacePointList nList)
		{
			List<Point3d> pList = new List<Point3d>();
			
			foreach (ControlPoint controlPoint in nList)
				pList.Add(controlPoint.Location);
			
			return pList;
		}
		internal static List<Point3d> ToPoint3dList(Curve curve, int pointIteration)
		{
			return curve.DivideEquidistant(curve.Rebuild(pointIteration, 3, true).GetLength() / pointIteration).ToList();
		}
		internal static List<List<Point3d>> ToPoint3dList(Surface surface, int pointIteration)
		{
        	List<List<Point3d>> allPoints = new List<List<Point3d>>();
        	List<Point3d> points = ToPoint3dList(surface.ToNurbsSurface().Rebuild(3, 3, pointIteration, pointIteration).Points),
        				  uPoints = new List<Point3d>();
			int u = Convert.ToInt32(Math.Sqrt(points.Count));
			
			for (int i = 1; i <= u; i++)
			{
				for (int e = i-1; e < u*i; e++)
					uPoints.Add(points[e]);
				
				allPoints.Add(uPoints);
				uPoints = new List<Point3d>();
			}
        	
        	return allPoints;
		}
		
        // Remove all extremely small curves and closed curves
        internal static void CleanCurves(ref List<Curve> c)
        {
            for (int i = 0; i < c.Count; i++)
            {
//                if (c[i].IsShort(.00000001) || c[i].IsClosed)
//                {
//                    c.RemoveAt(i);
//                    i--;
//                }
            }
        }
		
        // Curve-drawing method, used for all 2-dimensional graphs
        internal static void DrawCurve(RhinoDoc doc, List<Curve> intervals, List<Point3d> pts, List<Point3d> points)
        {
        	List<Guid> curveIDs = new List<Guid>();
			List<Guid> pointIDs = new List<Guid>();
            List<Curve> drawCurveIntervals = new List<Curve>();
            List<Point3d> drawPointIntervals = new List<Point3d>();
            drawCurveIntervals.AddRange(intervals);
            drawPointIntervals.AddRange(points);
            
            if (pts.Count > 1)
            {
            	drawCurveIntervals.Add(Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
            	
                for (int e = 0; e < drawCurveIntervals.Count; e++)
                {
                    curveIDs.Add(doc.Objects.AddCurve(drawCurveIntervals[e]));
                    curveIDs.Add(doc.Objects.AddPoint(drawCurveIntervals[e].PointAtEnd));
                }
        	}
            else if (pts.Count == 1)
            {
            	pointIDs.Add(doc.Objects.AddPoint(pts[0]));
            	for(int e = 0; e < 99999999; e++){}
            }
            
            for (int e = 0; e < drawPointIntervals.Count; e++)
                pointIDs.Add(doc.Objects.AddPoint(drawPointIntervals[e]));
            
            doc.Views.Redraw();
            for (int e = 0; e < curveIDs.Count; e++)
                doc.Objects.Delete(doc.Objects.Find(curveIDs[e]), true);
            for (int e = 0; e < pointIDs.Count; e++)
                doc.Objects.Delete(doc.Objects.Find(pointIDs[e]), true);
        }
        
        // Rebuilds surfaces to make them less complicated and thus easier to hide and show; used for 4-dimensional visuals
        internal static List<Surface> RebuildSurfaces(List<Surface> surfaces, int uPointCount, int vPointCount) // @Check
        {
        	List<Surface> rebuilt = new List<Surface>();
        	Surface surface = null;
        	Output.Here("A");
            for (int i = 0; i < surfaces.Count; i++)
            {
            	RhinoApp.WriteLine("Rebuilding Surfaces ({0}/{1})", i + 1, surfaces.Count);
            	surface = surfaces[i].Rebuild(3, 3, uPointCount, vPointCount);
            	
            	if (surface != null)
            		rebuilt.Add(surface);
            	else
            		rebuilt.Add(surfaces[i]);Output.Here(i);
            		
            }
        	
        	return rebuilt;
        }
        
        // Counts the number of surface errors in 4-dimensional graphing method
        internal static int SurfaceErrorCount(List<bool> errors)
        {
        	int count = 0;
        	
        	foreach (bool error in errors)
        		if (!error)
        			count++;
        	
        	return count;
        }
        
        // Provides feedback on errors and has the 4-dimensional visualization options
		internal static void ShowErrors(RhinoDoc doc, string EnglishName, bool error, bool mostFailed, bool allFailed = false)
		{
            if (allFailed)
                RhinoApp.WriteLine("All surfaces failed to generate. Recommend changing \"PointIteration\" value or manually generating surfaces using 3D commands.", EnglishName);
            else
            	if (mostFailed)
            		RhinoApp.WriteLine("Most surfaces failed to generate. Recommend changing \"PointIteration\" value or manually generating surfaces using 3D commands.", EnglishName);
            	else if (error)
            		RhinoApp.WriteLine("There were some failures in surface generation. Recommend changing \"PointIteration\" value or manually generating surfaces using 3D commands.", EnglishName);
        }
		
        // Generates a polysurface of a 3D representation of a 4D equation
        internal static List<Brep> BlendSurfaces(RhinoDoc doc, List<Surface> surfaces)
        {
        	/*
        	 * 1. Isolate two consecutive surfaces from list of surfaces
        	 * 2. Extract Edges
        	 * 3. Loft both edges
        	 * 4. Join the loft with the two other surfaces to form one new polysurface without deleting the original surfaces
        	 * 5. Repeat with all consecutive pairs of surfaces, and add each to a list
        	 * 
        	 * */
        	
        	List<Brep> breps = new List<Brep>();
        	
        	for (int i = 0; i < surfaces.Count - 1; i++)
        	{
        		List<Curve> curves = new List<Curve>();
        		List<Brep> joining = new List<Brep>();
        		
        		curves.AddRange(Curve.JoinCurves(surfaces[i].ToBrep().DuplicateEdgeCurves(true), doc.ModelAbsoluteTolerance).ToList());
        		curves.AddRange(Curve.JoinCurves(surfaces[i+1].ToBrep().DuplicateEdgeCurves(true), doc.ModelAbsoluteTolerance).ToList());
        		
        		joining.Add(surfaces[i].ToBrep());
        		joining.Add(surfaces[i+1].ToBrep());
        		joining.AddRange(Brep.CreateFromLoft(curves, Point3d.Unset, Point3d.Unset, LoftType.Straight, false));
        		//Output.ToRhinoConsole("Count", Brep.JoinBreps(joining.ToArray(), doc.ModelAbsoluteTolerance).ToList().Count);
        		breps.AddRange(Brep.JoinBreps(joining.ToArray(), doc.ModelAbsoluteTolerance).ToList());
        		//breps.AddRange(joining);
        	}
        	
        	return breps;
        }
        
        // If more than one object is created, a group is made to make selecting the graph easier
        internal static void CreateGroup(RhinoDoc doc, List<Surface> surfaces, List<Curve> curves = null, List<Point3d> points = null)
        {
        	List<Guid> IDs = new List<Guid>();
        	
        	if (surfaces != null)
				for (int i = 0; i < surfaces.Count; i++)
					IDs.Add(doc.Objects.AddSurface(surfaces[i]));
			if (curves != null)
				for (int i = 0; i < curves.Count; i++)
					IDs.Add(doc.Objects.AddCurve(curves[i]));
			if (points != null)
		        for (int i = 0; i < points.Count; i++)
		        	IDs.Add(doc.Objects.AddPoint(points[i]));
	        
	        if (IDs.Count > 1)
	        	doc.Groups.Add(IDs);
        }
        internal static void CreateGroup(RhinoDoc doc, List<List<Guid>> guids)
        {
        	List<Guid> IDs = new List<Guid>();
        	
        	for (int i = 0; i < guids.Count; i++)
        		doc.Groups.Add(guids[i]);
        }
        internal static void CreateGroup(RhinoDoc doc, List<Guid> guids)
        {
        	doc.Groups.Add(guids);
        }
        
        // Delete graph objects from Rhino interface
        internal static void ShowObjects(RhinoDoc doc, List<Guid> guids)
        {
        	foreach (Guid g in guids)
        		doc.Objects.Show(doc.Objects.Find(g), true);
        }
        internal static void ShowObjects(RhinoDoc doc, List<List<Guid>> guids)
        {
        	List<Guid> IDs = new List<Guid>();
        	
        	for (int i = 0; i < guids.Count; i++)
        		foreach (Guid g in guids[i])
        			doc.Objects.Show(doc.Objects.Find(g), true);
        }
        
        // Delete objects from Rhino interface
        internal static void DeleteObject(RhinoDoc doc, RhinoObject o)
        {
        	doc.Objects.Show(o, true);
        	doc.Objects.Delete(o, true);
        }
        internal static void DeleteObject(RhinoDoc doc, Guid guid)
        {
        	doc.Objects.Show(doc.Objects.Find(guid), true);
        	doc.Objects.Delete(doc.Objects.Find(guid), true);
        }
        internal static void DeleteObjects(RhinoDoc doc, List<Guid> guids)
        {
        	foreach (Guid g in guids)
        	{
        		doc.Objects.Show(doc.Objects.Find(g), true);
        		doc.Objects.Delete(doc.Objects.Find(g), true);
        	}
        }
        internal static void DeleteObjects(RhinoDoc doc, List<List<Guid>> guids)
        {
        	List<Guid> IDs = new List<Guid>();
        	
        	for (int i = 0; i < guids.Count; i++)
        		foreach (Guid g in guids[i])
        		{
        			doc.Objects.Show(doc.Objects.Find(g), true);
        			doc.Objects.Delete(doc.Objects.Find(g), true);
        		}
        }
        
        // Saves every frame of an animation to a directory
        internal static void Record(RhinoDoc doc, List<Guid> guids, string destination, int width, int height, string imageExtension, string videoExtension, decimal fps)
        {
        	List<Bitmap> frames = new List<Bitmap>();
        	ImageFormat imgFormat = null;
        	bool image = imageExtension != "NONE",
        		 video = videoExtension != "NONE";
        	string videoName = videoExtension == ".avi" ? "Animation.avi" : "eebsPH.avi";
        	
        	switch (imageExtension)
            {
            	case ".gif":
            		imgFormat = ImageFormat.Gif;
            		break;
            	case ".ico":
            		imgFormat = ImageFormat.Icon;
            		break;
            	case ".jpeg":
            		imgFormat = ImageFormat.Jpeg;
            		break;
            	case ".png":
            		imgFormat = ImageFormat.Png;
            		break;
            	default:
            		imgFormat = ImageFormat.Bmp;
            		break;
            }
        	
        	if (video)
        		Output.ToRhinoConsole("Readying frames...");
        	
        	for (int current = 0; current < guids.Count && (image || video); current++)
            {
            	doc.Objects.Show(doc.Objects.Find(guids[current]), true);
            	doc.Views.Redraw();
        		RhinoApp.Wait();
            	
            	RhinoView View = doc.Views.ActiveView;
        		var bitmap = View.CaptureToBitmap(new Size(width, height));
        		
        		frames.Add(bitmap);
        		
        		if (image)
        			try
	    			{
	    				bitmap.Save(@destination + "\\" + current + imageExtension, imgFormat);
	        			Output.ToRhinoConsole("Saved Frame " + current + "/" + guids.Count);
	        		}
	        		catch (Exception e)
	        		{
	        			Output.ToRhinoConsole("Could not save frame " + current + "as an image.");
	        			Output.ToRhinoConsole(e.Message);
	        		}
        		
        		doc.Objects.Hide(doc.Objects.Find(guids[current]), true);
            }
            
        	if (video)
        	{
        		var writer = new AviWriter(@destination + "\\" + videoName) { FramesPerSecond = fps, EmitIndex1 = true};
				var encoder = new MotionJpegVideoEncoderWpf(frames[0].Size.Width, frames[0].Size.Height, 100);
				var stream = writer.AddEncodingVideoStream(encoder);
				
	            stream.Width = frames[0].Size.Width;
				stream.Height = frames[0].Size.Height;
				
				var buffer = new byte[stream.Width * stream.Height * 4];
				
				for (int i = 0; i < frames.Count; i++)
				{
				    var bits = frames[i].LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
					System.Runtime.InteropServices.Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
				    stream.WriteFrame(true, buffer.ToArray(), 0, buffer.Length);
				    
				    Output.ToRhinoConsole("Saved Frame " + (i + 1) + "/" + frames.Count + " in Video");
				    RhinoApp.Wait();
				}
				
				writer.Close();
				
				if (videoExtension != ".avi")
				{
					FFMpegConverter videoConverter = new FFMpegConverter();
					
					videoConverter.ConvertMedia(@destination + "\\eebsPH.avi", @destination + "\\Animation" + videoExtension, videoExtension.Substring(1));
					
					if(System.IO.File.Exists((@destination + "\\eebsPH.avi")))
		            {
		                // Use a try block to catch IOExceptions, to
		                // handle the case of the file already being
		                // opened by another process.
		                try
		                {
		                	System.IO.File.Delete((@destination + "\\eebsPH.avi"));
		                }
		                catch (System.IO.IOException e)
		                {
		                    Output.ToRhinoConsole(e.Message);
		                }
		            }
				}
        	}
        }
        internal static void Record(RhinoDoc doc, List<List<Guid>> guids, string destination, int width, int height, string imageExtension, string videoExtension, decimal fps)
        {
        	List<Bitmap> frames = new List<Bitmap>();
        	ImageFormat imgFormat = null;
        	bool image = imageExtension != "NONE",
        		 video = videoExtension != "NONE";
        	string videoName = videoExtension == ".avi" ? "Animation.avi" : "eebsPH.avi";
        	
        	switch (imageExtension)
            {
            	case ".gif":
            		imgFormat = ImageFormat.Gif;
            		break;
            	case ".ico":
            		imgFormat = ImageFormat.Icon;
            		break;
            	case ".jpeg":
            		imgFormat = ImageFormat.Jpeg;
            		break;
            	case ".png":
            		imgFormat = ImageFormat.Png;
            		break;
            	default:
            		imgFormat = ImageFormat.Bmp;
            		break;
            }
        	
        	for (int current = 0; current < guids.Count && (image || video); current++)
            {
            	foreach (List<Guid> gs in guids)
            		doc.Objects.Show(doc.Objects.Find(gs[current]), true);
            	doc.Views.Redraw();
            	RhinoApp.Wait();
            	
            	RhinoView View = doc.Views.ActiveView;
        		var bitmap = View.CaptureToBitmap(new Size(width, height));
        		
        		frames.Add(bitmap);
        		
        		if (image)
        		{
        			try
	    			{
	    				bitmap.Save(@destination + "\\" + current + imageExtension, imgFormat);
	        			Output.ToRhinoConsole("Saved Frame " + current + "/" + guids.Count);
	        		}
	        		catch (Exception e)
	        		{
	        			Output.ToRhinoConsole("Could not save frame " + current + "as an image.");
	        			Output.ToRhinoConsole(e.Message);
	        		}
        		}
        		
        		foreach (List<Guid> gs in guids)
            		doc.Objects.Hide(doc.Objects.Find(gs[current]), true);
            }
            
        	if (video)
        	{
				var writer = new AviWriter(@destination + "\\" + videoName) { FramesPerSecond = fps, EmitIndex1 = true};
				var encoder = new MotionJpegVideoEncoderWpf(frames[0].Size.Width, frames[0].Size.Height, 100);
				var stream = writer.AddEncodingVideoStream(encoder);
				
	            stream.Width = frames[0].Size.Width;
				stream.Height = frames[0].Size.Height;
				
				var buffer = new byte[stream.Width * stream.Height * 4];
				
				for (int i = 0; i < frames.Count; i++)
				{
				    var bits = frames[i].LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
					System.Runtime.InteropServices.Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
				    stream.WriteFrame(true, buffer.ToArray(), 0, buffer.Length);
				    
				    Output.ToRhinoConsole("Saved Frame " + (i + 1) + "/" + frames.Count + " in Video");
				    RhinoApp.Wait();
				}
				
				writer.Close();
				
				if (videoExtension != ".avi")
				{
					FFMpegConverter videoConverter = new FFMpegConverter();
					
					videoConverter.ConvertMedia(@destination + "\\eebsPH.avi", @destination + "\\Animation" + videoExtension, videoExtension.Substring(1));
					
					if(System.IO.File.Exists((@destination + "\\eebsPH.avi")))
		            {
		                // Use a try block to catch IOExceptions, to
		                // handle the case of the file already being
		                // opened by another process.
		                try
		                {
		                	System.IO.File.Delete((@destination + "\\eebsPH.avi"));
		                }
		                catch (System.IO.IOException e)
		                {
		                    Output.ToRhinoConsole(e.Message);
		                }
		            }
				}
        	}
        }
        
        // Menu for Recording Animation
        internal static void RecordAnimation(RhinoDoc doc, List<List<Guid>> points, List<List<Guid>> curves, List<List<Guid>> both, List<Guid> surfaces)
        {
            List<string> imgFormatList = new List<string>(),
            			 vidFormatList = new List<string>(),
            			 types = new List<string>();
            GetOption getOption = new GetOption();
        	string destination = null;
        	int typeIndex = 0,
        		ifIndex = 0,
        		vfIndex = 0;
        	bool back = false;
        	
            imgFormatList.Add(".bmp");
            imgFormatList.Add(".ico");
            imgFormatList.Add(".gif");
            imgFormatList.Add(".jpeg");
            imgFormatList.Add(".png");
            imgFormatList.Add("NONE");
            vidFormatList.Add(".avi");
            vidFormatList.Add(".flv");
            vidFormatList.Add(".mov");
            vidFormatList.Add(".mp4");
            vidFormatList.Add("NONE");
        	types.Add("Surfaces");
        	types.Add("Curves");
        	types.Add("Points");
        	types.Add("Both");
            
        	OptionDouble fps = new OptionDouble(30, 1, 200);
        	OptionInteger width = new OptionInteger(1028, 320, 3840);
        	OptionInteger height = new OptionInteger(728, 200, 2160);
        	
        	while (!back)
            {
            	getOption.ClearCommandOptions();
	            getOption.SetCommandPrompt("Save Options");
	            var dIndex = getOption.AddOption("Destination", destination);
	            getOption.AddOptionInteger("Width", ref width);
	            getOption.AddOptionInteger("Height", ref height);
	            var ieIndex = getOption.AddOptionList("Image_Extension", imgFormatList, ifIndex);
	            var veIndex = getOption.AddOptionList("Video_Extension", vidFormatList, vfIndex);
	            getOption.AddOptionDouble("FPS", ref fps);
	            var bIndex = getOption.AddOption("Back");
	            
	            GetResult get_rc = new GetResult();
	            
	            while (getOption.OptionIndex() != bIndex && (get_rc != GetResult.Cancel || destination == null))
	            {
	            	// perform the get operation. This will prompt the user to input a string, but also
	                // allow for command line options defined above
					
	                get_rc = getOption.Get();
	                
	                if (getOption.OptionIndex() == dIndex)
	            	{
	            		using (var folderDialog = new FolderBrowserDialog())
	            		{
				            if (folderDialog.ShowDialog() == DialogResult.OK)
				            	destination = folderDialog.SelectedPath;
	            		}
	            		
	            		getOption.ClearCommandOptions();
			            getOption.SetCommandPrompt("Save Options");
			            dIndex = getOption.AddOption("Destination", destination);
			            getOption.AddOptionInteger("Width", ref width);
			            getOption.AddOptionInteger("Height", ref height);
			            ieIndex = getOption.AddOptionList("Image_Extension", imgFormatList, ifIndex);
			            veIndex = getOption.AddOptionList("Video_Extension", vidFormatList, vfIndex);
			            getOption.AddOptionDouble("FPS", ref fps);
			            bIndex = getOption.AddOption("Back");
	            	}
	            	else if (getOption.OptionIndex() == ieIndex)
	            		ifIndex = getOption.Option().CurrentListOptionIndex;
	            	else if (getOption.OptionIndex() == veIndex)
	            		vfIndex = getOption.Option().CurrentListOptionIndex;
	            	else if (getOption.OptionIndex() == bIndex)
						back = true;
	            	else if (destination == null)
	            	{
	            		Output.ToRhinoConsole("Please choose a directory.");
	            		continue;
	            	}
	            }
	            
	            getOption.ClearCommandOptions();
	            getOption.SetCommandPrompt("Ready the Viewing Window for Animation");
	            var tIndex = getOption.AddOptionList("Type", types, typeIndex);
	            var sIndex = getOption.AddOption("Show");
	            var aIndex = getOption.AddOption("Save");
	            var b2Index = getOption.AddOption("Back");
	            
	            get_rc = new GetResult();
	            
	            while (!back && getOption.OptionIndex() != b2Index)
	            {
	            	// perform the get operation. This will prompt the user to input a string, but also
	                // allow for command line options defined above
	                
	                get_rc = getOption.Get();
	                
	                if (getOption.OptionIndex() == sIndex)
	                	switch (typeIndex)
	        			{
	        				case 1:
	        					ShowMotion(doc, curves, true);
	        					break;
	        				case 2:
	        					ShowMotion(doc, points, true);
	        					break;
	        				case 3:
	        					ShowMotion(doc, both, true);
	        					break;
	        				default:
	        					ShowMotion(doc, surfaces, true);
	        					break;
	            		}
	            	else if (getOption.OptionIndex() == aIndex)
	            		switch (typeIndex)
						{
							case 1:
			        			Record(doc, curves, destination, width.CurrentValue, height.CurrentValue, imgFormatList[ifIndex], vidFormatList[vfIndex], Convert.ToDecimal(fps.CurrentValue));
								break;
							case 2:
								Record(doc, points, destination, width.CurrentValue, height.CurrentValue, imgFormatList[ifIndex], vidFormatList[vfIndex], Convert.ToDecimal(fps.CurrentValue));
								break;
							case 3:
								Record(doc, both, destination, width.CurrentValue, height.CurrentValue, imgFormatList[ifIndex], vidFormatList[vfIndex], Convert.ToDecimal(fps.CurrentValue));
								break;
							default:
								Record(doc, surfaces, destination, width.CurrentValue, height.CurrentValue, imgFormatList[ifIndex], vidFormatList[vfIndex], Convert.ToDecimal(fps.CurrentValue));
								break;
						}
	            	else if (getOption.OptionIndex() == tIndex)
	            		typeIndex = getOption.Option().CurrentListOptionIndex;
	            }
        	}
        }
        
        // My dynamic visualization method for 4-dimensional graphs
        internal static void ShowMotion(RhinoDoc doc, List<Guid> guids, bool move)
        {
            EscapeKeyEventHandler handler = new EscapeKeyEventHandler("Press <Esc> to stop visualization.");
            int current = 0;
            
            if (move)
            {
	            while (!handler.EscapeKeyPressed)
	            {
	            	doc.Objects.Hide(doc.Objects.Find(guids[current]), true);
	            	
	            	if (current == guids.Count - 1)
	        			doc.Objects.Show(doc.Objects.Find(guids[0]), true);
	            	else
	        			doc.Objects.Show(doc.Objects.Find(guids[current + 1]), true);
	            	
	            	doc.Views.Redraw();
	            	for(int e = 0; e < 9999999; e++) {}
	            	
	            	if (current + 1 == guids.Count)
	            		current = -1;
	            	
	            	current++;
	            }
        	}
            else
            {
            	ShowObjects(doc, guids);
	            doc.Views.Redraw();
            	
            	while (!handler.EscapeKeyPressed) {}
            }
            
            for (int e = 0; e < guids.Count; e++)
            	doc.Objects.Hide(doc.Objects.Find(guids[e]), true);
            
            
            doc.Views.Redraw();
            handler.Dispose();
        }
        internal static void ShowMotion(RhinoDoc doc, List<List<Guid>> guids, bool move)
        {
            EscapeKeyEventHandler handler = new EscapeKeyEventHandler("Press <Esc> to stop visualization.");
            int current = 0;
            
            if (move)
            {
	            while (!handler.EscapeKeyPressed)
	            {
	            	for (int c = 0; c < guids[current].Count; c++)
	            		doc.Objects.Hide(doc.Objects.Find(guids[current][c]), true);
	            	
	            	if (current == guids.Count - 1)
	            		for (int c = 0; c < guids[0].Count; c++)
	        				doc.Objects.Show(doc.Objects.Find(guids[0][c]), true);
	            	else
	            		for (int c = 0; c < guids[current].Count; c++)
	        				doc.Objects.Show(doc.Objects.Find(guids[current + 1][c]), true);
	            	
	            	doc.Views.Redraw();
	            	for(int e = 0; e < 9999999; e++) {}
	            	
	            	if (current + 1 == guids.Count)
	            		current = -1;
	            	
	            	current++;
	            }
            }
            else
            {
            	ShowObjects(doc, guids);
	            doc.Views.Redraw();
            	
            	while (!handler.EscapeKeyPressed) {}
            }
		    
        	for (int c = 0; c < guids.Count; c++)
	            for (int e = 0; e < guids[c].Count; e++)
	            	doc.Objects.Hide(doc.Objects.Find(guids[c][e]), true);
            
            doc.Views.Redraw();
            handler.Dispose();
        }
        
        // My step-wise visualization method for 4-dimensional graphs
        internal static void ShowSteps(RhinoDoc doc, List<Guid> guids, bool fullEq = false)
        {
        	GetOption getOption = new GetOption();
            
            getOption.AcceptNothing(false);
            getOption.EnableTransparentCommands(true);
            getOption.SetCommandPrompt("Surface Iterations");
            var nIndex = getOption.AddOption("Next");
            var pIndex = getOption.AddOption("Previous");
            var fIndex = getOption.AddOption("Finish");
            int current = 0,
            previous = 0;
            
            while (getOption.OptionIndex() != fIndex)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                if (fullEq)
                	ShowObjects(doc, guids);
                else
                {
	                doc.Objects.Show(doc.Objects.Find(guids[current]), true);
	                previous = current;
                }
	            doc.Views.Redraw();
				
                getOption.Get();
                
                if (!fullEq)
                {
                	if (getOption.OptionIndex() == nIndex)
	                	if (current == guids.Count - 1)
	                		current = 0;
	                	else
	                		current++;
	            	else if (getOption.OptionIndex() == pIndex)
	            		if (current == 0)
	                		current = guids.Count - 1;
	                	else
	                		current--;
	            	
	            	doc.Objects.Hide(doc.Objects.Find(guids[previous]), true);
                }
            }
            
            for (int e = 0; e < guids.Count; e++)
            	doc.Objects.Hide(doc.Objects.Find(guids[e]), true);
            
            doc.Views.Redraw();
        }
        internal static void ShowSteps(RhinoDoc doc, List<List<Guid>> guids)
        {
        	GetOption getOption = new GetOption();
            
            getOption.AcceptNothing(false);
            getOption.EnableTransparentCommands(true);
            getOption.SetCommandPrompt("Surface Iterations");
            var nIndex = getOption.AddOption("Next");
            var pIndex = getOption.AddOption("Previous");
            var fIndex = getOption.AddOption("Finish");
            int current = 0,
            previous = 0;
            
            while (true)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
                for (int c = 0; c < guids[current].Count; c++)
                	doc.Objects.Show(doc.Objects.Find(guids[current][c]), true);
                
                doc.Views.Redraw();
                previous = current;
                
                getOption.Get();
				
                if (getOption.OptionIndex() == nIndex)
            	{
                	if (current == guids.Count - 1)
                		current = 0;
                	else
                		current++;
            	}
            	else if (getOption.OptionIndex() == pIndex)
            	{
            		if (current == 0)
                		current = guids.Count - 1;
                	else
                		current--;
            	}
            	else if (getOption.OptionIndex() == fIndex)
            		break;
            	
            	for (int c = 0; c < guids[previous].Count; c++)
                	doc.Objects.Show(doc.Objects.Find(guids[previous][c]), true);
            	
	            for (int c = 0; c < guids[previous].Count; c++)
		            for (int e = 0; e < guids.Count; e++)
		            	doc.Objects.Hide(doc.Objects.Find(guids[e][c]), true);
            }
		    
        	for (int c = 0; c < guids.Count; c++)
	            for (int e = 0; e < guids[c].Count; e++)
	            	doc.Objects.Hide(doc.Objects.Find(guids[c][e]), true);
            
            doc.Views.Redraw();
        }
		
        // My visualization menu for all 4-dimensional graphs
        internal static void ShowVisualization(RhinoDoc doc, bool is4D, List<List<Guid>> points, List<List<Guid>> curves, List<Guid> surfaces, bool success)
        {
			List<List<Guid>> both = new List<List<Guid>>();
			List<string> types = new List<string>();
            GetOption getOption = new GetOption();
        	bool ptsExported = false,
        		 crvExported = false,
        		 srfExported = false,
        		 eqExported = false;
        	int typeIndex = 1;
        	types.Add("Full_Equation");
        	types.Add("Surfaces");
        	types.Add("Curves_And_Points");
        	types.Add("Curves");
        	types.Add("Points");
        	
        	OptionToggle visual = new OptionToggle(true, "Steps", "Movement");
        	
			for(int i = 0; i < curves.Count; i++)
			{
				List<Guid> guid = new List<Guid>();
				
				guid.AddRange(points[i]);
				guid.AddRange(curves[i]);
				
				both.Add(guid);
			}
			
            getOption.AcceptNothing(true);
            getOption.EnableTransparentCommands(true);
            getOption.SetCommandPrompt("4D Options");
            if (is4D)
            	getOption.AddOptionToggle("Visualization", ref visual);
            var tIndex = getOption.AddOptionList("Type", types, typeIndex);
            var sIndex = getOption.AddOption("Show");
            var aIndex = getOption.AddOption("Save_Animation");
            var eIndex = getOption.AddOption("Export");
            var qIndex = getOption.AddOption("Quit");
            
            while (true) {
				// perform the get operation. This will prompt the user to input a string, but also
				// allow for command line options defined above
				
				GetResult get_rc = getOption.Get();
                
				if (getOption.OptionIndex() == qIndex)
				{
					if (ptsExported)
						ShowObjects(doc, points);
					else
						DeleteObjects(doc, points);
					if (crvExported)
						ShowObjects(doc, curves);
					else
						DeleteObjects(doc, curves);
					if (srfExported)
						ShowObjects(doc, surfaces);
					else
						DeleteObjects(doc, surfaces);
            		
					doc.Views.Redraw();
					break;
				}
				if (getOption.OptionIndex() == sIndex)
				{
					if (visual.CurrentValue || typeIndex == 0)
						switch (typeIndex) {
							case 1:
								if (success)
									ShowMotion(doc, surfaces, is4D);
								else
									Output.ToRhinoConsole("All surfaces failed to generate. Try visualizing points or curves instead");
            					
								break;
							case 2:
								ShowMotion(doc, both, is4D);
								break;
							case 3:
								ShowMotion(doc, curves, is4D);
								break;
							case 4:
								ShowMotion(doc, points, is4D);
								break;
							default:
								if (success)
									ShowSteps(doc, surfaces, is4D);
								else
									Output.ToRhinoConsole("All surfaces failed to generate. Try visualizing points or curves instead");
            					
								break;
						}
					else {
						switch (typeIndex) {
							case 2:
								ShowSteps(doc, both);
								break;
							case 3:
								ShowSteps(doc, curves);
								break;
							case 4:
								ShowSteps(doc, points);
								break;
							default:
								if (success)
									ShowSteps(doc, surfaces);
								else
									Output.ToRhinoConsole("All surfaces failed to generate. Try visualizing points or curves instead");
            					
								break;
						}
					}
				} else if (getOption.OptionIndex() == eIndex)
					switch (typeIndex)
					{
						case 1:
							if (success)
							{
								if (srfExported)
									Output.ToRhinoConsole("Surfaces Already Exported.");
								else
								{
									CreateGroup(doc, surfaces);
				            		
									srfExported = true;
									Output.ToRhinoConsole("Surfaces Exported.");
								}
							}
							else
								Output.ToRhinoConsole("Surfaces failed to generate. Try exporting points or curves instead.");
        					
							break;
						case 2:
							if (ptsExported && crvExported)
								Output.ToRhinoConsole("Points and Curves Already Exported.");
							else if (crvExported)
							{
								CreateGroup(doc, points);
	        					
								ptsExported = true;
								Output.ToRhinoConsole("Points Exported. Curves Already Exported.");
							}
							else if (ptsExported)
							{
								CreateGroup(doc, curves);
	        					
								crvExported = true;
								Output.ToRhinoConsole("Curves Exported. Points Already Exported.");
							}
							else
							{
								CreateGroup(doc, both);
        						
								ptsExported = true;
								crvExported = true;
								Output.ToRhinoConsole("Points and Curves Exported.");
							}
        					
							break;
						case 3:
							if (crvExported)
								Output.ToRhinoConsole("Curves Already Exported.");
							else
							{
								CreateGroup(doc, curves);
	        					
								crvExported = true;
								Output.ToRhinoConsole("Curves Exported.");
							}
						    
							break;
						case 4:
							if (ptsExported)
								Output.ToRhinoConsole("Points Already Exported.");
							else
							{
								CreateGroup(doc, points);
	        					
								ptsExported = true;
								Output.ToRhinoConsole("Points Exported.");
							}
        					
							break;
						default:
							if (success)
							{
								if (eqExported)
									Output.ToRhinoConsole("Equation Already Exported.");
								else
								{
									//BlendSurfaces(doc, surfaces);
				            		
									eqExported = true;
									Output.ToRhinoConsole("Equation Exported.");
								}
							}
							else
								Output.ToRhinoConsole("Equation failed to generate. Try exporting surfaces instead.");
        					
							break;
					}
				else if (getOption.OptionIndex() == aIndex)
            		if (success)
						RecordAnimation(doc, points, curves, both, surfaces);
				else
					Output.ToRhinoConsole("Surfaces failed to generate. Try animating points or curves instead.");
				else if (getOption.OptionIndex() == tIndex)
					typeIndex = getOption.Option().CurrentListOptionIndex;
			}
        }
        
        // Main method for evaluating Curve Expressions
        internal static void CurveExpression(RhinoDoc doc, string EnglishName, Expression eq, ExpressionType type, VariablesUsed func, bool drawExpression, double oneLL, double oneUL, double twoLL, double twoUL, int pointIteration, Expression eqTwo = null, Expression eqThree = null)
        {
        	List<Point3d> points = new List<Point3d>();
            List<Curve> intervals = new List<Curve>();
			
            if (eqThree == null)
            {
            	switch (type)
            	{
        			case ExpressionType.CARTESIAN:
        				eqThree = new Expression("0+0*t");
        				break;
        			case ExpressionType.POLAR:
        				eqThree = new Expression((Math.PI / 2).ToString());
        				break;
        			default:
        				break;
            	}
            }
            
            switch (func)
            {
            	case VariablesUsed.IMPLICIT_CURVE:
            		intervals = Equation.ImplicitCurve(doc, eq, type, oneLL, oneUL, twoLL, twoUL, pointIteration, pointIteration);
            		break;
            	case VariablesUsed.PARAMETRIC_CURVE:
            		intervals = Equation.ParametricCurve(doc, eq, eqTwo, eqThree, type, ref points, drawExpression, oneLL, oneUL, pointIteration);
            		break;
            	default:
            		intervals = Equation.Curve(doc, eq, type, func, ref points, drawExpression, oneLL, oneUL, twoLL, twoUL, pointIteration);
            		break;
            }
			
            EndAction(doc, EnglishName, intervals, points);
        }
		
        //Main method for evaluating 3D Surface Expressions
        internal static void SurfaceExpression(RhinoDoc doc, string EnglishName, Expression eq, ExpressionType type, VariablesUsed func, bool uPriority, bool attemptNetworkSurface, double oneLL, double oneUL, double twoLL, double twoUL, double threeLL, double threeUL, int pointIteration, int curveIteration, double fourLL, double fourUL, int fourIteration, Expression eqY = null, Expression eqZ = null)
        {
            List<List<Point3d>> framePoints = new List<List<Point3d>>(),
            					points = new List<List<Point3d>>();
            List<List<Curve>> frameCurves = new List<List<Curve>>(),
            				  drawCurves = new List<List<Curve>>(),
            				  curves = new List<List<Curve>>();
            List<Surface> surfaces = new List<Surface>();
            List<Point3d> pFrame = new List<Point3d>();
            List<Curve> cFrame = new List<Curve>();
            List<Guid> drawIDs = new List<Guid>();
            List<bool> error4D = new List<bool>();
            bool expression4D = (type == ExpressionType.CARTESIAN_4D || type == ExpressionType.SPHERICAL_4D || type == ExpressionType.CYLINDRICAL_4D),
            	 succeed = false;
            Surface surface = null;
            double fourFrame = 0;
            
            fourFrame = (fourUL - fourLL) / fourIteration;
            
            if (fourFrame.Equals(0))
            	fourFrame = 1;
            
            EscapeKeyEventHandler handler = new EscapeKeyEventHandler("Press <Esc> to exit.");
            
            for (double fourthVar = fourLL; fourthVar <= fourUL && !handler.EscapeKeyPressed; fourthVar += fourFrame)
            {
            	RhinoApp.WriteLine("Generating Surfaces ({0}/{1})", Math.Round((fourthVar - fourLL) / fourFrame) + 1, fourIteration);
        		RhinoApp.Wait();
        		
        		// Show curves by frame as they are being calculated
        		
	            if (Equation.IsImplicit(func))
	            	curves = Equation.ImplicitSurface(doc, eq, type, oneLL, oneUL, twoLL, twoUL, threeLL, threeUL, pointIteration, curveIteration);
	            else if (Equation.IsParametric(func))
	            	curves = Equation.ParametricSurface(doc, ref points, eq, eqY, eqZ, type, uPriority, oneLL, oneUL, twoLL, twoUL, pointIteration, curveIteration, fourthVar);
	            else
	            	curves = Equation.Surface(doc, ref points, eq, type, func, uPriority, oneLL, oneUL, twoLL, twoUL, threeLL, threeUL, pointIteration, curveIteration, fourthVar);
	            
        		foreach (Curve crv in curves[0])
                    drawIDs.Add(doc.Objects.AddCurve(crv));
        		foreach (Curve crv in curves[1])
                    drawIDs.Add(doc.Objects.AddCurve(crv));
        		foreach (List<Point3d> list in points)
        			foreach (Point3d pt in list)
        				drawIDs.Add(doc.Objects.AddPoint(pt));
        		
        		doc.Views.Redraw();
        		RhinoApp.Wait(); // @Check
        		
        		// Surface Generation
        		
        		pFrame = new List<Point3d>();
        		cFrame = new List<Curve>();
        		
	            for (int i = 0; i < points.Count; i++)
	            	pFrame.AddRange(points[i]);
	            
	            framePoints.Add(pFrame);
	            cFrame.AddRange(curves[0]);
	            cFrame.AddRange(curves[1]);
	            frameCurves.Add(cFrame);
	            
            	succeed = false;
            	surface = Generate.Surface(doc, points, curves, attemptNetworkSurface);
            	
            	if (surface != null)
				{
					Output.ToRhinoConsole("Surface Generation Successful.");
            		surfaces.Add(surface);
            		succeed = true;
				}
				else
					Output.ToRhinoConsole("Surface Generation Failed.");
				
	            for (int i = 0; i < drawIDs.Count; i++)
	                doc.Objects.Delete(doc.Objects.Find(drawIDs[i]), true);
	            
	            drawIDs.Clear();
	            
	            error4D.Add(succeed);
            }
	        
            var breps = BlendSurfaces(doc, surfaces);
            // @CHANGE
//            foreach (Brep brep in breps)
//            	doc.Objects.AddBrep(brep);
            
            Output.ToRhinoConsole("errors.Count", SurfaceErrorCount(error4D));
            
            doc.Views.Redraw();
            if (!handler.EscapeKeyPressed)
            	EndAction(doc, EnglishName, type, error4D, framePoints, frameCurves, surfaces, pointIteration, curveIteration);
        }
 		
        // Decides whether or not to include curves and/or points
        // (Curve)
        internal static void EndAction(RhinoDoc doc, string EnglishName, List<Curve> intervals, List<Point3d> points)
        {
        	if (intervals.Count != 0)
            {
            	CreateGroup(doc, null, intervals, points);
                
                doc.Views.Redraw();
                RhinoApp.WriteLine("{0} added a graph to the document.", EnglishName);
            }
            else
            {
            	doc.Views.Redraw();
                RhinoApp.WriteLine("{0} failed to add a graph to the document. Try readjusting the bounds or editing the expression to get a real graph.", EnglishName);
            }
        }
        // (Surface)
        internal static void EndAction(RhinoDoc doc, string EnglishName, ExpressionType type, List<bool> errors, List<List<Point3d>> framePoints, List<List<Curve>> frameCurves, List<Surface> frameSurfaces, int pointIteration, int curveIteration)
        {
        	List<List<Guid>> framePointGuids = new List<List<Guid>>(),
        					 frameCurveGuids = new List<List<Guid>>();
            List<List<Point3d>> dummy = new List<List<Point3d>>();
            List<Guid> IDs = new List<Guid>();
            
        	for (int i = 0; i < frameCurves.Count; i++)
        	{
        		IDs = Generate.Guids(doc, frameCurves[i]);
    			frameCurveGuids.Add(IDs);
        	}
        	for (int i = 0; i < framePoints.Count; i++)
        	{
        		IDs = Generate.Guids(doc, framePoints[i]);
    			framePointGuids.Add(IDs);
        	}
    		
        	ShowErrors(doc, EnglishName, errors.Contains(false), Calculate.BoolCount(errors, false) > (int)(errors.Count / 2),  SurfaceErrorCount(errors) == errors.Count);
        	
        	ShowVisualization(doc, Equation.Is4D(type), framePointGuids, frameCurveGuids, Generate.Meshes(doc, RebuildSurfaces(frameSurfaces, pointIteration, curveIteration)), Calculate.BoolCount(errors, false) != errors.Count);
        }
	}
}