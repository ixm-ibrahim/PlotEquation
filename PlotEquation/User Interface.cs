using System;
using System.Linq;
using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;

namespace PlotEquation
{
    /// <summary>
    /// Escape key event handler helper class
    /// </summary>
    /// 
	class EscapeKeyEventHandler : IDisposable
    {
        bool _escapeKeyPressed = false;

        public EscapeKeyEventHandler(string message)
        {
            RhinoApp.EscapeKeyPressed += (RhinoApp_EscapeKeyPressed);
            RhinoApp.WriteLine(message);
        }

        public bool EscapeKeyPressed
        {
            get
            {
                RhinoApp.Wait(); // "pumps" the Rhino message queue
                return _escapeKeyPressed;
            }
        }

        void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
        {
            _escapeKeyPressed = true;
        }

        public void Dispose()
        {
            RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
        }
    }
	
	public static class Menu
	{
		public static bool GetInteger(string str, ref int n)
		{
			GetNumber getNumber = new GetNumber();
            GetResult get_rc = new GetResult();
            
            getNumber.AcceptNothing(true);
            getNumber.SetCommandPrompt(str);
            
            while (get_rc != GetResult.Cancel && get_rc != GetResult.Nothing && get_rc != GetResult.Number)
            {
                // perform the get operation. This will prompt the user to input a integer
				
                get_rc = getNumber.Get();
            }
            
            if (get_rc == GetResult.Nothing || get_rc == GetResult.Cancel)
            	return false;
            
            if (get_rc == GetResult.Number)
        		n = Convert.ToInt32(getNumber.Number());
            
            return true;
		}
		
		public static bool GetDouble(string str, ref double d)
		{
			GetNumber getNumber = new GetNumber();
            GetResult get_rc = new GetResult();
            
            getNumber.AcceptNothing(true);
            getNumber.SetCommandPrompt(str);
            
            while (get_rc != GetResult.Cancel && get_rc != GetResult.Nothing && get_rc != GetResult.Number)
            {
                // perform the get operation. This will prompt the user to input a double
				
                get_rc = getNumber.Get();
            }
            
            if (get_rc == GetResult.Nothing || get_rc == GetResult.Cancel)
            	return false;
            
            if (get_rc == GetResult.Number)
        		d = getNumber.Number();
            
            return true;
		}
		
		public static bool GetString(string str, ref string s, string defaultValue = "")
		{
			GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            
            getStr.AcceptNothing(true);
            getStr.SetCommandPrompt(str);
            
            if (!defaultValue.Equals(""))
            {
            	getStr.SetDefaultString(defaultValue);
            	getStr.SetCommandPromptDefault(defaultValue);
            }
            
            while (get_rc != GetResult.Cancel && get_rc != GetResult.Nothing&& get_rc != GetResult.String)
            {
                // perform the get operation. This will prompt the user to input a string with spaces
                
                get_rc = getStr.GetLiteralString();
            }
            
            if ((defaultValue.Equals("") && get_rc == GetResult.Nothing) || get_rc == GetResult.Cancel)
            	return false;
            
            if (get_rc == GetResult.String || get_rc == GetResult.Nothing)
        		s = getStr.StringResult();
            
            return true;
		}
		public static bool GetString(string str, ref string s, string optionStr, ref int optionInt, int lowerBound, int upperBound)
		{
			GetString getStr = new GetString();
            GetResult get_rc = new GetResult();
            
            OptionInteger n = new OptionInteger(optionInt, lowerBound, upperBound);
            
            getStr.AcceptNothing(true);
            getStr.SetCommandPrompt(str);
            
            getStr.AddOptionInteger(optionStr, ref n);
            
            while (get_rc != GetResult.Cancel && get_rc != GetResult.Nothing&& get_rc != GetResult.String)
            {
                // perform the get operation. This will prompt the user to input a string
                
                get_rc = getStr.GetLiteralString();
            }
            
            if (get_rc == GetResult.Nothing || get_rc == GetResult.Cancel)
            	return false;
            
            if (get_rc == GetResult.String)
        		s = getStr.StringResult();
            
            optionInt = n.CurrentValue;
            
            return true;
		}
	}
}