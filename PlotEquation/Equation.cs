using NCalc;
using System;
using System.Linq;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace PlotEquation
{
	public enum ExpressionType
	{
		NONE = 0, CARTESIAN, POLAR, CYLINDRICAL, CONICAL, CARTESIAN_4D, SPHERICAL_4D, CYLINDRICAL_4D, CONICAL_4D
	}
	
	public enum VariablesUsed
	{
		NONE = 0, ONE, TWO, PARAMETRIC_CURVE, IMPLICIT_CURVE, ONE_TWO, TWO_THREE, ONE_THREE, PARAMETRIC_SURFACE, IMPLICIT_SURFACE
	}
	
	public static class Input
	{
		// Regular 2D curve input
        public static void Curve(RhinoDoc doc, string EnglishName, ExpressionType type, ref bool savedDrawExpression, ref string savedLastExpression, ref double savedOneLowerBound, ref double savedOneUpperBound, ref double savedTwoLowerBound, ref double savedTwoUpperBound, ref int savedPointIteration)
        {
        	GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            string equation = null,
            	   simplifiedEq = null,
            	   one = null,
            	   two = null,
            	   expressionType = Equation.ToString(type);
            bool lastExpression = false,
            	 oneExists = false,
            	 twoExists = false,
            	 equalsExists = false;
			const double zFactor = 2;
			
            // Set up options
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionDouble oneLowerBound = new OptionDouble(savedOneLowerBound, -9999, 9999);
            OptionDouble oneUpperBound = new OptionDouble(savedOneUpperBound, -9999, 9999);
            OptionDouble twoLowerBound = new OptionDouble(savedTwoLowerBound, -9999, 9999);
            OptionDouble twoUpperBound = new OptionDouble(savedTwoUpperBound, -9999, 9999);
            OptionToggle drawExpression = new OptionToggle(savedDrawExpression, "Off", "On");
            OptionDouble zoomFactor = new OptionDouble(zFactor, 1, 10);
            
			RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);
            
			switch (type)
			{
				case ExpressionType.POLAR:
					one = "Theta";
					two = "R";
					break;
				default:
					one = "X";
					two = "Y";
					break;
			}
			
            getStr.EnableTransparentCommands(true);
            getStr.SetCommandPrompt("Bounds");
  			getStr.AcceptNumber(true, true);
            getStr.SetDefaultString(null);
            getStr.AcceptNothing(true);
            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
            var ziIndex = getStr.AddOption("Zoom_In");
            var zoIndex = getStr.AddOption("Zoom_Out");
            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
			
            while (true)
            {
                // perform the get operation. This will allow for command line options defined above

                get_rc = getStr.Get();
                
                if (getStr.CommandResult() != Result.Success)
                	break;
                if (get_rc == GetResult.Option)
                {
                	if (getStr.OptionIndex() == ziIndex)
	            	{
	            		oneLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
			            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                	if (getStr.OptionIndex() == zoIndex)
	            	{
	            		oneLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
			            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                    continue;
                }
                
                break;
            }
            
            getStr.ClearCommandOptions();
            getStr.SetDefaultString(savedLastExpression);
            getStr.SetCommandPrompt(expressionType + " Expression");
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
            getStr.AddOptionToggle("Draw_Expression", ref drawExpression);
			
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                if (!lastExpression)
                	get_rc = getStr.GetLiteralString();
                
                if (getStr.CommandResult() != Rhino.Commands.Result.Success)
                	break;
                
			    if (get_rc == GetResult.Number)
			    	equation = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
                    if (!lastExpression && get_rc != GetResult.Number)
                    	equation = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
                    if (!Equation.IsValid(new Expression(equation)))
                    	continue;
					
                    oneExists = false;
                    twoExists = false;
            	 	equalsExists = false;
            	 	
                    simplifiedEq = equation.Substring(equation.IndexOf('=') + 1);
                    oneExists |= simplifiedEq.IndexOf(one.ToLower(), StringComparison.Ordinal) != -1;
					twoExists |= simplifiedEq.IndexOf(two.ToLower(), StringComparison.Ordinal) != -1;
					equalsExists |= equation.IndexOf('=') != -1;
                    
                    if (((oneExists || Equation.HasRightVariables(equation, one.ToLower())) && (equation.StartsWith(two.ToLower() + "=", StringComparison.Ordinal) || equation.StartsWith("f(" + one.ToLower() + ")=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(equation, one.ToLower()))
                    	Graph.CurveExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + one.ToLower()), type, VariablesUsed.ONE, drawExpression.CurrentValue, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, pointIteration.CurrentValue);
                    
                    else if (((twoExists || Equation.HasRightVariables(equation, two.ToLower())) && (equation.StartsWith(one.ToLower() + "=", StringComparison.Ordinal) || equation.StartsWith("f(" + two.ToLower() + ")=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(equation, two.ToLower()))
                    	Graph.CurveExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + two.ToLower()), type, VariablesUsed.TWO, drawExpression.CurrentValue, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, pointIteration.CurrentValue);
                    
                    else if (equation.Length != 0)
                    {
						Output.ToRhinoConsole("There's an extra variable or character that is not '" + one.ToLower() + "' or '" + two.ToLower() + "', or both are in the same equation. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        equation = savedLastExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: {0}", equation);
                    }
					
                    continue;
                }
				
                break;
            }
            
            savedPointIteration = pointIteration.CurrentValue;
            savedOneLowerBound = oneLowerBound.CurrentValue;
            savedOneUpperBound = oneUpperBound.CurrentValue;
            savedTwoLowerBound = twoLowerBound.CurrentValue;
            savedTwoUpperBound = twoUpperBound.CurrentValue;
            savedDrawExpression = drawExpression.CurrentValue;
            if (equation != null)
            	savedLastExpression = equation;
        }
        
		// Implicit 2D curve input
        public static void ImplicitCurve(RhinoDoc doc, string EnglishName, ExpressionType type, ref string savedLastExpression, ref double savedOneLowerBound, ref double savedOneUpperBound, ref double savedTwoLowerBound, ref double savedTwoUpperBound, ref int savedPointIteration)
        {
        	GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            string equation = null,
            	   simplifiedEq = null,
            	   one = null,
            	   two = null;
            bool lastExpression = false,
            	 oneExists = false,
            	 twoExists = false,
            	 equalsExists = false,
            	 dummy = false;
			const double zFactor = 2;
			
            // Set up options
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionDouble oneLowerBound = new OptionDouble(savedOneLowerBound, -9999, 9999);
            OptionDouble oneUpperBound = new OptionDouble(savedOneUpperBound, -9999, 9999);
            OptionDouble twoLowerBound = new OptionDouble(savedTwoLowerBound, -9999, 9999);
            OptionDouble twoUpperBound = new OptionDouble(savedTwoUpperBound, -9999, 9999);
            OptionDouble zoomFactor = new OptionDouble(zFactor, 1, 10);
            
			RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);
			
            switch (type)
			{
				case ExpressionType.POLAR:
					one = "Theta";
					two = "R";
					break;
				default:
					one = "X";
					two = "Y";
					break;
			}
            
            getStr.EnableTransparentCommands(true);
            getStr.SetCommandPrompt("Bounds");
  			getStr.AcceptNumber(true, true);
            getStr.SetDefaultString(null);
            getStr.AcceptNothing(true);
            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
            var ziIndex = getStr.AddOption("Zoom_In");
            var zoIndex = getStr.AddOption("Zoom_Out");
            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
			
            while (true)
            {
                // perform the get operation. This will allow for command line options defined above

                get_rc = getStr.Get();
                
                if (getStr.CommandResult() != Rhino.Commands.Result.Success)
                	break;
                
                if (get_rc == GetResult.Option)
                {
                	if (getStr.OptionIndex() == ziIndex)
	            	{
	            		oneLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
			            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                	if (getStr.OptionIndex() == zoIndex)
	            	{
	            		oneLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
			            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                    continue;
                }
                
                break;
            }
            
            getStr.ClearCommandOptions();
            getStr.SetDefaultString(savedLastExpression);
            getStr.SetCommandPrompt("Implicit Expression");
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
			
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                if (!lastExpression)
                	get_rc = getStr.GetLiteralString();
                
                if (getStr.CommandResult() != Rhino.Commands.Result.Success)
                	break;
                
			    if (get_rc == GetResult.Number)
			    	equation = getStr.Number().ToString();
			    
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
                    if (!lastExpression && get_rc != GetResult.Number)
                    	equation = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
                    if (!Equation.IsValid(new Expression(equation)))
                    	continue;
					
                    oneExists = false;
                    twoExists = false;
            	 	equalsExists = false;
            	 	
                    simplifiedEq = equation.Substring(equation.IndexOf('=') + 1);
                    oneExists |= simplifiedEq.IndexOf(one.ToLower(), StringComparison.Ordinal) != -1;
					twoExists |= simplifiedEq.IndexOf(two.ToLower(), StringComparison.Ordinal) != -1;
					equalsExists |= equation.IndexOf('=') != -1;
                    
					if (Equation.HasRightVariables(equation, one.ToLower(), two.ToLower()) && (equation.StartsWith("0=", StringComparison.Ordinal) || !equalsExists))
                    	Graph.CurveExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + one.ToLower() + "*" + two.ToLower()), type, VariablesUsed.IMPLICIT_CURVE, dummy, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, pointIteration.CurrentValue);
                    
                    else if (equation.Length != 0)
                    {
						Output.ToRhinoConsole("There's an extra variable or character that is not '" + one.ToLower() + "' or '" + two.ToLower() + "'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        equation = savedLastExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: {0}", equation);
                    }
					
                    continue;
                }
				
                break;
            }
            
            savedPointIteration = pointIteration.CurrentValue;
            savedOneLowerBound = oneLowerBound.CurrentValue;
            savedOneUpperBound = oneUpperBound.CurrentValue;
            savedTwoLowerBound = twoLowerBound.CurrentValue;
            savedTwoUpperBound = twoUpperBound.CurrentValue;
            if (equation != null)
            	savedLastExpression = equation;
        }
        
        // Parametric 2D and 3D curve input
        public static void ParametricCurve(RhinoDoc doc, string EnglishName, ExpressionType type, ref bool savedDrawExpression, ref string savedLastOneExpression, ref string savedLastTwoExpression, ref string savedLastThreeExpression, ref double savedLowerBound, ref double savedUpperBound, ref int savedPointIteration, bool allThree = true)
        {
        	RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);

            GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            List<Curve> intervals =  new List<Curve>();
            bool lastExpression = false,
            	 tExists = false,
            	 equalsExists = false;
            string one = null,
            	   two = null,
            	   three = null,
            	   ONE = null,
            	   simplifiedOne = null,
                   TWO = null,
            	   simplifiedTwo = null,
                   THREE = null,
            	   simplifiedThree = null;
			
            // Set up options
  			OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionDouble lowerBound = new OptionDouble(savedLowerBound, -9999, 9999);
            OptionDouble upperBound = new OptionDouble(savedUpperBound, -9999, 9999);
            OptionToggle drawExpression = new OptionToggle(savedDrawExpression, "Off", "On");
			
			switch (type)
			{
				case ExpressionType.POLAR:
					one = "Theta";
					two = "R";
					three = "Phi";
					break;
				case ExpressionType.CYLINDRICAL:
					one = "Theta";
					two = "R";
					three = "Z";
					break;
				default:
					one = "X";
					two = "Y";
					three = "Z";
					break;
			}
			
            getStr.AcceptNumber(true, true);
            getStr.SetDefaultString(null);
            getStr.AcceptNothing(true);
            getStr.SetDefaultString(savedLastOneExpression);
            getStr.SetCommandPrompt(one + " Expression");
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
            getStr.AddOptionDouble("Lower_Limit", ref lowerBound);
            getStr.AddOptionDouble("Upper_Limit", ref upperBound);
            getStr.AddOptionToggle("Draw_Expression", ref drawExpression);
			
            while (true)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                if (!lastExpression)
                	get_rc = getStr.GetLiteralString();
                
                if (getStr.CommandResult() != Result.Success)
                	break;
                
			    if (get_rc == GetResult.Number)
			    	ONE = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
                    if (!lastExpression && get_rc != GetResult.Number)
                    	ONE = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
                    if (!Equation.IsValid(new Expression(ONE)))
                    	continue;
                    
					tExists = false;
					equalsExists = false;
            	 	
					simplifiedOne = ONE.Substring(ONE.IndexOf('=') + 1);
					tExists |= simplifiedOne.IndexOf('t') != -1;
					equalsExists |= ONE.IndexOf('=') != -1;
					
					if (((tExists || Equation.HasRightVariables(ONE, "t")) && (ONE.StartsWith(one.ToLower() + "=", StringComparison.Ordinal) || ONE.StartsWith(one.ToLower() + "(t)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(ONE, "t"))
						break;
					
					if (ONE.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not 't'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        ONE = savedLastOneExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: " + one.ToLower() + "(t)={0}", ONE);
                    }
					
                    continue;
                }
                break;
            }
			
            // Set up options 2
            getStr.SetDefaultString(savedLastTwoExpression);
            getStr.SetCommandPrompt(two + " Expression");
            lastExpression = false;
            
			while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                if (!lastExpression)
                	get_rc = getStr.GetLiteralString();
                
                if (getStr.CommandResult() != Result.Success)
                	break;
                
			    if (get_rc == GetResult.Number)
			    	TWO = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
                    if (!lastExpression && get_rc != GetResult.Number)
                    	TWO = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
                    if (!Equation.IsValid(new Expression(TWO)))
                    	continue;
					
					tExists = false;
					equalsExists = false;
            	 	
					simplifiedTwo = TWO.Substring(TWO.IndexOf('=') + 1);
					tExists |= simplifiedTwo.IndexOf('t') != -1;
					equalsExists |= TWO.IndexOf('=') != -1;
					
					if (((tExists || Equation.HasRightVariables(TWO, "t")) && (TWO.StartsWith(two.ToLower() + "=", StringComparison.Ordinal) || TWO.StartsWith(two.ToLower() + "(t)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(TWO, "t")) {
						if (allThree)
							break;
						
						Graph.CurveExpression(doc, EnglishName, new Expression(simplifiedOne + "+0*t"), type, VariablesUsed.PARAMETRIC_CURVE, drawExpression.CurrentValue, lowerBound.CurrentValue, upperBound.CurrentValue, 0, 0, pointIteration.CurrentValue, new Expression(simplifiedTwo + "+0*t"));
					}
					else if (TWO.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not 't'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        TWO = savedLastTwoExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: " + two.ToLower() + "(t)={0}", TWO);
                    }
					
                    continue;
                }
                break;
            }
			
			if (allThree)
			{
	            // Set up options 3
            	getStr.SetDefaultString(savedLastThreeExpression);
	            getStr.SetCommandPrompt(three + " Expression");
	            lastExpression = false;
				
	            while (get_rc != GetResult.Cancel)
	            {
	                // perform the get operation. This will prompt the user to input a string, but also
	                // allow for command line options defined above
					
	                if (!lastExpression)
	                	get_rc = getStr.GetLiteralString();
                	
					if (getStr.CommandResult() != Result.Success)
						break;
					
					if (get_rc == GetResult.Number)
						THREE = getStr.Number().ToString();
	                
	                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
	                {
						if (!lastExpression && get_rc != GetResult.Number)
							THREE = getStr.StringResult().ToLower().Replace(" ", "").Trim();
						
						if (!Equation.IsValid(new Expression(THREE)))
							continue;
						
						tExists = false;
						equalsExists = false;
	            	 	
						simplifiedThree = THREE.Substring(THREE.IndexOf('=') + 1);
						tExists |= simplifiedThree.IndexOf('t') != -1;
						equalsExists |= THREE.IndexOf('=') != -1;
						
						if (((tExists || Equation.HasRightVariables(THREE, "t")) && (THREE.StartsWith(three.ToLower() + "=", StringComparison.Ordinal) || THREE.StartsWith(three.ToLower() + "(t)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(THREE, "t"))
							Graph.CurveExpression(doc, EnglishName, new Expression(simplifiedOne + "+0*t"), type, VariablesUsed.PARAMETRIC_CURVE, drawExpression.CurrentValue, lowerBound.CurrentValue, upperBound.CurrentValue, 0, 0, pointIteration.CurrentValue, new Expression(simplifiedTwo + "+0*t"), new Expression(simplifiedThree + "+0*t"));
						
						else if (THREE.Length != 0)
						{
							Output.ToRhinoConsole("There's an extra variable or character that is not 't'. Please revise expression.");
							continue;
						}
	                }
	                else if (get_rc == GetResult.Option)
	                {
	                    if (getStr.OptionIndex() == leIndex)
	                    {
	                        THREE = savedLastThreeExpression;
	                        lastExpression = true;
	                        RhinoApp.WriteLine("Previous Equation: " + three.ToLower() + "(t)={0}", THREE);
	                    }
	
	                    continue;
	                }
	                break;
	            }
        	}
	            
            savedPointIteration = pointIteration.CurrentValue;
            savedLowerBound = lowerBound.CurrentValue;
            savedUpperBound = upperBound.CurrentValue;
            savedDrawExpression = drawExpression.CurrentValue;
			if (ONE != null)
				savedLastOneExpression = ONE;
			if (TWO != null)
				savedLastTwoExpression = TWO;
			if (THREE != null)
				savedLastThreeExpression = THREE;
        }
		
        // Regular 3D and 4D surface input
        public static void Surface(RhinoDoc doc, string EnglishName, ExpressionType type, ref bool savedUPriority, ref bool savedAttemptNetworkSurface, ref string savedLastExpression, ref double savedOneLowerBound, ref double savedOneUpperBound, ref double savedTwoLowerBound, ref double savedTwoUpperBound, ref double savedThreeLowerBound, ref double savedThreeUpperBound, ref int savedPointIteration, ref int savedCurveIteration, ref double savedFourLowerBound, ref double savedFourUpperBound, ref int savedFourIteration)
        {
        	GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            string equation = null,
            	   simplifiedEq = null,
            	   one = null,
            	   two = null,
            	   three = null,
            	   four = null,
            	   expressionType = Equation.ToString(type, true);
            bool lastExpression = false,
            	 oneExists = false,
            	 twoExists = false,
            	 threeExists = false,
            	 fourExists = false,
            	 equalsExists = false;
			const double zFactor= 2;
			
            // Set up options
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionInteger curveIteration = new OptionInteger(savedCurveIteration, 1, 100000);
            OptionDouble oneLowerBound = new OptionDouble(savedOneLowerBound, -9999, 9999);
            OptionDouble oneUpperBound = new OptionDouble(savedOneUpperBound, -9999, 9999);
            OptionDouble twoLowerBound = new OptionDouble(savedTwoLowerBound, -9999, 9999);
            OptionDouble twoUpperBound = new OptionDouble(savedTwoUpperBound, -9999, 9999);
            OptionDouble threeLowerBound = new OptionDouble(savedThreeLowerBound, -9999, 9999);
            OptionDouble threeUpperBound = new OptionDouble(savedThreeUpperBound, -9999, 9999);
            OptionInteger fourIteration = new OptionInteger(savedFourIteration, 1, 10000);
	        OptionDouble fourLowerBound = new OptionDouble(savedFourLowerBound, -9999, 9999);
	        OptionDouble fourUpperBound = new OptionDouble(savedFourUpperBound, -9999, 9999);
            OptionDouble zoomFactor = new OptionDouble(zFactor, 1, 10);
            OptionToggle uPriority = new OptionToggle(savedUPriority, "V", "U");
            OptionToggle attemptNetworkSurface = new OptionToggle(savedAttemptNetworkSurface, "No", "Yes");
	        
			RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);
            
			switch (type)
			{
				case ExpressionType.POLAR:
				case ExpressionType.SPHERICAL_4D:
					one = "Theta";
					two = "Phi";
					three = "R";
					break;
				case ExpressionType.CYLINDRICAL:
				case ExpressionType.CYLINDRICAL_4D:
					one = "Theta";
					two = "Z";
					three = "R";
        			break;
				case ExpressionType.CONICAL:
				case ExpressionType.CONICAL_4D:
					one = "Xi";
					two = "Psi";
					three = "Zeta";
					break;
				default:
					one = "X";
					two = "Y";
					three = "Z";
					break;
			}
			
			switch (type)
			{
				case ExpressionType.CARTESIAN_4D:
					four = "W";
					break;
				default:
            		if (Equation.Is4D(type))
						four = "S";
					break;
			}
			
            if (Equation.Is4D(type))
            {
            	getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
	            getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
	            getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
            }
            
            getStr.EnableTransparentCommands(true);
            getStr.SetCommandPrompt("Bounds");
  			getStr.AcceptNumber(true, true);
            getStr.SetDefaultString(null);
            getStr.AcceptNothing(true);
            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
            var ziIndex = getStr.AddOption("Zoom_In");
            var zoIndex = getStr.AddOption("Zoom_Out");
            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
			
            while (true)
            {
                // perform the get operation. This will allow for command line options defined above
                
                get_rc = getStr.Get();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Option)
                {
                	if (getStr.OptionIndex() == ziIndex)
	            	{
	            		oneLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		threeLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		threeUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
            			
					    if (Equation.Is4D(type))
					    {
					    	getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
					        getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
					        getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
					    }
					    getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
			            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                	if (getStr.OptionIndex() == zoIndex)
	            	{
	            		oneLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		threeLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		threeUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
            			
            			if (Equation.Is4D(type))
					    {
					    	getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
					        getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
					        getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
					    }
					    getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
			            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                    continue;
                }
                
                break;
            }
            
            getStr.ClearCommandOptions();
            getStr.SetDefaultString(savedLastExpression);
            getStr.SetCommandPrompt(expressionType + " Expression");
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
            getStr.AddOptionInteger("Curve_Iteration", ref curveIteration);
            getStr.AddOptionToggle("Priority", ref uPriority);
            getStr.AddOptionToggle("Try_Network_Surface", ref attemptNetworkSurface);
			
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above

				if (!lastExpression)
					get_rc = getStr.GetLiteralString();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Number)
					equation = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
					if (!lastExpression && get_rc != GetResult.Number)
						equation = getStr.StringResult().ToLower().Replace(" ", "").Trim();

					if (!Equation.IsValid(new Expression(equation)))
						continue;
					
                    oneExists = false;
                    twoExists = false;
                    threeExists = false;
                    fourExists = false;
            	 	equalsExists = false;
            	 	
                    simplifiedEq = equation.Substring(equation.IndexOf('=') + 1);
					oneExists |= simplifiedEq.IndexOf(one.ToLower(), StringComparison.Ordinal) != -1;
					twoExists |= simplifiedEq.IndexOf(two.ToLower(), StringComparison.Ordinal) != -1;
					threeExists |= simplifiedEq.IndexOf(three.ToLower(), StringComparison.Ordinal) != -1;
					equalsExists |= equation.IndexOf('=') != -1;
					
					if (!Equation.Is4D(type))
					{
						four = "0";
						fourIteration.CurrentValue = 1;
						fourLowerBound.CurrentValue = 1;
						fourUpperBound.CurrentValue = 1;
					}
					else
						fourExists |= simplifiedEq.IndexOf(four.ToLower(), StringComparison.Ordinal) != -1;
					
                    /*
                     * if ( ( (equation has the desired variables or is a constant) && (equation starts properly or has no equals) ) && equation is valid )
					 * if ( ( ("x exists" || "y exists" || "there are no extra variables") && ("equation starts with z=" || "equation starts with f(x,y)=" || "equation has no =") ) && "there are no extra variables" )
                     */
                    // @ CHANGE FUNC USAGE
					if (((oneExists || twoExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(equation, one.ToLower(), two.ToLower(), four.ToLower())) && (equation.StartsWith(three.ToLower() + "=", StringComparison.Ordinal) || equation.StartsWith("f(" + one.ToLower() + "," + two.ToLower() + ")=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(equation, one.ToLower(), two.ToLower(), four.ToLower()))
						Graph.SurfaceExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + one.ToLower() + "*" + two.ToLower() + "*" + four.ToLower()), type, VariablesUsed.ONE_TWO, uPriority.CurrentValue, attemptNetworkSurface.CurrentValue, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, threeLowerBound.CurrentValue, threeUpperBound.CurrentValue, pointIteration.CurrentValue, curveIteration.CurrentValue, fourLowerBound.CurrentValue, fourUpperBound.CurrentValue, fourIteration.CurrentValue);
					
					else if (((twoExists || threeExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(equation, two.ToLower(), three.ToLower(), four.ToLower())) && (equation.StartsWith(one.ToLower() + "=", StringComparison.Ordinal) || equation.StartsWith("f(" + two.ToLower() + "," + three.ToLower() + ")=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(equation, two.ToLower(), three.ToLower(), four.ToLower()))
						Graph.SurfaceExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + two.ToLower() + "*" + three.ToLower() + "*" + four.ToLower()), type, VariablesUsed.TWO_THREE, uPriority.CurrentValue, attemptNetworkSurface.CurrentValue, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, threeLowerBound.CurrentValue, threeUpperBound.CurrentValue, pointIteration.CurrentValue, curveIteration.CurrentValue, fourLowerBound.CurrentValue, fourUpperBound.CurrentValue, fourIteration.CurrentValue);
					
					else if (((oneExists || threeExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(equation, one.ToLower(), three.ToLower(), four.ToLower())) && (equation.StartsWith(two.ToLower() + "=", StringComparison.Ordinal) || equation.StartsWith("f(" + one.ToLower() + "," + three.ToLower() + ")=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(equation, one.ToLower(), three.ToLower(), four.ToLower()))
						Graph.SurfaceExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + one.ToLower() + "*" + three.ToLower() + "*" + four.ToLower()), type, VariablesUsed.ONE_THREE, uPriority.CurrentValue, attemptNetworkSurface.CurrentValue, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, threeLowerBound.CurrentValue, threeUpperBound.CurrentValue, pointIteration.CurrentValue, curveIteration.CurrentValue, fourLowerBound.CurrentValue, fourUpperBound.CurrentValue, fourIteration.CurrentValue);
					
					else if (equation.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not '" + one.ToLower() + "','" + two.ToLower() + "', or '" + three.ToLower() + "', or all three are in the same equation. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        equation = savedLastExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: {0}", equation);
                    }

                    continue;
                }

                break;
            }
            
            savedPointIteration = pointIteration.CurrentValue;
            savedCurveIteration = curveIteration.CurrentValue;
            savedOneLowerBound = oneLowerBound.CurrentValue;
            savedOneUpperBound = oneUpperBound.CurrentValue;
            savedTwoLowerBound = twoLowerBound.CurrentValue;
            savedTwoUpperBound = twoUpperBound.CurrentValue;
            savedThreeLowerBound = threeLowerBound.CurrentValue;
            savedThreeUpperBound = threeUpperBound.CurrentValue;
            savedUPriority = uPriority.CurrentValue;
            savedAttemptNetworkSurface = attemptNetworkSurface.CurrentValue;
			if (equation != null)
				savedLastExpression = equation;
            
            if (Equation.Is4D(type))
            {
            	savedFourIteration = fourIteration.CurrentValue;
	            savedFourLowerBound = fourLowerBound.CurrentValue;
	            savedFourUpperBound = fourUpperBound.CurrentValue;
            }
        }
        
        // Implicit 3D surface input
        public static void ImplicitSurface(RhinoDoc doc, string EnglishName, ExpressionType type, ref string savedLastExpression, ref double savedOneLowerBound, ref double savedOneUpperBound, ref double savedTwoLowerBound, ref double savedTwoUpperBound, ref double savedThreeLowerBound, ref double savedThreeUpperBound, ref int savedPointIteration, ref int savedCurveIteration)
        {
        	GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            string equation = null,
            	   simplifiedEq = null,
            	   one = null,
            	   two = null,
            	   three = null,
            	   expressionType = Equation.ToString(type, true);
            bool lastExpression = false,
            	 oneExists = false,
            	 twoExists = false,
            	 threeExists = false,
            	 equalsExists = false;
			const double zFactor= 2;
			
            // Set up options
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionInteger curveIteration = new OptionInteger(savedCurveIteration, 1, 100000);
            OptionDouble oneLowerBound = new OptionDouble(savedOneLowerBound, -9999, 9999);
            OptionDouble oneUpperBound = new OptionDouble(savedOneUpperBound, -9999, 9999);
            OptionDouble twoLowerBound = new OptionDouble(savedTwoLowerBound, -9999, 9999);
            OptionDouble twoUpperBound = new OptionDouble(savedTwoUpperBound, -9999, 9999);
            OptionDouble threeLowerBound = new OptionDouble(savedThreeLowerBound, -9999, 9999);
            OptionDouble threeUpperBound = new OptionDouble(savedThreeUpperBound, -9999, 9999);
            OptionDouble zoomFactor = new OptionDouble(zFactor, 1, 10);
	        
			RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);
            
			switch (type)
			{
				case ExpressionType.POLAR:
				case ExpressionType.SPHERICAL_4D:
					one = "Theta";
					two = "Phi";
					three = "R";
					break;
				case ExpressionType.CYLINDRICAL:
				case ExpressionType.CYLINDRICAL_4D:
					one = "Theta";
					two = "Z";
					three = "R";
        			break;
				case ExpressionType.CONICAL:
				case ExpressionType.CONICAL_4D:
					one = "Xi";
					two = "Psi";
					three = "Zeta";
					break;
				default:
					one = "X";
					two = "Y";
					three = "Z";
					break;
			}
			
            getStr.EnableTransparentCommands(true);
            getStr.SetCommandPrompt("Bounds");
  			getStr.AcceptNumber(true, true);
            getStr.SetDefaultString(null);
            getStr.AcceptNothing(true);
            getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
            var ziIndex = getStr.AddOption("Zoom_In");
            var zoIndex = getStr.AddOption("Zoom_Out");
            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
			
            while (true)
            {
                // perform the get operation. This will allow for command line options defined above

                get_rc = getStr.Get();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Option)
                {
                	if (getStr.OptionIndex() == ziIndex)
	            	{
	            		oneLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		threeLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		threeUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
            			getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
			            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                	if (getStr.OptionIndex() == zoIndex)
	            	{
	            		oneLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		oneUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		twoUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		threeLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		threeUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
            			getStr.AddOptionDouble(one + "_Lower_Limit", ref oneLowerBound);
			            getStr.AddOptionDouble(one + "_Upper_Limit", ref oneUpperBound);
			            getStr.AddOptionDouble(two + "_Lower_Limit", ref twoLowerBound);
			            getStr.AddOptionDouble(two + "_Upper_Limit", ref twoUpperBound);
			            getStr.AddOptionDouble(three + "_Lower_Limit", ref threeLowerBound);
			            getStr.AddOptionDouble(three + "_Upper_Limit", ref threeUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                    continue;
                }
                
                break;
            }
            
            getStr.ClearCommandOptions();
            getStr.SetDefaultString(savedLastExpression);
            getStr.SetCommandPrompt(expressionType + " Expression");
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
            getStr.AddOptionInteger("Curve_Iteration", ref curveIteration);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above

				if (!lastExpression)
					get_rc = getStr.GetLiteralString();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Number)
					equation = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
					if (!lastExpression && get_rc != GetResult.Number)
						equation = getStr.StringResult().ToLower().Replace(" ", "").Trim();

					if (!Equation.IsValid(new Expression(equation)))
						continue;
					
                    oneExists = false;
                    twoExists = false;
                    threeExists = false;
            	 	equalsExists = false;
            	 	
                    simplifiedEq = equation.Substring(equation.IndexOf('=') + 1);
					oneExists |= simplifiedEq.IndexOf(one.ToLower(), StringComparison.Ordinal) != -1;
					twoExists |= simplifiedEq.IndexOf(two.ToLower(), StringComparison.Ordinal) != -1;
					threeExists |= simplifiedEq.IndexOf(three.ToLower(), StringComparison.Ordinal) != -1;
					equalsExists |= equation.IndexOf('=') != -1;
					
					if (Equation.HasRightVariables(equation, one.ToLower(), two.ToLower(), three.ToLower()) && (equation.StartsWith("0=", StringComparison.Ordinal) || !equalsExists))
						Graph.SurfaceExpression(doc, EnglishName, new Expression(simplifiedEq + "+0*" + one.ToLower() + "*" + two.ToLower() + "*" + three.ToLower()), type, VariablesUsed.IMPLICIT_SURFACE, true, false, oneLowerBound.CurrentValue, oneUpperBound.CurrentValue, twoLowerBound.CurrentValue, twoUpperBound.CurrentValue, threeLowerBound.CurrentValue, threeUpperBound.CurrentValue, pointIteration.CurrentValue, curveIteration.CurrentValue, 0, 0, 0);
					
					else if (equation.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not '" + one.ToLower() + "','" + two.ToLower() + "', or '" + three.ToLower() + "', or all three are in the same equation. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        equation = savedLastExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: {0}", equation);
                    }

                    continue;
                }

                break;
            }
            
            savedPointIteration = pointIteration.CurrentValue;
            savedCurveIteration = curveIteration.CurrentValue;
            savedOneLowerBound = oneLowerBound.CurrentValue;
            savedOneUpperBound = oneUpperBound.CurrentValue;
            savedTwoLowerBound = twoLowerBound.CurrentValue;
            savedTwoUpperBound = twoUpperBound.CurrentValue;
            savedThreeLowerBound = threeLowerBound.CurrentValue;
            savedThreeUpperBound = threeUpperBound.CurrentValue;
			if (equation != null)
				savedLastExpression = equation;
        }
        
        // Parametric 3D and 4D surface input
        public static void ParametricSurface(RhinoDoc doc, string EnglishName, ExpressionType type, ref bool savedUPriority, ref bool savedAttemptNetworkSurface, ref string savedLastOneExpression, ref string savedLastTwoExpression, ref string savedLastThreeExpression, ref double savedULowerBound, ref double savedUUpperBound, ref double savedVLowerBound, ref double savedVUpperBound, ref int savedPointIteration, ref int savedCurveIteration, ref double savedFourLowerBound, ref double savedFourUpperBound, ref int savedFourIteration)
        {
        	RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);
			
            List<Curve> intervals =  new List<Curve>();
            GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            bool lastExpression = false,
            	 uExists = false,
            	 vExists = false,
            	 fourExists = false,
            	 equalsExists = false;
            string one = null,
            	   two = null,
            	   three = null,
            	   four = null,
            	   ONE = null,
            	   simplifiedOne = null,
                   TWO = null,
            	   simplifiedTwo = null,
                   THREE = null,
            	   simplifiedThree = null;
			const double zFactor= 2;
			
            // Set up options
  			OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
  			OptionInteger curveIteration = new OptionInteger(savedCurveIteration, 1, 100000);
            OptionDouble uLowerBound = new OptionDouble(savedULowerBound, -9999, 9999);
            OptionDouble uUpperBound = new OptionDouble(savedUUpperBound, -9999, 9999);
            OptionDouble vLowerBound = new OptionDouble(savedVLowerBound, -9999, 9999);
            OptionDouble vUpperBound = new OptionDouble(savedVUpperBound, -9999, 9999);
            OptionInteger fourIteration = new OptionInteger(savedFourIteration, 1, 10000);
	        OptionDouble fourLowerBound = new OptionDouble(savedFourLowerBound, -9999, 9999);
	        OptionDouble fourUpperBound = new OptionDouble(savedFourUpperBound, -9999, 9999);
            OptionDouble zoomFactor = new OptionDouble(zFactor, 1, 10);
            OptionToggle uPriority = new OptionToggle(savedUPriority, "V", "U");
            OptionToggle attemptNetworkSurface = new OptionToggle(savedAttemptNetworkSurface, "No", "Yes");
			
			switch (type)
			{
				case ExpressionType.SPHERICAL_4D:
				case ExpressionType.POLAR:
					one = "Theta";
					two = "Phi";
					three = "R";
					break;
				case ExpressionType.CYLINDRICAL_4D:
				case ExpressionType.CYLINDRICAL:
					one = "Theta";
					two = "Z";
					three = "R";
        			break;
				case ExpressionType.CONICAL_4D:
				case ExpressionType.CONICAL:
					one = "Xi";
					two = "Psi";
					three = "Zeta";
					break;
				default:
					one = "X";
					two = "Y";
					three = "Z";
					break;
			}
			
			if (Equation.Is4D(type))
            {
				switch (type)
				{
					case ExpressionType.CARTESIAN_4D:
						four = "W";
						break;
					default:
						four = "S";
						break;
				}
				
            	getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
	            getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
	            getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
            }
            
            getStr.SetCommandPrompt("Bounds");
            getStr.AcceptNothing(true);
            getStr.EnableTransparentCommands(true);
            getStr.AddOptionDouble("U_Lower_Limit", ref uLowerBound);
            getStr.AddOptionDouble("U_Upper_Limit", ref uUpperBound);
            getStr.AddOptionDouble("V_Lower_Limit", ref vLowerBound);
            getStr.AddOptionDouble("V_Upper_Limit", ref vUpperBound);
            var ziIndex = getStr.AddOption("Zoom_In");
            var zoIndex = getStr.AddOption("Zoom_Out");
            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
			
            while (true)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above

                get_rc = getStr.Get();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Option)
                {
                	if (getStr.OptionIndex() == ziIndex)
	            	{
	            		uLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		uUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		vLowerBound.CurrentValue /= zoomFactor.CurrentValue;
	            		vUpperBound.CurrentValue /= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
			            
			            if (Equation.Is4D(type))
			            {
							getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
				            getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
				            getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
			            }
			            getStr.AddOptionDouble("U_Lower_Limit", ref uLowerBound);
			            getStr.AddOptionDouble("U_Upper_Limit", ref uUpperBound);
			            getStr.AddOptionDouble("V_Lower_Limit", ref vLowerBound);
			            getStr.AddOptionDouble("V_Upper_Limit", ref vUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                	if (getStr.OptionIndex() == zoIndex)
	            	{
	            		uLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		uUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		vLowerBound.CurrentValue *= zoomFactor.CurrentValue;
	            		vUpperBound.CurrentValue *= zoomFactor.CurrentValue;
	            		
            			getStr.ClearCommandOptions();
            			
			            if (Equation.Is4D(type))
			            {
							getStr.AddOptionInteger(four + "_Iteration", ref fourIteration);
				            getStr.AddOptionDouble(four + "_Lower_Limit", ref fourLowerBound);
				            getStr.AddOptionDouble(four + "_Upper_Limit", ref fourUpperBound);
			            }
			            getStr.AddOptionDouble("U_Lower_Limit", ref uLowerBound);
			            getStr.AddOptionDouble("U_Upper_Limit", ref uUpperBound);
			            getStr.AddOptionDouble("V_Lower_Limit", ref vLowerBound);
			            getStr.AddOptionDouble("V_Upper_Limit", ref vUpperBound);
			            ziIndex = getStr.AddOption("Zoom_In");
			            zoIndex = getStr.AddOption("Zoom_Out");
			            getStr.AddOptionDouble("Zoom_Factor", ref zoomFactor);
	            	}
                    continue;
                }
                
                break;
            }
			
            getStr.ClearCommandOptions();
            getStr.SetDefaultString(savedLastOneExpression);
            var leIndex = getStr.AddOption("Last_Expression");
            getStr.SetCommandPrompt(one + " Expression");
            getStr.AddOptionInteger("Point_Iteration", ref pointIteration);
            getStr.AddOptionInteger("Curve_Iteration", ref curveIteration);
            getStr.AddOptionToggle("Priority", ref uPriority);
            getStr.AddOptionToggle("Try_Network_Surface", ref attemptNetworkSurface);
			
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
				if (!lastExpression)
					get_rc = getStr.GetLiteralString();
                
				if (getStr.CommandResult() != Rhino.Commands.Result.Success)
					break;
                
				if (get_rc == GetResult.Number)
					ONE = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || get_rc == GetResult.Number || lastExpression)
                {
					if (!lastExpression && get_rc != GetResult.Number)
						ONE = getStr.StringResult().ToLower().Replace(" ", "").Trim();
					
					if (!Equation.IsValid(new Expression(ONE)))
						continue;
					
					uExists = false;
					vExists = false;
					fourExists = false;
					equalsExists = false;
            	 	
					simplifiedOne = ONE.Substring(ONE.IndexOf('=') + 1);
					uExists |= simplifiedOne.IndexOf('u') != -1;
					vExists |= simplifiedOne.IndexOf('v') != -1;
					equalsExists |= ONE.IndexOf('=') != -1;
					
					if (!Equation.Is4D(type))
					{
						four = "0";
						fourIteration.CurrentValue = 1;
						fourLowerBound.CurrentValue  = 1;
						fourUpperBound.CurrentValue = 1;
					}
					else
						fourExists |= simplifiedOne.IndexOf(four.ToLower(), StringComparison.Ordinal) != -1;
					
					if (((uExists || vExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(ONE, "u", "v", four.ToLower())) && (ONE.StartsWith(one.ToLower() + "=", StringComparison.Ordinal) || ONE.StartsWith(one.ToLower() + "(u,v)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(ONE, "u", "v", four.ToLower()))
						break;
					if (ONE.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not 'u' or 'v'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        ONE = savedLastOneExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: " + one.ToLower() + "(u,v)={0}", ONE);
                    }
					
                    continue;
                }
                break;
            }
			
            // Set up options 2
            getStr.SetDefaultString(savedLastTwoExpression);
            getStr.SetCommandPrompt(two + " Expression");
            lastExpression = false;
			
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
				if (!lastExpression)
					get_rc = getStr.GetLiteralString();
				
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Number)
					TWO = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || lastExpression)
                {
					if (!lastExpression && get_rc != GetResult.Number)
						TWO = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
					if (!Equation.IsValid(new Expression(TWO)))
						continue;
					
					uExists = false;
					vExists = false;
					fourExists = false;
					equalsExists = false;
            	 	
					simplifiedTwo = TWO.Substring(TWO.IndexOf('=') + 1);
					uExists |= simplifiedTwo.IndexOf('u') != -1;
					vExists |= simplifiedTwo.IndexOf('v') != -1;
					equalsExists |= TWO.IndexOf('=') != -1;
					
					if (!Equation.Is4D(type))
					{
						four = "0";
						fourIteration.CurrentValue = 1;
						fourLowerBound.CurrentValue  = 1;
						fourUpperBound.CurrentValue = 1;
					}
					else
						fourExists |= simplifiedOne.IndexOf(four.ToLower(), StringComparison.Ordinal) != -1;
					
					if (((uExists || vExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(TWO, "u", "v", four.ToLower())) && (TWO.StartsWith(two.ToLower() + "=", StringComparison.Ordinal) || TWO.StartsWith(two.ToLower() + "(u,v)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(TWO, "u", "v", four.ToLower()))
						break;
					if (TWO.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not 'u' or 'v'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        TWO = savedLastTwoExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: " + two.ToLower() + "(u,v)={0}", TWO);
                    }
					
                    continue;
                }
                break;
            }

            // Set up options 3
            getStr.SetDefaultString(savedLastThreeExpression);
            getStr.SetCommandPrompt(three + " Expression");
            lastExpression = false;

            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
				if (!lastExpression)
					get_rc = getStr.GetLiteralString();
                
				if (getStr.CommandResult() != Result.Success)
					break;
                
				if (get_rc == GetResult.Number)
					THREE = getStr.Number().ToString();
                
                if (get_rc == GetResult.String || lastExpression)
                {
					if (!lastExpression && get_rc != GetResult.Number)
						THREE = getStr.StringResult().ToLower().Replace(" ", "").Trim();
                    
					if (!Equation.IsValid(new Expression(THREE)))
						continue;
					
					uExists = false;
					vExists = false;
					fourExists = false;
					equalsExists = false;
            	 	
					simplifiedThree = THREE.Substring(THREE.IndexOf('=') + 1);
					uExists |= simplifiedThree.IndexOf('u') != -1;
					vExists |= simplifiedThree.IndexOf('v') != -1;
					equalsExists |= THREE.IndexOf('=') != -1;
					
					if (!Equation.Is4D(type))
					{
						four = "0";
						fourIteration.CurrentValue = 1;
						fourLowerBound.CurrentValue  = 1;
						fourUpperBound.CurrentValue = 1;
					}
					else
						fourExists |= simplifiedOne.IndexOf(four.ToLower(), StringComparison.Ordinal) != -1;
					
					if (((uExists || vExists || (fourExists && Equation.Is4D(type)) || Equation.HasRightVariables(THREE, "u", "v", four.ToLower())) && (THREE.StartsWith(three.ToLower() + "=", StringComparison.Ordinal) || THREE.StartsWith(three.ToLower() + "(u,v)=", StringComparison.Ordinal) || !equalsExists)) && Equation.HasRightVariables(THREE, "u", "v", four.ToLower()))
						Graph.SurfaceExpression(doc, EnglishName, new Expression(simplifiedOne + "+0*u*v*" + four.ToLower()), type, VariablesUsed.PARAMETRIC_SURFACE, uPriority.CurrentValue, attemptNetworkSurface.CurrentValue, uLowerBound.CurrentValue, uUpperBound.CurrentValue, vLowerBound.CurrentValue, vUpperBound.CurrentValue, 0, 0, pointIteration.CurrentValue, curveIteration.CurrentValue, fourLowerBound.CurrentValue, fourUpperBound.CurrentValue, fourIteration.CurrentValue, new Expression(simplifiedTwo + "+0*u*v*" + four.ToLower()), new Expression(simplifiedThree + "+0*u*v*" + four.ToLower()));
					
					else if (THREE.Length != 0)
					{
						Output.ToRhinoConsole("There's an extra variable or character that is not 'u' or 'v'. Please revise expression.");
						continue;
					}
                }
                else if (get_rc == GetResult.Option)
                {
                    if (getStr.OptionIndex() == leIndex)
                    {
                        THREE = savedLastThreeExpression;
                        lastExpression = true;
                        RhinoApp.WriteLine("Previous Equation: " + three.ToLower() + "(u,v)={0}", THREE);
                    }
					
                    continue;
                }
                break;
            }
            
            savedPointIteration = pointIteration.CurrentValue;
            savedCurveIteration = curveIteration.CurrentValue;
            savedULowerBound = uLowerBound.CurrentValue;
            savedUUpperBound = uUpperBound.CurrentValue;
            savedVLowerBound = vLowerBound.CurrentValue;
            savedVUpperBound = vUpperBound.CurrentValue;
            savedUPriority = uPriority.CurrentValue;
            savedAttemptNetworkSurface = attemptNetworkSurface.CurrentValue;
			if (ONE != null)
				savedLastOneExpression = ONE;
			if (TWO != null)
				savedLastTwoExpression = TWO;
			if (THREE != null)
				savedLastThreeExpression = THREE;
            
			if (Equation.Is4D(type))
            {
            	savedFourIteration = fourIteration.CurrentValue;
	            savedFourLowerBound = fourLowerBound.CurrentValue;
	            savedFourUpperBound = fourUpperBound.CurrentValue;
            }
        }
	}
	
	public static class Equation
    {
		// Curve generation from equation
        internal static List<Curve> Curve(RhinoDoc doc, Expression eq, ExpressionType type, VariablesUsed func, ref List<Point3d> points, bool drawExpression, double oneLL, double oneUL, double twoLL, double twoUL, int pointIteration)
        {
            var handler = new EscapeKeyEventHandler("Press <Esc> to stop drawing.");
			
            List<Point3d> pts = new List<Point3d>();
            List<Curve> intervals =  new List<Curve>();
            string var = null;
            double ll = 0,
            	   ul = 0,
            	   secondVar = 0,
            	   pointFrame = 0;
            
			switch (func)
            {
            	case VariablesUsed.ONE:
            		ll = oneLL;
            		ul = oneUL;
            		
            		switch (type)
            		{
            			case ExpressionType.CARTESIAN:
            				var = "x";
            				break;
            			case ExpressionType.POLAR:
            				var = "theta";
            				break;
            			default:
            				break;
            		}
            		
            		break;
            	case VariablesUsed.TWO:
            		ll = twoLL;
            		ul = twoUL;
            		
            		switch (type)
            		{
            			case ExpressionType.CARTESIAN:
            				var = "y";
            				break;
            			case ExpressionType.POLAR:
            				var = "r";
            				break;
            			default:
            				break;
            		}
            		
            		break;
            }
			
            Equation.SetParameters(ref eq);
			
            pointFrame = (ul - ll) / pointIteration;
            
            for (double firstVar = ll; firstVar <= ul; firstVar += pointFrame)
            {
            	eq.Parameters[var] = Math.Round(firstVar, Calculate.DecimalCount(pointFrame));
            	
                secondVar = Convert.ToDouble(eq.Evaluate());
                
                if (Equation.IsRealNumberAt(secondVar))
                {
                	switch (type)
                	{
                		case ExpressionType.CARTESIAN:
                    		switch (func)
                    		{
                    			case VariablesUsed.ONE:
                					pts.Add(Calculate.CartesianPoint(firstVar, secondVar));
                    				break;
                    			case VariablesUsed.TWO:
                					pts.Add(Calculate.CartesianPoint(secondVar, firstVar));
                    				break;
                    		}
                    		
                			break;
                		case ExpressionType.POLAR:
                    		switch (func)
                    		{
                    			case VariablesUsed.ONE:
                					pts.Add(Calculate.PolarPoint(firstVar, secondVar));
                    				break;
                    			case VariablesUsed.TWO:
                					pts.Add(Calculate.PolarPoint(secondVar, firstVar));
                    				break;
                    		}
                    		
                			break;
                		default:
                			break;
                	}
                	
					if (drawExpression)
                    {
						Graph.DrawCurve(doc, intervals, pts, points);
						
						drawExpression &= !handler.EscapeKeyPressed;
                    }
                }
                else
                {
                	if (pts.Count == 1)
                	{
	        			eq.Parameters[var] = firstVar-pointFrame;
						
	        			switch (type)
	                	{
	                		case ExpressionType.CARTESIAN:
	                    		switch (func)
	                    		{
	                    			case VariablesUsed.ONE:
	                					points.Add(Calculate.CartesianPoint(firstVar-pointFrame, Convert.ToDouble(eq.Evaluate())));
	                    				break;
	                    			case VariablesUsed.TWO:
	                					points.Add(Calculate.CartesianPoint(Convert.ToDouble(eq.Evaluate()), firstVar-pointFrame));
	                    				break;
	                    		}
	                    		
	                			break;
	                		case ExpressionType.POLAR:
	                			eq.Parameters[var] = firstVar-pointFrame;
								
	                    		switch (func)
	                    		{
	                    			case VariablesUsed.ONE:
	                					points.Add(Calculate.PolarPoint(firstVar-pointFrame, Convert.ToDouble(eq.Evaluate())));
	                    				break;
	                    			case VariablesUsed.TWO:
	                					points.Add(Calculate.PolarPoint(Convert.ToDouble(eq.Evaluate()), firstVar-pointFrame));
	                    				break;
	                    		}
	                    		
	                			break;
	                		default:
	                			break;
	                	}
                    }
                    else if (pts.Count > 1)
                        intervals.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
					
                    pts = new List<Point3d>();
                }
                
                if (firstVar + pointFrame > ul && !Math.Round(firstVar, Calculate.DecimalCount(pointFrame)).Equals(ul))
                	firstVar = ul - pointFrame;
            }
            
            if (pts.Count == 1)
        	{
            	switch (type)
            	{
            		case ExpressionType.CARTESIAN:
            			switch (func)
                		{
                			case VariablesUsed.ONE:
            					points.Add(Calculate.CartesianPoint(ul, secondVar));
                				break;
                			case VariablesUsed.TWO:
            					points.Add(Calculate.CartesianPoint(secondVar, ul));
                				break;
                		}
                		
            			break;
            		case ExpressionType.POLAR:
            			switch (func)
                		{
                			case VariablesUsed.ONE:
            					points.Add(Calculate.PolarPoint(ul, secondVar));
                				break;
                			case VariablesUsed.TWO:
            					points.Add(Calculate.PolarPoint(secondVar, ul));
                				break;
                		}
                		
            			break;
            		default:
            			break;
            	}
            }
            else if (pts.Count > 1)
            	intervals.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
			
            return intervals;
        }
		
		// Curve generation from implicit equation
        internal static List<Curve> ImplicitCurve(RhinoDoc doc, Expression eq, ExpressionType type, double oneLL, double oneUL, double twoLL, double twoUL, int pointIteration, int curveIteration, VariablesUsed implicitSrf = VariablesUsed.NONE, double three = 0)
        {
        	List<List<Point3d>> dummy = new List<List<Point3d>>();
            List<List<Curve>> curves = new List<List<Curve>>();
            List<Point3d> corners = new List<Point3d>(),
            			  pts = new List<Point3d>();
			Point3d p1 = new Point3d(),
					p2 = new Point3d(),
					p3 = new Point3d(),
					p4 = new Point3d(),
					bottomLowerLeft = new Point3d(),
					bottomLowerRight = new Point3d(),
					bottomUpperLeft = new Point3d(),
					bottomUpperRight = new Point3d(),
					topLowerLeft = new Point3d(),
					topLowerRight = new Point3d(),
					topUpperLeft = new Point3d(),
					topUpperRight = new Point3d();
			string varThree = null;
            Surface surface = null,
            		plane = null;
            Point3d[] pointArray;
            Curve[] curveArray;
            double minX = 0,
            	   maxX = 0,
            	   minY = 0,
            	   maxY = 0,
            	   minZ = 0,
            	   maxZ = 0;
            
			if (Equation.Exists(implicitSrf))
			{
				switch (implicitSrf)
				{
					case VariablesUsed.ONE_TWO:
						switch (type)
						{
							case ExpressionType.CARTESIAN:
								varThree = "z";
								break;
							case ExpressionType.POLAR:
								varThree = "r";
								break;
							case ExpressionType.CYLINDRICAL:
								varThree = "r";
								break;
						}
						break;
					case VariablesUsed.ONE_THREE:
						switch (type)
						{
							case ExpressionType.CARTESIAN:
								varThree = "y";
								break;
							case ExpressionType.POLAR:
								varThree = "phi";
								break;
							case ExpressionType.CYLINDRICAL:
								varThree = "z";
								break;
						}
						break;
					case VariablesUsed.TWO_THREE:
						switch (type)
						{
							case ExpressionType.CARTESIAN:
								varThree = "x";
								break;
							case ExpressionType.POLAR:
								varThree = "theta";
								break;
							case ExpressionType.CYLINDRICAL:
								varThree = "theta";
								break;
						}
						break;
				}
			}
            
			switch (type)
			{
				case ExpressionType.CARTESIAN:
					curves = Equation.Surface(doc, ref dummy, eq, type, VariablesUsed.ONE_TWO, true, oneLL, oneUL, twoLL, twoUL, 0, 0, pointIteration, curveIteration, 0, implicitSrf, varThree);
					break;
				case ExpressionType.POLAR:
					curves = Equation.Surface(doc, ref dummy, eq, type, VariablesUsed.ONE_THREE, true, oneLL, oneUL, 0, 0, twoLL, twoUL, pointIteration, curveIteration, 0, implicitSrf, varThree);
					break;
			}
			
            surface = Generate.Surface(doc, dummy, curves, false);
            corners = BoundingBox(doc, surface);
            doc.Objects.AddSurface(surface);
            
            minX = corners[6].X;
            maxX = corners[0].X;
            minY = corners[6].Y;
            maxY = corners[0].Y;
            minZ = corners[6].Z;
            maxZ = corners[0].Z;
            
        	minX--;
        	minY--;
        	minZ--;
        	maxX++;
        	maxY++;
        	maxZ++;
        	
            if (Equation.Exists(implicitSrf))
            {
            	switch (implicitSrf)
				{
					case VariablesUsed.ONE_TWO:
            			minZ = three;
						break;
					case VariablesUsed.ONE_THREE:
            			minY = three;
						break;
					case VariablesUsed.TWO_THREE:
            			minX = three;
						break;
				}
            }
            else
            {
            	minZ = 0;
            }
            
            bottomLowerLeft = new Point3d(minX, minY, minZ);
			bottomLowerRight = new Point3d(maxX, minY, minZ);
			bottomUpperLeft = new Point3d(minX, maxY, minZ);
			bottomUpperRight = new Point3d(maxX, maxY, minZ);
			topLowerLeft = new Point3d(minX, minY, maxZ);
			topLowerRight = new Point3d(maxX, minY, maxZ);
			topUpperLeft = new Point3d(minX, maxY, maxZ);
			topUpperRight = new Point3d(maxX, maxY, maxZ);
            
			if (Equation.Exists(implicitSrf))
			{
				switch (implicitSrf)
				{
					case VariablesUsed.ONE_TWO:
						p1 = bottomLowerLeft;
						p2 = bottomLowerRight;
						p3 = bottomUpperRight;
						p4 = bottomUpperLeft;
						break;
					case VariablesUsed.ONE_THREE:
						p1 = bottomLowerLeft;
						p2 = bottomLowerRight;
						p2 = topLowerRight;
						p3 = topLowerLeft;
						break;
					case VariablesUsed.TWO_THREE:
						p1 = bottomLowerLeft;
						p2 = bottomUpperLeft;
						p2 = topUpperLeft;
						p3 = topLowerLeft;
						break;
				}
			}
			else
			{
				p1 = bottomLowerLeft;
				p2 = bottomLowerRight;
				p3 = bottomUpperRight;
				p4 = bottomUpperLeft;
			}
			
			plane = NurbsSurface.CreateFromCorners(p1, p2, p3, p4); //@CHECK
            //doc.Objects.AddSurface(plane);
            Rhino.Geometry.Intersect.Intersection.SurfaceSurface(surface, plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out curveArray, out pointArray);
            
            return curveArray.ToList();
        }
		
        // Curve generation from parametric equation
        internal static List<Curve> ParametricCurve(RhinoDoc doc, Expression eqOne, Expression eqTwo, Expression eqThree, ExpressionType type, ref List<Point3d> points, bool drawExpression, double ll, double ul, int pointIteration)
        {
            EscapeKeyEventHandler handler = new EscapeKeyEventHandler("Press <Esc> to stop drawing.");
			
            List<Point3d> pts = new List<Point3d>();
            List<Curve> intervals =  new List<Curve>();
            double one = 0,
            	   two = 0,
            	   three = 0,
            	   pointFrame = 0;
			
            Equation.SetParameters(ref eqOne);
            Equation.SetParameters(ref eqTwo);
            Equation.SetParameters(ref eqThree);
            
            pointFrame = (ul - ll) / pointIteration;
            
            for (double t = ll; t <= ul; t += pointFrame)
            {
                eqOne.Parameters["t"] = Math.Round(t, Calculate.DecimalCount(pointFrame));
                eqTwo.Parameters["t"] = Math.Round(t, Calculate.DecimalCount(pointFrame));
                eqThree.Parameters["t"] = Math.Round(t, Calculate.DecimalCount(pointFrame));
                
                one = Convert.ToDouble(eqOne.Evaluate());
                two = Convert.ToDouble(eqTwo.Evaluate());
                three = Convert.ToDouble(eqThree.Evaluate());
                
                if (Equation.IsRealNumberAt(one) && Equation.IsRealNumberAt(two) && Equation.IsRealNumberAt(three))
                {
                	switch (type)
		        	{
		        		case ExpressionType.CARTESIAN:
		        			pts.Add(Calculate.CartesianPoint(one, two, three));
		        			break;
		        		case ExpressionType.POLAR:
		        			pts.Add(Calculate.PolarPoint(one, two, three));
		        			break;
		        		case ExpressionType.CYLINDRICAL:
		        			pts.Add(Calculate.CylindricalPoint(one, two, three));
		        			break;
		        		default:
		        			break;
		        	}
                    
					if (drawExpression)
                    {
						Graph.DrawCurve(doc, intervals, pts, points);
						
                        if (handler.EscapeKeyPressed)
                        	drawExpression = false;
                    }
                }
                else
                {
                	if (pts.Count == 1)
                	{
                		eqOne.Parameters["t"] = Math.Round(t-pointFrame, Calculate.DecimalCount(pointFrame));
                		eqTwo.Parameters["t"] = Math.Round(t-pointFrame, Calculate.DecimalCount(pointFrame));
                		eqThree.Parameters["t"] = Math.Round(t-pointFrame, Calculate.DecimalCount(pointFrame));
                		
                		switch (type)
			        	{
			        		case ExpressionType.CARTESIAN:
			        			points.Add(Calculate.CartesianPoint(Convert.ToDouble(eqOne.Evaluate()), Convert.ToDouble(eqTwo.Evaluate()), Convert.ToDouble(eqThree.Evaluate())));
			        			break;
			        		case ExpressionType.POLAR:
			        			points.Add(Calculate.PolarPoint(Convert.ToDouble(eqOne.Evaluate()), Convert.ToDouble(eqTwo.Evaluate()), Convert.ToDouble(eqThree.Evaluate())));
			        			break;
			        		case ExpressionType.CYLINDRICAL:
			        			points.Add(Calculate.CylindricalPoint(Convert.ToDouble(eqOne.Evaluate()), Convert.ToDouble(eqTwo.Evaluate()), Convert.ToDouble(eqThree.Evaluate())));
			        			break;
			        		default:
			        			break;
			        	}
                    }
                    else if (pts.Count > 1)
                        intervals.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
					
                    pts = new List<Point3d>();
                }
                
                if (t + pointFrame > ul && !Math.Round(t, Calculate.DecimalCount(pointFrame)).Equals(ul))
                	t = ul - pointFrame;
            }
            
            if (pts.Count == 1)
        	{
            	switch (type)
	        	{
	        		case ExpressionType.CARTESIAN:
	        			pts.Add(Calculate.CartesianPoint(one, two, three));
	        			break;
	        		case ExpressionType.POLAR:
	        			pts.Add(Calculate.PolarPoint(one, two, three));
	        			break;
	        		case ExpressionType.CYLINDRICAL:
	        			pts.Add(Calculate.CylindricalPoint(one, two, three));
	        			break;
	        		default:
	        			break;
	        	}
            }
            if (pts.Count > 1)
            	intervals.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.ChordSquareRoot));
			
            return intervals;
        }
		
		// Surface generation from equation
        internal static List<List<Curve>> Surface(RhinoDoc doc, ref List<List<Point3d>> points, Expression eq, ExpressionType type, VariablesUsed func, bool uPriority, double oneLL, double oneUL, double twoLL, double twoUL, double threeLL, double threeUL, int pointIteration, int curveIteration, double fourthVar, VariablesUsed implicitSrf = VariablesUsed.NONE, string varThree = null)
        {
            List<List<Curve>> returnCurves = new List<List<Curve>>();
            List<Point3d> pts = new List<Point3d>();
            List<Curve> uCurves = new List<Curve>(),
            			vCurves = new List<Curve>();
            			
			points = new List<List<Point3d>>();
            string varOne = null,
            	   varTwo = null;
            double uLL = 0,
            	   uUL = 0,
            	   vLL = 0,
            	   vUL = 0,
            	   thirdVar = 0,
            	   thirdBefore = 0,
            	   curveFrame = 0,
            	   pointFrame = 0,
            	   maxIterations = curveIteration > pointIteration ? curveFrame : pointIteration;
            	
            Equation.SetParameters(ref eq);
			
            switch (func)
            {
            	case VariablesUsed.ONE_TWO:
            		uLL = oneLL;
            		uUL = oneUL;
            		vLL = twoLL;
            		vUL = twoUL;
            		
            		switch (type)
            		{
            			case ExpressionType.CARTESIAN_4D:
            			case ExpressionType.CARTESIAN:
            				varOne = "x";
            				varTwo = "y";
            				break;
            			case ExpressionType.SPHERICAL_4D:
            			case ExpressionType.POLAR:
            				varOne = "theta";
            				varTwo = "phi";
            				break;
            			case ExpressionType.CYLINDRICAL_4D:
            			case ExpressionType.CYLINDRICAL:
            				varOne = "theta";
            				varTwo = "z";
            				break;
            			default:
            				break;
            		}
            		
            		break;
            	case VariablesUsed.ONE_THREE:
            		uLL = oneLL;
            		uUL = oneUL;
            		vLL = threeLL;
            		vUL = threeUL;
            		
            		switch (type)
            		{
            			case ExpressionType.CARTESIAN_4D:
            			case ExpressionType.CARTESIAN:
            				varOne = "x";
            				varTwo = "z";
            				break;
            			case ExpressionType.SPHERICAL_4D:
            			case ExpressionType.POLAR:
            				varOne = "theta";
            				varTwo = "r";
            				break;
            			case ExpressionType.CYLINDRICAL_4D:
            			case ExpressionType.CYLINDRICAL:
            				varOne = "theta";
            				varTwo = "r";
            				break;
            			default:
            				break;
            		}
            		
            		break;
            	case VariablesUsed.TWO_THREE:
            		uLL = twoLL;
            		uUL = twoUL;
            		vLL = threeLL;
            		vUL = threeUL;
            		
            		switch (type)
            		{
            			case ExpressionType.CARTESIAN_4D:
            			case ExpressionType.CARTESIAN:
            				varOne = "y";
            				varTwo = "z";
            				break;
            			case ExpressionType.SPHERICAL_4D:
            			case ExpressionType.POLAR:
            				varOne = "phi";
            				varTwo = "r";
            				break;
            			case ExpressionType.CYLINDRICAL_4D:
            			case ExpressionType.CYLINDRICAL:
            				varOne = "z";
            				varTwo = "r";
            				break;
            			default:
            				break;
            		}
            		
            		break;
            }
            
            switch (type)
            {
            	case ExpressionType.CARTESIAN_4D:
            		eq.Parameters["w"] = fourthVar;
            		break;
            	default:
            		if (Equation.Is4D(type))
            			eq.Parameters["s"] = fourthVar;
            		break;
            }
            
            if (Equation.IsImplicit(func))
            {
            	func = implicitSrf;
            	eq.Parameters[varThree] = fourthVar;
            }
            
            curveFrame = curveIteration == pointIteration ? (uUL - uLL) / curveIteration : (uUL - uLL) / maxIterations;
            pointFrame = pointIteration == curveIteration ? (vUL - vLL) / pointIteration : (vUL - vLL) / maxIterations;
            
        	for (double firstVar = uLL; firstVar <= uUL; firstVar += curveFrame)
            {
                eq.Parameters[varOne] = Math.Round(firstVar, Calculate.DecimalCount(curveFrame));
				
                for (double secondVar = vLL; secondVar <= vUL; secondVar += pointFrame)
                {
                    eq.Parameters[varTwo] = Math.Round(secondVar, Calculate.DecimalCount(pointFrame));
					
                    thirdVar = Convert.ToDouble(eq.Evaluate());
                    // @DERIVE HERE
                    if (Double.IsPositiveInfinity(thirdVar))
                    	thirdVar = thirdBefore + .1;
                    else if (Double.IsNegativeInfinity(thirdVar))
                    	thirdVar = thirdBefore - .1;
                    else if (Double.IsNaN(thirdVar))
                    	thirdVar = 0;
                    
                    thirdBefore = thirdVar;
                    
                    if (Equation.IsRealNumberAt(thirdVar))
                        switch (type)
                    	{
                    		case ExpressionType.CARTESIAN:
            				case ExpressionType.CARTESIAN_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.CartesianPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.CartesianPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.CartesianPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		case ExpressionType.POLAR:
            				case ExpressionType.SPHERICAL_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.PolarPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.PolarPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.PolarPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		case ExpressionType.CYLINDRICAL:
            				case ExpressionType.CYLINDRICAL_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.CylindricalPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.CylindricalPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.CylindricalPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		default:
                    			break;
                    	}
                	
                    if (secondVar + pointFrame > vUL && !Math.Round(secondVar, Calculate.DecimalCount(pointFrame)).Equals(vUL))
	                	secondVar = vUL - pointFrame;
                }
				
                if (pts.Count > 1)
                	uCurves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform));
				
                if (uPriority)
                	points.Add(pts);
                
                pts = new List<Point3d>();
                
                if (firstVar + curveFrame > uUL && !Math.Round(firstVar, Calculate.DecimalCount(curveFrame)).Equals(uUL))
                	firstVar = uUL - curveFrame;
            }
        	
        	thirdBefore = 0;
            curveFrame = (vUL - vLL) / curveIteration;
            pointFrame = (uUL - uLL) / pointIteration;
            
            for (double secondVar = vLL; secondVar <= vUL; secondVar += curveFrame)
            {
                eq.Parameters[varTwo] = Math.Round(secondVar, Calculate.DecimalCount(curveFrame));
				
                for (double firstVar = uLL; firstVar <= uUL; firstVar += pointFrame)
                {
                    eq.Parameters[varOne] = Math.Round(firstVar, Calculate.DecimalCount(pointFrame));
					
                    thirdVar = Convert.ToDouble(eq.Evaluate());
                    
                    if (Double.IsPositiveInfinity(thirdVar))
                    	thirdVar = thirdBefore + .1;
                    else if (Double.IsNegativeInfinity(thirdVar))
                    	thirdVar = thirdBefore - .1;
                    else if (Double.IsNaN(thirdVar))
                    	thirdVar = 0;
                    
                    thirdBefore = thirdVar;
                    
                    if (Equation.IsRealNumberAt(thirdVar))
                        switch (type)
                    	{
                    		case ExpressionType.CARTESIAN:
            				case ExpressionType.CARTESIAN_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.CartesianPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.CartesianPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.CartesianPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		case ExpressionType.POLAR:
            				case ExpressionType.SPHERICAL_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.PolarPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.PolarPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.PolarPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		case ExpressionType.CYLINDRICAL:
            				case ExpressionType.CYLINDRICAL_4D:
                        		switch (func)
                        		{
                        			case VariablesUsed.ONE_TWO:
                    					pts.Add(Calculate.CylindricalPoint(firstVar, secondVar, thirdVar));
                        				break;
                        			case VariablesUsed.ONE_THREE:
                    					pts.Add(Calculate.CylindricalPoint(firstVar, thirdVar, secondVar));
                        				break;
                        			case VariablesUsed.TWO_THREE:
                    					pts.Add(Calculate.CylindricalPoint(thirdVar, firstVar, secondVar));
                        				break;
                        		}
                        		
                    			break;
                    		default:
                    			break;
                    	}
                	
	                if (firstVar + pointFrame > uUL && !Math.Round(firstVar, Calculate.DecimalCount(pointFrame)).Equals(uUL))
	                	firstVar = uUL - pointFrame;
                }
				
                if (pts.Count > 1)
                	vCurves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform));
				
                if (!uPriority)
                	points.Add(pts);
                
                pts = new List<Point3d>();
                
                if (secondVar + curveFrame > vUL && !Math.Round(secondVar, Calculate.DecimalCount(curveFrame)).Equals(vUL))
                	secondVar = vUL - curveFrame;
            }
            
            Graph.CleanCurves(ref uCurves);
            Graph.CleanCurves(ref vCurves);
            
            returnCurves.Add(uCurves);
            returnCurves.Add(vCurves);
            
            return returnCurves;
        }
		
		// Surface generation from equation
        internal static List<List<Curve>> ImplicitSurface(RhinoDoc doc, Expression eq, ExpressionType type, double oneLL, double oneUL, double twoLL, double twoUL, double threeLL, double threeUL, int pointIteration, int curveIteration)
        {
        	/* Process:
             * 
             * 1. Generate all the implicit curves that create the implicit equation
             * 2. Intersect each implicit curve with it's respective plane to get an ordered set of points
             * 3. Split implicit curve with set of points to get 3 lists (U, V, Z) of split curves that are used with the next step
             * 4. Generate each individual surface of implicit surface using EdgeSrf
             * 		a. Exclude one quaruplet of curves
             * 		b. Generate surface using EdgeSrf
             * 		c. Add surface to list of surfaces
             * 5. Either group, join, or boolean union all of the generated surfaces together
             * 		a. If success, export it as Surface
             * 		v. If failed, export it as List<Surface>
             * 
        	 */
        	
        	List<List<Curve>> returnCurves = new List<List<Curve>>();
        	List<List<Point3d>> dummy = new List<List<Point3d>>();
	        List<Curve> curves = new List<Curve>();
            List<Curve> uCurves = new List<Curve>(),
            			vCurves = new List<Curve>();
            double threeFrame = 0;
//            string varOne = null,
//            	   varTwo = null,
//            	   varThree = null;
//            
//    		switch (type)
//    		{
//    			case ExpressionType.CARTESIAN_4D:
//    			case ExpressionType.CARTESIAN:
//    				varOne = "x";
//    				varTwo = "y";
//    				varThree = "z";
//    				break;
//    			case ExpressionType.SPHERICAL_4D:
//    			case ExpressionType.POLAR:
//    				varOne = "theta";
//    				varTwo = "phi";
//    				varThree = "r";
//    				break;
//    			case ExpressionType.CYLINDRICAL_4D:
//    			case ExpressionType.CYLINDRICAL:
//    				varOne = "theta";
//    				varTwo = "z";
//    				varThree = "r";
//    				break;
//    			default:
//    				break;
//    		}
    		// @HERE MAKE SURFACE FUNCTION
            
            
            threeFrame = (threeUL - threeLL) / curveIteration;
			
			for (double thirdVar = threeLL; thirdVar <= threeUL; thirdVar += threeFrame)
            {
                
                switch (type)
				{
					case ExpressionType.CARTESIAN:
                		curves = Equation.ImplicitCurve(doc, eq, type, oneLL, oneUL, twoLL, twoUL, pointIteration, curveIteration, VariablesUsed.ONE_TWO, thirdVar);
						break;
					case ExpressionType.POLAR:
						curves = Equation.ImplicitCurve(doc, eq, type, oneLL, oneUL, twoLL, twoUL, pointIteration, curveIteration, VariablesUsed.ONE_TWO, thirdVar);
						break;
					case ExpressionType.CYLINDRICAL:
						curves = Equation.ImplicitCurve(doc, eq, type, oneLL, oneUL, twoLL, twoUL, pointIteration, curveIteration, VariablesUsed.ONE_TWO, thirdVar);
						break;
				}
				
		        foreach (Curve c in curves)
		        {
		        	doc.Objects.AddCurve(c);
		        }
                
                if (!threeUL.Equals(thirdVar + curveIteration))
                {
                	thirdVar = threeUL - curveIteration;
                }
            }
            
            Graph.CleanCurves(ref uCurves);
            Graph.CleanCurves(ref vCurves);
            
            returnCurves.Add(uCurves);
            returnCurves.Add(vCurves);
            
            return returnCurves;
        }
		
        // Surface generation from parametric equation
        internal static List<List<Curve>> ParametricSurface(RhinoDoc doc, ref List<List<Point3d>> points, Expression eqOne, Expression eqTwo, Expression eqThree, ExpressionType type, bool uPriority, double uLL, double uUL, double vLL, double vUL, int pointIteration, int curveIteration, double four)
        {
            List<List<Curve>> returnCurves = new List<List<Curve>>();
            List<Surface> surfaces = new List<Surface>();
        	List<Point3d> curves = new List<Point3d>(),
        				  pts = new List<Point3d>();
            List<Curve> uCurves = new List<Curve>(),
            			vCurves = new List<Curve>();
            points = new List<List<Point3d>>();
            double one = 0,
            	   two = 0,
            	   three = 0,
            	   oneBefore = 0,
            	   twoBefore = 0,
            	   threeBefore = 0,
            	   curveFrame = 0,
            	   pointFrame = 0;
			
            Equation.SetParameters(ref eqOne);
            Equation.SetParameters(ref eqTwo);
            Equation.SetParameters(ref eqThree);
			
            switch (type)
            {
	            case ExpressionType.CARTESIAN_4D:
	            	eqOne.Parameters["w"] = four;
	            	eqTwo.Parameters["w"] = four;
	            	eqThree.Parameters["w"] = four;
	            	
	            	break;
	            default:
	            	eqOne.Parameters["s"] = four;
	            	eqTwo.Parameters["s"] = four;
	            	eqThree.Parameters["s"] = four;
            	
	            	break;
            }
            
            curveFrame = (uUL - uLL) / curveIteration;
            pointFrame = (vUL - vLL) / pointIteration;
            
        	for (double u = uLL; u <= uUL; u += curveFrame)
            {
                eqOne.Parameters["u"] = Math.Round(u, Calculate.DecimalCount(curveFrame));
                eqTwo.Parameters["u"] = Math.Round(u, Calculate.DecimalCount(curveFrame));
                eqThree.Parameters["u"] = Math.Round(u, Calculate.DecimalCount(curveFrame));
				
                for (double v = vLL; v <= vUL; v += pointFrame)
                {
                    eqOne.Parameters["v"] = Math.Round(v, Calculate.DecimalCount(pointFrame));
                    eqTwo.Parameters["v"] = Math.Round(v, Calculate.DecimalCount(pointFrame));
                    eqThree.Parameters["v"] = Math.Round(v, Calculate.DecimalCount(pointFrame));
                    
                    one = Convert.ToDouble(eqOne.Evaluate());
                    two = Convert.ToDouble(eqTwo.Evaluate());
                    three = Convert.ToDouble(eqThree.Evaluate());
                	
                    if (Double.IsPositiveInfinity(one))
                    	one = oneBefore + .1;
                    else if (Double.IsNegativeInfinity(one))
                    	one = oneBefore - .1;
                    else if (Double.IsNaN(one))
                    	one = 0;
                    
                    oneBefore = one;
                    
                    if (Double.IsPositiveInfinity(two))
                    	two = twoBefore + .1;
                    else if (Double.IsNegativeInfinity(two))
                    	two = twoBefore - .1;
                    else if (Double.IsNaN(two))
                    	two = 0;
                    
                    twoBefore = two;
                    
                    if (Double.IsPositiveInfinity(three))
                    	three = threeBefore + .1;
                    else if (Double.IsNegativeInfinity(three))
                    	three = threeBefore - .1;
                    else if (Double.IsNaN(three))
                    	three = 0;
                    
                    threeBefore = three;
                    
                    if (Equation.IsRealNumberAt(one) && Equation.IsRealNumberAt(two) && Equation.IsRealNumberAt(three))
                        switch (type)
                    	{
                    		case ExpressionType.CARTESIAN:
	            			case ExpressionType.CARTESIAN_4D:
                    			pts.Add(Calculate.CartesianPoint(one, two, three));
                    			break;
                    		case ExpressionType.POLAR:
	            			case ExpressionType.SPHERICAL_4D:
                    			pts.Add(Calculate.PolarPoint(one, two, three));
                    			break;
                    		case ExpressionType.CYLINDRICAL:
                    		case ExpressionType.CYLINDRICAL_4D:
                    			pts.Add(Calculate.CylindricalPoint(one, two, three));
                    			break;
                    		default:
                    			break;
                    	}
                	
	                if (v + pointFrame > vUL && !Math.Round(v, Calculate.DecimalCount(pointFrame)).Equals(vUL))
	                	v = vUL - pointFrame;
                }
				
                if (pts.Count > 1)
                	uCurves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform));
				
                if (uPriority)
                	points.Add(pts);
                
                pts = new List<Point3d>();
                
                if (u + curveFrame > uUL && !Math.Round(u, Calculate.DecimalCount(curveFrame)).Equals(uUL))
                	u = uUL - curveFrame;
            }
        	
        	oneBefore = 0;
        	twoBefore = 0;
        	threeBefore = 0;
        	
            curveFrame = (vUL - vLL) / curveIteration;
            pointFrame = (uUL - uLL) / pointIteration;
            
            for (double v = vLL; v <= vUL; v += curveFrame)
            {
                eqOne.Parameters["v"] = Math.Round(Math.Round(v, Calculate.DecimalCount(curveFrame)), Calculate.DecimalCount(curveFrame));
                eqTwo.Parameters["v"] = Math.Round(Math.Round(v, Calculate.DecimalCount(curveFrame)), Calculate.DecimalCount(curveFrame));
                eqThree.Parameters["v"] = Math.Round(Math.Round(v, Calculate.DecimalCount(curveFrame)), Calculate.DecimalCount(curveFrame));
				
                for (double u = uLL; u <= uUL; u += pointFrame)
                {
	                eqOne.Parameters["u"] = Math.Round(Math.Round(u, Calculate.DecimalCount(pointFrame)), Calculate.DecimalCount(pointFrame));
	                eqTwo.Parameters["u"] = Math.Round(Math.Round(u, Calculate.DecimalCount(pointFrame)), Calculate.DecimalCount(pointFrame));
	                eqThree.Parameters["u"] = Math.Round(Math.Round(u, Calculate.DecimalCount(pointFrame)), Calculate.DecimalCount(pointFrame));
					
                    one = Convert.ToDouble(eqOne.Evaluate());
                    two = Convert.ToDouble(eqTwo.Evaluate());
                    three = Convert.ToDouble(eqThree.Evaluate());
                	// @MAKE METHOD
                    if (Double.IsPositiveInfinity(one))
                    	one = oneBefore + .1;
                    else if (Double.IsNegativeInfinity(one))
                    	one = oneBefore - .1;
                    else if (Double.IsNaN(one))
                    	one = 0;
                    
                    oneBefore = one;
                    
                    if (Double.IsPositiveInfinity(two))
                    	two = twoBefore + .1;
                    else if (Double.IsNegativeInfinity(two))
                    	two = twoBefore - .1;
                    else if (Double.IsNaN(two))
                    	two = 0;
                    
                    twoBefore = two;
                    
                    if (Double.IsPositiveInfinity(three))
                    	three = threeBefore + .1;
                    else if (Double.IsNegativeInfinity(three))
                    	three = threeBefore - .1;
                    else if (Double.IsNaN(three))
                    	three = 0;
                    
                    threeBefore = three;
                    
                    if (Equation.IsRealNumberAt(one) && Equation.IsRealNumberAt(two) && Equation.IsRealNumberAt(three))
                        switch (type)
                    	{
                    		case ExpressionType.CARTESIAN:
	            			case ExpressionType.CARTESIAN_4D:
                    			pts.Add(Calculate.CartesianPoint(one, two, three));
                    			break;
                    		case ExpressionType.POLAR:
	            			case ExpressionType.SPHERICAL_4D:
                    			pts.Add(Calculate.PolarPoint(one, two, three));
                    			break;
                    		case ExpressionType.CYLINDRICAL:
                    		case ExpressionType.CYLINDRICAL_4D:
                    			pts.Add(Calculate.CylindricalPoint(one, two, three));
                    			break;
                    		default:
                    			break;
                	}
                	
	                if (u + pointFrame > uUL && !Math.Round(u, Calculate.DecimalCount(pointFrame)).Equals(uUL))
	                	u = uUL - pointFrame;
                }
				
                if (pts.Count > 1)
                	vCurves.Add(Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform));
				
                if (!uPriority)
                	points.Add(pts);
                
                pts = new List<Point3d>();
                
                if (v + curveFrame > vUL && !Math.Round(v, Calculate.DecimalCount(curveFrame)).Equals(vUL))
                	v = vUL - curveFrame;
            }
            
            Graph.CleanCurves(ref uCurves);
            Graph.CleanCurves(ref vCurves);
            
            returnCurves.Add(uCurves);
            returnCurves.Add(vCurves);
            
            return returnCurves;
        }
		
        // Checks to see if a value is graphable, returns false if value is either positive or negative infinity or if value is imaginary
        internal static bool IsRealNumberAt(double d)
        {
            return (!Double.IsNegativeInfinity(d) && !Double.IsPositiveInfinity(d) && !Double.IsInfinity(d) && !Double.IsNaN(d));
        }
        
		// Returns true if the equation is not VariablesUsed.NONE
        internal static bool Exists(VariablesUsed func)
        {
        	return func != VariablesUsed.NONE;
        }
		
		// Returns true if the equation is second dimensional
        internal static bool Is2D(VariablesUsed func)
        {
        	return func == VariablesUsed.ONE || func == VariablesUsed.TWO;
        }
		
		// Returns true if the equation is third dimensional
        internal static bool Is3D(VariablesUsed func)
        {
        	return func == VariablesUsed.ONE_TWO || func == VariablesUsed.ONE_THREE || func == VariablesUsed.TWO_THREE;
        }
		
		// Returns true if the equation is fourth dimensional
        internal static bool Is4D(ExpressionType type)
        {
        	return type == ExpressionType.CARTESIAN_4D || type == ExpressionType.SPHERICAL_4D || type == ExpressionType.CYLINDRICAL_4D;
        }
		
        // Returns true if the equation is parametric
        internal static bool IsImplicit(VariablesUsed func)
        {
        	return func == VariablesUsed.IMPLICIT_CURVE || func == VariablesUsed.IMPLICIT_SURFACE;
        }
        
        // Returns true if the equation is parametric
        internal static bool IsParametric(VariablesUsed func)
        {
        	return func == VariablesUsed.PARAMETRIC_CURVE || func == VariablesUsed.PARAMETRIC_SURFACE;
        }
        
        // Returns the equation type as a string
		internal static string ToString(ExpressionType type, bool spherical = false)
        {
        	switch (type)
        	{
        		case ExpressionType.POLAR:
        			if (spherical)
        				return "Spherical";
        			return "Polar";
        		case ExpressionType.CYLINDRICAL:
        			return "Cylindrical";
        		case ExpressionType.CARTESIAN_4D:
        			return "4D Cartesian";
        		case ExpressionType.SPHERICAL_4D:
        			return "4D Spherical";
        		case ExpressionType.CYLINDRICAL_4D:
        			return "4D Cylindrical";
        		default:
        			return "Cartesian";
        	}
        }
		
		// Sets all parameters and mathematical functions to the expression
        internal static void SetParameters(ref Expression expression)
        {
        	expression.Parameters["pi"] = Math.PI;
            expression.Parameters["e"] = Math.E;
            
            expression.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
				double d = Double.NaN,
					   e = Double.NaN,
					   f = Double.NaN;
            	
            	if (args.Parameters.Length > 0)
					d =  Convert.ToDouble(args.Parameters[0].Evaluate());
				if (args.Parameters.Length > 1)
					e = Convert.ToDouble(args.Parameters[1].Evaluate());
				if (args.Parameters.Length > 2)
					f = Convert.ToDouble(args.Parameters[2].Evaluate());
            	
            	if (name == "abs")
            		args.Result = Math.Abs(d);
            	else if (name == "pow")
            		args.Result = Math.Pow(d, e);
            	else if (name == "sqrt")
            		args.Result = Math.Sqrt(d);
            	else if (name == "round")
            		args.Result = Math.Round(d,Convert.ToInt32(e));
            	else if (name == "sign")
            	{
            		if (d > 0)
            			args.Result = 1;
            		else if (d < 0)
            			args.Result = -1;
            		else
            			args.Result = 0;
            	}
            	else if (name == "min")
            		args.Result = Math.Min(d,e);
            	else if (name == "max")
            		args.Result = Math.Max(d,e);
            	else if (name == "ceiling")
            		args.Result = Math.Ceiling(d);
            	else if (name == "truncate")
            		args.Result = Math.Truncate(d);
            	else if (name == "exp")
            		args.Result = Math.Exp(d);
            	else if (name == "floor")
            		args.Result = Math.Floor(d);
            	else if (name == "remainder" || name == "ieeeremainder")
            		args.Result = Math.IEEERemainder(d, e);
            	else if (name == "ln")
            		args.Result = Math.Log(d);
            	else if (name == "log")
            		if (Double.IsNaN(e))
            			args.Result = Math.Log10(d);
            		else
            			args.Result = Math.Log10(e) / Math.Log10(d);
            	else if (name == "log10")
            		args.Result = Math.Log10(d);
                else if (name == "sin")
                	args.Result = Math.Sin(d % (2 * Math.PI));
                else if (name == "cos")
                	args.Result = Math.Cos(d % (2 * Math.PI));
                else if (name == "tan")
                	args.Result = Math.Tan(d % (2 * Math.PI));
                else if (name == "csc")
                    args.Result = 1 / Math.Sin(d % (2 * Math.PI));
                else if (name == "sec")
                    args.Result = 1 / Math.Cos(d % (2 * Math.PI));
                else if (name == "cot")
                    args.Result = 1 / Math.Tan(d % (2 * Math.PI));
                else if (name == "asin")
                	args.Result = Math.Asin(d % (2 * Math.PI));
                else if (name == "acos")
                	args.Result = Math.Acos(d % (2 * Math.PI));
                else if (name == "atan")
                	args.Result = Math.Atan(d % (2 * Math.PI));
                else if (name == "sinh")
                	args.Result = Math.Sinh(d);
                else if (name == "cosh")
                	args.Result = Math.Cosh(d);
                else if (name == "tanh")
                	args.Result = Math.Tanh(d);
                else if (name == "csch")
                	args.Result = 1 / Math.Sinh(d);
                else if (name == "sech")
                	args.Result = 1 / Math.Cosh(d);
                else if (name == "coth")
                	args.Result = 1 / Math.Tanh(d);
                else if (name == "sinc")
					if (Equals(d, 0))
						args.Result = 1;
					else
						args.Result = Math.Sin(d % (2 * Math.PI)) / d;
                else if (name == "random")
                {
                	var random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
        				
        			if (Double.IsNaN(d))
						d = 1;
                	
                	args.Result = random.NextDouble() * d;
                }
                else if (name == "randint")
                {
                	var random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
        				
        			if (d < e)
        				args.Result = (int) d + (int) ((e - d + 1) * random.NextDouble());
        			else
        				args.Result = (int) e + (int) ((d - e + 1) * random.NextDouble());
                }
                else if (name == "randdec")
                {
                	var random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
        				
        			if (d < e)
        				args.Result = d + (e - d + 1) * random.NextDouble();
        			else
        				args.Result = e + (d - e + 1) * random.NextDouble();
                }
            };
        }
        
        // Checks to see if the expression is valid and can be used
        internal static bool IsValid(Expression e)
        {
            if (e.HasErrors())
            {
                RhinoApp.WriteLine(e.Error);
                return false;
            }

            return true;
        }
		
        // Removes the substrings that are mathematical functions from equation; used for variable checking
        internal static void RemoveMathFunctions(ref string  eq)
        {
        	eq = eq.Replace("ieeeremainder(", "");
        	eq = eq.Replace("remainder(", "");
        	eq = eq.Replace("truncate(", "");
        	eq = eq.Replace("randdec(", "");
        	eq = eq.Replace("randint(", "");
        	eq = eq.Replace("ceiling(", "");
        	eq = eq.Replace("random(", "");
        	eq = eq.Replace("round(", "");
        	eq = eq.Replace("floor(", "");
        	eq = eq.Replace("log10(", "");
        	eq = eq.Replace("asin(", "");
        	eq = eq.Replace("acos(", "");
        	eq = eq.Replace("atan(", "");
        	eq = eq.Replace("sinh(", "");
        	eq = eq.Replace("cosh(", "");
        	eq = eq.Replace("tanh(", "");
        	eq = eq.Replace("csch(", "");
        	eq = eq.Replace("sech(", "");
        	eq = eq.Replace("coth(", "");
        	eq = eq.Replace("sinc(", "");
        	eq = eq.Replace("sign(", "");
        	eq = eq.Replace("sqrt(", "");
        	eq = eq.Replace("rand", "");
        	eq = eq.Replace("abs(", "");
        	eq = eq.Replace("pow(", "");
        	eq = eq.Replace("min(", "");
        	eq = eq.Replace("max(", "");
        	eq = eq.Replace("exp(", "");
        	eq = eq.Replace("log(", "");
        	eq = eq.Replace("sin(", "");
        	eq = eq.Replace("cos(", "");
        	eq = eq.Replace("tan(", "");
        	eq = eq.Replace("csc(", "");
        	eq = eq.Replace("sec(", "");
        	eq = eq.Replace("cot(", "");
        	eq = eq.Replace("ln(", "");
        	eq = eq.Replace("pi", "");
        }
        
        // Checks equation to make sure it has all the right elements to it
        internal static bool HasRightVariables(string equation, string a, string b = "", string c = "")
        {
        	if (equation.Length == 0)
        		return false;
        	
        	equation = equation.Substring(equation.IndexOf('=') + 1);
        	
        	RemoveMathFunctions(ref equation);
        	
        	if (equation.IndexOf(a, StringComparison.Ordinal) != -1 && a != "")
        		equation = equation.Replace(a, "");
        	if (equation.IndexOf(b, StringComparison.Ordinal) != -1 && b != "")
        		equation = equation.Replace(b, "");
        	if (equation.IndexOf(c, StringComparison.Ordinal) != -1 && c != "")
        		equation = equation.Replace(c, "");
        	if (equation.Length != 0)
        		return !CheckForBadVar(equation.ToCharArray());
        	
        	return true;
        }
        
        // Checks equation to make sure that there are no characters in string that are not acceptable variables
        internal static bool CheckForBadVar(char[] eq)
        {
        	foreach (char c in eq)
        		if ((int)c != 101 && ((int)c >= 97 && (int)c <= 122))
        			return true;
        	
        	return false;
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
        			return Rhino.Geometry.Intersect.Intersection.CurveCurve(Line(verticies[2], Midpoint(verticies[0], verticies[1])), Line(verticies[1], Midpoint(verticies[0], verticies[2])), 0, 0)[0].PointA;
        		case 4:
        			var centroids = new List<Point3d>();
        			
        			centroids.Add(Centroid(Triangle(verticies[0], verticies[1], verticies[2])));
        			centroids.Add(Centroid(Triangle(verticies[1], verticies[2], verticies[3])));
        			centroids.Add(Centroid(Triangle(verticies[2], verticies[3], verticies[0])));
        			centroids.Add(Centroid(Triangle(verticies[3], verticies[0], verticies[1])));
        			
        			return Rhino.Geometry.Intersect.Intersection.CurveCurve(Line(centroids[0], centroids[2]), Line(centroids[1], centroids[3]), 0, 0)[0].PointA;
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
	        						leftMidpoint = Midpoint(Line(leftPoint, topPoint)),
	        						rightMidpoint = Midpoint(Line(rightPoint, topPoint)),
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
	        						leftMidpoint = Midpoint(Line(leftPoint, downPoint)),
	        						rightMidpoint = Midpoint(Line(rightPoint, downPoint)),
	        						topMidpoint = Midpoint(Line(leftPoint, rightPoint)),
	        						newLeft = new Point3d(leftMidpoint.X - side1 * scale, leftMidpoint.Y, 0),
	        						newRight = leftMidpoint,
	        						newDown = new Point3d(downPoint.X - side1 * scale, downPoint.Y, 0);
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
	        				
	        				newLeft = rightMidpoint;
	        				newRight = new Point3d(rightMidpoint.X + side1 * scale, rightMidpoint.Y, 0);
	        				newDown = new Point3d(downPoint.X + side1 * scale, downPoint.Y, 0);
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
	        				
	        				newLeft = new Point3d(topMidpoint.X - side1 * scale, topMidpoint.Y + .5 * (leftPoint.Y - downPoint.Y), 0);
	        				newRight = new Point3d(topMidpoint.X + side1 * scale, topMidpoint.Y + .5 * (leftPoint.Y - downPoint.Y), 0);
	        				newDown = topMidpoint;
	        				
	        				newlyMade.Add(Triangle(newLeft, newRight, newDown));
    					}
    					
    					newTriangles.AddRange(newlyMade);
    					previouslyMade = newlyMade;
    				}
    				else if (i == 2)
    				{
    					newTriangles.Add(Triangle(Midpoint(Line(left, top)), Midpoint(Line(right, top)), Midpoint(Line(left, right))));
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
    				Point3d midpoint = Midpoint(line.PointAtStart, line.PointAtEnd),
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
	    					newLines.Add(Line(point1, Midpoint(line)));
	    					newLines.Add(Line(Midpoint(line), point2));
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
        		   scaleFactor = ScaleFactor(sides);
        	
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
    				Point3d centroid = Centroid(polygon);
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
        	Point3d center = Centroid(Triangle(left, right, top));
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
	        						leftMidpoint = Midpoint(Line(leftPoint, topPoint)),
	        						rightMidpoint = Midpoint(Line(rightPoint, topPoint)),
	        						newLeft = leftPoint,
	        						newRight = new Point3d(leftPoint.X + (right.X * scale), leftPoint.Y, leftPoint.Z),
	        						newTop = leftMidpoint,
	        						newUp = new Point3d(),
	        						newLeft1 = new Point3d(),
	        						newRight1 = new Point3d(),
	        						newTop1 = new Point3d();
	        				center = Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newLeft1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				newLeft = newRight;
	        				newRight = rightPoint;
	        				newTop = rightMidpoint;
	        				center = Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newRight1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				newLeft = leftMidpoint;
	        				newRight = rightMidpoint;
	        				newTop = topPoint;
	        				center = Centroid(Triangle(newLeft, newRight, newTop));
	        				newUp = new Point3d(center.X, center.Y, center.Z + height * scale);
	        				newTop1 = newUp;
	        				
	        				newTetrahedrons.Add(Tetrahedron(newLeft, newRight, newTop, newUp));
	        				
	        				center = Centroid(Triangle(newLeft1, newRight1, newTop1));
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
	        						newDownRight = Midpoint(downBottom, downLeft),
	        						newDownLeft = new Point3d(newDownRight.X - (side1 * scale), newDownRight.Y, newDownRight.Z),
	        						newDownBottom = new Point3d(downBottom.X - (side1 * scale), downBottom.Y, downBottom.Z),
	        						newUpRight = Midpoint(downBottom, upLeft),
	        						newUpLeft = new Point3d(newUpRight.X - (side1 * scale), newUpRight.Y, newUpRight.Z),
	        						newUpTop = Midpoint(downLeft, upLeft);
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownLeft = Midpoint(downBottom, downRight);
	        				newDownRight = new Point3d(newDownLeft.X + (side1 * scale), newDownLeft.Y, newDownLeft.Z);
	        				newDownBottom = new Point3d(downBottom.X + (side1 * scale), downBottom.Y, downBottom.Z);
    						newUpLeft = Midpoint(downBottom, upRight);
    						newUpRight = new Point3d(newUpLeft.X + (side1 * scale), newUpLeft.Y, newUpLeft.Z);
    						newUpTop = Midpoint(downRight, upRight);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownBottom = Midpoint(downLeft, downRight);
	        				newDownLeft = new Point3d(newDownBottom.X - .5 * (downBottom.X - downLeft.X), newDownBottom.Y + .5 * (downLeft.Y - downBottom.Y), newDownBottom.Z);
	        				newDownRight = new Point3d(newDownBottom.X + .5 * (downRight.X - downBottom.X), newDownBottom.Y + .5 * (downRight.Y - downBottom.Y), newDownBottom.Z);
    						newUpLeft = Midpoint(downLeft, upTop);
    						newUpRight = Midpoint(downRight, upTop);
    						newUpTop = new Point3d(newUpLeft.X + .5 * (upTop.X - upLeft.X), newUpLeft.Y + .5 * (upTop.Y - upLeft.Y), newUpLeft.Z);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
	        				
	        				newDownLeft = Midpoint(upLeft, upTop);
	        				newDownRight = Midpoint(upRight, upTop);
	        				newDownBottom = Midpoint(upLeft, upRight);
    						center = Centroid(Triangle(upLeft, upRight, upTop));
    						newUpLeft = Midpoint(center, upLeft);
    						newUpLeft.Z = newUpLeft.Z + (height * scale);
    						newUpRight = Midpoint(center, upRight);
    						newUpRight.Z = newUpRight.Z + (height * scale);
    						newUpTop = Midpoint(center, upTop);
    						newUpTop.Z = newUpTop.Z + (height * scale);
	        				
	        				newlyMade.Add(Octahedron(newDownLeft, newDownRight, newDownBottom, newUpLeft, newUpRight, newUpTop));
    					}
    					
    					newTetrahedrons.AddRange(newlyMade);
    					previouslyMade = newlyMade;
    				}
    				else if (i == 2)
    				{
    					newTetrahedrons.Add(Octahedron(Midpoint(Line(left, top)), Midpoint(Line(right, top)), Midpoint(Line(left, right)), Midpoint(Line(left, up)), Midpoint(Line(right, up)), Midpoint(Line(top, up))));
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
        			center = Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
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
	        						newBottomRight = Midpoint(bottomRightPoint, bottomLeftPoint),
	        						newTopLeft = Midpoint(bottomLeftPoint, topLeftPoint),
	        						newTopRight = Centroid(Quadrilateral(bottomLeftPoint, bottomRightPoint, topLeftPoint, topRightPoint)),
	        						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight)),
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
	        				newTopRight = Midpoint(bottomRightPoint, topRightPoint);
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newBottomRight1 = newUp;
    						
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
	        				newBottomLeft = newTopLeft;
	        				newBottomRight = newTopRight;
	        				newTopLeft = Midpoint(topLeftPoint, topRightPoint);
	        				newTopRight = topRightPoint;
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newTopRight1 = newUp;
	        				
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
	        				newBottomRight = newBottomLeft;
	        				newBottomLeft = Midpoint(topLeftPoint, bottomLeftPoint);
	        				newTopRight = newTopLeft;
	        				newTopLeft = topLeftPoint;
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newUp.Z = newUp.Z + (height * scale);
    						newTopLeft1 = newUp;
	        				
	        				newPyramids.Add(SquarePyramid(newBottomLeft, newBottomRight, newTopLeft, newTopRight, newUp));
	        				
    						newUp = Centroid(Quadrilateral(newBottomLeft1, newBottomRight1, newTopLeft1, newTopRight1));
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
        			center = Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
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
	        						newBottomRight = Midpoint(bottomRightPoint, bottomLeftPoint),
	        						newTopLeft = Midpoint(bottomLeftPoint, topLeftPoint),
	        						newTopRight = Centroid(Quadrilateral(bottomLeftPoint, bottomRightPoint, topLeftPoint, topRightPoint)),
	        						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight)),
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
	        				newTopRight = Midpoint(bottomRightPoint, topRightPoint);
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newBottomRight1 = newUp;
	        				newBottomRight2 = newDown;
    						
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
	        				newBottomLeft = newTopLeft;
	        				newBottomRight = newTopRight;
	        				newTopLeft = Midpoint(topLeftPoint, topRightPoint);
	        				newTopRight = topRightPoint;
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newTopRight1 = newUp;
	        				newTopRight2 = newDown;
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
	        				newBottomRight = newBottomLeft;
	        				newBottomLeft = Midpoint(topLeftPoint, bottomLeftPoint);
	        				newTopRight = newTopLeft;
	        				newTopLeft = topLeftPoint;
    						newUp = Centroid(Quadrilateral(newBottomLeft, newBottomRight, newTopLeft, newTopRight));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				newTopLeft1 = newUp;
	        				newTopLeft2 = newDown;
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft, newTopRight, newDown, newBottomLeft, newBottomRight, newUp));
	        				
    						newUp = Centroid(Quadrilateral(newBottomLeft1, newBottomRight1, newTopLeft1, newTopRight1));
    						newDown = newUp;
	        				newUp.Z = newUp.Z + (upHeight * scale);
	        				newDown.Z = newDown.Z - (downHeight * scale);
	        				
	        				newOctahedrons.Add(Octahedron(newTopLeft1, newTopRight1, newDown, newBottomLeft1, newBottomRight1, newUp));
	        				
    						newUp = Centroid(Quadrilateral(newBottomLeft2, newBottomRight2, newTopLeft2, newTopRight2));
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
        			center = Centroid(Quadrilateral(bottomLeft, bottomRight, topLeft, topRight));
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
        
        internal static List<Curve> DrawLindenmayerSystem(RhinoDoc doc, List<Curve> lines, ref List<Brep> pipes, string step, Turtle turtle, double lineLength, double lineScale, double lineThickness, double nextThickness, double thicknessIncrement, double thicknessScale, double turnAngle, List<string> drawVars, List<string> moveVars, List<string> controlVars, bool drawFractal, ref List<Guid> objects)
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
    					
    					if (!lineThickness.Equals(0) && !nextThickness.Equals(0))
    						objects.Add(doc.Objects.AddBrep(pipes.Last()));
    					
    					doc.Views.Redraw();
    					
    					Graph.Wait(10);
    				}
    			}
    			else if (moveVars.Contains(currentVar))
    				turtle.position += turtle.forward * lineLength;
    			else if (splitSteps[i].Equals("+"))
    				turtle.Yaw(turnAngle);
    			else if (splitSteps[i].Equals("-"))
    				turtle.Yaw(-turnAngle);
    			else if (splitSteps[i].Equals("^"))
    				turtle.Pitch(turnAngle);
    			else if (splitSteps[i].Equals("&"))
    				turtle.Pitch(-turnAngle);
        		else if (splitSteps[i].Equals("@"))
    				turtle.Roll(turnAngle);
        		else if (splitSteps[i].Equals("#"))
    				turtle.Roll(-turnAngle);
        		else if (splitSteps[i].Equals("!"))
        			turnAngle *= -1;
        		else if (splitSteps[i].Equals("|"))
        			turtle.forward *= -1;
    			else if (splitSteps[i].Equals("*"))
    				lineLength *= lineScale;
    			else if (splitSteps[i].Equals("/"))
    				lineLength /= lineScale;
    			else if (splitSteps[i].Equals("<"))
    				nextThickness += thicknessIncrement;
    			else if (splitSteps[i].Equals(">"))
    				nextThickness -= thicknessIncrement;
    			else if (splitSteps[i].Equals("$"))
    				nextThickness *= thicknessScale;
    			else if (splitSteps[i].Equals("%"))
    				nextThickness /= thicknessScale;
    			else if (splitSteps[i].Equals("["))
    			{
    				string newStep = "";
    				var temp = new Turtle();
        			temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);
    				
        			int index = String.IndexOfClosingBracket(splitSteps, i + 1);
        			
    				for(int o = i + 1; o < index - 1; o++)
    					newStep += splitSteps[o];
    				
	    			i = index - 1;
    				
    				lines = DrawLindenmayerSystem(doc, lines, ref pipes, newStep + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
    			}// B*[^A]*{RollArray, 360, 3, [^(30)A]}
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
							
		        			if (new Random((int) DateTime.Now.Ticks & 0x0000FFFF).NextDouble() <= probability)
		        				lines = DrawLindenmayerSystem(doc, lines, ref pipes, arguments[1] + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
						}
						
					}
					if (arguments.Count == 3)
					{
						double probability = 1;
						
						if (Double.TryParse(arguments[0], out probability) && probability >= 0 && probability <= 1)
						{
		    				var temp = new Turtle();
		        			temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);
							
		        			lines = DrawLindenmayerSystem(doc, lines, ref pipes, (new Random((int) DateTime.Now.Ticks & 0x0000FFFF).NextDouble() <= probability ? arguments[1] : arguments[2]) + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
						}
						
					}
					else if (arguments.Count == 4)
					{
						if (arguments[0].Equals("YawArray"))
						{
							double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
							command = "";
							
							for (int e = 0; e < String.GetDouble(arguments[2]); e++)
								command += "+(" + angle + ")" + arguments[3];
							
							var temp = new Turtle();
		        			temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);
							
		        			lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
						}
						else if (arguments[0].Equals("PitchArray"))
						{
							double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
							command = "";
							
							for (int e = 0; e < String.GetDouble(arguments[2]); e++)
								command += "^(" + angle + ")" + arguments[3];
							
							var temp = new Turtle();
		        			temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);
							
		        			lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
						}
						else if (arguments[0].Equals("RollArray"))
						{
							double angle = String.GetDouble(arguments[1]) / String.GetDouble(arguments[2]);
							command = "";
							
							for (int e = 0; e < String.GetDouble(arguments[2]); e++)
								command += "@(" + angle + ")" + arguments[3];
							
							var temp = new Turtle();
		        			temp.Initialize(turtle.position, turtle.forward, turtle.upAxis, turtle.rightAxis, turtle.forwardAxis);
							
		        			lines = DrawLindenmayerSystem(doc, lines, ref pipes, command + " ", temp, lineLength, lineScale, lineThickness, nextThickness, thicknessIncrement, thicknessScale, turnAngle, drawVars, moveVars, controlVars, drawFractal, ref objects);
						}
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
        
        internal static List<Curve> LindenmayerSystem(RhinoDoc doc, ref List<Brep> pipes, int iterations, double lineLength, double lineScale, double lineThickness, double thicknessIncerement, double thicknessScale, double angle, List<string> drawVars, List<string> moveVars, List<string> controlVars, IList<string> drawRules, IList<string> moveRules, IList<string> controlRules, string axiom, Direction drawDirection, bool drawFractal)
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
        	var lines = DrawLindenmayerSystem(doc, new List<Curve>(), ref pipes, steps.Last(), turtle, lineLength, lineScale, lineThickness, lineThickness, thicknessIncerement, thicknessScale, angle, drawVars, moveVars, controlVars, drawFractal, ref objects);
        	
        	Graph.DeleteObjects(doc, objects);
        	
        	return lines;
        }
	}
}