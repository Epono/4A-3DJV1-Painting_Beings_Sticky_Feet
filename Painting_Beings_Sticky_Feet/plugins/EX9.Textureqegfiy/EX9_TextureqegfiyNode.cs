#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "qegfiy", Category = "EX9.Texture", Help = "Basic template which creates a texture", Tags = "")]
	#endregion PluginInfo
	public class EX9_TextureqegfiyNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		//little helper class used to store information for each
		//texture resource
		public class Info
		{
			public int Slice;
			public int Width;
			public int Height;
			public int SpreadsCount;
			public Random Random;
		}
		
		[Input("Enable")]
		public ISpread<bool> FSEnableIn;
		
		[Input("Number of spreads", DefaultValue = 20, MinValue = 1)]
		public ISpread<int> FSpreadsCountIn;
		
		[Input("Width", DefaultValue = 64, MinValue = 1)]
		public ISpread<int> FWidthIn;

		[Input("Height", DefaultValue = 64, MinValue = 1)]
		public ISpread<int> FHeightIn;

		[Output("Texture Out")]
		public ISpread<TextureResource<Info>> FTextureOut;

		[Import()]
		public ILogger FLogger;

		public void OnImportsSatisfied()
		{
			//spreads have a length of one by default, change it
			//to zero so ResizeAndDispose works properly.
			FTextureOut.SliceCount = 0;
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FTextureOut.ResizeAndDispose(FSpreadsCountIn[0], CreateTextureResource);
			for (int i = 0; i < FSpreadsCountIn[0]; i++) {
				var textureResource = FTextureOut[i];
				var info = textureResource.Metadata;
				//recreate textures if resolution was changed
				if (info.Width != FWidthIn[i] || info.Height != FHeightIn[i] || info.SpreadsCount != FSpreadsCountIn[0]) {
					textureResource.Dispose();
					textureResource = CreateTextureResource(i);
					info = textureResource.Metadata;
				}
				if(info.SpreadsCount == FSpreadsCountIn[0]) {
					textureResource.NeedsUpdate = false;
				} else {
					textureResource.NeedsUpdate = true;
				}
				if(FSEnableIn[0] == true) {
					textureResource.NeedsUpdate = true;
				}
				
				FTextureOut[i] = textureResource;
			}
		}

		TextureResource<Info> CreateTextureResource(int slice)
		{
			Int32 unixTimestamp = DateTime.UtcNow.Millisecond*slice;
			Random random = new Random(unixTimestamp);
			var info = new Info {
				Slice = slice,
				Width = FWidthIn[slice],
				Height = FHeightIn[slice],
				Random = random,
				SpreadsCount = FSpreadsCountIn[0]
			};
			return TextureResource.Create(info, CreateTexture, UpdateTexture);
		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		Texture CreateTexture(Info info, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + info.Slice);
			return TextureUtils.CreateTexture(device, Math.Max(info.Width, 1), Math.Max(info.Height, 1));
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe void UpdateTexture(Info info, Texture texture)
		{
			TextureUtils.Fill32BitTexInPlace(texture, info, FillTexure);
		}

		//this is a pixelshader like method, which we pass to the fill function
		unsafe void FillTexure(uint* data, int row, int col, int width, int height, Info info)
		{
			//crate position in texture in the range [0..1]
			var x = (double)row / height;
			var y = (double)col / width;

			byte r = (byte)(info.Random.NextDouble()*255);
			byte g = (byte)(info.Random.NextDouble()*255);
			byte b = (byte)(info.Random.NextDouble()*255);
			//a pixel is just a 32-bit unsigned int value
			var pixel = UInt32Utils.fromARGB(255, r, g, b);

			//copy pixel into texture
			TextureUtils.SetPtrVal2D(data, pixel, row, col, width);
		}
	}
}