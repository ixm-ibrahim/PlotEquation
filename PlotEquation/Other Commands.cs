using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace PlotEquation
{
	// Animation
    public class Animate : Command
    {
        static Animate _instance;
        public Animate()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            _instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static Animate Instance
        {
            get;
            private set;
        }

        ///<returns>The command name as it appeaget_rc on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "Animate"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
			
        	//Select Layer
			
            // ---
			
            return Result.Success;
        }
    }
    
    // Stereographic Projection
    public class StereographicProjection : Command
    {
        public StereographicProjection()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static StereographicProjection Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "StereographicProjection"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
			
            double savedSphereRadius = Settings.GetDouble("sSR", 1);
            int savedPointIteration = Settings.GetInteger("sPI", 100);
            bool savedShowPoints = Settings.GetBool("sSP", false);
            bool savedDeleteSelection = Settings.GetBool("sDS", true);
            
            GetResult get_rc = new GetResult();
            GetPoint getPt = new GetPoint();
            Result r = new Result();
            Point3d origin;
            ObjRef[] sRefs;
            
            OptionDouble sphereRadius = new OptionDouble(savedSphereRadius, .000001, 100000);
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionToggle showPoints = new OptionToggle(savedShowPoints, "No", "Yes");
            OptionToggle deleteSelection = new OptionToggle(savedDeleteSelection, "No", "Yes");
            
            
            while (true)
            {
            	r = new Result();
            	r = RhinoGet.GetMultipleObjects("Pick Curves and Surfaces to Stereographically Project", false, ObjectType.AnyObject, out sRefs);
            	bool fail = false;
            	 
				foreach (ObjRef oRef in sRefs)
					fail |= oRef.Surface() == null && oRef.Curve() == null;
				
                if (!fail)
                	break;
                
                Output.ToRhinoConsole("Invalid selection. Only Curves and Surfaces can be selected.");
            }
            
			if (r != Result.Success)
				return r;
			
            getPt.EnableTransparentCommands(true);
            getPt.SetCommandPrompt("Select Origin Point");
            getPt.AcceptNothing(true);
            getPt.SetCommandPromptDefault("Origin");
            getPt.SetDefaultPoint(Point3d.Origin);
            getPt.AddOptionToggle("Delete_Selection", ref deleteSelection);
            //getPt.AddOptionDouble("Sphere_Radius", ref sphereRadius);
            getPt.AddOptionInteger("Point_Iteration", ref pointIteration);
            getPt.AddOptionToggle("Show_Points", ref showPoints);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getPt.Get();
                
                if (getPt.CommandResult() != Result.Success)
                	break;
                
                if (get_rc == GetResult.Point)
                	origin = getPt.Point();
                
                if (get_rc == GetResult.Option)
                    continue;
				
                break;
            }
            
            foreach (ObjRef sRef in sRefs)
            {
            	if (get_rc != GetResult.Cancel)
	            {
					if (sRef.Surface() != null)
					{
						Surface surface = sRef.Surface();
						
						if (deleteSelection.CurrentValue)
							Graph.DeleteObject(doc, sRef.Object());
						
		            	Generate.StereographicProjection(doc, Graph.ToPoint3dList(surface, pointIteration.CurrentValue), sphereRadius.CurrentValue, pointIteration.CurrentValue, showPoints.CurrentValue);
					}
					else if (sRef.Curve() != null)
					{
		            	Curve curve = sRef.Curve();
						
						if (deleteSelection.CurrentValue)
							Graph.DeleteObject(doc, sRef.Object());
						
		            	Generate.StereographicProjection(doc, Graph.ToPoint3dList(curve, pointIteration.CurrentValue), sphereRadius.CurrentValue, pointIteration.CurrentValue, showPoints.CurrentValue);
					}
	            }
            	
            	RhinoApp.Wait();
            }
			
			Settings.SetDouble("sSR", sphereRadius.CurrentValue);
            Settings.SetInteger("sPI", pointIteration.CurrentValue);
            Settings.SetBool("sSP", showPoints.CurrentValue);
            Settings.SetBool("sDS", deleteSelection.CurrentValue);
            
            // ---

            return Result.Success;
        }
    }
    
    // Inverse Stereographic Projection
    public class InverseStereographicProjection : Command
    {
        public InverseStereographicProjection()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
		
        ///<summary>The only instance of this command.</summary>
        public static InverseStereographicProjection Instance
        {
            get;
            private set;
        }
		
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "InverseStereographicProjection"; }
        }
        
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
			
            double savedSphereRadius = Settings.GetDouble("sSR", 1);
            int savedPointIteration = Settings.GetInteger("sPI", 100);
            bool savedShowPoints = Settings.GetBool("sSP", false);
            bool savedDeleteSelection = Settings.GetBool("sDS", true);
            
            GetResult get_rc = new GetResult();
            GetPoint getPt = new GetPoint();
            Result r = new Result();
            Point3d origin;
            ObjRef[] sRefs;
            
            OptionDouble sphereRadius = new OptionDouble(savedSphereRadius, .000001, 100000);
            OptionInteger pointIteration = new OptionInteger(savedPointIteration, 1, 100000);
            OptionToggle showPoints = new OptionToggle(savedShowPoints, "No", "Yes");
            OptionToggle deleteSelection = new OptionToggle(savedDeleteSelection, "No", "Yes");
            
            Output.ToRhinoConsole("Only 'x' and 'y' components will be used.");
            
            while (true)
            {
            	r = new Result();
            	r = RhinoGet.GetMultipleObjects("Pick Curves and Surfaces to Inverse Stereographically Project", false, ObjectType.AnyObject, out sRefs);
            	bool fail = false;
            	
				foreach (ObjRef oRef in sRefs)
					fail |= oRef.Surface() == null && oRef.Curve() == null;
				
                if (!fail)
                	break;
                
                Output.ToRhinoConsole("Invalid selection. Only Curves and Surfaces can be selected.");
            }
            
            getPt.EnableTransparentCommands(true);
            getPt.SetCommandPrompt("Select Origin Point");
            getPt.AcceptNothing(true);
            getPt.SetCommandPromptDefault("Origin");
            getPt.SetDefaultPoint(Point3d.Origin);
            getPt.AddOptionToggle("Delete_Selection", ref deleteSelection);
            //getPt.AddOptionDouble("Sphere_Radius", ref sphereRadius);
            getPt.AddOptionInteger("Point_Iteration", ref pointIteration);
            getPt.AddOptionToggle("Show_Points", ref showPoints);
            
            while (get_rc != GetResult.Cancel)
            {
                // perform the get operation. This will prompt the user to input a string, but also
                // allow for command line options defined above
				
                get_rc = getPt.Get();
                
                if (getPt.CommandResult() != Result.Success)
                	break;
                
                if (get_rc == GetResult.Point)
                	origin = getPt.Point();
                
                if (get_rc == GetResult.Option)
                    continue;
				
                break;
            }
            
            foreach (ObjRef sRef in sRefs)
            {
            	if (get_rc != GetResult.Cancel)
	            {
					if (sRef.Surface() != null)
					{
						Surface surface = sRef.Surface();
						
						if (deleteSelection.CurrentValue)
							Graph.DeleteObject(doc, sRef.Object());
						
		            	Generate.InverseStereographicProjection(doc, Graph.ToPoint3dList(surface, pointIteration.CurrentValue), sphereRadius.CurrentValue, showPoints.CurrentValue);
					}
					else if (sRef.Curve() != null)
					{
		            	Curve curve = sRef.Curve();
						
						if (deleteSelection.CurrentValue)
							Graph.DeleteObject(doc, sRef.Object());
						
		            	Generate.InverseStereographicProjection(doc, Graph.ToPoint3dList(curve, pointIteration.CurrentValue), sphereRadius.CurrentValue, showPoints.CurrentValue);
					}
	            }
            	
            	RhinoApp.Wait();
            }
			
			Settings.SetDouble("sSR", sphereRadius.CurrentValue);
            Settings.SetInteger("sPI", pointIteration.CurrentValue);
            Settings.SetBool("sSP", showPoints.CurrentValue);
            Settings.SetBool("sDS", deleteSelection.CurrentValue);
            
            // ---

            return Result.Success;
        }
    }
}