using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SharpAvi.Codecs;
using SharpAvi.Output;

namespace PlotEquation
{
	
    public class MandelbrotFractal : Command
    {
    	public MandelbrotFractal()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static MandelbrotFractal Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "Mandelbrot"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
			const double a1 = 0;
			const double a2 = 0;
			const double b1 = 2;
			const double b2 = -1;
            var c = new Complex(a1,a2);
            var d = new Complex(b1,b2);
            var p = new Quaternion(a1,a2,0,0);
            var q = new Quaternion(b1,b2,0,0);
            var r = new Quaternion(.5,.5,.5,.5);
            
            Output.ToRhinoConsole("Complex -2", Complex.Abs(new Complex(1.70852403534021E+205, -1.555219353676E+205)));
            
//            GetResult get_rc = new GetResult();
//            Result r = new Result();
//            Point3d[] corners;
//            double zoom = 0,
//            	   xMin = -1.7,
//            	   xMax = .7,
//            	   yMin = -1.2,
//            	   yMax = 1.2;
//            
//			
//	            Plane mandelbrot = new Plane(Point3d.Origin, xMax - xMin, yMax - yMin);
//	            
//	            
//	        	r = new Result();
//	        	r = RhinoGet.GetRectangle(out corners);
//	            
//				if (r != Result.Success)
//					return r;
//				
//	        	if (Math.Abs(corners[0].X - corners[1].X) > Math.Abs(corners[2].Y - corners[2].Y))
//	        	{
//	        		
//	        	}
//	        	else
//	        	{
//	        		
//	        	}
        	
//            cx:=cx+scale;
//		    cy:=cy+scale;
//		    scale:=scale/2;
			
            int savedPicWidth = Settings.GetInteger("sPW", 200);
            int savedPicHeight = Settings.GetInteger("sPH", 200);
            int savedIteration = Settings.GetInteger("sI", 500);
            int savedFPS = Settings.GetInteger("sFPS", 30);
            int savedFrames = Settings.GetInteger("sF", 150);
            double savedBailout = Settings.GetDouble("sB", 4);
            double savedPower = Settings.GetDouble("sP", 2);
            double savedXCenter = Settings.GetDouble("sXC", 0);
            double savedYCenter = Settings.GetDouble("sYC", 0);
            double savedRadius = Settings.GetDouble("sR", 2);
            double savedJuliaSetReal = Settings.GetDouble("sJSR", 0);
            double savedJuliaSetImaginary = Settings.GetDouble("sJSI", 0);
            double savedColorSkipRatio = Settings.GetDouble("sCSR", 0);
            double savedMinPower = Settings.GetDouble("sMiP", 0);
            double savedMaxPower = Settings.GetDouble("sMaP", 4);
            bool savedComplexConjugate = Settings.GetBool("sCC", false);
            string savedDestination = Settings.GetString("sD", null);
            FractalType fractalType = Settings.GetEnumValue("sFT", FractalType.Mandelbrot);
            ColoringMethod coloringMethod = Settings.GetEnumValue("sCM", ColoringMethod.Exterior);
            DomainColoring domainColoring = Settings.GetEnumValue("sDC", DomainColoring.Normal);
            OrbitTrap orbitTrap = Settings.GetEnumValue("sOT", OrbitTrap.Circle);
            
            List<string> colorPalettes = new List<string>();
            List<string> animations = new List<string>();
            List<string> imgFormats = new List<string>();
            List<string> vidFormats = new List<string>();
            List<string> rhinoExports = new List<string>();
            int ftIndex =(int) fractalType;
            int cmIndex = (int) coloringMethod;
            int dcIndex = (int) domainColoring;
            int otIndex = (int) orbitTrap;
            int cpIndex = 0;
            int imgIndex = 0;
            int aniIndex = 0;
            int vidIndex = 0;
            int eIndex = 0;
            
            colorPalettes.Add("ROYGBV");
            colorPalettes.Add("RGB");
            colorPalettes.Add("Monochrome");
            colorPalettes.Add("White");
            animations.Add("NONE");
//            animations.Add("Color_Change");
            animations.Add("Power_Change");
            animations.Add("Iteration_Change");
            animations.Add("Julia_Set_Unit_Circle_Revolution");
            imgFormats.Add(".bmp");
            imgFormats.Add(".ico");
            imgFormats.Add(".png");
            imgFormats.Add("NONE");
            vidFormats.Add(".avi");
//            vidFormats.Add(".flv");
//            vidFormats.Add(".mov");
//            vidFormats.Add(".mp4");
            rhinoExports.Add("NONE");
            rhinoExports.Add("BorderPoints");
            rhinoExports.Add("BorderCurves");
            rhinoExports.Add("InteriorPoints");
            rhinoExports.Add("ExteriorPoints");
            rhinoExports.Add("InteriorSurfaces");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
			
            // Set up options
            OptionInteger picWidth = new OptionInteger(savedPicWidth, 1, 10000);
            OptionInteger picHeight = new OptionInteger(savedPicHeight, 1, 10000);
            OptionInteger iteration = new OptionInteger(savedIteration, 1, 50000);
            OptionDouble bailout = new OptionDouble(savedBailout, 1, 10e99);
            OptionDouble power = new OptionDouble(savedPower, -1000, 1000);
            OptionDouble xCenter = new OptionDouble(savedXCenter, -10, 10);
            OptionDouble yCenter = new OptionDouble(savedYCenter, -9999, 9999);
            OptionDouble radius = new OptionDouble(savedRadius, .000000000000001, 10);
            OptionDouble juliaR = new OptionDouble(savedJuliaSetReal, -10, 10);
            OptionDouble juliaI = new OptionDouble(savedJuliaSetImaginary, -10, 10);
            OptionDouble skipRatio = new OptionDouble(savedColorSkipRatio, 0, 1);
        	OptionToggle complexConjugate = new OptionToggle(savedComplexConjugate, "False", "True");
        	OptionToggle pixelated = new OptionToggle(true, "False", "True");
            
        	getOption.AcceptNothing(true);
            getOption.EnableTransparentCommands(true);
            getOption.SetCommandPrompt("Mandelbrot Options");
            var ftOption = getOption.AddOptionEnumList("Type", fractalType);
            getOption.AddOptionDouble("Power", ref power);
            getOption.AddOptionToggle("Complex_Conjugate", ref complexConjugate);
            var cmOption = getOption.AddOptionEnumList("Coloring", coloringMethod);
            var dcOption = getOption.AddOptionEnumList("Domain_Options", domainColoring);
            var cpOption = getOption.AddOptionList("Colors", colorPalettes, cpIndex);
            getOption.AddOptionDouble("Color_Skip_Ratio", ref skipRatio);
            var otOption = getOption.AddOptionEnumList("Orbit_Trap", orbitTrap);
        	
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
                
