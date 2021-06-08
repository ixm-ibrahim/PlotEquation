using System;
using Rhino;
using Rhino.Commands;

namespace PlotEquation
{
	// Cartesian (x,y,z)
		// f(x) - f(y)
	    public class CartesianCurve : Command
	    {
	        static CartesianCurve _instance;
	        public CartesianCurve()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            _instance = this;
	        }
	
	        ///<summary>The only instance of this command.</summary>
	        public static CartesianCurve Instance
	        {
	            get;
	            private set;
	        }
	
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "CartesianCurve"; }
	        }
	        
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedXLowerBound = Settings.GetDouble("sXLB", -10);
	            double savedXUpperBound = Settings.GetDouble("sXUB", 10);
	            double savedYLowerBound = Settings.GetDouble("sYLB", -10);
	            double savedYUpperBound = Settings.GetDouble("sYUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            
	            Input.Curve(doc, EnglishName, ExpressionType.CARTESIAN, ref savedDrawExpression, ref savedLastExpression, ref savedXLowerBound, ref savedXUpperBound, ref savedYLowerBound, ref savedYUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sXLB", savedXLowerBound);
	            Settings.SetDouble("sXUB", savedXUpperBound);
	            Settings.SetDouble("sYLB", savedYLowerBound);
	            Settings.SetDouble("sYUB", savedYUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLE", savedLastExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
		
		// 0 = f(x,y)
	    public class ImplicitCartesianCurve : Command
	    {
	        static ImplicitCartesianCurve _instance;
	        public ImplicitCartesianCurve()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            _instance = this;
	        }
	
	        ///<summary>The only instance of this command.</summary>
	        public static ImplicitCartesianCurve Instance
	        {
	            get;
	            private set;
	        }
	
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "ImplicitCartesianCurve"; }
	        }
	        
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedXLowerBound = Settings.GetDouble("sXLB", -10);
	            double savedXUpperBound = Settings.GetDouble("sXUB", 10);
	            double savedYLowerBound = Settings.GetDouble("sYLB", -10);
	            double savedYUpperBound = Settings.GetDouble("sYUB", 10);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            
	            Input.ImplicitCurve(doc, EnglishName, ExpressionType.CARTESIAN, ref savedLastExpression, ref savedXLowerBound, ref savedXUpperBound, ref savedYLowerBound, ref savedYUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sXLB", savedXLowerBound);
	            Settings.SetDouble("sXUB", savedXUpperBound);
	            Settings.SetDouble("sYLB", savedYLowerBound);
	            Settings.SetDouble("sYUB", savedYUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
		
	    // x(t), y(t)
	    public class ParametricCurve : Command
	    {
	        static ParametricCurve _instance;
	        public ParametricCurve()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the Parametric command.</summary>
	        public static ParametricCurve Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "ParametricCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedLowerBound = Settings.GetDouble("sLB", -10);
	            double savedUpperBound = Settings.GetDouble("sUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastXExpression = Settings.GetString("sLXE", null);
	            string savedLastYExpression = Settings.GetString("sLYE", null);
	            string dummy = null;
				
	            Input.ParametricCurve(doc, EnglishName, ExpressionType.CARTESIAN, ref savedDrawExpression, ref savedLastXExpression, ref savedLastYExpression, ref dummy, ref savedLowerBound, ref savedUpperBound, ref savedPointIteration, false);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sLB", savedLowerBound);
	            Settings.SetDouble("sUB", savedUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLXE", savedLastXExpression);
	            Settings.SetString("sLYE", savedLastYExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // f(x,y) - f(x,z) - f(y,z)
	    public class CartesianSurface : Command
	    {
	        public CartesianSurface()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static CartesianSurface Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "CartesianSurface"; }
	        }
	        
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedXLowerBound = Settings.GetDouble("sXLB", -1);
	            double savedXUpperBound = Settings.GetDouble("sXUB", 1);
	            double savedYLowerBound = Settings.GetDouble("sYLB", -1);
	            double savedYUpperBound = Settings.GetDouble("sYUB", 1);
	            double savedZLowerBound = Settings.GetDouble("sZLB", -1);
	            double savedZUpperBound = Settings.GetDouble("sZUB", 1);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
	            
	            Input.Surface(doc, EnglishName, ExpressionType.CARTESIAN, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedXLowerBound, ref savedXUpperBound, ref savedYLowerBound, ref savedYUpperBound, ref savedZLowerBound, ref savedZUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sXLB", savedXLowerBound);
	            Settings.SetDouble("sXUB", savedXUpperBound);
	            Settings.SetDouble("sYLB", savedYLowerBound);
	            Settings.SetDouble("sYUB", savedYUpperBound);
	            Settings.SetDouble("sZLB", savedZLowerBound);
	            Settings.SetDouble("sZUB", savedZUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // x(t), y(t), z(t)
	    public class ParametricCurve3D : Command
	    {
	        static ParametricCurve3D _instance;
	        public ParametricCurve3D()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the Parametric command.</summary>
	        public static ParametricCurve3D Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "ParametricCurve3D"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedLowerBound = Settings.GetDouble("sLB", -10);
	            double savedUpperBound = Settings.GetDouble("sUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastXExpression = Settings.GetString("sLXE", null);
	            string savedLastYExpression = Settings.GetString("sLYE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	            
	            Input.ParametricCurve(doc, EnglishName, ExpressionType.CARTESIAN, ref savedDrawExpression, ref savedLastXExpression, ref savedLastYExpression, ref savedLastZExpression, ref savedLowerBound, ref savedUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sLB", savedLowerBound);
	            Settings.SetDouble("sUB", savedUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLXE", savedLastXExpression);
	            Settings.SetString("sLYE", savedLastYExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // x(u,v), y(u,v), z(u,v)
	    public class ParametricSurface : Command
	    {
	        static ParametricSurface _instance;
	        public ParametricSurface()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Parametric3D command.</summary>
	        public static ParametricSurface Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "ParametricSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("sUUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastXExpression = Settings.GetString("sLXE", null);
	            string savedLastYExpression = Settings.GetString("sLYE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
				
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.CARTESIAN, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastXExpression, ref savedLastYExpression, ref savedLastZExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("sUUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLXE", savedLastXExpression);
	            Settings.SetString("sLYE", savedLastYExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // f(w,x,y) - f(w,x,z) - f(w,y,z)
	    public class CartesianSurface4D : Command
	    {
	        public CartesianSurface4D()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static CartesianSurface4D Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "4DCartesianSurface"; }
	        }
	        
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedWIteration = Settings.GetInteger("sWI", 10);
	            double savedWLowerBound = Settings.GetDouble("sWLB", -1);
	            double savedWUpperBound = Settings.GetDouble("sWUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedXLowerBound = Settings.GetDouble("sXLB", -1);
	            double savedXUpperBound = Settings.GetDouble("sXUB", 1);
	            double savedYLowerBound = Settings.GetDouble("sYLB", -1);
	            double savedYUpperBound = Settings.GetDouble("sYUB", 1);
	            double savedZLowerBound = Settings.GetDouble("sZLB", -1);
	            double savedZUpperBound = Settings.GetDouble("sZUB", 1);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.Surface(doc, EnglishName, ExpressionType.CARTESIAN_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedXLowerBound, ref savedXUpperBound, ref savedYLowerBound, ref savedYUpperBound, ref savedZLowerBound, ref savedZUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedWLowerBound, ref savedWUpperBound, ref savedWIteration);
				
	            Settings.SetInteger("sWI", savedWIteration);
	            Settings.SetDouble("sWLB", savedWLowerBound);
	            Settings.SetDouble("sWUB", savedWUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sXLB", savedXLowerBound);
	            Settings.SetDouble("sXUB", savedXUpperBound);
	            Settings.SetDouble("sYLB", savedYLowerBound);
	            Settings.SetDouble("sYUB", savedYUpperBound);
	            Settings.SetDouble("sZLB", savedZLowerBound);
	            Settings.SetDouble("sZUB", savedZUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // x(w,u,v), y(w,u,v), z(w,u,v)
	    public class ParametricSurface4D : Command
	    {
	        public ParametricSurface4D()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static ParametricSurface4D Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "4DParametricSurface"; }
	        }
	        
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedWIteration = Settings.GetInteger("sWI", 10);
	            double savedWLowerBound = Settings.GetDouble("sWLB", -1);
	            double savedWUpperBound = Settings.GetDouble("sWUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("sUUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastXExpression = Settings.GetString("sLXE", null);
	            string savedLastYExpression = Settings.GetString("sLYE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.CARTESIAN_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastXExpression, ref savedLastYExpression, ref savedLastZExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedWLowerBound, ref savedWUpperBound, ref savedWIteration);
				
	            Settings.SetInteger("sWI", savedWIteration);
	            Settings.SetDouble("sWLB", savedWLowerBound);
	            Settings.SetDouble("sWUB", savedWUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("sUUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLXE", savedLastXExpression);
	            Settings.SetString("sLYE", savedLastYExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	
	
	// Polar (theta,r,phi)
		// r(theta) - theta(r)
	    public class PolarCurve : Command
	    {
	        static PolarCurve _instance;
	        public PolarCurve()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Polar command.</summary>
	        public static PolarCurve Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "PolarCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", 0);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", Math.PI);
	            double savedRLowerBound = Settings.GetDouble("sRLB", 0);
	            double savedRUpperBound = Settings.GetDouble("sRUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastExpression = Settings.GetString("sLE", null);
				
	            Input.Curve(doc, EnglishName, ExpressionType.POLAR, ref savedDrawExpression, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedRLowerBound);
	            Settings.SetDouble("sRUB", savedRUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLE", savedLastExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
		// pi/2 = r(theta,r)
	    public class ImplicitPolarCurve : Command
	    {
	        static ImplicitPolarCurve _instance;
	        public ImplicitPolarCurve()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Polar command.</summary>
	        public static ImplicitPolarCurve Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "ImplicitPolarCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", 0);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", Math.PI);
	            double savedRLowerBound = Settings.GetDouble("sRLB", 0);
	            double savedRUpperBound = Settings.GetDouble("sRUB", 10);
	            string savedLastExpression = Settings.GetString("sLE", null);
				
	            Input.ImplicitCurve(doc, EnglishName, ExpressionType.POLAR, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedRLowerBound);
	            Settings.SetDouble("sRUB", savedRUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // theta(t), r(t)
	    public class PolarParametric : Command
	    {
	        static PolarParametric _instance;
	        public PolarParametric()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the Parametric command.</summary>
	        public static PolarParametric Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "PolarParametricCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedLowerBound = Settings.GetDouble("sLB", -10);
	            double savedUpperBound = Settings.GetDouble("sUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string d = null;
				
	            Input.ParametricCurve(doc, EnglishName, ExpressionType.POLAR, ref savedDrawExpression, ref savedLastThetaExpression, ref savedLastRExpression, ref d, ref savedLowerBound, ref savedUpperBound, ref savedPointIteration, false);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sLB", savedLowerBound);
	            Settings.SetDouble("sUB", savedUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // r(theta,phi) - theta(r,phi) - phi(theta,r)
	    public class SphericalSurface : Command
	    {
	        public SphericalSurface()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static SphericalSurface Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "SphericalSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", 0);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", 2 * Math.PI);
	            double savedRLowerBound = Settings.GetDouble("sRLB", 0);
	            double savedRUpperBound = Settings.GetDouble("sRUB", 2 * Math.PI);
	            double savedPhiLowerBound = Settings.GetDouble("sPhiLB", 0);
	            double savedPhiUpperBound = Settings.GetDouble("sPhiUB", 2 * Math.PI);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
				
	            Input.Surface(doc, EnglishName, ExpressionType.POLAR, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedPhiLowerBound, ref savedPhiUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedRLowerBound);
	            Settings.SetDouble("sRUB", savedRUpperBound);
	            Settings.SetDouble("sPhiLB", savedPhiLowerBound);
	            Settings.SetDouble("sPhiUB", savedPhiUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // theta(t), phi(t), r(t)
	    public class SphericalParametricCurve : Command
	    {
	        static SphericalParametricCurve _instance;
	        public SphericalParametricCurve()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the Parametric command.</summary>
	        public static SphericalParametricCurve Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "SphericalParametricCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
				int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedLowerBound = Settings.GetDouble("sLB", -10);
	            double savedUpperBound = Settings.GetDouble("sUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastPhiExpression = Settings.GetString("sLPhiE", null);
				
	            Input.ParametricCurve(doc, EnglishName, ExpressionType.POLAR, ref savedDrawExpression, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastPhiExpression, ref savedLowerBound, ref savedUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sLB", savedLowerBound);
	            Settings.SetDouble("sUB", savedUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLPhiE", savedLastPhiExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // theta(u,v), phi(u,v), r(u,v)
	    public class SphericalParametricSurface : Command
	    {
	        static SphericalParametricSurface _instance;
	        public SphericalParametricSurface()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Parametric3D command.</summary>
	        public static SphericalParametricSurface Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "SphericalParametricSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("uUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastPhiExpression = Settings.GetString("sLPhiE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
				
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.POLAR, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastPhiExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("uUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLPhiE", savedLastPhiExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // r(s,theta,phi) - theta(s,r,phi) - phi(s,theta,r)
	    public class SphericalSurface4D : Command
	    {
	        public SphericalSurface4D()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static SphericalSurface4D Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "4DSphericalSurface"; }
	        }
			
	        // Spherical4D Command
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedSIteration = Settings.GetInteger("sSI", 10);
	            double savedSLowerBound = Settings.GetDouble("sSLB", -1);
	            double savedSUpperBound = Settings.GetDouble("sSUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", 0);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", Math.PI);
	            double savedPhiLowerBound = Settings.GetDouble("sPhiLB", 0);
	            double savedPhiUpperBound = Settings.GetDouble("sPhiUB", Math.PI);
	            double savedRLowerBound = Settings.GetDouble("sRLB", 0);
	            double savedRUpperBound = Settings.GetDouble("sRUB", Math.PI);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.Surface(doc, EnglishName, ExpressionType.SPHERICAL_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedPhiLowerBound, ref savedPhiUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedSLowerBound, ref savedSUpperBound, ref savedSIteration);
				
	            Settings.SetInteger("sSI", savedSIteration);
	            Settings.SetDouble("sSLB", savedSLowerBound);
	            Settings.SetDouble("sSUB", savedSUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedRLowerBound);
	            Settings.SetDouble("sRUB", savedRUpperBound);
	            Settings.SetDouble("sPhiLB", savedPhiLowerBound);
	            Settings.SetDouble("sPhiUB", savedPhiUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // theta(s,u,v), phi(s,u,v), r(s,u,v)
	    public class SphericalParametricSurface4D : Command
	    {
	        static SphericalParametricSurface4D _instance;
	        public SphericalParametricSurface4D()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Parametric3D command.</summary>
	        public static SphericalParametricSurface4D Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "4DSphericalParametricSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedSIteration = Settings.GetInteger("sSI", 10);
	            double savedSLowerBound = Settings.GetDouble("sSLB", -1);
	            double savedSUpperBound = Settings.GetDouble("sSUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("uUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastPhiExpression = Settings.GetString("sLPhiE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.SPHERICAL_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastPhiExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedSLowerBound, ref savedSUpperBound, ref savedSIteration);
				
	            Settings.SetInteger("sSI", savedSIteration);
	            Settings.SetDouble("sSLB", savedSLowerBound);
	            Settings.SetDouble("sSUB", savedSUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("uUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLPhiE", savedLastPhiExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	
	
	// Cylindrical
		// r(theta,z) - theta(r,z) - z(theta,r)
	    public class CylindricalSurface : Command
	    {
	        static CylindricalSurface _instance;
	        public CylindricalSurface()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the CylindricalSurface command.</summary>
	        public static CylindricalSurface Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "CylindricalSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", -1);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", 1);
	            double savedRLowerBound = Settings.GetDouble("sRLB", -1);
	            double savedRUpperBound = Settings.GetDouble("sRUB", 1);
	            double savedZLowerBound = Settings.GetDouble("sZLB", -1);
	            double savedZUpperBound = Settings.GetDouble("sZUB", 1);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
				
	            Input.Surface(doc, EnglishName, ExpressionType.CYLINDRICAL, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedZLowerBound, ref savedZUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedThetaLowerBound);
	            Settings.SetDouble("sRUB", savedThetaUpperBound);
	            Settings.SetDouble("sZLB", savedZLowerBound);
	            Settings.SetDouble("sZUB", savedZUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // theta(t), z(t), r(t)
	    public class CylindricalParametricCurve : Command
	    {
	        static CylindricalParametricCurve _instance;
	        public CylindricalParametricCurve()
	        {
	            _instance = this;
	        }
			
	        ///<summary>The only instance of the Parametric command.</summary>
	        public static CylindricalParametricCurve Instance
	        {
	            get { return _instance; }
	        }
			
	        public override string EnglishName
	        {
	            get { return "CylindricalParametricCurve"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
				int savedPointIteration = Settings.GetInteger("sPI", 100);
	            double savedLowerBound = Settings.GetDouble("sLB", -10);
	            double savedUpperBound = Settings.GetDouble("sUB", 10);
	            bool savedDrawExpression = Settings.GetBool("sDE", true);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	
	            Input.ParametricCurve(doc, EnglishName, ExpressionType.CYLINDRICAL, ref savedDrawExpression, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastZExpression, ref savedLowerBound, ref savedUpperBound, ref savedPointIteration);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetDouble("sLB", savedLowerBound);
	            Settings.SetDouble("sUB", savedUpperBound);
	            Settings.SetBool("sDE", savedDrawExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // theta(u,v), z(u,v), r(u,v)
	    public class CylindricalParametricSurface : Command
	    {
	        static CylindricalParametricSurface _instance;
	        public CylindricalParametricSurface()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Parametric3D command.</summary>
	        public static CylindricalParametricSurface Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "CylindricalParametricSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("sUUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            double d1 = 0,
	            	   d2 = 0;
	            int d3 = 0;
	            
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.CYLINDRICAL, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastZExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref d1, ref d2, ref d3);
				
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("sUUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	
	            // ---
	
	            return Result.Success;
	        }
	    }
	    
	    // r(s,theta,z) - theta(s,r,z) - z(s,theta,r)
	    public class CylindricalSurface4D : Command
	    {
	        public CylindricalSurface4D()
	        {
	            // Rhino only creates one instance of each command class defined in a
	            // plug-in, so it is safe to store a refence in a static property.
	            Instance = this;
	        }
			
	        ///<summary>The only instance of this command.</summary>
	        public static CylindricalSurface4D Instance
	        {
	            get;
	            private set;
	        }
			
	        ///<returns>The command name as it appears on the Rhino command line.</returns>
	        public override string EnglishName
	        {
	            get { return "4DCylindricalSurface"; }
	        }
			
	        // Cylindrical4D Command
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedSIteration = Settings.GetInteger("sSI", 10);
	            double savedSLowerBound = Settings.GetDouble("sSLB", -1);
	            double savedSUpperBound = Settings.GetDouble("sSUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedThetaLowerBound = Settings.GetDouble("sThetaLB", 0);
	            double savedThetaUpperBound = Settings.GetDouble("sThetaUB", Math.PI);
	            double savedRLowerBound = Settings.GetDouble("sRLB", 0);
	            double savedRUpperBound = Settings.GetDouble("sRUB", Math.PI);
	            double savedZLowerBound = Settings.GetDouble("sZLB", 0);
	            double savedZUpperBound = Settings.GetDouble("sZUB", Math.PI);
	            string savedLastExpression = Settings.GetString("sLE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.Surface(doc, EnglishName, ExpressionType.CYLINDRICAL_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastExpression, ref savedThetaLowerBound, ref savedThetaUpperBound, ref savedRLowerBound, ref savedRUpperBound, ref savedZLowerBound, ref savedZUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedSLowerBound, ref savedSUpperBound, ref savedSIteration);
				
	            Settings.SetInteger("sSI", savedSIteration);
	            Settings.SetDouble("sSLB", savedSLowerBound);
	            Settings.SetDouble("sSUB", savedSUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sThetaLB", savedThetaLowerBound);
	            Settings.SetDouble("sThetaUB", savedThetaUpperBound);
	            Settings.SetDouble("sRLB", savedRLowerBound);
	            Settings.SetDouble("sRUB", savedRUpperBound);
	            Settings.SetDouble("sZLB", savedZLowerBound);
	            Settings.SetDouble("sZUB", savedZUpperBound);
	            Settings.SetString("sLE", savedLastExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
				
	            // ---
				
	            return Result.Success;
	        }
	    }
	    
	    // theta(s,u,v), z(s,u,v), r(s,u,v)
	    public class CylindricalParametricSurface4D : Command
	    {
	        static CylindricalParametricSurface4D _instance;
	        public CylindricalParametricSurface4D()
	        {
	            _instance = this;
	        }
	
	        ///<summary>The only instance of the Parametric3D command.</summary>
	        public static CylindricalParametricSurface4D Instance
	        {
	            get { return _instance; }
	        }
	
	        public override string EnglishName
	        {
	            get { return "4DCylindricalParametricSurface"; }
	        }
			
	        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
	        {
	            // TODO: start here modifying the behaviour of your command.
	            // ---
				
	            int savedSIteration = Settings.GetInteger("sSI", 10);
	            double savedSLowerBound = Settings.GetDouble("sSLB", -1);
	            double savedSUpperBound = Settings.GetDouble("sSUB", 1);
	            int savedPointIteration = Settings.GetInteger("sPI", 100);
	            int savedCurveIteration = Settings.GetInteger("sCI", 100);
	            double savedULowerBound = Settings.GetDouble("sULB", -1);
	            double savedUUpperBound = Settings.GetDouble("sUUB", 1);
	            double savedVLowerBound = Settings.GetDouble("sVLB", -1);
	            double savedVUpperBound = Settings.GetDouble("sVUB", 1);
	            string savedLastRExpression = Settings.GetString("sLRE", null);
	            string savedLastThetaExpression = Settings.GetString("sLThetaE", null);
	            string savedLastZExpression = Settings.GetString("sLZE", null);
	            bool savedUPriority = Settings.GetBool("sUP", true);
	            bool savedAttemptNetworkSurface = Settings.GetBool("sANS", false);
	            
				
	            Input.ParametricSurface(doc, EnglishName, ExpressionType.CYLINDRICAL_4D, ref savedUPriority, ref savedAttemptNetworkSurface, ref savedLastThetaExpression, ref savedLastRExpression, ref savedLastZExpression, ref savedULowerBound, ref savedUUpperBound, ref savedVLowerBound, ref savedVUpperBound, ref savedPointIteration, ref savedCurveIteration, ref savedSLowerBound, ref savedSUpperBound, ref savedSIteration);
				
	            Settings.SetInteger("sSI", savedSIteration);
	            Settings.SetDouble("sSLB", savedSLowerBound);
	            Settings.SetDouble("sSUB", savedSUpperBound);
	            Settings.SetInteger("sPI", savedPointIteration);
	            Settings.SetInteger("sCI", savedCurveIteration);
	            Settings.SetDouble("sULB", savedULowerBound);
	            Settings.SetDouble("sUUB", savedUUpperBound);
	            Settings.SetDouble("sVLB", savedVLowerBound);
	            Settings.SetDouble("sVUB", savedVUpperBound);
	            Settings.SetString("sLRE", savedLastRExpression);
	            Settings.SetString("sLThetaE", savedLastThetaExpression);
	            Settings.SetString("sLZE", savedLastZExpression);
	            Settings.SetBool("sUP", savedUPriority);
	            Settings.SetBool("sANS", savedAttemptNetworkSurface);
	            
	
	            // ---
	
	            return Result.Success;
	        }
	    }
}