#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ColorMutate", Category = "Color", Help = "Basic template with one color in/out", Tags = "")]
	#endregion PluginInfo
	public class ColorColorMutateNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input Start", DefaultColor = new double[] {
			0.1,
			0.2,
			0.3,
			1.0
		})]
		public ISpread<RGBAColor> FInputStart;
		
		[Input("Input Source", DefaultColor = new double[] {
			0.1,
			0.2,
			0.3,
			1.0
		})]
		public ISpread<RGBAColor> FInputSource;
		
		[Input("Input Compare", DefaultColor = new double[] {
			0.1,
			0.2,
			0.3,
			1.0
		})]
		public ISpread<RGBAColor> FInputCompare;

		[Output("Output")]
		public ISpread<RGBAColor> FOutput;

		[Import()]
		public ILogger FLogger;
		
		private static int iter = 0;
		#endregion fields & pins

		public bool isNearColor(RGBAColor source, RGBAColor comp)
		{
			if(Math.Abs(source.R - comp.R) < 5
			&& Math.Abs(source.G - comp.G) < 6
			&& Math.Abs(source.B - comp.B) < 5)
				return true;
			
			return false;
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			Random rand = new Random();
			
			for (int i = 0; i < SpreadMax; i++)
			{
				if(isNearColor(FInputSource[i], FInputCompare[i]))
				{
					FOutput[i] = FInputCompare[i];
				}
				else
				{
					var nb = rand.Next(1,11);
					double r = FInputCompare[i].R
						, g = FInputCompare[i].G 
						, b = FInputCompare[i].B;
					switch(nb)
					{
					case 1 :
						r = FInputCompare[i].G;
						g = FInputCompare[i].R;
						break;
					case 2:
						r = FInputCompare[i].B;
						b = FInputCompare[i].R;
						break;
					case 3 :
						g = FInputCompare[i].B;
						b = FInputCompare[i].G;
						break;
					case 4:
						b = FInputCompare[i].G;
						g = FInputCompare[i].B;
						break;
					case 5 :
						r = rand.Next(0, 255);
						break;
					case 6:
						g = rand.Next(0, 255);
						break;
					case 7 :
						b = rand.Next(0, 255);
						break;
					case 8:
						r = (r + rand.Next(-10, 11)) % 255;
						break;
					case 9 :
						g = (g + rand.Next(-10, 11)) % 255;
						break;
					case 10:
						b = (b + rand.Next(-10, 11)) % 255;
						break;					
					};
					if(iter == 0)
						FOutput[i] = new RGBAColor(0,0,0,255);
					else
						FOutput[i] = new RGBAColor(r, g, b, 255);
				}
			}
			iter++;
		}
	}
}