                if (getOption.OptionIndex() == ftOption)
                    ftIndex = getOption.Option().CurrentListOptionIndex;
                if (getOption.OptionIndex() == cmOption)
                    cmIndex = getOption.Option().CurrentListOptionIndex;
                if (getOption.OptionIndex() == dcOption)
                    dcIndex = getOption.Option().CurrentListOptionIndex;
                if (getOption.OptionIndex() == cpOption)
                    cpIndex = getOption.Option().CurrentListOptionIndex;
                if (getOption.OptionIndex() == otOption)
                    otIndex = getOption.Option().CurrentListOptionIndex;
            }
            
            fractalType = (FractalType) ftIndex;
            coloringMethod = (ColoringMethod) cmIndex;
            domainColoring = (DomainColoring) dcIndex;
            orbitTrap = (OrbitTrap) otIndex;
            
            if (get_rc != GetResult.Cancel && getOption.CommandResult() == Result.Success)
            {
            	getOption.ClearCommandOptions();
	        	getOption.AcceptNothing(true);
	            getOption.EnableTransparentCommands(true);
            	getOption.SetCommandPrompt("Fractal Options");
            	getOption.AddOptionDouble("X_Center", ref xCenter);
            	getOption.AddOptionDouble("Y_Center", ref yCenter);
            	getOption.AddOptionDouble("Radius", ref radius);
            	getOption.AddOptionDouble("Bailout", ref bailout);
            	getOption.AddOptionDouble("JuliaSet_R", ref juliaR);
            	getOption.AddOptionDouble("JuliaSet_I", ref juliaI);
            	getOption.AddOptionInteger("Max_Iteration", ref iteration);
            	
	            while (get_rc != GetResult.Cancel)
	            {
	                // perform the get operation. This will prompt the user to input a string, but also
	                // allow for command line options defined above
					
	                get_rc = getOption.Get();
	                
	                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
	                	break;
	            }
	            
	            if (get_rc != GetResult.Cancel && getOption.CommandResult() == Result.Success)
            	{
	            	getOption.ClearCommandOptions();
		        	getOption.AcceptNothing(true);
		            getOption.EnableTransparentCommands(true);
            		getOption.SetCommandPrompt("Save Options");
	            	var dOption = getOption.AddOption("Destination", savedDestination);
            		getOption.AddOptionInteger("Width", ref picWidth);
            		getOption.AddOptionInteger("Height", ref picHeight);
		            var imgOption = getOption.AddOptionList("Image_Extension", imgFormats, imgIndex);
		            var aniOption = getOption.AddOptionList("Animation", animations, aniIndex);
		            var vidOption = getOption.AddOptionList("Video_Extension", vidFormats, vidIndex);
		            var eOption = getOption.AddOptionList("Export", rhinoExports, eIndex);
            		getOption.AddOptionToggle("Pixelated_Export", ref pixelated);
	            	
		            while (get_rc != GetResult.Cancel)
		            {
		                // perform the get operation. This will prompt the user to input a string, but also
		                // allow for command line options defined above
						
		                get_rc = getOption.Get();
		                
		                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
		                {
		                	if (get_rc == GetResult.Nothing && savedDestination == null)
		                	{
		                		Output.ToRhinoConsole("Please select destination directory.");
		                		continue;
		                	}
		                	
		                	break;
		                }
		                
		                if (getOption.OptionIndex() == dOption)
		                {
		            		using (var folderDialog = new FolderBrowserDialog())
		            		{
					            if (folderDialog.ShowDialog() == DialogResult.OK)
					            	savedDestination = folderDialog.SelectedPath;
		            		}
		            		
			            	getOption.ClearCommandOptions();
				        	getOption.AcceptNothing(true);
				            getOption.EnableTransparentCommands(true);
		            		getOption.SetCommandPrompt("Save Options");
			            	dOption = getOption.AddOption("Destination", savedDestination);
		            		getOption.AddOptionInteger("Width", ref picWidth);
		            		getOption.AddOptionInteger("Height", ref picHeight);
				            imgOption = getOption.AddOptionList("Image_Extension", imgFormats, imgIndex);
				            vidOption = getOption.AddOptionList("Video_Extension", vidFormats, vidIndex);
				            eOption = getOption.AddOptionList("Rhino_Export", rhinoExports, eIndex);
		            		getOption.AddOptionToggle("Pixelated_Export", ref pixelated);
		                }
		                else if (getOption.OptionIndex() == imgOption)
		                    imgIndex = getOption.Option().CurrentListOptionIndex;
		                else if (getOption.OptionIndex() == aniOption)
		                    aniIndex = getOption.Option().CurrentListOptionIndex;
		                else if (getOption.OptionIndex() == vidOption)
		                    vidIndex = getOption.Option().CurrentListOptionIndex;
		                else if (getOption.OptionIndex() == eOption)
		                    eIndex = getOption.Option().CurrentListOptionIndex;
		            }
		            
		            if (get_rc != GetResult.Cancel && getOption.CommandResult() == Result.Success)
		            {
		            	FractalType ft = (FractalType) ftIndex;
		            	ColoringMethod cm = (ColoringMethod) cmIndex;
		            	DomainColoring dc = (DomainColoring) dcIndex;
		            	OrbitTrap ot = (OrbitTrap) otIndex;
		            	ImageFormat imgFormat;
		            	var colors = new List<Color>();
		            	
		            	switch (imgFormats[imgIndex])
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
		            	switch (colorPalettes[cpIndex])
		            	{
		            		case "ROYGBV":
		            			colors = ColorPalette.Rainbow(skipRatio.CurrentValue);
		            			break;
		            		case "RGB":
		            			colors = ColorPalette.RGB(skipRatio.CurrentValue);
		            			break;
		            		case "Monochrome":
		            			colors = ColorPalette.Monochrome(skipRatio.CurrentValue);
		            			break;
		            		default:
		            			colors = ColorPalette.OneColor(Color.White);
		            			break;
		            	}
		            	
		            	var mandelbrot = new Mandelbrot();
		            	mandelbrot.Initialize(picWidth.CurrentValue, picHeight.CurrentValue, xCenter.CurrentValue, yCenter.CurrentValue, radius.CurrentValue, iteration.CurrentValue, bailout.CurrentValue, power.CurrentValue, complexConjugate.CurrentValue, ft, cm, dc, colors, ot);
		            	
		            	if (animations[aniIndex] != "NONE")
		            	{
		            		List<Bitmap> frames = new List<Bitmap>();
		            		OptionInteger fps = new OptionInteger(30, 1, 180);
		            		OptionInteger frameNumber = new OptionInteger(150, 1, 3000);
		            		
		            		switch(animations[aniIndex])
		            		{
		            			case "Julia_Set_Unit_Circle_Revolution":
		            				
		            				getOption.ClearCommandOptions();
						        	getOption.AcceptNothing(true);
						            getOption.EnableTransparentCommands(true);
				            		getOption.SetCommandPrompt("Julia Set Unit Circle Revolution Options");
				            		getOption.AddOptionInteger("Frames_Per_Second", ref fps);
				            		getOption.AddOptionInteger("Number_Of_Frames", ref frameNumber);
					            	getOption.AddOptionDouble("JuliaSet_R", ref juliaR);
					            	getOption.AddOptionDouble("JuliaSet_I", ref juliaI);
				            		
				            		while (get_rc != GetResult.Cancel)
						            {
						                // perform the get operation. This will prompt the user to input a string, but also
						                // allow for command line options defined above
										
						                get_rc = getOption.Get();
						                
						                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
						                	break;
						            }
				            		
				            		frames = mandelbrot.JuliaSetUnitCircle(frameNumber.CurrentValue, new Complex(juliaR.CurrentValue, juliaI.CurrentValue).Radius);
				            		
		            				break;
		            			default:
		            				OptionDouble minPower = new OptionDouble(0, -1000, 1000);
		            				OptionDouble maxPower = new OptionDouble(4, -1000, 1000);
		            				
		            				getOption.ClearCommandOptions();
						        	getOption.AcceptNothing(true);
						            getOption.EnableTransparentCommands(true);
				            		getOption.SetCommandPrompt("Power Change Options");
				            		getOption.AddOptionInteger("Frames_Per_Second", ref fps);
				            		getOption.AddOptionInteger("Number_Of_Frames", ref frameNumber);
				            		getOption.AddOptionDouble("Start_Power", ref minPower);
				            		getOption.AddOptionDouble("End_Power", ref maxPower);
				            		
				            		while (get_rc != GetResult.Cancel)
						            {
						                // perform the get operation. This will prompt the user to input a string, but also
						                // allow for command line options defined above
										
						                get_rc = getOption.Get();
						                
						                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
						                	break;
						            }
				            		
				            		frames = mandelbrot.PowerChange(frameNumber.CurrentValue, minPower.CurrentValue, maxPower.CurrentValue, new Complex(juliaR.CurrentValue, juliaI.CurrentValue));
				            		
		            				break;
		            		}
		            		
		            		if (get_rc != GetResult.Cancel)
		            		{
			            		string juliaSet = !juliaR.CurrentValue.Equals(0) && !juliaI.CurrentValue.Equals(0) && animations[aniIndex] == "Julia_Set_Unit_Circle_Revolution" ? " Julia Set At (" + juliaR.CurrentValue + "," + juliaI.CurrentValue + ")" : "Julia Set Unit Circle Revolution with Radius " + new Complex(juliaR.CurrentValue, juliaI.CurrentValue).Radius;
			            		string complexConj = complexConjugate.CurrentValue ? "Conjugate of " : "";
			            		string anim = animations[aniIndex] != "Julia_Set_Unit_Circle_Revolution" ? animations[aniIndex] + " of " : "";
			            		string fileName = anim + complexConj + ft + juliaSet + " - " + cm + dc + " Coloring - " + ot + " OrbitTrap - " + "Centered At (" + xCenter.CurrentValue + "," + yCenter.CurrentValue + ") With Radius " + radius.CurrentValue;
			            		
			            		var writer = new AviWriter(@savedDestination + "\\" + fileName + vidFormats[vidIndex]) { FramesPerSecond = fps.CurrentValue, EmitIndex1 = true};
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
		            		}
		            	}
		            	
		            	if (imgFormats[imgIndex] != "NONE")
		            	{
		            		Output.ToRhinoConsole("Generating Digital Image...");
		            		
		            		Bitmap fractal = mandelbrot.Generate(new Complex(juliaR.CurrentValue, juliaI.CurrentValue));
		            		string juliaSet = !juliaR.CurrentValue.Equals(0) && !juliaI.CurrentValue.Equals(0) ? " Julia Set At (" + juliaR.CurrentValue + "," + juliaI.CurrentValue + ")" : "";
		            		string complexConj = complexConjugate.CurrentValue ? "Conjugate of " : "";
		            		
		            		try
		            		{
		            			fractal.Save(@savedDestination + "\\" + complexConj + ft + juliaSet + " - " + cm + " Coloring - " + ot + " OrbitTrap - " + "Centered At (" + xCenter.CurrentValue + "," + yCenter.CurrentValue + ") With Radius " + radius.CurrentValue + imgFormats[imgIndex], imgFormat);
		            			Output.ToRhinoConsole("Success");
		            		}
		            		catch (Exception e)
		            		{
		            			Output.Here(e.Message);
		            		}
		            	}
		            	
		            	if (rhinoExports[eIndex] != "NONE")
		            	{
		            		mandelbrot.colors = ColorPalette.OneColor(Color.White);
		            		
		            		Bitmap fractal = mandelbrot.Generate(new Complex(juliaR.CurrentValue, juliaI.CurrentValue));
		            		List<Brep> breps = new List<Brep>();
		            		List<Curve> curves = new List<Curve>();
		            		List<Point3d> points = new List<Point3d>();
		            		
		            		switch (rhinoExports[eIndex])
				            {
				            	case "BorderPoints":
				            		points = Mandelbrot.BorderToPoints(fractal);
				            		
	            					foreach(Point3d point in points)
										doc.Objects.AddPoint(point);
				            		
				            		break;
				            	case "BorderCurves":
				            		curves = Mandelbrot.BorderToCurves(fractal, pixelated.CurrentValue);
				            		
	            					foreach(Curve curve in curves)
										doc.Objects.AddCurve(curve);
				            		
				            		break;
				            	case "InteriorPoints":
				            		points = Mandelbrot.InteriorToPoints(fractal);
				            		
	            					foreach(Point3d point in points)
										doc.Objects.AddPoint(point);
				            		
				            		break;
				            	case "ExteriorPoints":
				            		points = Mandelbrot.InteriorToPoints(fractal);
				            		
	            					foreach(Point3d point in points)
										doc.Objects.AddPoint(point);
				            		
				            		break;
				            	default:
				            		breps = Mandelbrot.InteriorToSurfaces(fractal, pixelated.CurrentValue);
				            		
	            					foreach(Brep brep in breps)
										doc.Objects.AddBrep(brep);
									
				            		break;
				            }
		            	}
		            }
	            }
            }
            
            Settings.SetInteger("sPW", picWidth.CurrentValue);
            Settings.SetInteger("sPH", picHeight.CurrentValue);
            Settings.SetInteger("sI", iteration.CurrentValue);
            Settings.SetDouble("sB", bailout.CurrentValue);
            Settings.SetDouble("sP", power.CurrentValue);
            Settings.SetDouble("sXC", xCenter.CurrentValue);
            Settings.SetDouble("sYC", yCenter.CurrentValue);
            Settings.SetDouble("sR", radius.CurrentValue);
            Settings.SetDouble("sJSR", juliaR.CurrentValue);
            Settings.SetDouble("sJSI", juliaI.CurrentValue);
            Settings.SetDouble("sCSR", skipRatio.CurrentValue);
            Settings.SetString("sD", savedDestination);
            Settings.SetBool("sCC", complexConjugate.CurrentValue);
            Settings.SetEnumValue("sFT", fractalType);
            Settings.SetEnumValue("sCM", coloringMethod);
            Settings.SetEnumValue("sDC", domainColoring);
            Settings.SetEnumValue("sOT", orbitTrap);
			
//			Complex juliaC = new Complex(0.285, 0.01);
//			Complex juliaC = new Complex(1, 0.4);
//			Complex juliaC = new Complex(-0.7269, 0.1889);
//			Complex juliaC = new Complex(0, 0);
			
			
//			// Julia Set Revolutions
//			
//			
//			List<Bitmap> frames = new List<Bitmap>();
//            
//			const double iteration = .005;
////			const double r = Math.PI / 4;
//			const double r = 1.077;
//			const double min = 0;
//			const double max = 2*Math.PI;
			
			
            // Power Change
            
//            Mandelbrot.FractalPowerChange(destination, "Burning Ship", 2, -4, 4, .5, ColorPalette.RGB(), false, 0, 0, 2, 100);
//            Mandelbrot.FractalPowerChange(destination, "Julia Set", 2, -4, 4, .05, ColorPalette.RGB(), false, 0, 0, 2, 100);





            // Zoom
            
//            List<Bitmap> frames = new List<Bitmap>();
//			const double frameCount = 450;
//            
//			Complex c = new Complex(0,0);
//			Complex w = new Complex(-.551302103055373, -.556000000023553);
//			double rc = 2;
//			double rw = .00000000000001;
//			
//			double rScale = Math.Pow(rw, 1 / frameCount);
//			double xScale = (c.R - w.R) / frameCount;
//			double yScale = (c.I - w.I) / frameCount;
//			
//            for (int i = 0; i < frameCount; i++)
//            {
//            	frames.Add(Mandelbrot.Mandelbrot(ColorPalette.RGB(), 2, false, w.R, w.I, rc, 200 + 6*i));Output.Here(i);RhinoApp.Wait();
//            	
//            	rc *= rScale;
////            	c.R -= xScale;
////            	c.I -= yScale;Output.ToRhinoConsole("c", c);Output.ToRhinoConsole("r", rc);
//            }
//            var writer = new AviWriter(@destination + "Zoom.avi") { FramesPerSecond = 30, EmitIndex1 = true};
//			var encoder = new MotionJpegVideoEncoderWpf(frames[0].Size.Width, frames[0].Size.Height, 100);
//			var stream = writer.AddEncodingVideoStream(encoder);
//			
//            stream.Width = frames[0].Size.Width;
//			stream.Height = frames[0].Size.Height;
//			
//			var buffer = new byte[stream.Width * stream.Height * 4];
//			
//			for (int i = 0; i < frames.Count; i++)
//			{
//			    var bits = frames[i].LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
//				System.Runtime.InteropServices.Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
//			    stream.WriteFrame(true, buffer.ToArray(), 0, buffer.Length);
//			    
//			    Output.ToRhinoConsole("Saved Frame " + (i + 1) + "/" + frames.Count + " in Video");
//			    RhinoApp.Wait();
//			}
//			
//			writer.Close();
						
			
			
			
			
			
            //Color Change
            
//            List<Bitmap> frames = new List<Bitmap>();
//            
//            for (int i = 1535; i > 0; i--)
//            {Output.Here(i);RhinoApp.Wait();
//            	frames.Add(Mandelbrot.JuliaSet(ColorPalette.RGB(i), 2, juliaC,  0, 0, 2));
//            }
//            var writer = new AviWriter(@destination + "ColorChange.avi") { FramesPerSecond = 90, EmitIndex1 = true};
//			var encoder = new MotionJpegVideoEncoderWpf(frames[0].Size.Width, frames[0].Size.Height, 100);
//			var stream = writer.AddEncodingVideoStream(encoder);
//			
//            stream.Width = frames[0].Size.Width;
//			stream.Height = frames[0].Size.Height;
//			
//			var buffer = new byte[stream.Width * stream.Height * 4];
//			
//			for (int i = 0; i < frames.Count; i++)
//			{
//			    var bits = frames[i].LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
//				System.Runtime.InteropServices.Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
//			    stream.WriteFrame(true, buffer.ToArray(), 0, buffer.Length);
//			    
//			    Output.ToRhinoConsole("Saved Frame " + (i + 1) + "/" + frames.Count + " in Video");
//			    RhinoApp.Wait();
//			}
//			
//			writer.Close();
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class SierpinksiTriangle : Command
    {
    	public SierpinksiTriangle()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static SierpinksiTriangle Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SierpinskiTriangle"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 7);
            double savedBottomSideLength = Settings.GetDouble("sBSL", 10);
            double savedLeftSideLength = Settings.GetDouble("sLSL", 10);
            double savedRightSideLength = Settings.GetDouble("sRSL", 10);
            bool savedInverse = Settings.GetBool("sIn", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble bottomSideLength = new OptionDouble(savedBottomSideLength, .0001, 10000);
            OptionDouble leftSideLength = new OptionDouble(savedLeftSideLength, .0001, 10000);
            OptionDouble rightSideLength = new OptionDouble(savedRightSideLength, .0001, 10000);
            OptionToggle inverse = new OptionToggle(savedInverse, "False", "True");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Sierpinski Triangle");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Bottom_Side_Length", ref bottomSideLength);
            getOption.AddOptionDouble("Left_Side_Length", ref leftSideLength);
            getOption.AddOptionDouble("Right_Side_Length", ref rightSideLength);
            getOption.AddOptionToggle("Complement", ref inverse);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.SierpinskiTriangle(iterations.CurrentValue, bottomSideLength.CurrentValue, leftSideLength.CurrentValue, rightSideLength.CurrentValue, inverse.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sBSL", bottomSideLength.CurrentValue);
            Settings.SetDouble("sRSL", rightSideLength.CurrentValue);
            Settings.SetDouble("sLSL", leftSideLength.CurrentValue);
            Settings.SetBool("sIn", inverse.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class KochCurve : Command
    {
    	public KochCurve()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static KochCurve Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "KochCurve"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 5);
            double savedSideLength = Settings.GetDouble("sSL", 5);
            double savedAngle = Settings.GetDouble("sA", 60);
            int savedCurveTypeIndex = Settings.GetInteger("sCT", 0);
            var curveTypes = new List<string>();
            var curveTypesInt = new List<string>();
            
            curveTypes.Add("I");
            curveTypes.Add("II");
            curveTypes.Add("III");
            curveTypesInt.Add("1");
            curveTypesInt.Add("2");
            curveTypesInt.Add("3");
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble sideLength = new OptionDouble(savedSideLength, .0001, 10000);
            OptionDouble angle = new OptionDouble(savedAngle, 1, 90);
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Koch Curve");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Side_Length", ref sideLength);
            getOption.AddOptionDouble("Angle", ref angle);
            var ctOption = getOption.AddOptionList("Curve_Type", curveTypes, savedCurveTypeIndex);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
                if (get_rc == GetResult.Nothing)
                {
                	List<Curve> fractal = Generate.KochSnowflake(iterations.CurrentValue, 3, sideLength.CurrentValue, angle.CurrentValue, Convert.ToInt32(curveTypesInt[savedCurveTypeIndex]), false);
		            
//                	if (savedCurveTypeIndex != 2)
//                		fractal = Curve.JoinCurves(fractal).ToList();
                	
		            foreach (Curve line in fractal)
		            	doc.Objects.AddCurve(line);
                	
		            break;
                }
                if (getOption.OptionIndex() == ctOption)
                    savedCurveTypeIndex = getOption.Option().CurrentListOptionIndex;
            }
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sSL", sideLength.CurrentValue);
            Settings.SetDouble("sA", angle.CurrentValue);
            Settings.SetInteger("sCT", savedCurveTypeIndex);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class KochSnowflake : Command
    {
    	public KochSnowflake()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static KochSnowflake Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "KochSnowflake"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 5);
            int savedSideNumber = Settings.GetInteger("sSN", 3);
            double savedSideLength = Settings.GetDouble("sSL", 5);
            double savedAngle = Settings.GetDouble("sA", 60);
            int savedCurveTypeIndex = Settings.GetInteger("sCT", 0);
            var curveTypes = new List<string>();
            var curveTypesInt = new List<string>();
            
            curveTypes.Add("I");
            curveTypes.Add("II");
            curveTypes.Add("III");
            curveTypesInt.Add("1");
            curveTypesInt.Add("2");
            curveTypesInt.Add("3");
            
            curveTypes.Add("1");
            curveTypes.Add("2");
            curveTypes.Add("3");
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionInteger sideNumber = new OptionInteger(savedSideNumber, 3, 15);
            OptionDouble sideLength = new OptionDouble(savedSideLength, .0001, 10000);
            OptionDouble angle = new OptionDouble(savedAngle, 1, 90);
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Koch Snowflake");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionInteger("Number_of_Sides", ref sideNumber);
            getOption.AddOptionDouble("Side_Length", ref sideLength);
            getOption.AddOptionDouble("Angle", ref angle);
            var ctOption = getOption.AddOptionList("Curve_Type", curveTypes, savedCurveTypeIndex);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
                if (get_rc == GetResult.Nothing)
                {
                	List<Curve> fractal = Generate.KochSnowflake(iterations.CurrentValue, sideNumber.CurrentValue, sideLength.CurrentValue, angle.CurrentValue, Convert.ToInt32(curveTypesInt[savedCurveTypeIndex]));
		            
                	if (savedCurveTypeIndex != 2)
                		fractal = Curve.JoinCurves(fractal).ToList();
                	
		            foreach (Curve line in fractal)
		            	doc.Objects.AddCurve(line);
                	
		            break;
                }
                if (getOption.OptionIndex() == ctOption)
                    savedCurveTypeIndex = getOption.Option().CurrentListOptionIndex;
            }
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetInteger("sSN", sideNumber.CurrentValue);
            Settings.SetDouble("sSL", sideLength.CurrentValue);
            Settings.SetDouble("sA", angle.CurrentValue);
            Settings.SetInteger("sCT", savedCurveTypeIndex);
			
            // ---
			
            return Result.Success;
        }
    }
    
    public class SierpinksiNGon : Command
    {
    	public SierpinksiNGon()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static SierpinksiNGon Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SierpinskiNGon"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 5);
            int savedSideNumber = Settings.GetInteger("sSN", 3);
            double savedRadius = Settings.GetDouble("sR", 5);
            bool savedCentralPolygon = Settings.GetBool("sCP", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionInteger sideNumber = new OptionInteger(savedSideNumber, 3, 15);
            OptionDouble radius = new OptionDouble(savedRadius, .0001, 10000);
            OptionToggle centralPolygon = new OptionToggle(savedCentralPolygon, "No", "Yes");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Sierpinski n-gon");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionInteger("Number_of_Sides", ref sideNumber);
            getOption.AddOptionDouble("Radius", ref radius);
            getOption.AddOptionToggle("Central_Polygon", ref centralPolygon);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.SierpinskiNGon(iterations.CurrentValue, sideNumber.CurrentValue, radius.CurrentValue, centralPolygon.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetInteger("sSN", sideNumber.CurrentValue);
            Settings.SetDouble("sR", radius.CurrentValue);
            Settings.SetBool("sCP", centralPolygon.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class SierpinksiTetrahedron : Command
    {
    	public SierpinksiTetrahedron()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static SierpinksiTetrahedron Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SierpinskiTetrahedron"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 3);
            double savedBottomSideLength = Settings.GetDouble("sBSL", 10);
            double savedLeftSideLength = Settings.GetDouble("sLSL", 10);
            double savedRightSideLength = Settings.GetDouble("sRSL", 10);
            double savedHeight = Settings.GetDouble("sH", 8.1649658);
            bool savedInverse = Settings.GetBool("sIn", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble bottomSideLength = new OptionDouble(savedBottomSideLength, .0001, 10000);
            OptionDouble leftSideLength = new OptionDouble(savedLeftSideLength, .0001, 10000);
            OptionDouble rightSideLength = new OptionDouble(savedRightSideLength, .0001, 10000);
            OptionDouble height = new OptionDouble(savedHeight, .0001, 10000);
            OptionToggle inverse = new OptionToggle(savedInverse, "False", "True");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Sierpinski Tetrahedron");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Bottom_Side_Length", ref bottomSideLength);
            getOption.AddOptionDouble("Left_Side_Length", ref leftSideLength);
            getOption.AddOptionDouble("Right_Side_Length", ref rightSideLength);
            getOption.AddOptionDouble("Height", ref height);
            getOption.AddOptionToggle("Complement", ref inverse);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.SierpinskiTetrahedron(iterations.CurrentValue, bottomSideLength.CurrentValue, leftSideLength.CurrentValue, rightSideLength.CurrentValue, height.CurrentValue, inverse.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sBSL", bottomSideLength.CurrentValue);
            Settings.SetDouble("sRSL", rightSideLength.CurrentValue);
            Settings.SetDouble("sLSL", leftSideLength.CurrentValue);
            Settings.SetDouble("sH", height.CurrentValue);
            Settings.SetBool("sIn", inverse.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class SierpinksiPyramid : Command
    {
    	public SierpinksiPyramid()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static SierpinksiPyramid Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SierpinskiPyramid"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 3);
            double savedHorizontalSideLength = Settings.GetDouble("sHSL", 10);
            double savedVerticalSideLength = Settings.GetDouble("sVSL", 10);
            double savedAngle = Settings.GetDouble("sA", 90);
            double savedHeight = Settings.GetDouble("sH", 7.0710678);
            bool savedInverse = Settings.GetBool("sIn", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble horizontalSideLength = new OptionDouble(savedHorizontalSideLength, .0001, 10000);
            OptionDouble verticalSideLength = new OptionDouble(savedVerticalSideLength, .0001, 10000);
            OptionDouble angle = new OptionDouble(savedAngle, 10, 170);
            OptionDouble height = new OptionDouble(savedHeight, .0001, 10000);
            OptionToggle inverse = new OptionToggle(savedInverse, "False", "True");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Sierpinski Pyramid");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Horizontal_Side_Length", ref horizontalSideLength);
            getOption.AddOptionDouble("Vartical_Side_Length", ref verticalSideLength);
            getOption.AddOptionDouble("Angle", ref angle);
            getOption.AddOptionDouble("Height", ref height);
            getOption.AddOptionToggle("Complement", ref inverse);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.SierpinskiPyramid(iterations.CurrentValue, horizontalSideLength.CurrentValue, verticalSideLength.CurrentValue, angle.CurrentValue, height.CurrentValue, inverse.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sHSL", horizontalSideLength.CurrentValue);
            Settings.SetDouble("sVSL", verticalSideLength.CurrentValue);
            Settings.SetDouble("sA", angle.CurrentValue);
            Settings.SetDouble("sH", height.CurrentValue);
            Settings.SetBool("sIn", inverse.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class SierpinksiOctahedron : Command
    {
    	public SierpinksiOctahedron()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static SierpinksiOctahedron Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "SierpinskiOctahedron"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 3);
            double savedHorizontalSideLength = Settings.GetDouble("sHSL", 10);
            double savedVerticalSideLength = Settings.GetDouble("sVSL", 10);
            double savedAngle = Settings.GetDouble("sA", 90);
            double savedTopHeight = Settings.GetDouble("sTH", 7.0710678);
            double savedBottomHeight = Settings.GetDouble("sBH", 7.0710678);
            bool savedInverse = Settings.GetBool("sIn", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble horizontalSideLength = new OptionDouble(savedHorizontalSideLength, .0001, 10000);
            OptionDouble verticalSideLength = new OptionDouble(savedVerticalSideLength, .0001, 10000);
            OptionDouble angle = new OptionDouble(savedAngle, .0001, 10000);
            OptionDouble topHeight = new OptionDouble(savedTopHeight, .0001, 10000);
            OptionDouble bottomHeight = new OptionDouble(savedBottomHeight, .0001, 10000);
            OptionToggle inverse = new OptionToggle(savedInverse, "False", "True");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("Sierpinski Octahedron");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Horizontal_Side_Length", ref horizontalSideLength);
            getOption.AddOptionDouble("Vertical_Side_Length", ref verticalSideLength);
            getOption.AddOptionDouble("Angle", ref angle);
            getOption.AddOptionDouble("Top_Height", ref topHeight);
            getOption.AddOptionDouble("Bottom_Height", ref bottomHeight);
            getOption.AddOptionToggle("Complement", ref inverse);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.SierpinskiOctahedron(iterations.CurrentValue, horizontalSideLength.CurrentValue, verticalSideLength.CurrentValue, angle.CurrentValue, topHeight.CurrentValue, bottomHeight.CurrentValue, inverse.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sHSL", horizontalSideLength.CurrentValue);
            Settings.SetDouble("sVSL", verticalSideLength.CurrentValue);
            Settings.SetDouble("sA", angle.CurrentValue);
            Settings.SetDouble("sTH", topHeight.CurrentValue);
            Settings.SetDouble("sBH", bottomHeight.CurrentValue);
            Settings.SetBool("sIn", inverse.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class KochSnowflake3D : Command
    {
    	public KochSnowflake3D()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static KochSnowflake3D Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "3DKochSnowflake"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            int savedIteration = Settings.GetInteger("sI", 3);
            double savedWidth = Settings.GetDouble("sW", 10);
            double savedLength = Settings.GetDouble("sL", 10);
            double savedHeight = Settings.GetDouble("sH", 10);
            double savedAngle = Settings.GetDouble("sA", 90);
            bool savedCentralPolygon = Settings.GetBool("sCP", false);
            bool savedInverse = Settings.GetBool("sIn", false);
            bool cancel = false;
            
            OptionInteger iterations = new OptionInteger(savedIteration, 1, 15);
            OptionDouble width = new OptionDouble(savedWidth, .0001, 10000);
            OptionDouble length = new OptionDouble(savedLength, .0001, 10000);
            OptionDouble height = new OptionDouble(savedHeight, .0001, 10000);
            OptionDouble angle = new OptionDouble(savedAngle, .0001, 10000);
            OptionToggle centralPolygon = new OptionToggle(savedCentralPolygon, "False", "True");
            OptionToggle inverse = new OptionToggle(savedInverse, "False", "True");
            
            GetOption getOption = new GetOption();
            GetResult get_rc = new GetResult();
            
            getOption.AcceptNothing(true);
            getOption.SetCommandPrompt("3D Koch Snowflake Cube Method");
            getOption.AddOptionInteger("Iterations", ref iterations);
            getOption.AddOptionDouble("Length", ref length);
            getOption.AddOptionDouble("Width", ref width);
            getOption.AddOptionDouble("Height", ref height);
            getOption.AddOptionDouble("Angle", ref angle);
            getOption.AddOptionToggle("Central_Polygon", ref centralPolygon);
            getOption.AddOptionToggle("Complement", ref inverse);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getOption.Get();
                
				cancel |= getOption.CommandResult() == Result.Cancel;
                
                if (getOption.CommandResult() != Result.Success || get_rc == GetResult.Nothing)
                	break;
            }
            
            List<Brep> fractal = Generate.KochSnowflake3D_CubeMethod(iterations.CurrentValue, length.CurrentValue, width.CurrentValue, height.CurrentValue, angle.CurrentValue, centralPolygon.CurrentValue, inverse.CurrentValue);
            
            foreach (Brep triangle in fractal)
            	doc.Objects.AddBrep(triangle);
            
            doc.Views.Redraw();
            
            Settings.SetInteger("sI", iterations.CurrentValue);
            Settings.SetDouble("sL", length.CurrentValue);
            Settings.SetDouble("sW", width.CurrentValue);
            Settings.SetDouble("sH", height.CurrentValue);
            Settings.SetDouble("sA", angle.CurrentValue);
            Settings.SetBool("sCP", centralPolygon.CurrentValue);
            Settings.SetBool("sIn", inverse.CurrentValue);
			
            // ---
			
            
            return Result.Success;
        }
    }
    
    public class LindenmayerSystem : Command
    {
    	public LindenmayerSystem()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static LindenmayerSystem Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "LSystem"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            
            string variable = "";
            string[] dvariables = null,
            		 mvariables = null,
            		 cvariables = null;
            List<string> drules = new List<string>(),
            			 mrules = new List<string>(),
            			 crules = new List<string>(),
            			 drawDirections = new List<string>();
            bool cancel = false;
            
            drawDirections.Add("Right");
            drawDirections.Add("Forward");
            drawDirections.Add("Up");
            
            int savedIterations = Settings.GetInteger("sI", 0);
            int savedDrawDirection = Settings.GetInteger("sDD", 0);
            double savedLineLength = Settings.GetDouble("sLL", 1);
            double savedLineIncrement = Settings.GetDouble("sLI", .2);
            double savedLineScale = Settings.GetDouble("sLS", .5);
            double savedLineThickness = Settings.GetDouble("sLT", 0);
            double savedThicknessIncrement = Settings.GetDouble("sTI", .1);
            double savedThicknessScale = Settings.GetDouble("sTS", .5);
            double savedTurnAngle = Settings.GetDouble("sTA", 45);
            double savedTurnAngleIncrement = Settings.GetDouble("sTAI", 10);
            bool savedDrawUp = Settings.GetBool("sDU", true);
            bool savedDrawFractal = Settings.GetBool("sDF", true);
            string savedAxiom = Settings.GetString("sA", "A");
            string[] savedDrawVariables = Settings.GetStringList("sDV", new string[] {"F"});
            string[] savedMoveVariables = Settings.GetStringList("sMV", new string[] {"f"});
            string[] savedControlVariables = Settings.GetStringList("sCV", new string[] {"A", "B"});
            string[] savedDrawRules = Settings.GetStringList("sDR", new string[] {"F+F-F-F+F"});
            string[] savedMoveRules = Settings.GetStringList("sMR", new string[] {"none"});
            string[] savedControlRules = Settings.GetStringList("sCR", new string[]{"none", "none"});
            
            List<string> drawRules = savedDrawRules.ToList(),
            			 moveRules = savedMoveRules.ToList(),
            			 controlRules = savedControlRules.ToList();
            string dvars = "",
            	   mvars = "",
            	   cvars = "";
            int dIndex = savedDrawDirection;
            
            if (savedDrawVariables != null && savedDrawVariables.Length != 0)
            	foreach (string var in savedDrawVariables)
            		dvars += var + " ";
            else
            	dvars = "F";
            
            if (savedMoveVariables != null && savedMoveVariables.Length != 0)
            	foreach (string var in savedMoveVariables)
            		mvars += var + " ";
            else
            	mvars = "f";
            
            if (savedControlVariables != null && savedControlVariables.Length != 0)
            	foreach (string var in savedControlVariables)
            		cvars += var + " ";
            else
            	cvars = " ";
            
            if (Menu.GetString("Input Draw Variables", ref variable, dvars))
            {
			    dvariables = String.WordsToArray(variable);
			    
            	if (Menu.GetString("Input Move Variables", ref variable, mvars))
            	{
			        mvariables = String.WordsToArray(variable);
			        
		            if (Menu.GetString("Input Control Variables", ref variable, cvars))
		            {
			            cvariables = String.WordsToArray(variable);
			            
			            if (Menu.GetString("Axiom", ref variable, savedAxiom))
			            {
			            	savedAxiom = variable;
			            	
			            	for (int i = drawRules.Count; i < dvariables.Length; i++)
			            		drawRules.Add("none");
			            	
			            	for (int i = moveRules.Count; i < mvariables.Length; i++)
			            		moveRules.Add("none");
			            	
			            	for (int i = controlRules.Count; i < cvariables.Length; i++)
			            		controlRules.Add("none");
			            	
			            	
			        		for (int i = 0; i < dvariables.Length; i++)
			        		{
			        			if (Menu.GetString("Rule " + dvariables[i] + " ➞ ", ref variable, drawRules[i]))
			        				drules.Add(variable);
			        			else
			        			{
			        				cancel = true;
			        				break;
			        			}
			        		}
			        		for (int i = 0; i < mvariables.Length && !cancel; i++)
			        		{
			        			if (Menu.GetString("Rule " + mvariables[i] + " ➞ ", ref variable, moveRules[i]))
			        				mrules.Add(variable);
			        			else
			        			{
			        				cancel = true;
			        				break;
			        			}
			        		}
			        		for (int i = 0; i < cvariables.Length && !cancel; i++)
			        		{
			        			if (Menu.GetString("Rule " + cvariables[i] + " ➞ ", ref variable, controlRules[i]))
			        				crules.Add(variable);
			        			else
			        			{
			        				cancel = true;
			        				break;
			        			}
			        		}
			        		
			        		GetOption getOption = new GetOption();
				            GetResult get_rc = new GetResult();
				            
				            OptionInteger iterations = new OptionInteger(savedIterations, 0, 15);
				            OptionDouble lineLength = new OptionDouble(savedLineLength, .001, 1000);
				            OptionDouble lineScale = new OptionDouble(savedLineScale, .001, 1000);
				            OptionDouble lineIncrement = new OptionDouble(savedLineIncrement, .001, 1000);
				            OptionDouble lineThickness = new OptionDouble(savedLineThickness, 0, 100);
				            OptionDouble thicknessIncrement = new OptionDouble(savedThicknessIncrement, .001, 1000);
				            OptionDouble thicknessScale = new OptionDouble(savedThicknessScale, .001, 1000);
				            OptionDouble turnAngle = new OptionDouble(savedTurnAngle, -359.99999999, 359.99999999);
				            OptionDouble turnAngleIncrement = new OptionDouble(savedTurnAngleIncrement, -359.99999999, 359.99999999);
				            OptionToggle drawUp = new OptionToggle(savedDrawUp, "Right", "Up");
				            OptionToggle drawFractal = new OptionToggle(savedDrawFractal, "Off", "On");
				            
				            getOption.SetCommandPrompt("L-System Options");
				            getOption.AcceptNothing(true);
				            
				            getOption.AddOptionInteger("Iterations", ref iterations);
				            getOption.AddOptionDouble("Line_Length", ref lineLength);
				            getOption.AddOptionDouble("Line_Increment", ref lineIncrement);
				            getOption.AddOptionDouble("Line_Scale", ref lineScale);
				            getOption.AddOptionDouble("Line_Thickness", ref lineThickness);
				            getOption.AddOptionDouble("Thickness_Increment", ref thicknessIncrement);
				            getOption.AddOptionDouble("Thickness_Scale", ref thicknessScale);
				            getOption.AddOptionDouble("Turn_Angle", ref turnAngle);
				            getOption.AddOptionDouble("Angle_Increment", ref turnAngle);
				            var dOption = getOption.AddOptionList("Draw_Direction", drawDirections, dIndex);
				            getOption.AddOptionToggle("Draw_Fractal", ref drawFractal);
				            
				            while (!cancel && get_rc != GetResult.Cancel)
				            {
				                // perform the get operation. This will prompt the user to input a string
				                
				                get_rc = getOption.Get();
				                
				                if (get_rc == GetResult.Nothing)
				                {
				                	Direction drawDirection = Direction.NONE;
				                	
				                	switch (dIndex)
				                	{
				                		case 0:
				                			drawDirection = Direction.RIGHT;
				                			break;
				                		case 1:
				                			drawDirection = Direction.FORWARD;
				                			break;
				                		default:
				                			drawDirection = Direction.UP;
				                			break;
				                	}
				                	
				                	var pipes = new List<Brep>();
				                	
				                	List<Curve> fractal = Generate.LindenmayerSystem(doc, ref pipes, iterations.CurrentValue, lineLength.CurrentValue, lineIncrement.CurrentValue, lineScale.CurrentValue, lineThickness.CurrentValue, thicknessIncrement.CurrentValue, thicknessScale.CurrentValue, turnAngle.CurrentValue, turnAngleIncrement.CurrentValue, dvariables.ToList(), mvariables.ToList(), cvariables.ToList(), drules, mrules, crules, savedAxiom, drawDirection, drawFractal.CurrentValue);
				                	
				                	foreach (Curve line in fractal)
				                		doc.Objects.AddCurve(line);
				                	foreach (Brep pipe in pipes)
				                		doc.Objects.AddBrep(pipe);
				                	
				                	doc.Views.Redraw();
				                	Output.Here("success");
				                	break;
				                }
				                if (getOption.OptionIndex() == dOption)
				                	dIndex = getOption.Option().CurrentListOptionIndex;
				            }
				            
				            savedIterations = iterations.CurrentValue;
				            savedLineLength = lineLength.CurrentValue;
				            savedLineIncrement = lineIncrement.CurrentValue;
				            savedLineScale = lineScale.CurrentValue;
				            savedLineThickness = lineThickness.CurrentValue;
				            savedThicknessIncrement = thicknessIncrement.CurrentValue;
				            savedThicknessScale = thicknessScale.CurrentValue;
				            savedTurnAngle = turnAngle.CurrentValue;
				            savedTurnAngleIncrement = turnAngleIncrement.CurrentValue;
				            savedDrawFractal = drawFractal.CurrentValue;
				            drawRules = drules;
				            moveRules = mrules;
				            controlRules = crules;
		            	}
		            }
            	}
            }
            Output.ToRhinoConsole("end");
            Settings.SetInteger("sI", savedIterations);
            Settings.SetInteger("sDD", dIndex);
            Settings.SetDouble("sLL", savedLineLength);
            Settings.SetDouble("sLI", savedLineIncrement);
            Settings.SetDouble("sLS", savedLineScale);
            Settings.SetDouble("sLT", savedLineThickness);
            Settings.SetDouble("sTS", savedThicknessIncrement);
            Settings.SetDouble("sTS", savedThicknessScale);
            Settings.SetDouble("sTA", savedTurnAngle);
            Settings.SetDouble("sTAI", savedTurnAngleIncrement);
            Settings.SetBool("sDF", savedDrawFractal);
            Settings.SetString("sA", savedAxiom);
            Settings.SetStringList("sDV", dvariables.ToArray());
            Settings.SetStringList("sMV", mvariables.ToArray());
            Settings.SetStringList("sCV", cvariables.ToArray());
            Settings.SetStringList("sDR", drawRules.ToArray());
            Settings.SetStringList("sMR", moveRules.ToArray());
            Settings.SetStringList("sCR", controlRules.ToArray());
            
            // ---
			
            
            return Result.Success;
        }
    }
}